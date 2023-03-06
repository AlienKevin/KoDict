using MessagePack;

using Index = System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.List<int>>;

public class Dict
{
    [MessagePackObject]
    public class Example
    {
        [Key("type")]
        public string Type;
        [Key("ko")]
        public string Expr;
    }

    [MessagePackObject]
    public class Sense
    {
        [Key("english_lemma")]
        public string EnglishLemma;
        [Key("english_definition")]
        public string EnglishDefinition;
        [Key("examples")]
        public List<Example> Examples;
    }

    [MessagePackObject]
    public class Entry
    {
        [Key("word")]
        public string Word;
        [Key("index")]
        public int Index;
        [Key("origin")]
        public string Origin;
        [Key("vocabulary_level")]
        public string VocabularyLevel;
        [Key("pos")]
        public string Pos;
        [Key("prs")]
        public List<string> Prs;
        [Key("senses")]
        public List<Sense> Senses;
    }

    public List<Entry> Entries;
    private Index d0Neighborhood;
    private Index d1Neighborhood;
    private Index enD0Neighborhood;
    private Index enD1Neighborhood;

    private byte[] readBytes(string filename)
    {
        return File.ReadAllBytes(File.Exists(filename) ? filename : $"../KoDict/{filename}");
    }

    public record MatchIndex(int EntryIndex, int SenseIndex);

    public record Match
    {
        public record ExactMatch(List<MatchIndex> Indices) : Match();
        public record D1Match(List<MatchIndex> Indices) : Match();

        private Match() { }

        public List<MatchIndex> GetMatchIndices()
        {
            return this switch
            {
                ExactMatch match => match.Indices,
                D1Match match => match.Indices,
            };
        }

        public List<Entry> GetEntries(Dict dict)
        {
            return this switch
            {
                ExactMatch match => match.Indices.Select((index) => dict.Entries[index.EntryIndex]).ToList(),
                D1Match match => match.Indices.Select((index) => dict.Entries[index.EntryIndex]).ToList(),
            };
        }

        public bool IsExact()
        {
            return this switch
            {
                ExactMatch _ => true,
                _ => false,
            };
        }
    }

    internal Match lookupWord(string query)
    {
        List<int> d0Index;
        List<int> d1Index;
        string queryJamos = Hangeul.Hangeuls2Jamos(query);
        ulong queryHash = Farmhash.Sharp.Farmhash.Hash64(queryJamos);
        if (this.d0Neighborhood.TryGetValue(queryHash, out d0Index))
        {
            return new Match.ExactMatch(d0Index.Select(index => new MatchIndex(EntryIndex: index, SenseIndex: 0)).ToList());
        }
        else
        {
            List<MatchIndex> results = new List<MatchIndex>();
            for (var k = 0; k < queryJamos.Count(); k++)
            {
                ulong queryD1Hash = Farmhash.Sharp.Farmhash.Hash64(queryJamos.Remove(k, 1));
                if (this.d1Neighborhood.TryGetValue(queryD1Hash, out d1Index))
                    results.AddRange(d1Index.Select(index =>
                    {
                        string wordJamos = Hangeul.Hangeuls2Jamos(this.Entries[index].Word);
                        // System.Console.WriteLine($"{wordJamos} {queryJamos} {Fastenshtein.Levenshtein.Distance(wordJamos, queryJamos)}");
                        return (index, Fastenshtein.Levenshtein.Distance(wordJamos, queryJamos));
                    }).Where(indexAndD => indexAndD.Item2 <= 1).OrderBy(indexAndD => indexAndD.Item2)
                    .Select(indexAndD => new MatchIndex(EntryIndex: indexAndD.Item1, SenseIndex: 0)));
            }
            if (results.Count() == 0)
            {
                return lookupEnWord(query);
            }
            else
            {
                return new Match.D1Match(results);
            }
        }
    }

    public static int PackEntryAndSenseIndex(int entryIndex, int senseIndex)
    {
        // Compress entryIndex and senseIndex into a single 32-bit signed integer
        // Assume entryIndex is less than 2^(31 - 6) = 33554432
        // ASsume senseIndex is less than 2^6 = 64
        return (entryIndex << 6) | senseIndex;
    }

    public static MatchIndex UnpackEntryAndSenseIndex(int index)
    {
        // System.Console.WriteLine($"index: {index}");
        // System.Console.WriteLine($"EntryIndex: {index >> 6}");
        // System.Console.WriteLine($"SenseIndex: {index & 0b111111}");
        return new MatchIndex(EntryIndex: index >> 6, SenseIndex: index & 0b111111);
    }

    private Match lookupEnWord(string query_)
    {
        List<int> d0Index;
        List<int> d1Index;
        string query = query_.ToLower();
        ulong queryHash = Farmhash.Sharp.Farmhash.Hash64(query);
        if (this.enD0Neighborhood.TryGetValue(queryHash, out d0Index))
        {
            return new Match.ExactMatch(d0Index.Select(index => UnpackEntryAndSenseIndex(index)).ToList());
        }
        else
        {
            List<MatchIndex> results = new List<MatchIndex>();
            for (var k = -1; k < query.Count(); k++)
            {
                ulong queryD1Hash = Farmhash.Sharp.Farmhash.Hash64(k < 0 ? query : query.Remove(k, 1));
                if (this.enD1Neighborhood.TryGetValue(queryD1Hash, out d1Index))
                    results.AddRange(d1Index.Select(index =>
                    {
                        var matchIndex = Dict.UnpackEntryAndSenseIndex(index);
                        var minD = this.Entries[matchIndex.EntryIndex].Senses[matchIndex.SenseIndex].EnglishLemma.Split("; ")
                        .Select(word => Fastenshtein.Levenshtein.Distance(word, query)).Min();
                        System.Console.WriteLine($"minD: {minD}");
                        return (index, minD);
                    }).Where(indexAndD => indexAndD.Item2 <= 1).OrderBy(indexAndD => indexAndD.Item2)
                    .Select(indexAndD => UnpackEntryAndSenseIndex(indexAndD.Item1)));
            }

            return new Match.D1Match(results);
        }
    }

    public Dict()
    {
        byte[] dictMsgpack = readBytes("dict.msgpack.l4z");
        var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
        this.Entries = MessagePackSerializer.Deserialize<List<Entry>>(dictMsgpack, lz4Options);
        byte[] d0NeighborhoodMsgpack = readBytes("d0Neighborhood.msgpack");
        this.d0Neighborhood = MessagePackSerializer.Deserialize<Index>(d0NeighborhoodMsgpack);
        byte[] d1NeighborhoodMsgpack = readBytes("d1Neighborhood.msgpack");
        this.d1Neighborhood = MessagePackSerializer.Deserialize<Index>(d1NeighborhoodMsgpack);
        byte[] enD0NeighborhoodMsgpack = readBytes("enD0Neighborhood.msgpack");
        this.enD0Neighborhood = MessagePackSerializer.Deserialize<Index>(enD0NeighborhoodMsgpack);
        byte[] enD1NeighborhoodMsgpack = readBytes("enD1Neighborhood.msgpack");
        this.enD1Neighborhood = MessagePackSerializer.Deserialize<Index>(enD1NeighborhoodMsgpack);
    }
}