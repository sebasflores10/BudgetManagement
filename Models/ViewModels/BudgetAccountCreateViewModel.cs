using BudgetManagement.Models.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BudgetManagement.Models.ViewModels
{
    /// <summary>
    /// (Udemy): 113. Formulario de Cuentas [2:50 mins]
    /// </summary>
    public class BudgetAccountCreateViewModel : BudgetAccount
    {
        // Atributos deben ser nullable
        // Si no, ModelState no los valida
        public IEnumerable<SelectListItem>? AccountType { get; set; }
    }
}
