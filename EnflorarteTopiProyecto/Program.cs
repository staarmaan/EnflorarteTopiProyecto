using EnflorarteTopiProyecto.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;

using OpcionesBd;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
var dbType = builder.Configuration.GetSection("DatabaseType").Value ?? "sql";
var dbTypeNormalized = dbType.Trim().ToLowerInvariant();

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Protege todos los formularios por defecto (Anti-CSRF)
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    OpcionesBD.UsarBD(options, dbType, builder);
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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (dbTypeNormalized == "sql")
    {
        var connectionString = db.Database.GetConnectionString();

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            var builderSql = new SqlConnectionStringBuilder(connectionString);
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

        db.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'dbo.arreglo', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.arreglo (
        arreglo_id INT IDENTITY(1,1) PRIMARY KEY,
        nombre NVARCHAR(100) NOT NULL,
        foto_ruta NVARCHAR(300) NULL,
        descripcion NVARCHAR(500) NULL
    );
END");

        db.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'dbo.arreglo_flor', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.arreglo_flor (
        arreglo_id INT NOT NULL,
        flor_id INT NOT NULL,
        cantidad INT NOT NULL,
        color_seleccionado NVARCHAR(50) NOT NULL DEFAULT(N'a elegir'),

        CONSTRAINT pk_arreglo_flor PRIMARY KEY (arreglo_id, flor_id),
        CONSTRAINT fk_arreglo_flor_arreglo FOREIGN KEY (arreglo_id)
            REFERENCES dbo.arreglo(arreglo_id)
            ON DELETE CASCADE
            ON UPDATE NO ACTION,
        CONSTRAINT fk_arreglo_flor_flor FOREIGN KEY (flor_id)
            REFERENCES dbo.flor(flor_id)
            ON DELETE NO ACTION
            ON UPDATE NO ACTION,
        CONSTRAINT chk_arreglo_flor_cantidad CHECK (cantidad > 0)
    );
END");

    db.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'dbo.arreglo_flor', N'U') IS NOT NULL
AND COL_LENGTH('dbo.arreglo_flor', 'color_seleccionado') IS NULL
BEGIN
    ALTER TABLE dbo.arreglo_flor
        ADD color_seleccionado NVARCHAR(50) NOT NULL CONSTRAINT DF_arreglo_flor_color_seleccionado DEFAULT(N'a elegir');
END");

        db.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'dbo.flor_inventario_color', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.flor_inventario_color (
        flor_inventario_color_id INT IDENTITY(1,1) PRIMARY KEY,
        flor_id INT NOT NULL,
        color NVARCHAR(50) NOT NULL,
        cantidad INT NOT NULL DEFAULT(0),

        CONSTRAINT fk_flor_inventario_color_flor FOREIGN KEY (flor_id)
            REFERENCES dbo.flor(flor_id)
            ON DELETE CASCADE
            ON UPDATE NO ACTION,
        CONSTRAINT ck_flor_inventario_color_cantidad_nonnegative CHECK (cantidad >= 0),
        CONSTRAINT uq_flor_inventario_color UNIQUE (flor_id, color)
    );
END");

        db.Database.ExecuteSqlRaw(@"
INSERT INTO dbo.flor_inventario_color (flor_id, color, cantidad)
SELECT f.flor_id, v.color, 0
FROM dbo.flor f
CROSS JOIN (VALUES (N'Rojo'), (N'Rosa Claro'), (N'Fiusha'), (N'Blanca'), (N'Lila')) v(color)
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.flor_inventario_color fic
    WHERE fic.flor_id = f.flor_id AND fic.color = v.color
);");
    }
}

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
