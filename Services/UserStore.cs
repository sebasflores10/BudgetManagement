using BudgetManagement.Models.Data;
using Microsoft.AspNetCore.Identity;

namespace BudgetManagement.Services
{
    /// <summary>
    /// (Udemy): 172. Configurando el Sistema de Usuarios (!Importante)
    /// Las siguientes interfaces son necesarias para implementar un sistema de autenticación 
    /// y autorización de usuarios en una aplicación ASP.NET Core. 
    /// Estas interfaces proporcionan métodos para crear, actualizar, eliminar y 
    /// buscar usuarios, así como para manejar la información de correo electrónico 
    /// y contraseñas usando "Microsoft.AspNetCore.Identity".
    /// </summary>
    public class UserStore : IUserStore<Models.Data.User>,
        IUserEmailStore<Models.Data.User>,
        IUserPasswordStore<Models.Data.User>
    {
        private readonly IUserRepository _userRepository;

        public UserStore(IUserRepository userRepository)
        {
            this._userRepository = userRepository;
        }


        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            user.user_id = await _userRepository.CreateUserAsync(user);
            return IdentityResult.Success;
        }

        public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            // Si no es necesario, se puede finalizar este método con
            // la siguiente línea de código, para evitar que el recolector de basura (GC)
            // llame al finalizador de la clase. La razón es porque "UserManager<T>" implementa
            // la interfaz IDisposable, y al implementar esta interfaz, que hereda
            // IUserStore<T>, IUserEmailStore<T> y IUserPasswordStore<T>. Las conexiones a 
            // la a base de datos se manejan en "Services/UserRepository.cs",
            // por lo que no es necesario liberar recursos adicionales en esta clase.
            GC.SuppressFinalize(this);
        }

        public async Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return await _userRepository.GetUserByEmailAsync(normalizedEmail);
        }

        public Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await _userRepository.GetUserByEmailAsync(normalizedUserName);
        }

        public Task<string?> GetEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.email);
        }

        public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.password_hash);
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.user_id.ToString());
        }

        public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.email);
        }

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetEmailAsync(User user, string? email, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedEmailAsync(User user, string? normalizedEmail, CancellationToken cancellationToken)
        {
            user.standard_email = normalizedEmail;
            return Task.CompletedTask;
        }

        public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(User user, string? passwordHash, CancellationToken cancellationToken)
        {
            user.password_hash = passwordHash;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            await _userRepository.UpdateUser(user);
            return IdentityResult.Success;
        }
    }
}
