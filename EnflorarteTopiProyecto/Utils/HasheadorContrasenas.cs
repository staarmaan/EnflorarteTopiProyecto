using EnflorarteTopiProyecto.Models;
using Microsoft.AspNetCore.Identity;

namespace EnflorarteTopiProyecto.Utils
{
    public static class HasheadorContrasenas
    {
        // En ambas funciones, el parametro usuarioDto se utiliza para crear un hasheo de contraseñas mas reforzado, pero funcionaria igual si no se pasara.
        // En otras palabras, pasamos al usuario al hasheo para que sea mas seguro.

        public static string HashearContrasena(string contrasenaSinHashear)
        {
            var hasher = new PasswordHasher<string>();
            return hasher.HashPassword(null, contrasenaSinHashear);
        }

        // Recordar que contrasenaIngresada no está hasheada, es la que el usuario ingresa en algun campo de texto.
        public static bool VerificarContrasena(string contrasenaHasheada, string contrasenaIngresada)
        {
            var hasher = new PasswordHasher<string>();
            var resultado = hasher.VerifyHashedPassword(null, contrasenaHasheada, contrasenaIngresada);
            return resultado == PasswordVerificationResult.Success
                || resultado == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
