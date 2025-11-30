namespace BSBackupSystem.Model.Diplo;

public abstract class UnitOrder
{
    public Guid Id { get; set; } = default;
    public string Player { get; set; } = default!;
    public string UnitType { get; set; } = default!;
    public string Unit { get; set; } = default!;
    public string? UnitCoast { get; set; }

    public string Result { get; set; } = default!;
    public string ResultReason { get; set; } = default!;

    protected string UnitDescription => $"{UnitType} {Unit}{(UnitCoast is not null ? $" {UnitCoast}" : string.Empty)}";
    protected string ResultString => $"{(Result != "SUCCEEDS" ? $" -- {Result}, {ResultReason}" : "")}";
    public abstract string OrderString { get; }

    public override string ToString() => $"{OrderString}{ResultString}";
}

public class HoldOrder : UnitOrder
{
    public override string OrderString => $"{UnitDescription} Holds";
}

public class MoveOrder : UnitOrder
{
    public string To { get; set; } = default!;
    public override string OrderString => $"{UnitDescription} To {To}";
}

public class SupportHoldOrder : UnitOrder
{
    public string Supporting { get; set; } = default!;
    public override string OrderString => $"{UnitDescription} Supports {Supporting} Hold";
}

public class SupportMoveOrder : UnitOrder
{
    public string SupportingFrom { get; set; } = default!;
    public string SupportingTo { get; set; } = default!;
    public override string OrderString => $"{UnitDescription} Supports {SupportingFrom} To {SupportingTo}";
}

public class ConvoyOrder : UnitOrder
{
    public string ConvoyFrom { get; set; } = default!;
    public string ConvoyTo { get; set; } = default!;
    public override string OrderString => $"{UnitDescription} Convoys {ConvoyFrom} To {ConvoyTo}";
}

public class RetreatOrder : UnitOrder
{
    public string To { get; set; } = default!;
    public override string OrderString => $"{UnitDescription} Retreats To {To}";
}

public class BuildOrder : UnitOrder
{
    public override string OrderString => $"Builds {UnitDescription}";
}

public class DisbandOrder : UnitOrder
{
    public override string OrderString => $"Disbands {UnitDescription}";
}