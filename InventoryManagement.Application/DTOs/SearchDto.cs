namespace InventoryManagement.Application.DTOs
{
    public class SearchDto
    {
        public string SearchTerm { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;



        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string SortBy { get; set; } = "name";
        public bool SortDescending { get; set; } = false;




    }
}