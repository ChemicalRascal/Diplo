using BSBackupSystem.Common;
using System.Text;

namespace BSBackupSystem.Model.Diplo;

public abstract class UnitOrder : IFnvHashable
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
    public string CanonicalString => $"{Player} -- {ToString()}";

    public virtual IEnumerable<string?> FnvStrings(params string?[] extraFields)
        => [this.GetType().Name, Player, UnitType, Unit, UnitCoast,
            Result, ResultReason, ..extraFields];

    IEnumerable<byte> IFnvHashable.GetBytestream()
    {
        foreach (var fnvField in FnvStrings())
        {
            foreach (var b in Encoding.UTF8.GetBytes(fnvField ?? string.Empty))
            { yield return b; }
            yield return 1;
        }
        yield break;
    }
}

public class HoldOrder : UnitOrder
{
    public override string OrderString => $"{UnitDescription} Holds";
}

public class MoveOrder : UnitOrder
{
    public string To { get; set; } = default!;
    public string? ToCoast { get; set; }
    public bool? ViaConvoy { get; set; } = null; // For later development, not currently persisted.
    public override string OrderString => $"{UnitDescription} To {To}";

    public override IEnumerable<string?> FnvStrings(params string?[] extraFields)
        => base.FnvStrings([To, ToCoast, ViaConvoy?.ToString(), ..extraFields]);
}

public class SupportHoldOrder : UnitOrder
{
    public string Supporting { get; set; } = default!;
    public override string OrderString => $"{UnitDescription} Supports {Supporting} Hold";

    public override IEnumerable<string?> FnvStrings(params string?[] extraFields)
        => base.FnvStrings([Supporting, ..extraFields]);
}

public class SupportMoveOrder : UnitOrder
{
    public string SupportingFrom { get; set; } = default!;
    public string SupportingTo { get; set; } = default!;
    public override string OrderString => $"{UnitDescription} Supports {SupportingFrom} To {SupportingTo}";

    public override IEnumerable<string?> FnvStrings(params string?[] extraFields)
        => base.FnvStrings([SupportingFrom, SupportingTo, ..extraFields]);
}

public class ConvoyOrder : UnitOrder
{
    public string ConvoyFrom { get; set; } = default!;
    public string ConvoyTo { get; set; } = default!;
    public override string OrderString => $"{UnitDescription} Convoys {ConvoyFrom} To {ConvoyTo}";

    public override IEnumerable<string?> FnvStrings(params string?[] extraFields)
        => base.FnvStrings([ConvoyFrom, ConvoyTo, ..extraFields]);
}

public abstract class RetreatOrder : UnitOrder
{
}

public class RetreatMoveOrder : UnitOrder
{
    public string To { get; set; } = default!;
    public string? ToCoast { get; set; }
    public override string OrderString => $"{UnitDescription} Retreats To {To}";

    public override IEnumerable<string?> FnvStrings(params string?[] extraFields)
        => base.FnvStrings([To, ToCoast, ..extraFields]);
}

public class RetreatDisbandOrder : UnitOrder
{
    public override string OrderString => $"{UnitDescription} Disbands";
}

public class BuildOrder : UnitOrder
{
    public override string OrderString => $"Builds {UnitDescription}";
}

public class DisbandOrder : UnitOrder
{
    public override string OrderString => $"Disbands {UnitDescription}";
}

public static partial class Extensions
{
    extension(UnitOrder order)
    {
        public bool IsRetreat =>
            order.GetType().IsAssignableTo(typeof(RetreatOrder));
    }

    extension(IEnumerable<UnitOrder> orders)
    {
        public uint GetOrderSetHash() => 
            orders.Aggregate(0u, (runningHash, order) =>
                runningHash ^= order.GetFnv1());

        public IEnumerable<UnitOrder> NoRetreats() =>
            orders.Where(o => !o.IsRetreat);

        public IEnumerable<UnitOrder> OnlyRetreats() =>
            orders.Where(o => o.IsRetreat);
    }
}