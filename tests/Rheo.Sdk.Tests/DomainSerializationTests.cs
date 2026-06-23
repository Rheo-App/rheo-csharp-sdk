using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Rheo.Sdk.Types;
using Xunit;

namespace Rheo.Sdk.Tests;

public sealed class DomainSerializationTests
{
    // Mirrors RheoClient.BuildJsonOptions for the parts relevant to the domain union.
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    [Fact]
    public void AutoParts_SerializesNestedShapeWithDiscriminator()
    {
        var request = new UpsertItemRequest
        {
            Title = "Stötdämpare Bak — Kawasaki ER-6F",
            ImageUrls = new[] { "https://example.com/1.jpg" },
            Domain = new AutoPartsDomain
            {
                Vehicle = new DonorVehicle { Manufacturer = "Kawasaki", Model = "ER-6F", Year = 2006, VehicleType = "MC" },
                Part = new PartInfo
                {
                    Name = "Stötdämpare Bak",
                    OemNumber = "31400452",
                    CatalogCodes = new Dictionary<string, string> { ["recopart"] = "7118" },
                },
                ConditionGrade = "A",
            },
        };

        var json = JsonSerializer.Serialize(request, Json);

        using var doc = JsonDocument.Parse(json);
        // Wire shape: { "title": ..., "domain": { "domain": "auto_parts", "vehicle": {...}, "part": {...} } }
        var domain = doc.RootElement.GetProperty("domain");
        Assert.Equal("auto_parts", domain.GetProperty("domain").GetString());
        Assert.Equal("Kawasaki", domain.GetProperty("vehicle").GetProperty("manufacturer").GetString());
        Assert.Equal("31400452", domain.GetProperty("part").GetProperty("oemNumber").GetString());
        Assert.Equal("7118", domain.GetProperty("part").GetProperty("catalogCodes").GetProperty("recopart").GetString());
        Assert.Equal("A", domain.GetProperty("conditionGrade").GetString());
        // The discriminator must appear exactly once (no duplicate "domain" key).
        var domainKeyCount = domain.EnumerateObject().Count(p => p.Name == "domain");
        Assert.Equal(1, domainKeyCount);
    }

    [Fact]
    public void AutoParts_RoundTripsBackToTypedDomain()
    {
        var request = new UpsertItemRequest
        {
            Title = "Volvo part",
            ImageUrls = Array.Empty<string>(),
            Domain = new AutoPartsDomain { Part = new PartInfo { OemNumber = "312567-VOLVO" } },
        };

        var json = JsonSerializer.Serialize(request, Json);
        var back = JsonSerializer.Deserialize<UpsertItemRequest>(json, Json);

        var auto = Assert.IsType<AutoPartsDomain>(back!.Domain);
        Assert.Equal("312567-VOLVO", auto.Part!.OemNumber);
        Assert.Equal("auto_parts", auto.Domain);
    }

    [Fact]
    public void UnitDomain_SerializesDiscriminantOnly()
    {
        var request = new UpsertItemRequest { Title = "Tool", ImageUrls = Array.Empty<string>(), Domain = new ToolsDomain() };

        var json = JsonSerializer.Serialize(request, Json);

        using var doc = JsonDocument.Parse(json);
        var domain = doc.RootElement.GetProperty("domain");
        Assert.Equal("tools", domain.GetProperty("domain").GetString());
        // Unit domains carry only the discriminant, written exactly once.
        Assert.Single(domain.EnumerateObject());
    }
}
