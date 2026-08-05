namespace BudgetManagement.Models
{
    /// <summary>
    /// (Udemy): Reporte Semanal - Algoritmo
    /// Llamado por el método 'Weekly' en "Controllers/TransactionsController.cs"
    /// </summary>
    public class WeeklyReportViewModel
    {
        public decimal Ingreso => transactionResultByWeek.Sum(x => x.Ingreso);
        public decimal Egreso => transactionResultByWeek.Sum(x => x.Egreso);
        public decimal Total => Ingreso - Egreso;
        public DateTime dateReference { get; set; }
        public IEnumerable<TransactionResultByWeek>? transactionResultByWeek { get; set; }
    }
}
