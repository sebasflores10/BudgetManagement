using BudgetManagement.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Security.Principal;

namespace BudgetManagement.Services
{
    public interface ICategoryRepository
    {
        Task CreateCategory(Category category);
        Task DeleteCategory(int category_id);
        Task EditCategory(Category category);
        Task<Category> GetCategoryByID(int category_id, int user_id);
        Task<IEnumerable<Category>> GetUserCategory(int user_id);
        Task<IEnumerable<Category>> GetUserOperationTypeTransaction(int user_id, OperationType operation_type_id);
    }

    /// <summary>
    /// (Udemy): 133. Creando Categorías
    /// </summary>
    public class CategoryRepository : ICategoryRepository
    {
        private readonly string databaseConnectionString;

        public CategoryRepository(IConfiguration configuration)
        {
            databaseConnectionString = configuration.GetConnectionString("DefaultConnection");
        }


        /// <summary>
        /// Método 'CreateCategory'
        /// Permite al usuario crear categorías a la base de datos a la tabla [dbo].[category]
        /// (Udemy): 133. Creando Categorías
        /// </summary>
        /// <param name="category">Parámetro que captura el objeto "Models/Category.cs"</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task CreateCategory(Category category)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                var connID = await conn.QuerySingleAsync<int>("sp_create_category",
                    new
                    {
                        category.category_name,
                        category.operation_type_id,
                        category.user_id
                    },
                    commandType: System.Data.CommandType.StoredProcedure);

                category.category_id = connID;
            }

            catch (Exception e)
            {
                throw new ArgumentException($"Error while creating category {category.category_name} | ",
                    e.Message);
            }
        }


        /// <summary>
        /// Método 'GetUserCategory'
        /// Permite consultar las categorías del usuario a la base de datos.
        /// (Udemy): Índice de Categorías.
        /// </summary>
        /// <param name="user_id">Parámetro que captura el ID del usuario</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task<IEnumerable<Category>> GetUserCategory(int user_id)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                return await conn.QueryAsync<Category>(
                    @"SELECT * FROM [dbo].[category] WHERE user_id = @user_id",
                    new { user_id });
            }

            catch (Exception e)
            {
                throw new ArgumentException(@$"Error while retrieving user category |", 
                    e.Message);
            }
        }


        /// <summary>
        /// Método 'GetCategoryByID'
        /// Permite consultar categorías por su ID y por el ID del usuario.
        /// (Udemy): Editar Categorías.
        /// </summary>
        /// <param name="category_id">Parámetro que captura el ID de la categoría</param>
        /// <param name="user_id">Parámetro que captura el ID del usuario</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task<Category> GetCategoryByID(int category_id, int user_id)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                return await conn.QueryFirstOrDefaultAsync<Category>(
                    @"SELECT * FROM [dbo].[category] 
                    WHERE category_id = @category_id 
                    AND user_id = @user_id", new { category_id, user_id });
            }

            catch (Exception e)
            {
                throw new ArgumentException(@$"Error while retrieving category ID
                    {category_id} |", e.Message);
            }
        }


        /// <summary>
        /// Método 'GetCategoryByID'
        /// Permite editar categorías del usuario a la base de datos.
        /// (Udemy): Editar Categorías
        /// </summary>
        /// <param name="category">Parámetro que captura el objeto "Models/Category.cs"</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task EditCategory(Category category)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                await conn.ExecuteAsync("sp_edit_category", new 
                { 
                    category.category_id,
                    category.category_name,
                    category.operation_type_id,
                    category.user_id
                }, 
                    commandType: System.Data.CommandType.StoredProcedure);
            }

            catch (Exception e)
            {
                throw new ArgumentException(@$"Error while editing {category.category_name} " +
                    "category with ID {category.category_id} |", e.Message);
            }
        }


        /// <summary>
        /// Método 'DeleteCategory'
        /// Permite al usuario eliminar categorías a la base de datos a la tabla
        /// [dbo].[category].
        /// (Udemy): 137. Borrar Categorías
        /// </summary>
        /// <param name="category_id">Parámetro que captura el ID de la categoría</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task DeleteCategory(int category_id)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                await conn.ExecuteAsync("sp_delete_category", new { category_id },
                    commandType: System.Data.CommandType.StoredProcedure);
            }

            catch (Exception e)
            {
                throw new ArgumentException(@$"Error while deleting category ID
                    {category_id} |", e.Message);
            }
        }


        /// <summary>
        /// Método 'GetUserOperationTypeTransaction'
        /// Permite consultar a la base de datos los tipos de operación que tiene el usuerio
        /// en sus categorías de la tabla [dbo].[categories]. Para que puedan ser 
        /// consultados por las transacciones de la tabla [dbo].[transactions].
        /// (Udemy): 141. DropDown Cascada [5:10 mins]
        /// </summary>
        /// <param name="user_id">Parámetro que captura el ID del usuario</param>
        /// <param name="operation_type_id">Parámetro que captura el ID del tipo de 
        /// operación</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task<IEnumerable<Category>> GetUserOperationTypeTransaction
            (int user_id, OperationType operation_type_id)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                return await conn.QueryAsync<Category>(
                    @"SELECT * FROM [dbo].[category] 
                    WHERE user_id = @user_id 
                    AND operation_type_id = @operation_type_id",
                    new { user_id, operation_type_id });
            }

            catch (Exception e)
            {
                throw new ArgumentException("Error while retrieving user operation type " +
                    $"{operation_type_id} for its categories |",e.Message);
            }
        }
    }
}
