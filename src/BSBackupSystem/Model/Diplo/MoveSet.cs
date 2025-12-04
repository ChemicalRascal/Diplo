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
    public int PreRetreatHash { get; set; } = default!;
    public int FullHash { get; set; } = default!;
}