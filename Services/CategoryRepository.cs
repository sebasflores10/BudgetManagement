using BudgetManagement.Models.Data;
using BudgetManagement.Models.Enums;
using BudgetManagement.Models.ViewModels;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Security.Principal;

namespace BudgetManagement.Services
{
    public interface ICategoryRepository
    {
        Task<bool> CategoryExist(string category_name, int user_id, OperationType operation_type_id);
        Task<int> CountUserCategories(int user_id);
        Task CreateCategory(Category category);
        Task DeleteCategory(int category_id);
        Task EditCategory(Category category);
        Task<Category> GetCategoryByID(int category_id, int user_id);
        Task<IEnumerable<Category>> GetUserCategory(int user_id, PaginationViewModel pagination);
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
        /// (Udemy): 135. Índice de Categorías.
        /// (Udemy): 184. Paginación - Back-End - Query [Feature de paginación a "Views/Category.Index.cshtml"]
        /// </summary>
        /// <param name="user_id">Parámetro que captura el ID del usuario</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task<IEnumerable<Category>> GetUserCategory
            (int user_id, PaginationViewModel pagination)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                return await conn.QueryAsync<Category>(
                    @$"SELECT * 
                    FROM 
                        [dbo].[category] 
                    WHERE 
                        user_id = @user_id
                    ORDER BY
                        category_name
                    OFFSET 
                        {pagination.jumping_records}
                    ROWS FETCH NEXT
                        {pagination.total_records}
                    ROWS ONLY",
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
                    $"and category with ID {category.category_id} |", e.Message);
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


        /// <summary>
        /// Método 'CountUserCategories'
        /// Permite consultar en la base de datos la tabla [dbo].[categories] y contar
        /// el total de categorías que posee el usuario.
        /// (Udemy): 185. Paginación - Contando los Registros
        /// </summary>
        /// <param name="user_id">Parámetro que captura el ID del usuario</param>
        /// <returns>Devuelve la cantidad total de categorías que posee un usuario</returns>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task<int> CountUserCategories(int user_id)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                return await conn.ExecuteScalarAsync<int>(
                    @"SELECT COUNT(*)
                    FROM [dbo].[category] 
                    WHERE user_id = @user_id", new { user_id });
            }

            catch (Exception e)
            {
                throw new ArgumentException("Error while counting user's categories |", 
                    e.Message);
            }
        }


        /// <summary>
        /// Método 'CategoryExist'
        /// Permite consultar a la base de datos para verificar en la tabla [dbo].[category]
        /// si la categoría que el usuario desea crear, ya existe en los records.
        /// </summary>
        /// <param name="category_name">Parámetro que captura el nombre de la categoría</param>
        /// <param name="user_id">Parámetro que captura el ID del usuario</param>
        /// <param name="operation_type_id">Parámetro que captura el ID del tipo de operación</param>
        /// <exception cref="ArgumentException">Excepción que permite capturar
        /// el dato que falló a la hora de actualizar</exception>
        public async Task<bool> CategoryExist(string category_name, 
            int user_id, OperationType operation_type_id)
        {
            try
            {
                using var conn = new SqlConnection(databaseConnectionString);
                var exist = await conn.QueryFirstOrDefaultAsync<int>
                    (@"SELECT 1 AS category_exist FROM [dbo].[category] 
                    WHERE category_name = @category_name
                    AND operation_type_id = @operation_type_id
                    AND user_id = @user_id;", new { category_name, user_id, operation_type_id });

                return exist == 1;
            }

            catch (Exception e)
            {
                throw new ArgumentException("An error occurred while looking for " +
                    $"category {category_name} |", e.Message);
            }
        }
    }
}
