namespace SoftDemat.Application.Common;

public static class PageRequest
{
    public const int DefaultSize = 20;
    public const int MaxSize = 100;

    public static (int Page, int Size, int Skip) Normalize(int page, int size)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedSize = size < 1 ? DefaultSize : Math.Min(size, MaxSize);
        return (normalizedPage, normalizedSize, (normalizedPage - 1) * normalizedSize);
    }

    public static int PageCount(int totalCount, int size)
        => size == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)size);

    public static bool IsDescending(string? sortDirection)
        => string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
}
