using BudgetManagement.Services;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Transactions}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
