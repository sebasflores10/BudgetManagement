using BudgetManagement.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Security.Principal;

namespace BudgetManagement.Services
{
    public interface IBudgetAccountRepository
    {
        Task CreateBudgetAccount(BudgetAccount account);
        Task DeleteBudgetAccount(int account_id);
        Task EditBudgetAccount(BudgetAccountCreateViewModel account);
        Task<IEnumerable<BudgetAccount>> GetAllUserBudgetAccounts(int user_id);
        Task<BudgetAccount> GetBudgetAccountByID(int account_id, int user_id);
    }

    /// <summary>
    /// (Udemy): 126. Insertar Cuenta
    /// </summary>
    public class BudgetAccountRepository : IBudgetAccountRepository
    {
        private readonly string databaseConnectionString;

        public BudgetAccountRepository(IConfiguration configuration)
        {
            databaseConnectionString = configuration.GetConnectionString("DefaultConnection"); 
        }


        /// <summary>
        /// Método 'CreateBudgetAccount'
        /// Permite crear una nueva cuenta a la base de datos a la tabla [dbo].[budget_account]
        /// (Udemy): 126. Insertar Cuenta
        /// </summary>
        /// <param name="account">Parámetro que captura el objeto "Models/BudgetAccount.cs"</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task CreateBudgetAccount(BudgetAccount account)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);

                var connID = await conn.QuerySingleAsync<int>("sp_create_budget_account", 
                    new 
                    {
                        account.account_name,
                        account.account_type_id,
                        account.account_balance,
                        account.description
                    }, 
                    commandType: System.Data.CommandType.StoredProcedure);

                account.account_id = connID;
            }

            catch (Exception e)
            {
                throw new ArgumentException($"Error while creating account {account.account_name} |",
                    e.Message);
            }
        }


        /// <summary>
        /// Método 'GetAllUserBudgetAccounts'
        /// Permite consultar todas las cuentas del usuario a la base de datos.
        /// (Udemy): 127. Indice de Cuentas - Query
        /// </summary>
        /// <param name="user_id">Parámetro que captura el ID del usuario</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task<IEnumerable<BudgetAccount>> GetAllUserBudgetAccounts(int user_id) 
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                return await conn.QueryAsync<BudgetAccount>(
                    @"SELECT 
                        bacc.account_id, bacc.account_name, bacc.account_balance,
                        acct.account_type_name AS AccountType
                    FROM 
                        [dbo].[budget_account] AS bacc
                    INNER JOIN 
                        [dbo].[account_types] AS acct
                    ON 
                        acct.account_type_id = bacc.account_type_id
                    WHERE
                        acct.user_id = @user_id
                    ORDER BY
                        acct.user_order", new { user_id });
            }

            catch (Exception e)
            {
                throw new ArgumentException($"Error while consulting accounts for user ID {user_id} |",
                    e.Message);
            }
        }



        /// <summary>
        /// Método 'GetBudgetAccountByID'
        /// Permite consultar la cuenta del usuario por su ID perteneciente al mismo tipo
        /// de cuenta en el que se encuentre.
        /// (Udemy): Editando Cuentas - Agregando Íconos a la Aplicación
        /// </summary>
        /// <param name="account_id">Parámetro que captura el ID de la cuenta del usuario</param>
        /// <param name="user_id">Parámetro que captura el ID del usuario</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task <BudgetAccount> GetBudgetAccountByID(int account_id, int user_id)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                return await conn.QueryFirstOrDefaultAsync<BudgetAccount>(
                    @"SELECT 
                        bacc.account_id, bacc.account_name, bacc.account_balance,
                        bacc.description, acct.account_type_id
                    FROM 
                        [dbo].[budget_account] AS bacc
                    INNER JOIN 
                        [dbo].[account_types] AS acct
                    ON 
                        acct.account_type_id = bacc.account_type_id
                    WHERE
                        acct.user_id = @user_id
                    AND
                        bacc.account_id = @account_id", new { account_id, user_id });
            }

            catch (Exception e)
            {
                throw new ArgumentException(@$"Error while consulting accounts for 
                    account ID {account_id} |", e.Message);
            }
        }


        /// <summary>
        /// Método 'EditBudgetAccount'
        /// Permite al usuario poder editar la cuenta a la base de datos.
        /// (Udemy): Editando Cuentas - Agregando Íconos a la Aplicación
        /// </summary>
        /// <param name="account">Parámetro quee captura el objeto
        /// "Models/BudgetAccountCreateViewModel.cs", que hereda de 
        /// "Models/BudgetAccount.cs"</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task EditBudgetAccount(BudgetAccountCreateViewModel account)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                await conn.ExecuteAsync("sp_edit_budget_account",
                    new
                    {
                        account.account_id,
                        account.account_name,
                        account.account_type_id,
                        account.account_balance,
                        account.description
                    }, commandType: System.Data.CommandType.StoredProcedure);
            }

            catch (Exception e)
            {
                throw new ArgumentException(@$"Error while editing {account.account_name} with 
                    account ID {account.account_id} |", e.Message);
            }
        }


        /// <summary>
        /// Método 'DeleteBudgetAccount'
        /// Permite eliminar registros de cuentas del usuario a la base de datos.
        /// (Udemy): Borrando Cuentas
        /// </summary>
        /// <param name="account_id">Parámetro que captura el ID de la cuenta del usuario</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task DeleteBudgetAccount(int account_id)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                await conn.ExecuteAsync("sp_delete_budget_account", new { account_id },
                    commandType: System.Data.CommandType.StoredProcedure);
            }

            catch (Exception e)
            {
                throw new ArgumentException(@$"Error while deleting budget account |", e.Message);
            }
        }
    }
}
