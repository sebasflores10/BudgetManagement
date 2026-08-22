namespace BudgetManagement.Models.ViewModels
{
    /// <summary>
    /// (Udemy): 185.Paginación - Contando los Registros [3:30 mins]
    /// </summary>
    public class PaginationResponseViewModel
    {
        public int page { get; set; } = 1;
        public int records_by_page { get; set; } = 10;
        public int total_records { get; set; }
        public int total_amount_pages => (int)Math.Ceiling((double)total_records / records_by_page);
        public string url { get; set; }
    }

    // Creamos una clase genérica para que pueda ser usado en la vista "Views/Shared/_Pagination.cshtml"
    public class PaginationResponseViewModel<T> : PaginationResponseViewModel
    {
        public IEnumerable<T> elements { get; set; }
    }
}
