using BudgetManagement.Models;
using BudgetManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient.Diagnostics;

namespace BudgetManagement.Controllers
{
    /// <summary>
    /// (Udemy): 100. Formulario Tipo Cuentas
    /// </summary>
    public class AccountTypeController : Controller
    {
        private readonly IAccountTypeRepository _accountTypeRepository;
        private readonly IUserService _userService;

        public AccountTypeController(IAccountTypeRepository accountTypeRepository,
            IUserService userService)
        {
            this._accountTypeRepository = accountTypeRepository;
            this._userService = userService;
        }
        /// <summary>
        /// Método 'Index'
        /// Permite mostrar la página Index de AccountType. En SQL Server, la
        /// tabla sería havia [dbo].[account_types]
        /// (Udemy): 100. Formulario Tipo Cuentas
        /// (Udemy): 115. Listado Tipos Cuentas - Modificado
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var user_id = _userService.GetUserID();
            var accountTypes = await _accountTypeRepository.GetUserAccountTypes(user_id);
            return View(accountTypes);
        }

        /// <summary>
        /// Método 'CreateAccountType'
        /// Permite mostrar al usuario la vista para crear nuevos tipos de cuentas.
        /// (Udemy): 100. Formulario Tipo Cuentas
        /// </summary>
        /// <returns>Devuelve la vista para crear nuevos tipos de cuentas</returns>
        [HttpGet]
        public IActionResult CreateAccountType()
        {
            return View();
        }


        /// <summary>
        /// Método 'CreateAccountType'
        /// Permite al usuario realizar la acción de crear nuevos tipos de cuentas
        /// y guardarlas en la base de datos en 'Services/AccountTypeRepository.cs'
        /// (Udemy): 101. Validando el Formulario
        /// (Udemy): 113. Validaciones Personalizadas a Nivel de Controlador
        /// </summary>
        /// <param name="accountType">Parámetro que captura el objeto 
        /// "Models/AccountType.cs"</param>
        /// <returns>Devuelve la vista con el nuevo tipo de cuenta ya creado</returns>
        [HttpPost]
        public async Task<IActionResult> CreateAccountType(AccountType accountType)
        {
            // (Udemy): 101. Validando el Formulario [2:15 mins]
            if (!ModelState.IsValid)
            {
                return View(accountType);
            }

            var user_id = _userService.GetUserID();

            var exist = await _accountTypeRepository
                .AccountTypeNameExist(accountType.account_type_name, user_id);

            if (exist)
            {
                // Si ya existe, mostramos el mensaje de error en el campo
                // "nameof" especifica cual seria el atributo condicionado
                ModelState.AddModelError(nameof(accountType.account_type_name),
                    $"El tipo de cuenta {accountType.account_type_name} ya existe");

                return View(accountType);
            }

            accountType.user_id = user_id;

            await _accountTypeRepository.CreateAccountType(accountType);

            return RedirectToAction("Index");
        }


