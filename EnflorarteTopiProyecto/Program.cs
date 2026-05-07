using EnflorarteTopiProyecto.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection not found in configuration");

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Protege todos los formularios por defecto (Anti-CSRF)
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});
//builder.Services.AddScoped<IPasswordService, PasswordService>();

// Autenticaci�n por cookies y autorizaci�n por roles
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/ControladorSesion/Index";
        options.LogoutPath = "/ControladorSesion/CerrarSesion";
        options.AccessDeniedPath = "/ControladorAccesoDenegado/Index";
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EsSupervisor", p => p.RequireRole("supervisor"));
    options.AddPolicy("EsVentas", p => p.RequireRole("ventas", "supervisor"));
    options.AddPolicy("EsFlorista", p => p.RequireRole("florista", "supervisor"));
    options.AddPolicy("EsRepartidor", p => p.RequireRole("repartidor", "supervisor"));
});

var mexicoCulture = new CultureInfo("es-MX");
CultureInfo.DefaultThreadCurrentCulture = mexicoCulture;
CultureInfo.DefaultThreadCurrentUICulture = mexicoCulture;

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(mexicoCulture);
    options.SupportedCultures = new[] { mexicoCulture };
    options.SupportedUICultures = new[] { mexicoCulture };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var connectionStringForDb = db.Database.GetConnectionString();

    if (!string.IsNullOrWhiteSpace(connectionStringForDb))
    {
        var builderSql = new SqlConnectionStringBuilder(connectionStringForDb);
        var dbName = builderSql.InitialCatalog;

        if (!string.IsNullOrWhiteSpace(dbName))
        {
            builderSql.InitialCatalog = "master";

            using var connection = new SqlConnection(builderSql.ConnectionString);
            connection.Open();

            using var createDbCmd = connection.CreateCommand();
            createDbCmd.CommandText = $@"
            IF DB_ID(N'{dbName.Replace("'", "''")}') IS NULL
            BEGIN
                CREATE DATABASE [{dbName.Replace("]", "]]" )}];
            END";
            createDbCmd.ExecuteNonQuery();
        }
    }

    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRequestLocalization();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapControllerRoute(
    name: "default",
    //pattern: "{controller=Home}/{action=Index}/{id?}") // Te lleva a la p�gina principal Home
    pattern: "{controller=ControladorSesion}/{action=Index}/{id?}" // Te lleva a la p�gina de inicio de sesi�n
); 

/*
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    //pattern: "{controller=Home}/{action=Index}/{id?}") // Te lleva a la p�gina principal Home
    pattern: "{controller=ControladorSesion}/{action=Index}/{id?}") // Te lleva a la p�gina de inicio de sesi�n
    .WithStaticAssets();
*/

app.Run();
