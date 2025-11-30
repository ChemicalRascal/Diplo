namespace BSBackupSystem.Model.App;

public class AppRole : AppRole<Guid>
{
    public AppRole()
    {
        Id = Guid.NewGuid();
    }

    public AppRole(string roleName) : this()
    {
        Name = roleName;
    }
}

public class AppRole<TKey> where TKey : IEquatable<TKey>
{
    public AppRole() { }

    public AppRole(string roleName) : this()
    {
        Name = roleName;
    }

    public virtual TKey Id { get; set; } = default!;

    public virtual string? Name { get; set; }

    public virtual string? NormalizedName { get; set; }

    public virtual string? ConcurrencyStamp { get; set; }

    public override string ToString() => Name ?? string.Empty;
}
