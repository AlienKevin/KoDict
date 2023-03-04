using MessagePack;

class BuildDict
{
    static void Main(string[] args)
    {
        using (StreamReader r = new StreamReader("../dict.json"))
        {
            string json = r.ReadToEnd();
            var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
            File.WriteAllBytes(path: "../dict.msgpack.l4z", MessagePackSerializer.ConvertFromJson(json, lz4Options));
        }
    }
}
