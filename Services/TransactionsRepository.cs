using BudgetManagement.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Transactions;

namespace BudgetManagement.Services
{
    public interface ITransactionsRepository
    {
        Task CreateTransaction(Transactions transactions);
        Task DeleteTransaction(int transaction_id);
        Task<Transactions> GetTransactionByID(int transaction_id, int user_id);
        Task UpdateTransaction(Transactions transactions, decimal previous_amount, int previous_account_id);
    }


    /// <summary>
    /// Udemy): 138. Creando Transacciones
    /// </summary>
    public class TransactionsRepository : ITransactionsRepository
    {
        private readonly string databaseConnectionString;

        public TransactionsRepository(IConfiguration configuration)
        {
            databaseConnectionString = configuration.GetConnectionString("DefaultConnection");
        }


        /// <summary>
        /// Método 'CreateTransaction'
        /// Permite al usuario poder crear transacciones a la base de datos en la 
        /// tabla [dbo].[transactions]
        /// (Udemy): 138. Creando Transacciones
        /// </summary>
        /// <param name="transactions">Parámetro que captura el objeto "Models/Transactions.cs"</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task CreateTransaction(Transactions transactions)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                var connId = await conn.QuerySingleAsync<int>("sp_create_transaction", new
                {
                    transactions.user_id,
                    transactions.transaction_date,
                    transactions.amount,
                    transactions.notes,
                    transactions.account_id,
                    transactions.category_id
                }, commandType: System.Data.CommandType.StoredProcedure);

                transactions.transaction_id = connId;
            }

            catch (Exception e)
            {
                throw new ArgumentException($"Error while creating transaction with ID {transactions.transaction_id} | ",
                    e.Message);
            }
        }


        /// <summary>
        /// Método 'UpdateTrnasaction'
        /// Permite al usuario poder editar transacciones a la base de datos en la tabla
        /// [dbo].[transactions].
        /// (Udemy): Actualizando Transacciones - Parte 1
        /// </summary>
        /// <param name="transactions">Parámetro que captura el objeto "Models/Transactions.cs"</param>
        /// <param name="previous_amount">Parámetro que captura el monto anterior del
        /// que fue inicialmente creado la transacción.</param>
        /// <param name="previous_account_id">Parámetro que captura el ID de la cuenta
        /// anterior, en caso de que el usuario la haya actualizado</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task UpdateTransaction(Transactions transactions,
            decimal previous_amount, int previous_account_id)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                await conn.ExecuteAsync("sp_update_transaction", new { 
                    transactions.transaction_id,
                    transactions.user_id,
                    transactions.transaction_date,
                    transactions.amount,
                    previous_amount,
                    transactions.notes,
                    transactions.account_id,
                    previous_account_id,
                    transactions.category_id
                }, commandType: System.Data.CommandType.StoredProcedure);
            }

            catch (Exception e)
            {
                throw new ArgumentException($"Error while updating transaction with ID {transactions.transaction_id} | ",
                    e.Message);
            }
        }


        /// <summary>
        /// Método 'GetTransactionByID'
        /// Permite consultar la transacción por su ID junto con el usuario ID en la 
        /// tabla [dbo].[transactions], haciendo un 'INNER JOIN' con la tabla
        /// [dbo].[category]
        /// (Udemy): Actualizando Transacciones - Parte 1 
        /// </summary>
        /// <param name="transaction_id">Parámetro que captura el ID de la transacción</param>
        /// <param name="user_id">Parámetro que captura el ID del usuario</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task<Transactions> GetTransactionByID(int transaction_id, int user_id)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                return await conn.QueryFirstOrDefaultAsync<Transactions>(
                    @"SELECT transactions.*, cat.operation_type_id 
                    FROM [dbo].[transactions]
                    INNER JOIN [dbo].[category] AS cat
                    ON cat.category_id = transactions.category_id
                    WHERE transactions.transaction_id = @transaction_id
                    AND transactions.user_id = @user_id",
                    new { transaction_id, user_id });
            }

            catch (Exception e)
            {
                throw new ArgumentException($"Error while searching transaction with ID {transaction_id} | ",
                    e.Message);
            }
        }


        /// <summary>
        /// Método 'DeleteTransaction'
        /// Permite al usuario eliminar transacciones por su ID a la base de datos en 
        /// la tabla [dbo].[transactions].
        /// (Udemy): Borrar Transacciones - Un Formulario Con Dos Acciones
        /// </summary>
        /// <param name="transaction_id">Parámetro que captura el ID de la transacción</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task DeleteTransaction(int transaction_id)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                await conn.ExecuteAsync("sp_delete_transaction", new { transaction_id },
                    commandType: System.Data.CommandType.StoredProcedure);
            }

            catch (Exception e)
            {
                throw new ArgumentException($"Error while searching transaction with ID {transaction_id} | ",
                    e.Message);
            }
        }
    }
}
