using AutoMapper;
using BudgetManagement.Models;
using BudgetManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Principal;

namespace BudgetManagement.Controllers
{
    /// <summary>
    /// (Udemy): 113. Formulario de Cuentas
    /// </summary>
    public class BudgetAccountController : Controller
    {
        private readonly IAccountTypeRepository _accountTypeRepository;
        private readonly IUserService _userService;
        private readonly IBudgetAccountRepository _budgetAccountRepository;
        private readonly IMapper _mapper;
        private readonly ITransactionsRepository _transactionsRepository;


        public BudgetAccountController(IAccountTypeRepository accountTypeRepository,
            IUserService userService,
            IBudgetAccountRepository budgetAccountRepository,
            IMapper mapper,
            ITransactionsRepository transactionsRepository)
        {
            this._accountTypeRepository = accountTypeRepository;
            this._userService = userService;
            this._budgetAccountRepository = budgetAccountRepository;
            this._mapper = mapper;
            this._transactionsRepository = transactionsRepository;
        }


        /// <summary>
        /// Método 'Index'
        /// Vista principal donde el usuario puede ver el listado de todas sus cuentas
        /// ([dbo].[budget_account]) con sus tipos de cuentas ([dbo].[account_types]).
        /// (Udemy): 127. Indice de Cuentas - Query [6:20 mins]
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var user_id = _userService.GetUserID();
            var budgetAccountWithAccountType = await _budgetAccountRepository
                .GetAllUserBudgetAccounts(user_id);

            // Agrupamos las cuentas con su tipo de cuenta, ya que pueden haber
            // varios tipos de cuenta en una cuenta del usuario
            var model = budgetAccountWithAccountType
                .GroupBy(x => x.AccountType)
                .Select(group => new BudgetAccountIndexViewModel
                {
                    AccountType = group.Key,
                    BudgetAccount = group.AsEnumerable()
                }).ToList();

            return View(model);
        }


        /// <summary>
        /// Método 'CreateBudgetAccount'
        /// Permite al usuario mostrar la vista para crear nuevas cuentas para las tablas
        /// [dbo].[budget_account]
        /// (Udemy): 113. Formulario de Cuentas
        /// (Udemy): 126. Insertar Cuentas - Modificado
        /// </summary>
        /// <returns>Muestra la vista "Views/BudgetAccounts/CreateBudgetAccount.cshtml"</returns>
        [HttpGet]
        public async Task<IActionResult> CreateBudgetAccount()
        {
            var user_id = _userService.GetUserID();
            var model = new BudgetAccountCreateViewModel();
            model.AccountType = await GetUserAccountTypesToBudgetAccount(user_id);

            return View(model);
        }


