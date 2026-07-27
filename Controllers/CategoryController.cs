using BudgetManagement.Models;
using BudgetManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManagement.Controllers
{
    /// <summary>
    /// (Udemy): 133. Creando Categorías
    /// </summary>
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserService _userService;

        public CategoryController(ICategoryRepository categoryRepository,
            IUserService userService)
        {
            this._categoryRepository = categoryRepository;
            this._userService = userService;
        }


        /// <summary>
        /// Método 'Index'
        /// Permite mostrar al usuario en una tabla de la página principal de 
        /// "Models/Category.cs" todas las categorías del usuario.
        /// (Udemy): 135. Índice de Categorías
        /// </summary>
        /// <returns>Muestra al usuario la vista "Views/Category/Index.cshtml"</returns>
        public async Task<IActionResult> Index()
        {
            var user_id = _userService.GetUserID();
            var category = await _categoryRepository.GetUserCategory(user_id);
            return View(category);
        }


        /// <summary>
        /// Método 'CreateCategory'
        /// Permite al mostrarle al usuario la vista de crear categoría
        /// "Views/Category/CreateCategory.cshtml"
        /// (Udemy): 133. Creando Categorías
        /// </summary>
        /// <returns>Muestra la vista "Views/Category/CreateCategory.cshtml" al usuario</returns>
        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View();
        }


        /// <summary>
        /// Método 'CreateCategory'
        /// Permite al usuario realizar la acción de crear categorías.
        /// (Udemy): 133. Creando Categorías
        /// </summary>
        /// <param name="category">Parámetro que captura el objeto "Models/Category.cs"</param>
        /// <returns>Dirige al usuario a la vista "Views/Category/Index.cshtml" luego
        /// de crear categorías.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            var user_id = _userService.GetUserID();
            category.user_id = user_id;
            await _categoryRepository.CreateCategory(category);
            return RedirectToAction("Index");
        }


        /// <summary>
        /// Método 'EditCategory'
        /// Permite al usuario mostrar la vista de editar categorías, la cual sería
        /// "Views/Category/EditCategory.cshtml".
        /// (Udemy): Editar Categorías
        /// </summary>
        /// <param name="category_id">Parámetro que captura el ID de la categoría</param>
        /// <returns>Dirige al usuario a la vista de editar categorías</returns>
        [HttpGet]
        public async Task<IActionResult> EditCategory(int category_id)
        {
            var user_id = _userService.GetUserID();
            var category = await _categoryRepository.GetCategoryByID(category_id, user_id);

            if(category is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            return View(category);
        }


        /// <summary>
        /// Método 'EditCategory'
        /// Permite al usuario realizar la acción de editar categorías.
        /// (Udemy): Editar Categorías
        /// </summary>
        /// <param name="editCat">Parámetro que captura el objeto "Models/Category.cs"</param>
        /// <returns>Dirige al usuario a la vista "Views/Category/Index.cshtml" luego
        /// de haber editado las categorías</returns>
        [HttpPost]
        public async Task<IActionResult> EditCategory(Category editCat)
        {
            if (!ModelState.IsValid)
            {
                return View(editCat);
            }

            var user_id = _userService.GetUserID();
            var category = await _categoryRepository.GetCategoryByID(editCat.category_id, user_id);

            if (category is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            editCat.user_id = user_id;

            await _categoryRepository.EditCategory(editCat);
            return RedirectToAction("Index");
        }


        /// <summary>
        /// Método 'DeleteCategory'
        /// Permite al usuario mostrar la vista "Views/Category/DeleteCategory.cshtml" para
        /// poder eliminar categorías.
        /// (Udemy): 137. Borrar Categorías
        /// </summary>
        /// <param name="category_id">Parámetro que captura el ID de la categoría</param>
        /// <returns>Muestra al usuario la vista "Views/Category/DeleteCategory.cshtml"
        /// para eliminar categoría</returns>
        [HttpGet]
        public async Task<IActionResult> DeleteCategory(int category_id)
        {
            var user_id = _userService.GetUserID();
            var category = await _categoryRepository.GetCategoryByID(category_id, user_id);

            if (category is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            return View(category);
        }


        /// <summary>
        /// Método 'DeleteCategory'
        /// Permite al usuario la acción de eliminar categorías.
        /// (Udemy): 137. Borrar Categorías
        /// </summary>
        /// <param name="category_id">Parámetro que captura el ID de la categoría</param>
        /// <returns>Dirige al usuario a la vista "Views/Category/Index.cshtml" al
        /// eliminar existosamente una categoría</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteCategoryConfirmed(int category_id)
        {
            var user_id = _userService.GetUserID();
            var category = await _categoryRepository.GetCategoryByID(category_id, user_id);

            if (category is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            await _categoryRepository.DeleteCategory(category_id);
            return RedirectToAction("Index");
        }
    }
}
