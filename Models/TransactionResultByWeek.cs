namespace BudgetManagement.Models
{
    /// <summary>
    /// (Udemy): 156. Reporte Semanal - Query - Group By con DateDiff
    /// (Udemy): 157. Reporte Semanal - Algoritmo [Actualizado]
    /// </summary>
    public class TransactionResultByWeek
    {
        public int Semana { get; set; }
        public decimal Monto { get; set; }
        public OperationType operation_type_id { get; set; }
        public decimal Ingreso { get; set; }
        public decimal Egreso { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
    }
}
