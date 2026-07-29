namespace BudgetManagement.Models
{
    /// <summary>
    /// (Udemy): Actualizando Transacciones - Parte 1
    /// (Udemy): Devolviendo al Usuario al Lugar Donde se Encontraba [Actualizado]
    /// </summary>
    public class UpdateTransactionsViewModel : TransactionsCreateViewModel
    {
        public decimal previous_amount { get; set; }
        public int previous_account_id { get; set; }
        public string? returnURL { get; set; }
    }
}
