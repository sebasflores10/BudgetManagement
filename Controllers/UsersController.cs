using BudgetManagement.Models.Data;
using BudgetManagement.Models.ViewModels;
using BudgetManagement.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManagement.Controllers
{
    /// <summary>
    /// (Udemy): 171. Formulario de Registro de Usuario
    /// </summary>
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUserService _userService;

        public UsersController(UserManager<User> userManager,
            SignInManager<User> signInManager,
            IUserService userService)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._userService = userService;
        }


        public IActionResult Index()
        {
            return View();
        }



        /// <summary>
        /// Método 'RegisterUser'
        /// Permite mostrar al usuario la vista de registro de usuario que sería
        /// "Views/Users/RegisterUser.cshtml"
        /// (Udemy): 173. Creando un Usuario
        /// (Udemy): 179. Atributo Authorize - Excepción de autorización [AllowAnonymous] [5:25 mins]
        /// </summary>
        /// <returns>Muestra la vista de registro de usuario al usuario</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterUser()
        {
            return View();
        }



        /// <summary>
        /// Método 'RegisterUser'
        /// Permite al usuario poder registrarse en la aplicación, 
        /// enviando los datos del formulario de registro.
        /// (Udemy): 171. Formulario de Registro
        /// (Udemy): 173. Creando un Usuario 
        /// (Udemy): 179. Atributo Authorize - Excepción de autorización [AllowAnonymous] [5:25 mins]
        /// </summary>
        /// <param name="model">Parámetro que captura el objeto 
        /// "Models/ViewModels/RegisterUserViewModel.cs"</param>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser(RegisterUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User() { email = model.email };

            var result = await _userManager.CreateAsync(user, password: model.password);

            if(result.Succeeded)
            {
                // Logueamos al usuario, y le permitimos estar logueado aún
                // después de cerrar el navegador
                await _signInManager.SignInAsync(user, isPersistent: true);
                return RedirectToAction("Index", "Transactions");
            }
            else
            {
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }
        }



        /// <summary>
        /// Método 'UserLogin'
        /// Permite mostrar la vista de login al usuario, la cual sería:
        /// "Views/Users/UserLogin.cshtml"
        /// (Udemy): 177. Implementando el Login
        /// (Udemy): 179. Atributo Authorize - Excepción de autorización [AllowAnonymous] [5:25 mins]
        /// </summary>
        /// <returns>Muestra la vista al usuario en "Views/Users/UserLogin.cshtml"</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult UserLogin()
        {
            return View();
        }


        /// <summary>
        /// Método 'UserLogin'
        /// Permite al usuario la acción de poder ingresar a su sesión de perfil en la
        /// aplicación con el uso de "Microsoft.AspNetCore.Identity".
        /// (Udemy): 177. Implementando el Login
        /// (Udemy): 179. Atributo Authorize - Excepción de autorización [AllowAnonymous] [5:25 mins]
        /// </summary>
        /// <param name="model">Parámetro que captura el objeto 
        /// "Models/ViewModels/UserLoginViewModel.cs"</param>
        /// <returns>Dirige al usuario a la página inicial de la aplicación</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> UserLogin(UserLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.email,
                model.password, model.remember_me, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Transactions");
            }
            else
            {
                ModelState.AddModelError(string.Empty, 
                    "El usuario o la contraseña son incorrectos");
                return View(model);
            }
        }



        public IActionResult _LayoutUnsignedUser()
        {
            return View();
        }



        /// <summary>
        /// Método 'UserLogout'
        /// Permite el cierre de sesión de los usuarios con el uso de 
        /// 'Microsoft.AspNetCore.Identity'
        /// (Udemy): 176. Implementando el Logout
        /// </summary>
        public async Task<IActionResult> UserLogout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Transactions");
        }
    }
}
