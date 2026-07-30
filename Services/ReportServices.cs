using BudgetManagement.Models;
using System.Security.Principal;

namespace BudgetManagement.Services
{
    public interface IReportServices
    {
        Task<TransactionsDashboard> GetTransactionsReport(int user_id, int month, int year, dynamic ViewBag);
        Task<TransactionsDashboard> GetTransactionsReportByBudgetAccount(int user_id, int account_id, int month, int year, dynamic ViewBag);
    }


    /// <summary>
    /// (Udemy): 154. Refactorizando
    /// </summary>
    public class ReportServices : IReportServices
    {
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly HttpContext httpContext;

        public ReportServices(ITransactionsRepository transactionsRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            this._transactionsRepository = transactionsRepository;
            this.httpContext = httpContextAccessor.HttpContext;
        }


        /// <summary>
        /// Método 'GetTransactionsReportByBudgetAccount'
        /// Permite al usuario dirigir al reporte de sus cuentas a la vista 
        /// "Views/BudgetAccount/BudgetAccountDashboard.cshtml", donde pueda
        /// ver el calendario de transacciones de esa cuenta por día.
        /// (Udemy): 154. Refactorizando
        /// </summary>
        /// <param name="user_id">Parámetro que captura el ID del usuario</param>
        /// <param name="account_id">Parámetro que captura el ID de la cuenta</param>
        /// <param name="month">Parámetro que captura el mes del calendario</param>
        /// <param name="year">Parámetro que captura el año del calendario</param>
        /// <param name="ViewBag">Parámetro que captura el ViewBag/etiqueta del
        /// nombre de la cuenta, o cualquier atributo de la cuenta</param>
        /// <returns>Devuelve el modelo del calendario para las cuentas del usuario (BudgetAccount)</returns>
        public async Task<TransactionsDashboard> GetTransactionsReportByBudgetAccount
            (int user_id, int account_id, int month, int year, dynamic ViewBag)
        {
            (DateTime start_date, DateTime end_date) = DateGenerator(month, year);

            var transactionsByBudgetAccount = new GetTransactionsByBudgetAccount()
            {
                user_id = user_id,
                account_id = account_id,
                start_date = start_date,
                end_date = end_date
            };

            var transactions = await _transactionsRepository
                .GetTransactionsByBudgetAccount(transactionsByBudgetAccount);

            ViewBag.account_id = account_id;
            var model = GenerateTransactionsDashboard(start_date, end_date, transactions);
            PopulateViewBagNavigationDates(ViewBag, start_date);
            return model;
        }


        /// <summary>
        /// Método 'GetTransactionsReport'
        /// Permite generar el calendario del reporte de las transacciones del usuario que
        /// son mostradas en la vista "Views/Transactions/Index.cshtml"
        /// (Udemy): 154. Refactorizando
        /// </summary>
        /// <param name="user_id">Parámetro que captura el ID del usuario</param>
        /// <param name="month">Parámetro que captura el mes del calendario</param>
        /// <param name="year">Parámetro que captura el año del calendario</param>
        /// <param name="ViewBag">Parámetro que captura el ViewBag/etiqueta del
        /// nombre de la transacción, o cualquier atributo de las transacciones del usuario</param>
        /// <returns>Devuelve el modelo del calendario para las transacciones del usuario (Transactions)</returns>
        public async Task<TransactionsDashboard> GetTransactionsReport
            (int user_id, int month, int year, dynamic ViewBag)
        {
            (DateTime start_date, DateTime end_date) = DateGenerator(month, year);

            var userTransactions = new GetUserTransactionRequest()
            {
                user_id = user_id,
                start_date = start_date,
                end_date = end_date
            };

            var transactions = await _transactionsRepository
                .GetTransactionsByUserID(userTransactions);

            var model = GenerateTransactionsDashboard(start_date, end_date, transactions);

            PopulateViewBagNavigationDates(ViewBag, start_date);

            return model;
        }




        ////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// Método 'DateGenerator'
        /// Permite generar dinámicamente las fechas para los calendarios de transacciones
        /// y cuentas del usuario.
        /// (Udemy): 154. Refactorizando
        /// </summary>
        /// <param name="month">Parámetro que captura el mes del calendario</param>
        /// <param name="year">Parámetro que captura el año del calendario</param>
        /// <returns></returns>
        private (DateTime start_datee, DateTime end_date) DateGenerator(int month, int year)
        {
            DateTime start_date;
            DateTime end_date;

            if (month <= 0 || month > 12 || year <= 1900)
            {
                var today = DateTime.Today;
                start_date = new DateTime(today.Year, today.Month, 1);
            }
            else
            {
                start_date = new DateTime(year, month, 1);
            }

            end_date = start_date.AddMonths(1).AddDays(-1);

            return (start_date, end_date);
        }


        /// <summary>
        /// Método 'GenerateTransactionsDashboard'
        /// Permite generar el model adecuado para las transacciones del usuario por día,
        /// mes y año para la vista "Views/Transactions/Index.cshtml"
        /// </summary>
        /// <param name="start_date">Parámetro que captura la fecha de inicio</param>
        /// <param name="end_date">Parámetro que captura la fecha final</param>
        /// <param name="transactions">Parámetro que captura la colección de transacciones
        /// del usuario</param>
        private static TransactionsDashboard GenerateTransactionsDashboard
            (DateTime start_date, DateTime end_date, IEnumerable<Transactions> transactions)
        {
            var model = new TransactionsDashboard();

            var transactionsByDate = transactions.OrderByDescending(x => x.transaction_date)
                .GroupBy(x => x.transaction_date)
                .Select(group => new TransactionsDashboard.TransactionsByDate()
                {
                    transaction_date = group.Key,
                    user_transactions_list = group.AsEnumerable()
                });

            model.transactions_by_date = transactionsByDate;
            model.start_date = start_date;
            model.end_date = end_date;
            return model;
        }


        /// <summary>
        /// Método 'PopulateViewBagNavigationDates'
        /// Permite llenar los campos de los ViewBag/etiquetas de los atributos, tanto como
        /// para las transacciones como para las cuentas del usuario.
        /// (Udemy): 154. Refactorizando
        /// </summary>
        /// <param name="ViewBag">Parámetro que captura los ViewBag/etiquetas de los
        /// atributos de transacciones del usuario, o cuentas.</param>
        /// <param name="start_date">Parámetro que captura la fecha de inicio</param>
        private void PopulateViewBagNavigationDates(dynamic ViewBag, DateTime start_date)
        {
            // (Udemy): 150. Vista de Movimientos de Cuentas [4:15 mins]
            ViewBag.previous_month = start_date.AddMonths(-1).Month;
            ViewBag.previous_year = start_date.AddMonths(-1).Year;
            ViewBag.month_later = start_date.AddMonths(1).Month;
            ViewBag.year_later = start_date.AddMonths(1).Year;
            // (Udemy): 151. Devolviendo al Usuario al Lugar Donde se Encontraba [4:15 mins]
            // Sustraemos la URL para luego de de que el usuario modifique o elimine
            // alguna transacción, se vuelva a dirigir a la vista
            // "Views/BudgetAccount/BudgetAccountDashboard.cshtml"
            ViewBag.returnURL = httpContext.Request.Path + httpContext.Request.QueryString;
        }
    }
}
