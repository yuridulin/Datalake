using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datalake.Database.Converters;

/// <summary>
/// Конвертер, обрабатывающий экстремальные значения чисел при работе с JSON
/// </summary>
public class NanToNullFloatConverter : JsonConverter<float?>
{
	/// <inheritdoc/>
	public override float? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> reader.TokenType == JsonTokenType.Null ? null : reader.GetSingle();

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, float? value, JsonSerializerOptions options)
	{
		if (value == null || float.IsNaN(value.Value) || float.IsInfinity(value.Value))
			writer.WriteNullValue();
		else
			writer.WriteNumberValue(value.Value);
	}
}