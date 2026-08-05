using AutoMapper;
using BudgetManagement.Models;
using BudgetManagement.Services;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Globalization;
using System.Security.Principal;
using System.Transactions;

namespace BudgetManagement.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly IUserService _userService;
        private readonly IBudgetAccountRepository _budgetAccountRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IReportServices _reportServices;

        public TransactionsController(ITransactionsRepository transactionsRepository,
            IUserService userService,
            IBudgetAccountRepository budgetAccountRepository,
            ICategoryRepository categoryRepository,
            IMapper mapper,
            IReportServices reportServices)
        {
            this._transactionsRepository = transactionsRepository;
            this._userService = userService;
            this._budgetAccountRepository = budgetAccountRepository;
            this._categoryRepository = categoryRepository;
            this._mapper = mapper;
            this._reportServices = reportServices;
        }


        /// <summary>
        /// Método 'Index'
        /// Permite al usuario poder visualizar, editar o eliminar sus transaccions de la
        /// vista "Views/Transactions/Index.cshtml".
        /// (Udemy): 153. Vista del Reporte Diario
        /// (Udemy): 154. Refactorizando [Modificado] - Código se cambió a "Services/ReportServices.cs"
        /// </summary>
        /// <param name="month">Parámetro que captura el mes del calendario</param>
        /// <param name="year">Parámetro que captura el año del calendario</param>
        /// <returns>Dirige al usuario a la vista principal de transacciones
        /// "Views/Transactions/Index.cshtml".</returns>
        public async Task<IActionResult> Index(int month, int year)
        {
            var user_id = _userService.GetUserID();
            var model = await _reportServices
                .GetTransactionsReport(user_id, month, year, ViewBag);
            return View(model);
        }


        /// <summary>
        /// Método 'CreateTransaction'
        /// Permite mostrar al usuario la vista de crear transacciones
        /// "Views/Transactions/CreateTransaction.cshtml"
        /// (Udemy): 138. Creando Transacciones
        /// </summary>
        /// <returns>Dirige al usuario a la vista "Views/Transactions/CreateTransaction.cshtml"</returns>
        [HttpGet]
        public async Task<IActionResult> CreateTransaction()
        {
            var user_id = _userService.GetUserID();
            var model = new TransactionsCreateViewModel();
            // Obtenemos las cuentas del usuario para mostrarlos en un <select>
            model.Cuentas = await GetBudgetAccountsToTransactions(user_id);
            // Obtenemos las categorías del usuario para mostrarlos en un <select>
            // (Udemy): 141. DropDown Cascada
            model.Categorias = await CategoriesSelectListItem
                (user_id, model.operation_type_id);
            return View(model);
        }


        /// <summary>
        /// Método 'CreateTransaction'
        /// Permite al usuario la acción de poder crear una transacción desde 
        /// "Views/Transactions/CreateTransaction.cshtml"
        /// (Udemy): Insertando la Transacción
        /// </summary>
        /// <param name="model">Parámetro que captura el objeto 
        /// "Models/TransactionsCreateViewModel.cs"</param>
        /// <returns>Dirige al usuario a la vista "Views/Transactions/Index.cshtml"
        /// al instertar existosamente la transacción</returns>
        [HttpPost]
        public async Task<IActionResult> CreateTransaction
            (TransactionsCreateViewModel model)
        {
            var user_id = _userService.GetUserID();

            if (!ModelState.IsValid)
            {
                model.Cuentas = await GetBudgetAccountsToTransactions(user_id);
                model.Categorias = await CategoriesSelectListItem
                    (user_id, model.operation_type_id);
                return View(model);
            }

            var account = await _budgetAccountRepository
                .GetBudgetAccountByID(model.account_id, user_id);

            if (account is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            var category = _categoryRepository.GetCategoryByID(model.category_id, user_id);

            if(category is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            model.user_id = user_id;

            // Si es un Egreso, lo convertimos en negativo
            // TODO: Validar el campo para que el valor no sea negativo
            if(model.operation_type_id == OperationType.Egreso)
            {
                model.amount *= -1;
            }

            await _transactionsRepository.CreateTransaction(model);

            return RedirectToAction("Index");
        }


        /// <summary>
        /// Método 'UpdateTransaction'
        /// Permite al usuario mostrar la vista para editar transacciones, la cual es
        /// "Views/Transactions/UpdateTransaction.cshtml"
        /// (Udemy): Actualizando Transacciones - Parte 2
        /// (Udemy): Devolviendo al Usuario al Lugar Donde se Encontraba [Actualizado]
        /// </summary>
        /// <param name="transaction_id">Parámetro que captura el ID de la transacción</param>
        /// <returns>Dirige al usuario a la vista "Views/Transactions/UpdateTransaction.cshtml"</returns>
        [HttpGet]
        public async Task<IActionResult> UpdateTransaction(int transaction_id,
            string returnURL = null)
        {
            var user_id = _userService.GetUserID();
            var transaction = await _transactionsRepository
                .GetTransactionByID(transaction_id, user_id);

            if(transaction is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            var model = _mapper.Map<UpdateTransactionsViewModel>(transaction);

            model.previous_amount = model.amount;

            if(model.operation_type_id == OperationType.Egreso)
            {
                model.previous_amount = model.amount * -1;
            }

            model.previous_account_id = transaction.account_id;

            model.Categorias = await CategoriesSelectListItem(user_id, 
                transaction.operation_type_id);

            model.Cuentas = await GetBudgetAccountsToTransactions(user_id);
            model.returnURL = returnURL;

            return View(model);
        }


        /// <summary>
        /// Método 'UpdateTransaction'
        /// Permite al usuario realizar la acción de editar una transacción.
        /// (Udemy): Actualizando Transacciones - Parte 2
        /// (Udemy): Devolviendo al Usuario al Lugar Donde se Encontraba [Actualizado - 3:00 mins]
        /// </summary>
        /// <param name="model">Parámetro que captura el objeto 
        /// "Models/UpdateTransactionsViewModel.cs", la cual hereda de
        /// "Models/TransactionsCreateViewModel.cs", que a su vez hereda de
        /// "Models/Transactions.cs"</param>
        /// <returns>Dirige al usuario a la vista "Views/Transactions/Index.cshtml"
        /// al editar exitosamente una transacción</returns>
        [HttpPost]
        public async Task<IActionResult> UpdateTransaction
            (UpdateTransactionsViewModel model)
        {
            var user_id = _userService.GetUserID();

            if (!ModelState.IsValid)
            {
                model.Categorias = await CategoriesSelectListItem(user_id,
                    model.operation_type_id);
                model.Cuentas = await GetBudgetAccountsToTransactions(user_id);
                return View(model);
            }

            var account = await _budgetAccountRepository
                .GetBudgetAccountByID(model.account_id, user_id);

            if(account is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            var category = await _categoryRepository
                .GetCategoryByID(model.category_id, user_id);

            if(category is null)
            {
                return RedirectToAction("NotFoun", "Home");
            }


            var transaction = _mapper.Map<Transactions>(model);
            transaction.user_id = user_id;
            if (model.operation_type_id == OperationType.Egreso)
            {
                transaction.amount *= -1;
            }

            await _transactionsRepository
                .UpdateTransaction(transaction, model.previous_amount, 
                model.previous_account_id);

            if (string.IsNullOrEmpty(model.returnURL))
            {
                // Dirigimos al usuario a una URL que tenemos en el dominio del controller
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(model.returnURL);
            }
        }


        /// <summary>
        /// Método 'DeleteTransaction'
        /// Permite al usuario la acción de eliminar la transacción por su ID desde la 
        /// vista "Views/Transactions/DeleteTransaction.cshtml".
        /// (Udemy): Borrar Transacciones - Un Formulario Con Dos Acciones
        /// (Udemy): 151. Devolviendo al Usuario al Lugar Donde se Encontraba [Actualizado - 6:07 mins]
        /// </summary>
        /// <param name="transaction_id">Parámetro que captura el ID de la transacción</param>
        /// <returns>Dirige al usuario a la vista "Views/Transactions/Index.cshtml"
        /// luego de borrar la transacción</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteTransaction(int transaction_id,
            string returnURL = null)
        {
            var user_id = _userService.GetUserID();

            var transaction = await _transactionsRepository
                .GetTransactionByID(transaction_id, user_id);

            if (transaction is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            await _transactionsRepository.DeleteTransaction(transaction_id);

            if (string.IsNullOrEmpty(returnURL))
            {
                // Dirigimos al usuario a una URL que tenemos en el dominio del controller
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(returnURL);
            }
        }


        ////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////



        /// <summary>
        /// Método 'GetBudgetAccountsToTransactions'
        /// Permite saber todas las cuentas [BudgetAccount] que el usuario posee.
        /// (Udemy): 138. Creando Transacciones
        /// </summary>
        /// <param name="user_id">Parámetro que captura el ID del usuario</param>
        private async Task<IEnumerable<SelectListItem>> GetBudgetAccountsToTransactions
            (int user_id)
        {
            var accounts = await _budgetAccountRepository.GetAllUserBudgetAccounts(user_id);
            return accounts.Select(x => new SelectListItem(x.account_name, 
                x.account_id.ToString()));
        }



        /// <summary>
        /// Método 'CategoriesSelectListItem'
        /// Permite listar todas las categorías del usuario basados en su tipo de 
        /// operación [1 = Ingreso | 2 = Egreso] en el "<select></select>" de las vistas
        /// "Views/Transactions/CreateTransaction.cshtml" y "Views/Transactions/EditTransaction.cshtml"
        /// (Udemy): 141. DropDown Cascada
        /// </summary>
        /// <param name="user_id">Parámetro que captura el ID del usuario</param>
        /// <param name="operation_type_id">Parámetro que captura el ID del tipo de operación</param>
        private async Task<IEnumerable<SelectListItem>> CategoriesSelectListItem
            (int user_id, OperationType operation_type_id)
        {
            var categories = await _categoryRepository
                .GetUserOperationTypeTransaction(user_id, operation_type_id);

            return categories.Select(x => 
                new SelectListItem(x.category_name, x.category_id.ToString()));
        }


        /// <summary>
        /// Método 'GetCategoriesList'
        /// Permite devolver una respuesta exitosa al cuerpo de las vistas
        /// "Views/Transactions/CreateTransaction.cshtml" y "Views/Transactions/EditTransaction.cshtml"
        /// a la hora de obtener las categorías basdaos en su tipo de operación que fueron
        /// validados en el método 'GetCategoriesList'.
        /// (Udemy): 141. DropDown Cascada
        /// </summary>
        /// <param name="operation_type_id">Parámetro que captura el ID del tipo de operación</param>
        [HttpPost]
        public async Task<IActionResult> GetCategoriesList
            ([FromBody]OperationType operation_type_id)
        {
            var user_id = _userService.GetUserID();
            var categories = await CategoriesSelectListItem(user_id, operation_type_id);
            return Ok(categories);
        }


        /// <summary>
        /// Método 'Weekly'
        /// Permite al usuario pder visualizar en la vista "Views/Transactions/Weekly.cshtml"
        /// las transacciones que realizó por semana.
        /// (Udemy): 156. Reporte Semanal - Query - Group By con DateDiff
        /// (Udemy): 157. Reporte Semanal - Algoritmo
        /// </summary>
        /// <param name="month">Parámetro que captura el mes</param>
        /// <param name="year">Parámetro que captura el año</param>
        /// <returns>Muestra al usuario la vista "Views/Transactions/Weekly.cshtml"
        /// con las transacciones realizadas por semana.</returns>
        public async Task<IActionResult> Weekly(int month, int year)
        {
            var user_id = _userService.GetUserID();

            // Especificamos que vamos a obtener una colección de 
            // "Models/TransactionResultByWeek.cs" en vez de usar "var". Si definimos como
            // variable, el conflicto o bug que puede ocasionar el ViewBag como "dynamic",
            // va a afectar el visualizar las transacciones por semana del usuario.
            IEnumerable<TransactionResultByWeek> transactionsByWeek = await _reportServices
                .GetTransactionResultByWeek(user_id, month, year, ViewBag);

            // (Udemy): 157. Reporte Semanal - Algoritmo [1:50 mins]
            var grouped = transactionsByWeek.GroupBy(x => x.Semana)
                .Select(x => new TransactionResultByWeek()
                {
                    Semana = x.Key,
                    Ingreso = x.Where(x => x.operation_type_id == OperationType.Ingreso)
                        .Select(x => x.Monto).FirstOrDefault(),
                    Egreso = x.Where(x => x.operation_type_id == OperationType.Egreso)
                        .Select(x => x.Monto).FirstOrDefault()
                }).ToList();

            if(year == 0 || month == 0)
            {
                var today = DateTime.Now;
                year = today.Year;
                month = today.Month;
            }

            // Al condicionar, definimos el dia actual, mes  y año
            var dateReference = new DateTime(year, month, 1);
            // Generamos un arreglo para validar años bisiestos o mes de febrero 
            var monthDays = Enumerable.Range(1, dateReference.AddMonths(1).AddDays(-1).Day);

            // Luego, separamos los días de 7 en 7 para saber la semana en la que estamos
            var separatingDaysOfMonth = monthDays.Chunk(7).ToList();

            for (int i = 0; i < separatingDaysOfMonth.Count(); i++)
            {
                var week = i + 1;
                var start_date = new DateTime(year, month, separatingDaysOfMonth[i].First());
                var end_date = new DateTime(year, month, separatingDaysOfMonth[i].Last());
                var weekGroup = grouped.FirstOrDefault(x => x.Semana == week);

                if (weekGroup is null)
                {
                    grouped.Add(new TransactionResultByWeek()
                    {
                        Semana = week,
                        start_date = start_date,
                        end_date = end_date
                    });
                }
                else
                {
                    weekGroup.start_date = start_date;
                    weekGroup.end_date = end_date;
                }
            }

            grouped = grouped.OrderByDescending(x => x.Semana).ToList();

            var model = new WeeklyReportViewModel();
            model.transactionResultByWeek = grouped;
            model.dateReference = dateReference;

            return View(model);
        }


        /// <summary>
        /// Método 'Monthly'
        /// Permite mostrar al usuario las transacciones realizadas por mes en la vista
        /// "Views/Transactions/Monthly.cs".
        /// (Udemy): 159. Reporte Mensual - Query
        /// </summary>
        /// <param name="year">Parámetro que captura el año</param>
        /// <returns>Muestra el reporte de las transacciones realizadas del usuario en la 
        /// vista "Views/Transactions/Monthly.cs"</returns>
        public async Task<IActionResult> Monthly(int year)
        {
            var user_id = _userService.GetUserID();
            if (year == 0) 
            {
                year = DateTime.Today.Year;
            }

            var transactionsByMonth = await _transactionsRepository
                .MonthlyTransactionsResult(user_id, year);

            var groupedTransactions = transactionsByMonth.GroupBy(x => x.Mes)
                .Select(x => new MonthlyResultSQL()
                {
                    Mes = x.Key,
                    Ingreso = x.Where(x => x.operation_type_id == OperationType.Ingreso)
                        .Select(x => x.Monto).FirstOrDefault(),
                    Egreso = x.Where(x => x.operation_type_id == OperationType.Egreso)
                        .Select(x => x.Monto).FirstOrDefault()
                }).ToList();

            for(int month = 1; month <= 12; month++)
            {
                var transaction = groupedTransactions.FirstOrDefault(x => x.Mes == month);
                var dateReference = new DateTime(year, month, 1);

                if(transaction is null)
                {
                    groupedTransactions.Add(new MonthlyResultSQL()
                    {
                        Mes = month,
                        dateReference = dateReference
                    });
                }
                else
                {
                    transaction.dateReference = dateReference;
                }
            }

            groupedTransactions = groupedTransactions.OrderBy(x => x.Mes).ToList();

            // Luego tenemos que mostrar la info al usuario, por lo que creamos el modelo
            // "Models/MonthlyReportViewModel.cs" para mostrar los datos a la vista

            var model = new MonthlyReportViewModel();
            model.year = year;
            model.transactionsByMonth = groupedTransactions;

            return View(model);
        }



        public async Task<IActionResult> ExcelReport()
        {
            return View();
        }


        /// <summary>
        /// Método 'ExportingExcelByMonth'
        /// Permite al usuario exportar sus transacciones realizadas por mes 
        /// en un archivo de Excel.
        /// (Udemy): 161. Exportar Excel - Por Mes
        /// </summary>
        /// <param name="month">Parámetro que captura el mes</param>
        /// <param name="year">Parámetro que captura el año</param>
        [HttpGet]
        public async Task<FileResult> ExportingExcelByMonth(int month, int year) 
        {
            var start_date = new DateTime(year, month, 1);
            var end_date = start_date.AddMonths(1).AddDays(-1);
            var user_id = _userService.GetUserID();

            IEnumerable<Transactions> transactions = await ListingTransactionsByUserID
                (start_date, end_date, user_id);

            var culture = new CultureInfo("es-ES");
            string monthFormat = start_date.ToString("MMMM", culture);

            monthFormat = culture.TextInfo.ToTitleCase(monthFormat);

            var fileName = $"ReporteTransaccionesMensuales_{monthFormat}_{year}.xlsx";

            return GenerateExcel(fileName, transactions);
        }


        /// <summary>
        /// Método 'GenerateExcel'
        /// Permite generar el archivo Excel para cuando sea llamado por el usuario en 
        /// la vista "Views/Transactions/ExcelReport.cshtml".
        /// (Udemy): 161. Exportar Excel - Por Mes
        /// </summary>
        /// <param name="filename">Parámetro para poder generar el nombre del archivo Excel</param>
        /// <param name="transactions">Parámetro que captura en una colección el objeto
        /// "Models/Transactions.cs"</param>
        private FileResult GenerateExcel(string filename, 
            IEnumerable<Transactions> transactions)
        {
            DataTable dataTable = new DataTable("Transactions");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Fecha"),
                new DataColumn("Cuenta"),
                new DataColumn("Categoría"),
                new DataColumn("Tipo de Operación"),
                new DataColumn("Monto")
            });

            foreach(var excel in transactions)
            {
                dataTable.Rows.Add(excel.transaction_date, 
                    excel.Cuenta, 
                    excel.Categoria,
                    excel.operation_type_id, 
                    excel.amount);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);

                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        filename);
                }
            }
        }


        /// <summary>
        /// Método 'ExportExcelByYear'
        /// Permite al usuario exportar sus transacciones realizadas por año 
        /// en un archivo de Excel.
        /// (Udemy): 162. Exportar a Excel - Por Año
        /// </summary>
        /// <param name="year">Parámetro que captura el año</param>
        [HttpGet]
        public async Task<FileResult> ExportExcelByYear(int year)
        {
            var start_date = new DateTime(year, 1, 1);
            var end_date = start_date.AddYears(1).AddDays(-1);
            var user_id = _userService.GetUserID();

            IEnumerable<Transactions> transactions = await ListingTransactionsByUserID
                (start_date, end_date, user_id);

            var fileName = $"ReporteTransaccionesAnuales_{year}.xlsx";

            return GenerateExcel(fileName, transactions);
        }


        /// <summary>
        /// Método 'ExportAllExcel'
        /// Permite al usuario exportar todas sus transacciones a un archivo de Excel.
        /// (Udemy): 162. Exportar a Excel - Por Año
        /// </summary>
        [HttpGet]
        public async Task<FileResult> ExportAllExcel()
        {
            var start_date = DateTime.Today.AddYears(-100);
            var end_date = DateTime.Today.AddYears(100);
            var user_id = _userService.GetUserID();

            IEnumerable<Transactions> transactions = await ListingTransactionsByUserID
                (start_date, end_date, user_id);

            var fileName = $"ReporteTransaccionesCompleto_{DateTime.Today.ToString("dd-MM-yyyy")}.xlsx";

            return GenerateExcel(fileName, transactions);
        }


        /// <summary>
        /// Método 'ListingTransactionsToExcel'
        /// Permite listar todas las transacciones del usuario para poder exportarlas 
        /// a un archivo de Excel.
        /// </summary>
        /// <param name="start_date">Parámetro que captura la fecha de inicio</param>
        /// <param name="end_date">Parámetro que captura la fecha fin</param>
        /// <param name="user_id">Parámetro que captura el ID del usuario</param>
        private async Task<IEnumerable<Transactions>> ListingTransactionsByUserID
            (DateTime start_date, DateTime end_date, int user_id)
        {
            return await _transactionsRepository
                            .GetTransactionsByUserID(new GetUserTransactionRequest()
                            {
                                user_id = user_id,
                                start_date = start_date,
                                end_date = end_date
                            });
        }


        public async Task<IActionResult> Calendar()
        {
            return View();
        }


        /// <summary>
        /// Método 'GetTransactionsToFullCalendar'
        /// Permite mostrar al usuario las transacciones que realizó en un 
        /// calendario en la vista "Views/Transactions/Calendar.cshtml". En este caso,
        /// el calendario que se utiliza sería FullCalendar.
        /// (Udemy): 165. Mostrando las Transacciones de de la Base de Datos en el Calendario
        /// </summary>
        /// <param name="start">Parámetro obligatorio de FullCalendar para obtener la
        /// fecha de inicio</param>
        /// <param name="end">Parámetro obligatorio para obtener la fecha fin,</param>
        /// <returns>Muestra las todas las transacciones del usuario en la vista
        /// del calendario.</returns>
        public async Task<JsonResult> GetTransactionsToFullCalendar
            (DateTime start, DateTime end)
        {
            var user_id = _userService.GetUserID();

            IEnumerable<Transactions> transactions = await ListingTransactionsByUserID
                (start, end, user_id);

            var calendarEvents = transactions.Select(x => new
            {
                title = $"{x.Cuenta} - {x.amount.ToString("N")}",
                start = x.transaction_date.ToString("yyyy-MM-dd"),
                end = x.transaction_date.ToString("yyyy-MM-dd"),
                color = x.operation_type_id == OperationType.Ingreso ? "green" : "red"
            });

            return Json(calendarEvents);
        }


        /// <summary>
        /// Método 'GetTransactionsByDateInFullCalendar'
        /// Permite mostrar al usuario las transacciones que realizó en un día.
        /// (Udemy): 166. Evento Click en el Calendario
        /// </summary>
        /// <param name="date">Parámetro oara obtener la fecha</param>
        /// <returns>Muestra al usuario las transacciones que realizó en un día</returns>
        public async Task<JsonResult> GetTransactionsByDateInFullCalendar
            (DateTime date)
        {
            var user_id = _userService.GetUserID();

            var start_date = date.Date;                        // ej. 2026-08-03 00:00:00
            var end_date = date.Date.AddDays(1).AddTicks(-1);  // ej. 2026-08-03 23:59:59.9999999

            IEnumerable<Transactions> transactions = await ListingTransactionsByUserID
                (start_date, end_date, user_id);

            return Json(transactions);
        }


        //[GoogleScopedAuthorize(CalendarService.Scope.Calendar)]
        //public async Task<IActionResult> CreateEventCalendar()
        //{
        //    GoogleCredential credential = await ;

        //    return View();
        //}

    }
}
