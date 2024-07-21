namespace Randomizer.Graph.Combo.SuperMetroid.Model;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


[JsonConverter(typeof(NoteConverter))]
internal record Note(string[] Notes);

internal class NoteConverter : JsonConverter<Note>
{
    public override Note Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        if (root.ValueKind == JsonValueKind.String)
        {
            return new Note(new string[] { root.GetString() ?? "" });
        }
        else if (root.ValueKind == JsonValueKind.Array)
        {
            return new Note(root.EnumerateArray().Select(e => e.GetString()).ToArray()!);
        }
        else
        {
            throw new Exception($"Invalid type for Note: {root.ValueKind}");
        }
    }

    public override void Write(Utf8JsonWriter writer, Note value, JsonSerializerOptions options)
    {
        throw new Exception("The method or operation is not implemented.");
    }
}
