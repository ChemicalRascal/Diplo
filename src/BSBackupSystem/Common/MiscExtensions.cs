namespace BSBackupSystem.Common;

public static class MiscExtensions
{
    extension(string str)
    {
        public string? ToNullIfEmpty() => str.Length == 0 ? null : str;

        public string? ToNullIfEmptyOrWhitespace() => str.Trim().ToNullIfEmpty();
    }
}
