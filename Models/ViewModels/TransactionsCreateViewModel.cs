using BudgetManagement.Models.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BudgetManagement.Models.ViewModels
{
    /// <summary>
    /// (Udemy): 138. Creando Transacciones
    /// </summary>
    public class TransactionsCreateViewModel : Transactions
    {
        public IEnumerable<SelectListItem>? Cuentas { get; set; }
        [Display(Name = "Categoría")]
        public IEnumerable<SelectListItem>? Categorias { get; set; }
        
        //public OperationType operation_type_id { get; set; } = OperationType.Ingreso;
    }
}
