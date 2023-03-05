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

    internal List<Entry> lookupWord(string query)
    {
        List<Entry> results = new List<Entry>();
        List<int> d0Index;
        List<int> d1Index;
        ulong queryHash = Farmhash.Sharp.Farmhash.Hash64(Hangeul.Hangeuls2Jamos(query));
        if (this.d0Neighborhood.TryGetValue(queryHash, out d0Index))
        {
            results.AddRange(d0Index.Select(index => this.Entries[index]));
        }
        else if (this.d1Neighborhood.TryGetValue(queryHash, out d1Index))
        {
            results.AddRange(d1Index.Select(index =>
            {
                string word = this.Entries[index].Word;
                return (index, Fastenshtein.Levenshtein.Distance(word, query));
            }).Where((index, d) => d <= 1).OrderBy(indexAndD => indexAndD.Item2).Select(indexAndD => this.Entries[indexAndD.Item1]));
        }
        return results;
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