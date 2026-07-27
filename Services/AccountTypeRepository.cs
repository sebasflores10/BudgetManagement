using BudgetManagement.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Security.Principal;

namespace BudgetManagement.Services
{
    /// <summary>
    /// Interfaces de "Services/AccountTypeRepository.cs" para que sean llamados por los
    /// controladores de las vistas.
    /// </summary>
    public interface IAccountTypeRepository
    {
        Task CreateAccountType(AccountType account);
        Task<bool> AccountTypeNameExist(string account_type_name, int user_id);
        Task<IEnumerable<AccountType>> GetUserAccountTypes(int user_id);
        Task EditAccountType(AccountType account);
        Task<AccountType> GetAccountTypeByID(int account_type_id, int user_id);
        Task DeleteAccountType(int account_type_id);
        Task ReorderAccountTypes(IEnumerable<AccountType> accountTypes);
    }

    public class AccountTypeRepository : IAccountTypeRepository
    {
        private readonly string databaseConnectionString;

        /// <summary>
        /// Constructor 'AccountTypeRepository'
        /// Contiene la cadena de conexión a la base de datos.
        /// </summary>
        /// <param name="configuration">Variable que contiene la interfaz 
        /// de conexión a la base de datos</param>
        public AccountTypeRepository(IConfiguration configuration)
        {
            databaseConnectionString = configuration.GetConnectionString("DefaultConnection");
        }



        ////////////////////// Métodos AccountTypeRepository.cs //////////////////////

        /// <summary>
        /// Método 'CreateAccountType'
        /// Permite al usuario crear un nuevo tipo de cuenta en la base de datos.
        /// (Udemy): 122. Generando el Orden Correcto al Insertar - Procedimiento Almacenado con Dapper
        /// </summary>
        /// <param name="account">Parámetro que captura el objeto "Models/AccountType.cs"</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task CreateAccountType(AccountType account)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                // Modificado - Udemy en el comentario del método 'CreateAccountType'
                var connID = await conn.QuerySingleAsync<int>("sp_insert_account_type", 
                    new
                    {
                        account.account_type_name,
                        account.user_id
                    }, 
                    commandType: System.Data.CommandType.StoredProcedure);

                account.user_id = connID;
            }

