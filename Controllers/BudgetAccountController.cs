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


        public BudgetAccountController(IAccountTypeRepository accountTypeRepository,
            IUserService userService,
            IBudgetAccountRepository budgetAccountRepository,
            IMapper mapper)
        {
            this._accountTypeRepository = accountTypeRepository;
            this._userService = userService;
            this._budgetAccountRepository = budgetAccountRepository;
            this._mapper = mapper;
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

    }
}
