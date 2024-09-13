using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datalake.Server.Services.Receiver;

public class JsonObjectConverter : JsonConverter<object>
{
	public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		switch (reader.TokenType)
		{
			case JsonTokenType.Number:
				if (reader.TryGetDouble(out double doubleValue))
				{
					return doubleValue;
				}
				break;
			case JsonTokenType.String:
				return reader.GetString();
			case JsonTokenType.True:
			case JsonTokenType.False:
				return reader.GetBoolean();
			default:
				using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
				{
					return doc.RootElement.Clone();
				}
		}
		throw new JsonException();
	}

	public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}