            catch(Exception e)
            {
                throw new ArgumentException($"Can't create account type {account.account_type_name} |",
                    e.Message);
            }
        }



        /// <summary>
        /// Método 'AccountTypeNameExist'
        /// Permite verificar el que record, o el tipo de cuenta ya existe de parte
        /// del usuario.
        /// (Udemy): 113. Validaciones Personalizadas a Nivel de Controlador [5:00 mins]
        /// </summary>
        /// <param name="account_type_name">Parámetro que captura el nombre del tipo
        /// de cuenta.</param>
        /// <param name="user_id">Parámetro que captura el ID del usuario</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task<bool> AccountTypeNameExist(string account_type_name, int user_id)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                var exist = await conn.QueryFirstOrDefaultAsync<int>
                    (@"SELECT 1 AS account_type_exist FROM [dbo].[account_types] 
                    WHERE account_type_name = @account_type_name 
                    AND user_id = @user_id;", new { account_type_name, user_id });

                return exist == 1;
            }

            catch (Exception e)
            {
                throw new ArgumentException(@$"An error occurred while looking for 
                {account_type_name} |", e.Message);
            }
        }



        /// <summary>
        /// Método 'GetAccountTypes'
        /// Permite buscar los tipos de cuenta que pertenecen a un usuario en específico.
        /// (Udemy): 115. Listado Tipos Cuentas
        /// (Udemy): 121. Aplicando Múltiples Queries a la Base de Datos - Modificado [6:00 mins]
        /// </summary>
        /// <param name="user_id">Parámetro que captura el ID del usuario</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task<IEnumerable<AccountType>> GetUserAccountTypes(int user_id)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                // QueryAsync permite realizar query de SELECT mas eficientemente
                // Mapeamos una colección de objetos de tipo AccountType
                return await conn.QueryAsync<AccountType>(
                    @"SELECT 
                        *
                    FROM
                        [dbo].[account_types]
                    WHERE
                        user_id = @user_id
                    ORDER BY
                        user_order", new { user_id });
            }

            catch (Exception e)
            {
                throw new ArgumentException(@$"An error occurred for the user's accounts |", 
                    e.Message);
            }
        }


        /// <summary>
        /// Método 'EditAccountType'
        /// Permite actualizar el nombre de un tipo de cuenta en la base de datos.
        /// (Udemy): 117. Actualizando Tipos Cuentas
        /// </summary>
        /// <param name="account">Parámetro que captura el objeto "Models/AccountType.cs"</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task EditAccountType(AccountType account)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                await conn.ExecuteAsync("sp_update_account_type", new
                {
                    account.account_type_id,
                    account.account_type_name
                }, commandType: System.Data.CommandType.StoredProcedure);
            }

            catch (Exception e)
            {
                throw new ArgumentException(@$"Can't update account type 
                {account.account_type_name} |", e.Message);
            }
        }


        /// <summary>
        /// Método 'GetAccountTypeByID'
        /// Permite consultar a la base de datos las cuentas del usuario que coincidan
        /// con su ID de tipo de cuenta y ID de usuario.
        /// (Udemy): 117. Actualizando Tipos Cuentas
        /// </summary>
        /// <param name="account_type_id">Parámetro que capture el ID del tipo de cuenta</param>
        /// <param name="user_id">Parámetro que captura el ID del usuario</param>
        /// <returns>Devuelve la cuenta que cumple con los requisitos de los IDs 
        /// de tipo de cuenta y usuario.</returns>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task<AccountType> GetAccountTypeByID(int account_type_id, int user_id)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                return await conn.QueryFirstOrDefaultAsync<AccountType>(
                    @"SELECT 
                        account_type_id, account_type_name, user_order
                    FROM
                        [dbo].[account_types]
                    WHERE
                        account_type_id = @account_type_id
                    AND
                        user_id = @user_id",
                    new { account_type_id, user_id });
            }

            catch (Exception e)
            {
                throw new ArgumentException(@$"An error ocurred with account type ID
                {account_type_id} |", e.Message);
            }
        }



        /// <summary>
        /// Método 'DeleteAccountType'
        /// Permite eliminar un tipo de cuenta por su ID.
        /// (Udemy): 118. Borrando Tipos Cuentas
        /// </summary>
        /// <param name="account_type_id">Parámetro que captura el ID del tipo de cuenta</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task DeleteAccountType(int account_type_id)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                await conn.ExecuteAsync("sp_delete_account_type", 
                    new { account_type_id },
                    commandType: System.Data.CommandType.StoredProcedure); 
            }

            catch (Exception e)
            {
                throw new ArgumentException(@$"Error while deleting account type ID
                {account_type_id} |", e.Message);
            }
        }


        /// <summary>
        /// Método 'ReorderAccountTypes'
        /// Permite al usuario reordenar los tipos de cuenta en "Views/AccountType/Index.cshtml"
        /// y actualizarlos también en la base de datos.
        /// (Udemy): 121. Aplicando Múltiples Queries a la Base de Datos
        /// Nota: Ligeros cambios fueron agregados al código.
        /// </summary>
        /// <param name="accountTypes">Parámetro que permite capturar la colección de 
        /// tipos de cuenta con la función de reordenar en "Views/AccountType/Index.cshtml"</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task ReorderAccountTypes(IEnumerable<AccountType> accountTypes)
        {
            // Como estamos usando una colección de tipos de cuenta, algún tipo de cuenta
            // puede fallar a la hora de actualizar el orden con el feature de 
            // JQuery UI. Por lo que los vamos a capturar con una proyección usando 
            // Dapper, y luego pasarlo al "catch" para capturar el error.

            var accountTypeIds = accountTypes
                .Select(accountTypes => accountTypes.account_type_id)
                .ToList();

            try
            {
                var query = "UPDATE [dbo].[account_types] SET user_order = @user_order WHERE account_type_id = @account_type_id";

                // Luego nos conectamos a la base de datos y procedemos a actualizar el orden

                using var conn = new SqlConnection(databaseConnectionString);
                await conn.ExecuteAsync(query, accountTypes);
            }

            catch (Exception e)
            {
                var ids = string.Join(", ", accountTypeIds);
                throw new ArgumentException(@$"Error while reordering account types with IDs:
                {accountTypeIds} |", e.Message);
            }
        }
    }
}
