using AutoMapper;
using BudgetManagement.Models;
using BudgetManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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


        public async Task<IActionResult> Index()
        {
            return View();
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
        /// </summary>
        /// <param name="transaction_id">Parámetro que captura el ID de la transacción</param>
        /// <returns>Dirige al usuario a la vista "Views/Transactions/UpdateTransaction.cshtml"</returns>
        [HttpGet]
        public async Task<IActionResult> UpdateTransaction(int transaction_id)
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

            return View(model);
        }


        /// <summary>
        /// Método 'UpdateTransaction'
        /// Permite al usuario realizar la acción de editar una transacción.
        /// (Udemy): Actualizando Transacciones - Parte 2
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

            return RedirectToAction("Index");
        }


        /// <summary>
        /// Método 'DeleteTransaction'
        /// Permite al usuario la acción de eliminar la transacción por su ID desde la 
        /// vista "Views/Transactions/DeleteTransaction.cshtml".
        /// (Udemy): Borrar Transacciones - Un Formulario Con Dos Acciones
        /// </summary>
        /// <param name="transaction_id">Parámetro que captura el ID de la transacción</param>
        /// <returns>Dirige al usuario a la vista "Views/Transactions/Index.cshtml"
        /// luego de borrar la transacción</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteTransaction(int transaction_id)
        {
            var user_id = _userService.GetUserID();

            var transaction = await _transactionsRepository
                .GetTransactionByID(transaction_id, user_id);

            if (transaction is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            await _transactionsRepository.DeleteTransaction(transaction_id);
            return RedirectToAction("Index");
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
