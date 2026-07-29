namespace BudgetManagement.Models
{
    /// <summary>
    /// (Udemy): 152. Reporte Diario - Query
    /// </summary>
    public class GetUserTransactionRequest
    {
        public int user_id { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
    }
}
