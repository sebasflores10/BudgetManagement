using System.ComponentModel.DataAnnotations;

namespace BudgetManagement.Models.ViewModels
{
    /// <summary>
    /// (Udemy): 171. Formulario de Registro de Usuario
    /// </summary>
    public class RegisterUserViewModel
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
    }
}