        /// <summary>
        /// Método 'EditAccountType'
        /// Permite al usuario visualizar los tipos de cuenta que desee modificar,
        /// siempre y cuando cumpla con los requisitos de ID en tipo de cuenta y en 
        /// el usuario.
        /// (Udemy): Actualizando Tipos Cuentas
        /// </summary>
        /// <param name="account_type_id">Parámetro que captura el ID del tipo de cuenta</param>
        /// <returns>Devuelve la vista de editar tipos de cuenta que posea el usuario</returns>
        [HttpGet]
        public async Task<IActionResult> EditAccountType(int account_type_id)
        {
            var user_id = _userService.GetUserID();
            var accountType = await _accountTypeRepository
                .GetAccountTypeByID(account_type_id, user_id);

            // Si no tiene permisos para ver los tipos de cuenta
            if (accountType == null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            return View(accountType);
        }


        /// <summary>
        /// Método 'EditAccountType'
        /// Permite realizar la acción de editar el tipo de cuenta del usuario.
        /// (Udemy): Actualizando Tipos Cuentas
        /// </summary>
        /// <param name="accountType">Parámetro que captura el objeto 
        /// "Models/AccountType.cs"</param>
        /// <returns>Retorna a la vista "Views/AccountType/Index.cshtml" luego
        /// de editar el tipo de cuenta del usuario</returns>
        [HttpPost]
        public async Task<IActionResult> EditAccountType(AccountType accountType)
        {
            var user_id = _userService.GetUserID();
            var accountTypeExist = await _accountTypeRepository
                .GetAccountTypeByID(accountType.account_type_id, user_id);

            if(accountTypeExist is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            await _accountTypeRepository.EditAccountType(accountType);

            return RedirectToAction("Index");
        }


        /// <summary>
        /// Método 'Delete'
        /// Verifica si el tipo de cuenta que desea eliminar el usuario existe en la base de datos.
        /// (Udemy): Borrando Tipos Cuentas
        /// </summary>
        /// <param name="account_type_id">Parámetro que captura el ID del tipo de cuenta</param>
        [HttpGet]
        public async Task<IActionResult> DeleteAccountType(int account_type_id)
        {
            var user_id = _userService.GetUserID();
            var accountTypeExist = await _accountTypeRepository
                .GetAccountTypeByID(account_type_id, user_id);
            
            if(accountTypeExist is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            return View(accountTypeExist);
        }


        /// <summary>
        /// Método 'DeleteAccountTypeConfirmed'
        /// Permite eliminar el tipo de cuenta del usuario en la base de datos.
        /// (Udemy): Borrando Tipos Cuentas
        /// </summary>
        /// <param name="account_type_id">Parámetro que captura el ID del tipo de cuenta</param>
        /// <returns>Devuelve a la vista "Views/AccountType/Index.cshtml" después de
        /// eliminar el tipo de cuenta</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteAccountTypeConfirmed(int account_type_id) 
        {
            var user_id = _userService.GetUserID();
            var accountTypeExist = await _accountTypeRepository
                .GetAccountTypeByID(account_type_id, user_id);

            if (accountTypeExist is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            await _accountTypeRepository.DeleteAccountType(account_type_id);
            return RedirectToAction("Index");
        }


        /// <summary>
        /// Método 'VerifyAccountTypeExist'
        /// Para valdaciones con JavaScript usando JSON
        /// Permite serializar objetos del navegador al backend. En este caso, 
        /// el backend es el controlador "Controllers/AccountTypeController.cs"
        /// y el objeto es "Models/AccountType.cs"
        /// (Udemy): Validaciones Personalizadas con JavaScript Utilizando Remote
        /// </summary>
        /// <param name="account_type_name">Parámetro que captura el nombre del tipo
        /// de cuenta enviado desde la vista "Views/AccountType/CreateAccountType.cshtml"</param>
        /// <returns>Devuelve el objeto AccountType serializado en formato JSON</returns>
        [HttpGet]
        public async Task<IActionResult> VerifyAccountTypeExist(string account_type_name)
        {
            
            var user_id = _userService.GetUserID();
            var exist = await _accountTypeRepository
                .AccountTypeNameExist(account_type_name, user_id);

            if (exist)
            {
                return Json($"El tipo de cuenta {account_type_name} ya existe");
            }

            return Json(true);
        }


        /// <summary>
        /// Método 'ReorderAccountTypes'
        /// Permite al usuario poder reordenar sus tipos de cuentas y actualizarlos
        /// en la base de datos.
        /// (Udemy): 121. Aplicando Múltiples Queries a la Base de Datos
        /// </summary>
        /// <param name="ids">Parámetro que captura los ids de los tipos de cuenta del
        /// usuario en "Views/AccountType/Index.cshtml".</param>
        [HttpPost]
        public async Task<IActionResult> ReorderAccountTypes([FromBody] int[] ids)
        {
            var user_id = _userService.GetUserID();
            var accountTypes = await _accountTypeRepository.GetUserAccountTypes(user_id);
            // Obtenemos los IDs que provienen de la base de datos
            var accountTypesIds = accountTypes.Select(x => x.account_type_id); 
            // Obtenemos los IDs que provienen del Frontend para comparar
            var _exceptUserAccountTypes = ids.Except(accountTypesIds).ToList();

            // Si no existe algún ID que estuviese fuera de "accountTypesIds"
            // quiere decir que todo fue correcto.
            // Condicionamos entonces:
            if (_exceptUserAccountTypes.Count() > 0)
            {
                // Bloqueamos la acción
                return Forbid();
            }

            var reorderAccountTypes = ids.Select((value_id, index_order) =>
                new AccountType() 
                { 
                    account_type_id = value_id, 
                    user_order = index_order + 1 
                })
                .AsEnumerable();

            await _accountTypeRepository.ReorderAccountTypes(reorderAccountTypes);

            return Ok();
        }
    }
}
