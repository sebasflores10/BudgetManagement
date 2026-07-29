namespace BudgetManagement.Models
{
    /// <summary>
    /// (Udemy): 149. Movimientos de Cuentas
    /// Vista para mostrar la data al usuario en un Dashboard 
    /// (Views/BudgetAccount/Dashboard.cshtml)
    /// </summary>
    public class TransactionsDashboard
    {
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public IEnumerable<TransactionsByDate> transactions_by_date { get; set; }
        public decimal deposits_balance => transactions_by_date.Sum(x => x.deposits);
        public decimal withdrawals_balance => transactions_by_date.Sum(x => x.withdrawals);
        public decimal total => deposits_balance - withdrawals_balance;
        public class TransactionsByDate
        {
            public DateTime transaction_date { get; set; }
            public IEnumerable<Transactions> user_transactions_list { get; set; }
            public decimal deposits =>
                user_transactions_list.Where(x => x.operation_type_id == OperationType.Ingreso)
                .Sum(x => x.amount);
            public decimal withdrawals =>
                user_transactions_list.Where(x => x.operation_type_id == OperationType.Egreso)
                .Sum(x => x.amount);
        }
    }
}
