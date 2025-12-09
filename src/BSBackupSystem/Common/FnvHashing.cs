using Analyzer.Utilities;

namespace BSBackupSystem.Common;

public interface IFnvHashable
{
    public IEnumerable<byte> GetBytestream();
}

/// <summary>
/// Implements the Fowler-Noll-Vo hashing algorithm.
/// </summary>
public static class FnvHashing
{
    private static readonly uint offsetBasis = 2166136261;
    private static readonly uint prime = 16777619;

    public static uint Fnv1(IFnvHashable hashable)
    {
        var hash = offsetBasis;

        foreach (var b in hashable.GetBytestream())
        {
            unchecked
            {
                hash *= prime;
                hash ^= b;
            }
        }

        return hash;
    }

    public static uint Fnv1a(IFnvHashable hashable)
    {
        var hash = offsetBasis;

        foreach (var b in hashable.GetBytestream())
        {
            unchecked
            {
                hash ^= b;
                hash *= prime;
            }
        }

        return hash;
    }

    extension(IFnvHashable hashable)
    {
        public uint GetFnv1() => Fnv1(hashable);
        public uint GetFnv1a() => Fnv1a(hashable);
    }

}
