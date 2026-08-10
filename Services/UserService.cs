using System.Security.Claims;

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
        private readonly HttpContext httpContext;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContext = httpContextAccessor.HttpContext;
        }


        /// <summary>
        /// Método 'GetUserID'
        /// Permite consultar en la aplicación el "Claim" del usuario, es decir, si está
        /// logueado o no.
        /// (Udemy): 178. Obteniendo el ID del Usuario
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public int GetUserID()
        {
            if (httpContext.User.Identity.IsAuthenticated)
            {
                var id_claim = httpContext.User.Claims
                    .Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();

                var id = int.Parse(id_claim.Value);
                return id;
            }


            return 0;
        }
    }
}
