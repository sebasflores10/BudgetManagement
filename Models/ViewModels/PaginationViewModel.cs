namespace BudgetManagement.Models.ViewModels
{
    /// <summary>
    /// (Udemy): 184. Paginación - Back-End - Query [3:00 mins]
    /// </summary>
    public class PaginationViewModel
    {
        public int page { get; set; } = 1;
        private int records_by_page = 10;
        private readonly int max_records_by_page = 50;

        public int total_records
        {
            get
            {
                return records_by_page;
            }
            set
            {
                records_by_page = (value > max_records_by_page) ?
                    max_records_by_page : value;
            }
        }

        public int jumping_records => total_records * (page - 1);
    }
}
