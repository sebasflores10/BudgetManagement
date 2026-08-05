namespace BudgetManagement.Models
{
    /// <summary>
    /// (Udemy): 159. Reporte Mensual - Query
    /// </summary>
    public class MonthlyReportViewModel
    {
        public IEnumerable<MonthlyResultSQL> transactionsByMonth { get; set; }
        public decimal Ingreso => transactionsByMonth.Sum(x => x.Ingreso);
        public decimal Egreso => transactionsByMonth.Sum(x => x.Egreso);
        public decimal Total => Ingreso - Egreso;
        public int year { get; set; }
    }
}
