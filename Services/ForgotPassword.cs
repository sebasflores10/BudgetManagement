using System.Net;
using System.Net.Mail;

namespace BudgetManagement.Services
{
    public interface IForgotPassword
    {
        Task SendEmailToChangePassword(string receptor, string link);
    }

    /// <summary>
    /// (Udemy): 192. ¿Olvidó su contraseña? - Enviando Emails
    /// </summary>
    public class ForgotPassword : IForgotPassword
    {
        private readonly IConfiguration configuration;

        public ForgotPassword(IConfiguration configuration)
        {
            this.configuration = configuration;
        }


        public async Task SendEmailToChangePassword(string receptor, string link)
        {
            try
            {
                var email = configuration.GetValue<string>("CONFIGURATION_EMAIL:EMAIL");
                var password = configuration.GetValue<string>("CONFIGURATION_EMAIL:PASSWORD");
                var host = configuration.GetValue<string>("CONFIGURATION_EMAIL:HOST");
                var port = configuration.GetValue<int>("CONFIGURATION_EMAIL:PORT");

                var client = new SmtpClient(host, port);
                client.EnableSsl = true;

                client.Credentials = new NetworkCredential(email, password);
                var sender = email;
                var subject = "¿Olvidó su contraseña?";
                var htmlEmail =
                    @$"Saludos,

                Este mensaje es para la recuperación de su contraseña para ingresar a BudgetManagement.

                Omita el responder a este email. Para poder acceder de nuevo a su cuenta,
                ingrese en el siguiente link:

                {link}

                ¡Que tenga buen día!
    
                Atentamente,
                Equipo de BudgetManagement";

                var bodyMessage = new MailMessage(sender, receptor, subject, htmlEmail);
                await client.SendMailAsync(bodyMessage);
            }

            catch(SmtpException e)
            {
                Console.WriteLine($"SMTP error: {e.StatusCode} - {e.Message}");
                throw;
            }
        }
    }
}
