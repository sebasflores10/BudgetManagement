namespace BudgetManagement.Models
{
    /// <summary>
    /// (Udemy): 149. Movimientos de Cuentas
    /// </summary>
    public class GetTransactionsByBudgetAccount
    {
        public int user_id { get; set; }
        public int account_id { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
    }
}
