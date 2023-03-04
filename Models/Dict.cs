using MessagePack;

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

    public Dict()
    {
        var dictName = "dict.msgpack.l4z";
        var dictPath = File.Exists(dictName) ? dictName : $"../KoDict/{dictName}";
        byte[] msgpack = File.ReadAllBytes(dictPath);
        var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
        this.Entries = MessagePackSerializer.Deserialize<List<Entry>>(msgpack, lz4Options);
    }
}