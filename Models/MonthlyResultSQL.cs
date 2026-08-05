namespace BudgetManagement.Models
{
    /// <summary>
    /// (Udemy): 159. Reporte Mensual - Query
    /// </summary>
    public class MonthlyResultSQL
    {
        public int Mes { get; set; }
        public DateTime dateReference { get; set; }
        public decimal Monto { get; set; }
        public decimal Ingreso { get; set; }
        public decimal Egreso { get; set; }
        public OperationType operation_type_id { get; set; }
    }
}
