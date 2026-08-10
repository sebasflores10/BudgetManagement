using System.ComponentModel.DataAnnotations;

namespace BudgetManagement.Models.ViewModels
{
    /// <summary>
    /// (Udemy): 177. Implementando el Login
    /// </summary>
    public class UserLoginViewModel
    {
        [Required(ErrorMessage = "El email es requerido.")]
        [EmailAddress(ErrorMessage = "El email no es válido.")]
        [Display(Name = "Email")]
        [StringLength(100, ErrorMessage = "El email no puede tener más de 100 caracteres.")]
        public string email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "La contraseña no puede tener más de 100 caracteres.")]
        public string password { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        [Display(Name = "¿Recuérdame?")]
        public bool remember_me { get; set; }
    }
}
