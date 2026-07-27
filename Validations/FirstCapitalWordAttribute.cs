using System.ComponentModel.DataAnnotations;

namespace BudgetManagement.Validations
{
    /// <summary>
    /// (Udemy): 107. Validaciones Personalizadas por Atributo
    /// </summary>
    public class FirstCapitalWordAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var firstLetter = value.ToString()[0].ToString();
            
            if(firstLetter != firstLetter.ToUpper())
            {
                return new ValidationResult("La primera letra debe ser mayúscula");
            }

            return ValidationResult.Success;
        }
    }
}
