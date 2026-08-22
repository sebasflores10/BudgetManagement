using BudgetManagement.Models.Data;
using BudgetManagement.Models.ViewModels;
using BudgetManagement.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

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
        private readonly IForgotPassword _forgotPassword;
        private readonly IUserRepository _userRepository;

        public UsersController(UserManager<User> userManager,
            SignInManager<User> signInManager,
            IUserService userService,
            IForgotPassword forgotPassword,
            IUserRepository userRepository)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._userService = userService;
            this._forgotPassword = forgotPassword;
            this._userRepository = userRepository;
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



        /// <summary>
        /// Método 'ForgotPassword'
        /// Permite mostrar al usuario la vista "Views/Users/ForgotPassword.cshtml" para
        /// realizar acciones de olvido de contraseñas.
        /// (Udemy): 193. ¿Olvidó su Contraseña? - Generando el Código Secreto
        /// </summary>
        /// <param name="message">Parámetro que muestra el mensaje del olvido de contraseña
        /// al usuario</param>
        /// <returns>Muestra la vista "Views/Users/ForgotPassword.cshtml" al usuario</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword(string message = "")
        {
            ViewBag.Message = message;
            return View();
        }


        /// <summary>
        /// Método 'ForgotPassword'
        /// Permite al usuario realizar la acción de reinicio de contraseña.
        /// (Udemy): 193. ¿Olvidó su Contraseña? - Generando el Código Secreto
        /// </summary>
        /// <param name="model">Parámetro que captura el objeto 
        /// "Models/ViewModels/ForgotPasswordViewModel.cs"</param>
        /// <returns>Muestra la vista principal "Views/Users/ForgotPassword.cshtml"</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var message = "Listo. Revise su bandeja de entrada o inbox donde encontrará los siguientes pasos para la recuperación de su contraseña.";
            ViewBag.Message = message;
            ModelState.Clear();

            var user = await _userManager.FindByEmailAsync(model.email);

            if (user is null)
            {
                return View();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var tokenBase64 = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var link = Url.Action("ResetPassword", "Users", new { token = tokenBase64 },
                protocol: Request.Scheme);
            await _forgotPassword.SendEmailToChangePassword(model.email, link);
            return View();
        }



        /// <summary>
        /// Método 'ResetPassword'
        /// Permite al usuario poder obtener un token que le permita poder reiniciar
        /// su contraseña. Utiliza la clase "Models/ViewModels/PasswordResetViewModel.cs"
        /// (Udemy): 194. ¿Olvidó su Contraseña? - Permitiendo el Cambio de Contraseña
        /// </summary>
        /// <param name="code">Parámetro que captura el token de reinicio de contraseña</param>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token = null)
        {
            if(token is null)
            {
                var message = "Token no encontrado";
                return RedirectToAction("ForgotPassword", new { message });
            }

            var model = new PasswordResetViewModel();
            model.token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            return View(model);
        }


        /// <summary>
        /// Método 'ResetPassword'
        /// Permite la acción del cambio de contraseña del usuario por medio del uso de
        /// un token.
        /// (Udemy): 194. ¿Olvidó su Contraseña? - Permitiendo el Cambio de Contraseña
        /// </summary>
        /// <param name="model">Parámetro que captura el objeto 
        /// "Models/ViewModels/PasswordResetViewModel.cs"</param>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(PasswordResetViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.email);

            if (user is null) 
            {
                ModelState.AddModelError(string.Empty, "El email no es válido");
                return View(model); 
            }

            var results = await _userManager.ResetPasswordAsync(user, model.token, model.password);

            if (!results.Succeeded)
            {
                foreach(var error in results.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            return RedirectToAction("PasswordChanged");
        }


        /// <summary>
        /// Método 'PasswordChanged'
        /// Muestra la vista al usuario con un mensaje exitoso del cambio de contraseña.
        /// (Udemy): 194. ¿Olvidó su Contraseña? - Permitiendo el Cambio de Contraseña
        /// </summary>
        /// <returns>Muestra la vista "Views/Users/PasswordChanged.cshtml"</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult PasswordChanged()
        {
            return View();
        }
    }
}
