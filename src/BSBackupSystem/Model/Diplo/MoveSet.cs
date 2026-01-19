namespace BSBackupSystem.Model.Diplo;

public class MoveSet
{
    public Guid Id { get; set; } = default;
    public int Year { get; set; }
    public string SeasonName { get; set; } = default!;
    public int SeasonIndex { get; set; }
    public MoveSet? PreviousSet { get; set; }
    public List<UnitOrder> Orders { get; set; } = [];
    public string State { get; set; } = default!;
    public DateTimeOffset? FirstSeen { get; set; }
    public DateTimeOffset? LastSeen { get; set; }
    public uint PreRetreatHash { get; set; } = default!;
    public uint FullHash { get; set; } = default!;

    public bool Satisfied => State != "SATISFIED";
}