        /// <summary>
        /// Método 'CreateBudgetAccount'
        /// Permite realizar la acción de crear una cuenta para el usuario.
        /// (Udemy): 126. Insertar Cuentas
        /// </summary>
        /// <param name="model">Parámetro que captura el objeto 
        /// "Models/BudgetAccountCreateViewModel.cs", que hereda de "Models/BudgetAccount.cs"</param>
        [HttpPost]
        public async Task<IActionResult> CreateBudgetAccount
            (BudgetAccountCreateViewModel model)
        {
            var user_id = _userService.GetUserID();
            var accountType = await _accountTypeRepository
                .GetAccountTypeByID(model.account_type_id, user_id);

            if(accountType is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            // Temporary Debug - Claude
            if (!ModelState.IsValid)
            {
                model.AccountType = await GetUserAccountTypesToBudgetAccount(user_id);
                return View(model);
            }

            await _budgetAccountRepository.CreateBudgetAccount(model);
            return RedirectToAction("Index");
        }


        /// <summary>
        /// Método 'EditBudgetAccount'
        /// Permite armar y mostrar la vista al usuario para editar la cuenta.
        /// (Udemy): Editando Cuentas - Agregando Íconos a la Aplicación
        /// </summary>
        /// <param name="account_id">Parámetro que captura el ID de la cuenta</param>
        /// <returns>Muestra la vista de editar las cuentas al usuario</returns>
        [HttpGet]
        public async Task<IActionResult> EditBudgetAccount(int account_id)
        {
            var user_id = _userService.GetUserID();
            var account = await _budgetAccountRepository
                .GetBudgetAccountByID(account_id, user_id);

            if(account is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            var model = _mapper.Map<BudgetAccountCreateViewModel>(account);

            model.AccountType = await GetUserAccountTypesToBudgetAccount(user_id);
            return View(model); 
        }


        /// <summary>
        /// Método 'EditBudgetAccount'
        /// Permite realizar la acción de editar la cuenta desde la vista 
        /// "Views/BudgetAccount/EditBudgetAccount.cshtml"
        /// (Udemy): Editando Cuentas - Agregando Íconos a la Aplicación
        /// </summary>
        /// <param name="model">Parámetro que captura el objeto 
        /// "Models/BudgetAccountCreateViewModel.cs", que hereda de "Models/BudgetAccount.cs"</param>
        /// <returns>Devuelve a la vista principal de cuentas "Views/BudgetAccount/Index.cshtml"</returns>
        [HttpPost]
        public async Task<IActionResult> EditBudgetAccount
            (BudgetAccountCreateViewModel model) 
        {
            var user_id = _userService.GetUserID();
            var account = await _budgetAccountRepository
                .GetBudgetAccountByID(model.account_id, user_id);

            if (account is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            var accountType = _accountTypeRepository
                .GetAccountTypeByID(model.account_type_id, user_id);

            if(accountType is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            await _budgetAccountRepository.EditBudgetAccount(model);
            // "return View": Genera un bug que retorna la vista con el
            // "Models/BudgetAccountIndexViewModel.cs" como null y no la muestra.
            return RedirectToAction("Index");
        }


        /// <summary>
        /// Método 'DeleteBudgetAccountConfirmed'
        /// Permite al usuario usar la vista "Views/BudgetAccount/DeleteBudgetAccount.cshtml".
        /// (Udemy): 132. Borrando Cuentas
        /// </summary>
        /// <param name="account_id">Parámetro que captura el ID de la cuenta del usuario</param>
        /// <returns>Dirige al usuario a la vista "Views/BudgetAccount/Index.cshtml"
        /// después de eliminar la cuenta</returns>
        [HttpGet]
        public async Task<IActionResult> DeleteBudgetAccount(int account_id)
        {
            var user_id = _userService.GetUserID();
            var account = await _budgetAccountRepository
                .GetBudgetAccountByID(account_id, user_id);

            if (account is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            return View(account);
        }


        //////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Método 'DeleteBudgetAccountConfirmed'
        /// Realiza la acción de eliminar la cuenta del usuario desde la vista
        /// "Views/BudgetAccount/DeleteBudgetAccount.cshtml".
        /// (Udemy): 132. Borrando Cuentas
        /// </summary>
        /// <param name="account_id">Parámetro que captura el ID de la cuenta del usuario</param>
        /// <returns>Dirige al usuario a la vista "Views/BudgetAccount/Index.cshtml"
        /// después de eliminar la cuenta</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteBudgetAccountConfirmed(int account_id)
        {
            var user_id = _userService.GetUserID();
            var account = await _budgetAccountRepository
                .GetBudgetAccountByID(account_id, user_id);

            if (account is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            await _budgetAccountRepository.DeleteBudgetAccount(account_id);
            return RedirectToAction("Index");
        }


        /// <summary>
        /// Método 'GetUserAccountTypesToBudgetAccount'
        /// Permite comparar los tipos de cuenta del usuario desde Frontend y el Backend 
        /// para mostrarlos en el "<select></select>" de la vista
        /// "Views/BudgetAccount/CreateBudgetAccount.cshtml"
        /// (Udemy): 126. Insertar Cuentas
        /// </summary>
        /// <param name="user_id">Parámetro que encapsula el ID del usuario desde el
        /// Frontend</param>
        /// <returns>Retorna todos los tipos de cuentas que posee el usuario y 
        /// los muestra en el SelectListItem de las vistas de editar y crear.</returns>
        private async Task<IEnumerable<SelectListItem>> GetUserAccountTypesToBudgetAccount
            (int user_id)
        {
            var accountType = await _accountTypeRepository.GetUserAccountTypes(user_id);
            return accountType.Select(x =>
                new SelectListItem(x.account_type_name, x.account_type_id.ToString()));
            // En el anterior dice, muestra el texto del valor que voy a extraer
        }


        /// <summary>
        /// Método 'BudgetAccountToTransactionsDashboard'
        /// Permite al usuario dirigirse al reporte de las transacciones que se han
        /// realizado para la cuenta que seleccionó en los links de la lista de cuentas
        /// en "Views/BudgetAccount/Index.cshtml"
        /// (Udemy): Movimientos de Cuentas [10:10 mins]
        /// (Udemy): 151. Devolviendo al Usuario al Lugar Donde se Encontraba [Actualizado]
        /// </summary>
        /// <param name="account_id">Parámetro que captura el ID de la cuenta de 
        /// "Models/BudgetAccount.cs"</param>
        /// <param name="month">Parámetro que captura el mes</param>
        /// <param name="year">Parámetro que captura el año</param>
        /// <returns></returns>
        public async Task<IActionResult> BudgetAccountDashboard
            (int account_id, int month, int year)
        {
            var user_id = _userService.GetUserID();
            var account = await _budgetAccountRepository
                .GetBudgetAccountByID(account_id, user_id);

            if(account is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

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

            var transactionsByBudgetAccount = new GetTransactionsByBudgetAccount() 
            { 
                user_id = user_id,
                account_id = account_id,
                start_date = start_date,
                end_date = end_date
            };

            var transactions = await _transactionsRepository
                .GetTransactionsByBudgetAccount(transactionsByBudgetAccount);

            var model = new TransactionsDashboard();
            ViewBag.Cuenta = account.account_name;
            ViewBag.account_id = account_id;

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
            // (Udemy): 151. Devolviendo al Usuario al Lugar Donde se Encontraba [4:15 mins]
            // Sustraemos la URL para luego de de que el usuario modifique o elimine
            // alguna transacción, se vuelva a dirigir a la vista
            // "Views/BudgetAccount/BudgetAccountDashboard.cshtml"
            ViewBag.returnURL = HttpContext.Request.Path + HttpContext.Request.QueryString;

            return View(model);
        }
    }
}
