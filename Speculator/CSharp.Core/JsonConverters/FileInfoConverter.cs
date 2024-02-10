// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any non-commercial
// purpose.
//
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.

using Newtonsoft.Json;

namespace CSharp.Core.JsonConverters;

public class FileInfoConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(FileInfo);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var fileInfo = (FileInfo)value;
        serializer.Serialize(writer, fileInfo?.FullName ?? string.Empty);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var path = serializer.Deserialize<string>(reader);
        return new FileInfo(path);
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;
}