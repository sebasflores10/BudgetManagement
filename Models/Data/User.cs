using System.ComponentModel.DataAnnotations;

namespace BudgetManagement.Models.Data
{
    /// <summary>
    /// (Udemy): 172. Configurando el Sistema de Usuarios
    /// </summary>
    public class User
    {
        public int user_id { get; set; }
        public string email { get; set; }
        public string standard_email { get; set; }
        public string password_hash { get; set; }
    }
}
