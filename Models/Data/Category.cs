using BudgetManagement.Models.Enums;
using BudgetManagement.Validations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Emit;

namespace BudgetManagement.Models.Data
{
    /// <summary>
    /// (Udemy): 133. Creando Categorías
    /// </summary>
    public class Category
    {
        public int category_id { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es requerido")]
        [StringLength(maximumLength: 50, 
            ErrorMessage = "El máximo de caracteres es de 50")]
        [Display(Name = "Nombre de la categoría")]
        [FirstCapitalWord]
        [Remote(action: "VerifyingCategoryExist", controller: "Category",
            AdditionalFields = nameof(operation_type_id))]
        public string category_name { get; set; }

        [Display(Name = "Tipo de operación de categoría")]
        [EnumDataType(typeof(OperationType), 
            ErrorMessage = "El tipo de operación cataloga si es Ingreso (1) o un Egreso (2)")]
        public OperationType operation_type_id { get; set; }
        
        public int user_id { get; set; }
        public string? category_exist { get; set; }
    }
}
