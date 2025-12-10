using EnflorarteTopiProyecto.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Protege todos los formularios por defecto (Anti-CSRF)
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});
//builder.Services.AddScoped<IPasswordService, PasswordService>();

// Autenticación por cookies y autorización por roles
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/ControladorSesion/Index";
        options.LogoutPath = "/ControladorSesion/CerrarSesion";
        options.AccessDeniedPath = "/AccesoDenegado";
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EsSupervisor", p => p.RequireRole("supervisor"));
    options.AddPolicy("EsVentas", p => p.RequireRole("ventas", "supervisor"));
    options.AddPolicy("EsFlorista", p => p.RequireRole("florista", "supervisor"));
    options.AddPolicy("EsRepartidor", p => p.RequireRole("repartidor", "supervisor"));
});

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

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    //pattern: "{controller=Home}/{action=Index}/{id?}") // Te lleva a la página principal Home
    pattern: "{controller=ControladorSesion}/{action=Index}/{id?}") // Te lleva a la página de inicio de sesión
    .WithStaticAssets();


app.Run();
