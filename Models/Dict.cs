using Newtonsoft.Json;

public class Dict
{
    public class Example
    {
        public string type;
        public string expr;
    }

    public class Sense
    {
        public string english_lemma;
        public string english_definition;
        public List<Example> examples;
    }

    public class Entry
    {
        public string word;
        public string origin;
        public string vocabulary_level;
        public string pos;
        public List<string> prs;
        public List<Sense> senses;
    }

    public List<Entry> entries;

    public Dict()
    {
        using (StreamReader r = new StreamReader("dict.json"))
        {
            string json = r.ReadToEnd();
            this.entries = JsonConvert.DeserializeObject<List<Entry>>(json);
        }
    }
}