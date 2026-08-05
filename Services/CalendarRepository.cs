using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using System.Runtime.InteropServices;

namespace BudgetManagement.Services
{
    public interface ICalendarRepository
    {
        // Define methods for interacting with the Google Calendar API
        //Task<IEnumerable<CalendarEvent>> GetEventsAsync(DateTime start, DateTime end);
        //Task AddEventAsync(CalendarEvent calendarEvent);
        //Task UpdateEventAsync(CalendarEvent calendarEvent);
        //Task DeleteEventAsync(string eventId);
        Task CreateEventAsync();
    }


    /// <summary>
    /// Fuera de Udemy.
    /// Usando la implementación de Google Calendar para uso gratuito. FullCalendar ahora
    /// es por método de pago,
    /// </summary>
    public class CalendarRepository : ICalendarRepository
    {
        private readonly string[] Scopes = { CalendarService.Scope.Calendar };
        private readonly string _applicationName = "BudgetManagement";
        private readonly string databaseConnectionString;

        public CalendarRepository()
        {
            
        }



        /// <summary>
        /// Método 'CreateEventAsync'
        /// Crea un evento nuevo en Google Calendar usando la API de Google con el 
        /// calendario principal del usuario.
        /// </summary>
        public async Task CreateEventAsync()
        {
            // Cargamos las credenciales parar acceder a Google Calendar.
            UserCredential credential;
            // Cargamos el ID del calendario
            string calendarId = "primary";

            // Luego, tenemos que especificar los permisos que va a tener el usuario
            using (var stream = new FileStream("client_secret.json", FileMode.Open,
                FileAccess.Read)) 
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None);
            }

            // Inicializamos el servicio de Google Calendar
            var service = new CalendarService
                (new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName
            });


            // Deifinmos los detalles del evento que queremos crear
            Event newGoogleEvent = new Event()
            {
                Summary = "Reunión de prueba",
                Location = "Hatillo 1",
                Description = "Esta es una reunión de prueba.",
                Start = new EventDateTime()
                {
                    DateTime = DateTime.Now.AddDays(1),
                    TimeZone = "America/Costa_Rica"
                },
                End = new EventDateTime()
                {
                    DateTime = DateTime.Now.AddDays(1).AddHours(1),
                    TimeZone = "America/Costa_Rica"
                }
            };

            // Usamos el calendario principal del usuario, para luego crear el evento
            // en Google Calendar            
            var request = service.Events.Insert(newGoogleEvent, calendarId);
            Event createdEvent = await request.ExecuteAsync(); 
        }
    }
}
