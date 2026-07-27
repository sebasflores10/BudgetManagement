namespace BudgetManagement.Models
{
    /// <summary>
    /// (Udemy): Actualizando Transacciones - Parte 1
    /// </summary>
    public class UpdateTransactionsViewModel : TransactionsCreateViewModel
    {
        public decimal previous_amount { get; set; }
        public int previous_account_id { get; set; }
    }
}
