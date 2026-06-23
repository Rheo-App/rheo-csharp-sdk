using System.Text.Json.Serialization;

namespace Rheo.Sdk.Types;

public enum RheoItemStatus
{
    Draft,
    Active,
    Sold,
    NotListed,
    Archived,
}

public enum RheoItemType
{
    Item,
    Container,
}

/// <summary>
/// How an item is shipped. Serializes snake_case: <c>integrated</c> (default — Rheo arranges
/// shipping, buyer pays actual), <c>seller_shipped</c> (you ship on your own carrier and report
/// tracking via <c>Orders.SubmitTrackingAsync</c>), <c>fixed</c> (flat <c>ShippingCost</c> Rheo
/// collects), <c>pickup_only</c> (collection only).
/// </summary>
public enum ShippingStrategy
{
    Integrated,
    SellerShipped,
    Fixed,
    PickupOnly,
}

// ============================================================================
// DOMAIN SYSTEM — typed mirror of the backend AssetDomain
// (rheo-market/src/models/items/domain.rs).
//
// Serialized as an internally-tagged union: the "domain" key is the discriminant
// (the canonical snake_case slug) and selects which attributes are valid. The
// discriminator is emitted by [JsonPolymorphic]; construct the concrete subtype
// for the domain you want. All attribute properties are optional.
// ============================================================================

[JsonPolymorphic(TypeDiscriminatorPropertyName = "domain")]
[JsonDerivedType(typeof(AutoPartsDomain), "auto_parts")]
[JsonDerivedType(typeof(VehiclesDomain), "vehicles")]
[JsonDerivedType(typeof(ElectronicsDomain), "electronics")]
[JsonDerivedType(typeof(ComputersDomain), "computers")]
[JsonDerivedType(typeof(FashionDomain), "fashion")]
[JsonDerivedType(typeof(BooksDomain), "books")]
[JsonDerivedType(typeof(WatchesDomain), "watches")]
[JsonDerivedType(typeof(HomeGardenDomain), "home_garden")]
[JsonDerivedType(typeof(ArtDomain), "art")]
[JsonDerivedType(typeof(SportsDomain), "sports")]
[JsonDerivedType(typeof(ToysDomain), "toys")]
[JsonDerivedType(typeof(ToolsDomain), "tools")]
[JsonDerivedType(typeof(JewelryDomain), "jewelry")]
[JsonDerivedType(typeof(MusicDomain), "music")]
[JsonDerivedType(typeof(CollectiblesDomain), "collectibles")]
[JsonDerivedType(typeof(BeautyDomain), "beauty")]
[JsonDerivedType(typeof(IndustrialDomain), "industrial")]
[JsonDerivedType(typeof(OtherDomain), "other")]
public abstract class DomainObject
{
    /// <summary>The canonical slug for this domain. Not serialized — the discriminator carries it.</summary>
    [JsonIgnore]
    public abstract string Domain { get; }
}

/// <summary>The donor vehicle a part was dismantled from.</summary>
public sealed class DonorVehicle
{
    /// <summary>Manufacturer of the donor vehicle, e.g. "Kawasaki", "Volvo".</summary>
    public string? Manufacturer { get; init; }

    /// <summary>Model name of the donor vehicle, e.g. "ER-6F", "XC90".</summary>
    public string? Model { get; init; }

    /// <summary>Model year of the donor vehicle.</summary>
    public int? Year { get; init; }

    /// <summary>Vehicle category: "Bil", "MC", "Skoter", "ATV", "Husvagn", "Moped", "Traktor".</summary>
    public string? VehicleType { get; init; }
}

/// <summary>The specific part being sold.</summary>
public sealed class PartInfo
{
    /// <summary>Human-readable part name, e.g. "Stötdämpare Bak".</summary>
    public string? Name { get; init; }

    /// <summary>OEM manufacturer part number, e.g. "31400452".</summary>
    public string? OemNumber { get; init; }

    /// <summary>
    /// Part-type codes from external ERP/catalog systems. Key is the system name
    /// (e.g. "recopart", "fen-sys", "tecdoc"), value is that system's code.
    /// </summary>
    public IReadOnlyDictionary<string, string>? CatalogCodes { get; init; }
}

/// <summary>Auto parts and alternative vehicle parts (MC, snowmobile, ATV, caravan, etc.).</summary>
public sealed class AutoPartsDomain : DomainObject
{
    [JsonIgnore]
    public override string Domain => "auto_parts";

