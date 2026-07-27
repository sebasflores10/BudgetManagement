using BudgetManagement.Validations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BudgetManagement.Models
{
    /// <summary>
    /// (Udemy): 100. Formulario Tipo Cuentas
    /// </summary>
    public class AccountType /*: IValidatableObject*/
    {
        public int account_type_id { get; set; }
        [Required(ErrorMessage = "El nombre del tipo de cuenta es requerido")]
        [Display(Name = "Nombre del tipo de cuenta")]
        [StringLength(maximumLength: 50, MinimumLength = 3,
            ErrorMessage = "La cantidad de caracteres para el nombre del tipo de cuenta debe ser entre {0} y {1}")]
        [FirstCapitalWord] // (Udemy): 107. Validaciones Personalizadas por Atributo
        [Remote(action: "VerifyAccountTypeExist", controller: "AccountType")] // (Udemy): Validaciones Personalizadas con JavaScript Utilizando Remote
        public string account_type_name { get; set; }
        public int user_id { get; set; }
        public int user_order { get; set; }
        public bool? account_type_exist { get; set; }


        /// <summary>
        /// (Udemy): 108. Validacione Personalizadas por Modelo
        /// </summary>
        /// <param name="validationContext">Parámetro para la clase abstracta 'ValidationContext'
        /// que permite realizar las validaciones en el contexto del modelo. El cual seria:
        /// "Models/AccountType.cs"</param>
        /// <exception cref="NotImplementedException">Excepción que se 
        /// produce cuando no se implementa un método o una operación solicitados.
        /// Referencia: https://learn.microsoft.com/es-es/dotnet/api/system.notimplementedexception?view=net-10.0</exception>
        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if(account_type_name != null && account_type_name.Length > 0)
        //    {
        //        var firstLetter = account_type_name[0].ToString();

        //        if (firstLetter != firstLetter.ToUpper())
        //        {
        //            yield return new ValidationResult("La primera letra del nombre del tipo de cuenta debe ser mayúscula",
        //                new[] { nameof(account_type_name) });
        //        }
        //    }
        //    else
        //    {
        //        throw new NotImplementedException();
        //    }
        //}
    }
}
