namespace BudgetManagement.Services
{
    public interface IUserService
    {
        int GetUserID();
    }


    /// <summary>
    /// Clase 'UserService' que implementa la interfaz 'IUserService'
    /// Forma temporal para testeo:
    /// (Udemy): 116. Evitando Repetir Código
    /// </summary>
    public class UserService : IUserService
    {
        public int GetUserID()
        {
            var user_id = 1;
            return user_id;
        }
    }
}