    /// <summary>The donor vehicle the part was pulled from.</summary>
    public DonorVehicle? Vehicle { get; init; }

    /// <summary>The part being sold.</summary>
    public PartInfo? Part { get; init; }

    /// <summary>Condition grade: "A*", "A", "B", "C".</summary>
    public string? ConditionGrade { get; init; }

    /// <summary>Free-text condition note, e.g. "Smärrepig".</summary>
    public string? ConditionNote { get; init; }
}

/// <summary>Whole vehicles for sale — cars, motorcycles, boats, caravans.</summary>
public sealed class VehiclesDomain : DomainObject
{
    [JsonIgnore]
    public override string Domain => "vehicles";

    /// <summary>Swedish registration plate, e.g. "ABC123". Triggers a biluppgifter.se lookup.</summary>
    public string? RegistrationPlate { get; init; }
    public string? Make { get; init; }
    public string? Model { get; init; }
    public int? Year { get; init; }
    public int? MileageKm { get; init; }

    /// <summary>"petrol", "diesel", "electric", or "hybrid".</summary>
    public string? FuelType { get; init; }
    public string? Color { get; init; }
}

/// <summary>Consumer electronics, cameras, audio, phones.</summary>
public sealed class ElectronicsDomain : DomainObject
{
    [JsonIgnore]
    public override string Domain => "electronics";
    public string? SerialNumber { get; init; }
    public int? MemoryGb { get; init; }

    /// <summary>Battery health (0–100).</summary>
    public int? BatteryHealthPercentage { get; init; }
}

/// <summary>PCs, laptops, tablets, and peripherals. Same attribute shape as electronics.</summary>
public sealed class ComputersDomain : DomainObject
{
    [JsonIgnore]
    public override string Domain => "computers";
    public string? SerialNumber { get; init; }
    public int? MemoryGb { get; init; }

    /// <summary>Battery health (0–100). Relevant for laptops.</summary>
    public int? BatteryHealthPercentage { get; init; }
}

/// <summary>Clothing, shoes, bags, and accessories.</summary>
public sealed class FashionDomain : DomainObject
{
    [JsonIgnore]
    public override string Domain => "fashion";

    /// <summary>Free-form size label, e.g. "M", "42", "UK 8".</summary>
    public string? Size { get; init; }
    public string? Material { get; init; }

    /// <summary>"mens", "womens", "unisex", or "kids".</summary>
    public string? Gender { get; init; }
}

/// <summary>Books, magazines, DVDs, and vinyl records.</summary>
public sealed class BooksDomain : DomainObject
{
    [JsonIgnore]
    public override string Domain => "books";
    public string? Isbn { get; init; }
    public string? Author { get; init; }
    public string? Publisher { get; init; }

    /// <summary>BCP 47 language code, e.g. "sv", "en".</summary>
    public string? Language { get; init; }
}

/// <summary>Wristwatches and pocket watches.</summary>
public sealed class WatchesDomain : DomainObject
{
    [JsonIgnore]
    public override string Domain => "watches";
    public string? Brand { get; init; }
    public string? ModelReference { get; init; }

    /// <summary>"automatic", "quartz", or "manual".</summary>
    public string? MovementType { get; init; }
    public int? CaseSizeMm { get; init; }
}

// --- Unit domains: discriminant only, no attributes ---

public sealed class HomeGardenDomain : DomainObject { [JsonIgnore] public override string Domain => "home_garden"; }
public sealed class ArtDomain : DomainObject { [JsonIgnore] public override string Domain => "art"; }
public sealed class SportsDomain : DomainObject { [JsonIgnore] public override string Domain => "sports"; }
public sealed class ToysDomain : DomainObject { [JsonIgnore] public override string Domain => "toys"; }
public sealed class ToolsDomain : DomainObject { [JsonIgnore] public override string Domain => "tools"; }
public sealed class JewelryDomain : DomainObject { [JsonIgnore] public override string Domain => "jewelry"; }
public sealed class MusicDomain : DomainObject { [JsonIgnore] public override string Domain => "music"; }
public sealed class CollectiblesDomain : DomainObject { [JsonIgnore] public override string Domain => "collectibles"; }
public sealed class BeautyDomain : DomainObject { [JsonIgnore] public override string Domain => "beauty"; }
public sealed class IndustrialDomain : DomainObject { [JsonIgnore] public override string Domain => "industrial"; }
public sealed class OtherDomain : DomainObject { [JsonIgnore] public override string Domain => "other"; }
