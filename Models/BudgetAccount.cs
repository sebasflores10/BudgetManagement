using BudgetManagement.Validations;
using System.ComponentModel.DataAnnotations;

namespace BudgetManagement.Models
{
    /// <summary>
    /// (Udemy): 113. Formulario de Cuentas [dbo].[budget_account]
    /// </summary>
    public class BudgetAccount
    {
        public int account_id { get; set; }
        
        [Required(ErrorMessage = "El nombre de la cuenta es requerido")]
        [StringLength(maximumLength: 50, 
            ErrorMessage = "El nombre de la cuenta debe ser un máximo de 50 caracteres")]
        [Display(Name = "Nombre de la cuenta")]
        [FirstCapitalWord] // Revisar - (Udemy): 107. Validaciones Personalizadas por Atributo
        public string account_name { get; set; }

        [Required(ErrorMessage = "El ID del tipo de cuenta es requerido")]
        [Display(Name = "ID del tipo de cuenta")]
        public int account_type_id { get; set; } // FK hacia PK [dbo].[account_types}
        
        [Required(ErrorMessage = "El balance de la cuenta es requerido")]
        [Display(Name = "Balance")]
        [Range(typeof(decimal), "-999999999999.99", "999999999999.99",
            ErrorMessage = "El balance permitido a nivel de sistema debe ser entre {1} y {2}")]
        public decimal account_balance { get; set; }

        [Required(ErrorMessage = "El nombre de la cuenta es requerido")]
        [StringLength(maximumLength: 100,
            ErrorMessage = "La descripción de la cuenta debe ser un máximo de 100 caracteres")]
        [Display(Name = "Descripción")]
        public string description { get; set; }
        public string AccountType { get; set; } // (Udemy): 127. Indice de Cuentas - Query [2:40 mins]
    }
}
