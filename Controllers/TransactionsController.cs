using AutoMapper;
using BudgetManagement.Models;
using BudgetManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public TransactionsController(ITransactionsRepository transactionsRepository,
            IUserService userService,
            IBudgetAccountRepository budgetAccountRepository,
            ICategoryRepository categoryRepository,
            IMapper mapper)
        {
            this._transactionsRepository = transactionsRepository;
            this._userService = userService;
            this._budgetAccountRepository = budgetAccountRepository;
            this._categoryRepository = categoryRepository;
            this._mapper = mapper;
        }


        public async Task<IActionResult> Index(int month, int year)
        {
            var user_id = _userService.GetUserID();

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

            var userTransactions = new GetUserTransactionRequest()
            {
                user_id = user_id,
                start_date = start_date,
                end_date = end_date
            };

            var transactions = await _transactionsRepository
                .GetTransactionsByUserID(userTransactions);

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

            // (Udemy): 150. Vista de Movimientos de Cuentas [4:15 mins]
            ViewBag.previous_month = start_date.AddMonths(-1).Month;
            ViewBag.previous_year = start_date.AddMonths(-1).Year;
            ViewBag.month_later = start_date.AddMonths(1).Month;
            ViewBag.year_later = start_date.AddMonths(1).Year;
            ViewBag.returnURL = HttpContext.Request.Path + HttpContext.Request.QueryString;

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
    }
}
