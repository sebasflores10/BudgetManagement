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
        Task<IEnumerable<TransactionResultByWeek>> GetTransactionResultsByWeek(GetUserTransactionRequest model);
        Task<IEnumerable<Transactions>> GetTransactionsByBudgetAccount(GetTransactionsByBudgetAccount model);
        Task<IEnumerable<Transactions>> GetTransactionsByUserID(GetUserTransactionRequest model);
        Task<IEnumerable<MonthlyResultSQL>> MonthlyTransactionsResult(int user_id, int year);
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


        /// <summary>
        /// Método 'GetTransactionsByBudgetAccount'
        /// Permite la consulta a la base de datos para mostrar en las transacciones
        /// comparando por el ID de la cuenta, ID de la categoría, y el ID del usuario
        /// para consultar todas las transacciones realizadas entre una fecha de inicio
        /// y una fecha límite.
        /// (Udemy): 149. Movimientos de Cuentas
        /// </summary>
        /// <param name="model">Parámetro que captura el objeto 
        /// "Models/GetTransactionsByBudgetAccount.cs"</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task<IEnumerable<Transactions>> GetTransactionsByBudgetAccount
            (GetTransactionsByBudgetAccount model)
        {
            try 
            {
                using var conn = new SqlConnection(databaseConnectionString);
                return await conn.QueryAsync<Transactions>(@"SELECT 
                    tra.transaction_id, tra.amount, tra.transaction_date, 
                    cat.category_name AS Categoria, cat.operation_type_id,
                    bacc.account_name AS Cuenta 
                FROM 
                    [dbo].[transactions] tra
                INNER JOIN 
                    [dbo].[category] cat
                ON 
                    cat.category_id = tra.category_id
                INNER JOIN 
                    [dbo].[budget_account] bacc
                ON 
                    bacc.account_id = tra.account_id
                WHERE 
                    tra.account_id = @account_id
                AND 
                    tra.user_id = @user_id
                AND 
                    tra.transaction_date 
                BETWEEN 
                    @start_date AND @end_date", model);
            }

            catch (Exception e)
            {
                throw new ArgumentException($"Error while searching transaction " +
                    $"for budget account {model.account_id} | ", e.Message);
            }
        }


        /// <summary>
        /// Método 'GetTransactionByUserID'
        /// Permite consultar a la base de datos una colección de transacciones que el 
        /// usuario ha realizado durante un día por su ID de usuario a la tabla
        /// [dbo].[transactions]. 
        /// Llamado por el "Index()" en "Controllers/TransactionsController.cs"
        /// (Udemy): 152. Reporte Diario - Query
        /// </summary>
        /// <param name="model">Parámetro que captura el objeto de 
        /// "Models/GetUserTransactionRequest.cs"</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task<IEnumerable<Transactions>> GetTransactionsByUserID
            (GetUserTransactionRequest model)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                return await conn.QueryAsync<Transactions>(
                    @"SELECT 
                        tra.transaction_id, tra.amount, tra.transaction_date, 
                        cat.category_name AS Categoria, cat.operation_type_id,
                        bacc.account_name AS Cuenta 
                    FROM 
                        [dbo].[transactions] tra
                    INNER JOIN 
                        [dbo].[category] cat
                    ON 
                        cat.category_id = tra.category_id
                    INNER JOIN 
                        [dbo].[budget_account] bacc
                    ON 
                        bacc.account_id = tra.account_id
                    WHERE
                        tra.user_id = @user_id
                    AND 
                        tra.transaction_date 
                    BETWEEN 
                        @start_date AND @end_date
                    ORDER BY
                        tra.transaction_date DESC", model);
                // Para permitir al usuario observar desde "Views/Transactions/Index.cshtml"
                // la última transacción que se editó o creó, visualizarse en la tabla desde
                // el más nuevo hasta el más viejo
            }

            catch (Exception e)
            {
                throw new ArgumentException($"Error while searching transaction for user ID " +
                    $"{model.user_id} | ", e.Message);
            }
        }


        /// <summary>
        /// Método 'GetTransactionResultsByWeek'
        /// Permite consultar a la base de datos la cantidad de diferencias de semanas 
        /// que en que se realizaron las transacciones en la tabla [dbo].[transactions] 
        /// entre distintas fechas ("start_date" y "end_date") 
        /// en la categoría donde se colecta.
        /// (Udemy): 156. Reporte Semanal - Query - Group By con DateDiff
        /// </summary>
        /// <param name="model">Parámetro que captura el objeto 
        /// "Models/TransactionResultByWee.cs""</param>
        /// <returns>Devuelve una tabla de la base de datos con resultados en columnas:
        /// [Semana, Monto, opration_type_id]</returns>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task<IEnumerable<TransactionResultByWeek>> GetTransactionResultsByWeek
            (GetUserTransactionRequest model)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                return await conn.QueryAsync<TransactionResultByWeek>
                    (@"SELECT 
                        DATEDIFF(d ,@start_date, tra.transaction_date) / 7 + 1 AS Semana,
                        SUM(amount) AS Monto,
                        cat.operation_type_id
                    FROM 
                        [dbo].[transactions] tra
                    INNER JOIN
                        [dbo].[category] cat
                    ON
                        cat.category_id = tra.category_id
                    WHERE
                        tra.user_id = @user_id AND transaction_date
                    BETWEEN
                        @start_date AND @end_date
                    GROUP BY
                        DATEDIFF(d ,@start_date, tra.transaction_date) / 7,
                        cat.operation_type_id
                    ORDER BY
                        Semana", model);
            }

            catch (Exception e)
            {
                throw new ArgumentException($"Error while searching transaction for user ID " +
                    $"{model.user_id} | ", e.Message);
            }
        }


        /// <summary>
        /// Método 'MonthlyTransactionsResult
        /// Permite consultar a la base de datos los reportes del usuario de las 
        /// transacciones que realizó durante el mes en el año de la consulta del reporte.
        /// Consulta la tabla [dbo].[transactions] haciendo un INNER JOIN con [dbo].[category]
        /// (Udemy): 159. Reporte Mensual - Query
        /// </summary>
        /// <param name="user_id">Parámetro que captura el ID del usuario</param>
        /// <param name="year">Parámetro que captura el año</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task<IEnumerable<MonthlyResultSQL>> MonthlyTransactionsResult
            (int user_id, int year)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                return await conn.QueryAsync<MonthlyResultSQL>
                    (@"SELECT 
                        MONTH(tra.transaction_date) AS Mes,
                        SUM(tra.amount) AS Monto,
                        cat.operation_type_id
                    FROM
                        [dbo].[transactions] tra
                    INNER JOIN
                        [dbo].[category] cat
                    ON
                        cat.category_id = tra.category_id
                    WHERE
                        tra.user_id = @user_id AND YEAR(transaction_date) = @year
                    GROUP BY
                        MONTH(tra.transaction_date), cat.operation_type_id",
                    new { user_id, year });
            }

            catch (Exception e)
            {
                throw new ArgumentException($"Error while searching transaction for user ID " +
                    $"{user_id} | ", e.Message);
            }
        }
    }
}
