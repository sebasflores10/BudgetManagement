using System.ComponentModel.DataAnnotations;

namespace BudgetManagement.Models.ViewModels
{
    /// <summary>
    /// (Udemy): ¿Olvidó su Contraseña? - Permitiendo el Cambio de Contraseña
    /// </summary>
    public class PasswordResetViewModel
    {
        [Required(ErrorMessage = "El email es requerido")]
        [StringLength(100, ErrorMessage = "El email no debe tener más de {1} caracteres.")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "El email tiene que ser un correo electrónico válido")]
        public string email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña nueva")]
        public string password { get; set; }
        
        public string token { get; set; }
    }
}
