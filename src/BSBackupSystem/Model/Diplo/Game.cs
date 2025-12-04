namespace BSBackupSystem.Model.Diplo;

public class Game
{
    public Guid Id { get; set; } = default;
    public string Uri { get; set; } = default!;
    public string ForeignId { get; set; } = default!;
    public DateTimeOffset CreationTime { get; set; } = default;
    public List<MoveSet> MoveSets { get; set; } = [];
}