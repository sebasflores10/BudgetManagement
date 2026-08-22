using BudgetManagement.Models;
using BudgetManagement.Models.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BudgetManagement.Services
{
    public interface IUserRepository
    {
        //Task<Models.Data.User> GetUserByEmailAsync(string email);
        //Task CreateUserAsync(Models.Data.User user);
        Task<int> CreateUserAsync(User user);
        Task<User> GetUserByEmailAsync(string email);
        Task UpdateUser(User user);
    }

    /// <summary>
    /// (Udemy): 172. Configurando el Sistema de Usuarios
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly string databaseConnectionString;

        public UserRepository(IConfiguration configuration)
        {
            databaseConnectionString = configuration.GetConnectionString("DefaultConnection");
        }


        /// <summary>
        /// Método 'CreateUser'
        /// Permite crear un nuevo usuario en la base de datos utilizando 
        /// un procedimiento almacenado hacia la tabla [dbo].[users].
        /// (Udemy): 172. Configurando el Sistema de Usuarios
        /// </summary>
        /// <param name="user">Parámetro que captura el objeto
        /// "Models/Data/User.cs"</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task<int> CreateUserAsync(User user)
        {
            try
            {
                //user.standard_email = user.email.ToUpper();
                using var connection = new SqlConnection(databaseConnectionString);
                var user_id = await connection.QuerySingleAsync<int>
                    (@"sp_create_user", 
                    new 
                    { 
                        user.email, 
                        user.standard_email, 
                        user.password_hash 
                    },
                    commandType: System.Data.CommandType.StoredProcedure);

                await connection.ExecuteAsync("sp_create_new_user_data", new { user_id },
                    commandType: System.Data.CommandType.StoredProcedure);

                return user_id;
            }

            catch (Exception e)
            {
                throw new ArgumentException($"Error while creating user with email {user.email} | ",
                    e.Message);
            }
        }


        /// <summary>
        /// Método 'GetUserByEmailAsync'
        /// Permite consultar a la base de datos en la tabla [dbo].[users] 
        /// un usuario por su email.
        /// (Udemy): 172. Configurando el Sistema de Usuarios
        /// </summary>
        /// <param name="email">Parámetro que captura el email del usuario</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task<User> GetUserByEmailAsync(string email)
        {
            try
            {
                using var connection = new SqlConnection(databaseConnectionString);
                var user = await connection.QuerySingleOrDefaultAsync<User>(
                    @"SELECT * FROM [dbo].[users] WHERE standard_email = @standard_email",
                    new { standard_email = email.ToUpper() });
                return user;
            }

            catch (Exception e)
            {
                throw new ArgumentException($"Error while creating user with email {email} | ",
                    e.Message);
            }
        }


        /// <summary>
        /// Método 'UpdateUser'
        /// Permite actualizar la información del usuario a la base de datos en la tabla
        /// [dbo].[users] cuando ocurre algún reinicio de contraseña, por ejemplo.
        /// (Udemy): 194. ¿Olvidó su Contraseña? - Permitiendo el Cambio de Contraseña
        /// </summary>
        /// <param name="user">Parámetro que captura el objeto "Models/Data/User.cs"</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task UpdateUser(User user)
        {
            try
            {
                using var connection = new SqlConnection(databaseConnectionString);
                await connection.ExecuteAsync("sp_update_user", new
                {
                    user.user_id,
                    user.password_hash
                }, commandType: System.Data.CommandType.StoredProcedure);
            }

            catch (Exception e)
            {
                throw new ArgumentException($"Error while updating user {user.email} | ",
                    e.Message);
            }
        }
    }
}
