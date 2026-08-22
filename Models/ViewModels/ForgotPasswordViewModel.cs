using System.ComponentModel.DataAnnotations;

namespace BudgetManagement.Models.ViewModels
{
    /// <summary>
    /// (Udemy): ¿Olvidó su Contraseña? - Generando el Código Secreto
    /// </summary>
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email debe ser un correo electrónico válido")]
        [StringLength(100, ErrorMessage = "El email no debe ser mayor a {1} caracteres")]
        [Display(Name = "Email")]
        public string email { get; set; }
    }
}
