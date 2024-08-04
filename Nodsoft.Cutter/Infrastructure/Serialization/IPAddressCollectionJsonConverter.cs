using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nodsoft.Cutter.Infrastructure.Serialization;

public class IPAddressCollectionJsonConverter<TList> : JsonConverter<TList>
    where TList : IList<IPAddress>, new()
{
    public override TList Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        // Instantiate from type
        TList addresses = [];
        
        // Read the start of the array
        reader.Read();
        while (reader.TokenType != JsonTokenType.EndArray)
        {
            // Read the next token
            reader.Read();
            
            // Add the IP address to the list
            if (reader.TokenType == JsonTokenType.String)
            {
                addresses.Add(IPAddress.Parse(reader.GetString()));
            }
        }
        
        return addresses;
    }

    public override void Write(Utf8JsonWriter writer, TList value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (IPAddress address in value)
        {
            writer.WriteStringValue(address.ToString());
        }
        writer.WriteEndArray();
    }
}