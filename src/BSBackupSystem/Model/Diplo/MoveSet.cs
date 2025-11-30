namespace BSBackupSystem.Model.Diplo;

public class MoveSet
{
    public Guid Id { get; set; } = default;
    public int Year { get; set; }
    public int SeasonIndex { get; set; }
    public MoveSet? PreviousSet { get; set; }
    public List<UnitOrder> Orders { get; set; } = [];
    public string State { get; set; } = default!;
    public DateTimeOffset? FirstSeen { get; set; }
    public DateTimeOffset? LastSeen { get; set; }
    public string PreRetreatHash { get; set; } = default!;
    public string FullHash { get; set; } = default!;
}