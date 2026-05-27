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

[JsonPolymorphic(TypeDiscriminatorPropertyName = "domain")]
[JsonDerivedType(typeof(AutoPartsDomain), "auto_parts")]
[JsonDerivedType(typeof(ElectronicsDomain), "electronics")]
[JsonDerivedType(typeof(FashionDomain), "fashion")]
public abstract class DomainObject
{
    public abstract string Domain { get; }
}

public sealed class AutoPartsDomain : DomainObject
{
    public override string Domain => "auto_parts";
    public string? PartName { get; init; }
    public string? OemCode { get; init; }
    public string? Manufacturer { get; init; }
    public IReadOnlyList<string>? CompatibleModels { get; init; }
    public int? DonorVehicleYear { get; init; }
    /// <summary>Vehicle type: "Bil", "MC", "Skoter", "ATV", "Husvagn", "Moped", "Traktor"</summary>
    public string? VehicleType { get; init; }
    public string? ConditionGrade { get; init; }
}

public sealed class ElectronicsDomain : DomainObject
{
    public override string Domain => "electronics";
    public string? Brand { get; init; }
    public string? Model { get; init; }
    public int? StorageGb { get; init; }
    public string? Condition { get; init; }
}

public sealed class FashionDomain : DomainObject
{
    public override string Domain => "fashion";
    public string? Brand { get; init; }
    public string? Size { get; init; }
    public string? Color { get; init; }
    public string? Condition { get; init; }
}
