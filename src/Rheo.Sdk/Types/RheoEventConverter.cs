using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rheo.Sdk.Types;

/// <summary>
/// Reads the <c>eventType</c> discriminator at any position in the JSON object and
/// deserializes into the matching concrete event. Concrete types carry no converter,
/// so re-deserializing them does not recurse back into this converter.
/// </summary>
internal sealed class RheoEventConverter : JsonConverter<RheoEvent>
{
    public override RheoEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("eventType", out var typeProp) || typeProp.ValueKind != JsonValueKind.String)
            throw new JsonException("Rheo event payload is missing the 'eventType' discriminator");

        var eventType = typeProp.GetString();
        var concrete = eventType switch
        {
            "item.created" => typeof(ItemCreatedEvent),
            "item.images_ready" => typeof(ItemImagesReadyEvent),
            "listing.created" => typeof(ListingCreatedEvent),
            "listing.ended" => typeof(ListingEndedEvent),
            "listing.failed" => typeof(ListingFailedEvent),
            "item.sold" => typeof(ItemSoldEvent),
            _ => throw new JsonException($"Unknown Rheo event type '{eventType}'"),
        };

        return (RheoEvent)root.Deserialize(concrete, options)!;
    }

    public override void Write(Utf8JsonWriter writer, RheoEvent value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
}
