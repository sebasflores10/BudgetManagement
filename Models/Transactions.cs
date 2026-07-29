using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;
using System.Security.Principal;

namespace BudgetManagement.Models
{
    /// <summary>
    /// (Udemy): 138. Creando Transacciones
    /// (Udemy): Actualizando Transacciones - Parte 1 [Modificado]
    /// </summary>
    public class Transactions
    {
        public int transaction_id { get; set; }
        public int user_id { get; set; }

        [DataType(DataType.DateTime)] // (Udemy): 139. Trabajando con Fechas en un Formulario
        [Display(Name = "Fecha de la transacción")]
        public DateTime transaction_date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(typeof(decimal), "-999999999999.99", "999999999999.99",
            ErrorMessage = "El monto permitido a nivel de sistema debe ser entre {1} y {2}")]
        [Display(Name = "Monto de transacción")]
        public decimal amount { get; set; }
        
        [StringLength(maximumLength: 200,
            ErrorMessage = "La cantidad máxima de caracteres es de {1}")]
        [Display(Name = "Notas")]
        public string notes { get; set; }
        
        [Display(Name = "Cuenta")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una cuenta")]
        public int account_id { get; set; }
        
        [Display(Name = "Categoría")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría")]
        public int category_id { get; set; }

        [Display(Name = "Tipo de Operación")]
        public OperationType operation_type_id { get; set; } = OperationType.Ingreso;

        public string? Cuenta { get; set; }// (Udemy): 149. Movimientos de Cuentas
        public string? Categoria { get; set; }// (Udemy): 149. Movimientos de Cuentas
    }
}
