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
[JsonDerivedType(typeof(AutomotivePartDomain), "automotive_part")]
[JsonDerivedType(typeof(ElectronicsDomain), "electronics")]
[JsonDerivedType(typeof(FashionDomain), "fashion")]
public abstract class DomainObject
{
    public abstract string Domain { get; }
}

public sealed class AutomotivePartDomain : DomainObject
{
    public override string Domain => "automotive_part";
    public string? OemCode { get; init; }
    public string? Manufacturer { get; init; }
    public IReadOnlyList<string>? CompatibleModels { get; init; }
    public int? DonorVehicleYear { get; init; }
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
