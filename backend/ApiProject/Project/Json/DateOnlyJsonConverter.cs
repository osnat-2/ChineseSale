using System.Globalization;
using Newtonsoft.Json;

namespace Project.Json;

public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string DateFormat = "yyyy-MM-dd";

    public override DateOnly ReadJson(
        JsonReader reader,
        Type objectType,
        DateOnly existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.String ||
            !DateOnly.TryParseExact(
                reader.Value as string,
                DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
        {
            throw new JsonSerializationException($"A date in {DateFormat} format is required.");
        }

        return date;
    }

    public override void WriteJson(JsonWriter writer, DateOnly value, JsonSerializer serializer) =>
        writer.WriteValue(value.ToString(DateFormat, CultureInfo.InvariantCulture));
}
