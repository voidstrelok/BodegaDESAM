namespace BodegaDESAM.Services;

using Microsoft.EntityFrameworkCore;

public sealed class PageRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = PaginationDefaults.DefaultPageSize;
    public string? Search { get; set; }

    public void Normalize()
    {
        Page = Math.Max(1, Page);
        PageSize = PaginationDefaults.AllowedPageSizes.Contains(PageSize)
            ? PageSize
            : PaginationDefaults.DefaultPageSize;
        Search = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim();
    }
}

public sealed class PagedResult<T>
{
    public PagedResult(IReadOnlyList<T> items, int totalItems, int page, int pageSize)
    {
        Items = items;
        TotalItems = totalItems;
        PageSize = pageSize;
        TotalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        Page = Math.Min(Math.Max(1, page), TotalPages);
    }

    public IReadOnlyList<T> Items { get; }
    public int TotalItems { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
}

public static class PaginationDefaults
{
    public const int DefaultPageSize = 25;
    public static readonly int[] AllowedPageSizes = [25, 50, 100];
}

public static class PaginationExtensions
{
    public static async Task<PagedResult<T>> ToPagedAsync<T>(
        this IQueryable<T> query,
        PageRequest request,
        CancellationToken cancellationToken = default)
    {
        request.Normalize();
        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)request.PageSize));
        request.Page = Math.Min(request.Page, totalPages);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalItems, request.Page, request.PageSize);
    }
}
