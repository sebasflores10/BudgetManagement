using BudgetManagement.Services;
using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Añadiendo el nuevo bulder.Service para "Services/AccountTypeRepository.cs"
// Usamos el servicio 'Transient' debido a que el repositorio no comparte datos.
// Se exporta la interfaz personalizada que ser creo, junto a la clase donde esta
builder.Services.AddTransient<IAccountTypeRepository, AccountTypeRepository>();

// Servicio de la clase 'Services/UserService.cs'
builder.Services.AddTransient<IUserService, UserService>();

// Servicio de la clase 'Services/BudgetAccountRepository.cs'
builder.Services.AddTransient<IBudgetAccountRepository, BudgetAccountRepository>();

// Servicio del NuGet 'AutoMapper'
// Opción 1: (A veces devuelve:
// "cannot convert from 'System.Type' to 'System.Action<AutoMapper.IMapperConfigurationExpression>'")
//builder.Services.AddAutoMapper(typeof(Program));

////////

// Opción 2:
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(Program));
});

// Servicio de la clase 'Services/CategoryRepository.cs'
builder.Services.AddTransient<ICategoryRepository, CategoryRepository>();

// Servicio de la clase 'Services/TransactionsRepository.cs'
builder.Services.AddTransient<ITransactionsRepository, TransactionsRepository>();

// Servicios de la clase 'Services/ReportServices.cs'
builder.Services.AddTransient<IReportServices, ReportServices>();
builder.Services.AddHttpContextAccessor();

// Servicios de la clase 'Services/CalendarRepository.cs'
builder.Services.AddTransient<ICalendarRepository, CalendarRepository>();

//////////////////
// ExtraMile
//////////////////

// Lo siguiente evita que JavaScript serialice los valores de "Cuenta" y "Categoria" de
// "Models/Transactions.cs" como camelCaes, haciendo que las variables inicien siempre con
// minúscula debido a que siempre llama la librería 'System.Text.Json'
// Esto cause que para el modal del calendario de FullCalendar, aparezcan como 'undefined'
// Mantiene el casing original de C#
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Configuramos la llamada para el uso de Google Calendar en "Views/Transactions/Calendar.cshtml"
builder.Services.AddAuthentication(o => { 
    o.DefaultChallengeScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
    o.DefaultForbidScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
    o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogleOpenIdConnect(options => {
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google";
});

//////////////////////////////////////////////////////////////////////

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Para "favicon.ico"
// href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-10.0"
app.MapStaticAssets();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

// Agregamos al pipeline los metodos para que se registre el usuario para usar Google Calendar
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Transactions}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

