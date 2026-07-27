namespace BudgetManagement.Models
{
    /// <summary>
    /// (Udemy): 127. Indice de Cuentas - Query [3:30 mins]
    /// </summary>
    public class BudgetAccountIndexViewModel
    {
        public string AccountType { get; set; }
        public IEnumerable<BudgetAccount> BudgetAccount { get; set; }
        public decimal account_balance => BudgetAccount.Sum(x => x.account_balance);
    }
}
