



namespace InventoryManagement.Domain.Common
{
    public class PagedResult<T>
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<T> Items { get; set; }
        public bool HasNextPage => (PageNumber * PageSize) < TotalCount;
        public bool HasPreviousPage => PageNumber > 1;
    }
}