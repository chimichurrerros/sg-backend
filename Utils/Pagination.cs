using System.ComponentModel.DataAnnotations;

namespace BackEnd.Utils
{
    public class Pagination(int currentPage, int pageSize, int totalElements)
    {
        [Range(1, int.MaxValue, ErrorMessage = "PageSize must be greater than 0.")]
        public int CurrentPage { get; } = currentPage;
        [Range(1, int.MaxValue, ErrorMessage = "PageSize must be greater than 0.")]
        public int PageSize { get; } = pageSize;
        public int TotalElements { get; } = totalElements;
        public int TotalPages { get; } = pageSize > 0 ? (int)Math.Ceiling(totalElements / (double)pageSize) : 0;
    }
}