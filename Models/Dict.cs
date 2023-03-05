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

    private byte[] readBytes(string filename)
    {
        return File.ReadAllBytes(File.Exists(filename) ? filename : $"../KoDict/{filename}");
    }

    public record Match
    {
        public record ExactMatch(List<Entry> Entries) : Match();
        public record D1Match(List<Entry> Entries) : Match();

        private Match() { }

        public List<Entry> GetEntries()
        {
            return this switch
            {
                ExactMatch entries => entries.Entries,
                D1Match entries => entries.Entries,
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
            return new Match.ExactMatch(d0Index.Select(index => this.Entries[index]).ToList());
        }
        else
        {
            List<Entry> results = new List<Entry>();
            for (var k = 0; k < queryJamos.Count(); k++)
            {
                ulong queryD1Hash = Farmhash.Sharp.Farmhash.Hash64(queryJamos.Remove(k, 1));
                if (this.d1Neighborhood.TryGetValue(queryD1Hash, out d1Index))
                    results.AddRange(d1Index.Select(index =>
                    {
                        string wordJamos = Hangeul.Hangeuls2Jamos(this.Entries[index].Word);
                        // System.Console.WriteLine($"{wordJamos} {queryJamos} {Fastenshtein.Levenshtein.Distance(wordJamos, queryJamos)}");
                        return (index, Fastenshtein.Levenshtein.Distance(wordJamos, queryJamos));
                    }).Where(indexAndD => indexAndD.Item2 <= 1).OrderBy(indexAndD => indexAndD.Item2).Select(indexAndD => this.Entries[indexAndD.Item1]));
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
    }
}