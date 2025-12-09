using System.ComponentModel.DataAnnotations;

namespace EnflorarteTopiProyecto.Models
{
    public class UsuarioDto
    {
        /* 
        Esta clase es un DTO (Data Transfer Object) para la entidad Usuario.
        Nos permite validar y transferir datos relacionados con los usuarios.
        Esta clase se utiliza en los archivos cshtml que estan en la carpeta Views/ControladorUsuarios
        */

        /*
        Usamos DataAnnotations para validar los datos de entrada. 
        Display permite que se muestren los datos con otro nombre diferente al declarado en el codigo.
        */

        public int Id { get; set; } // Solo utilizado para la edicion de usuarios.


        [Display(Name = "nombre completo")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        [StringLength(200, ErrorMessage = "El {0} no debe exceder {1} carácteres.")]
        public string Nombre { get; set; } = string.Empty; // Por ejemplo, Nombre se utiliza en Crear.cshtml, para permitir ingresar el nombre del usuario y que se valide con esta clase.


        [Display(Name = "tipo de usuario")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        [EnumDataType(typeof(RolUsuario), ErrorMessage = "El {0} seleccionado no es válido.")]
        public RolUsuario Rol { get; set; } // Puede ser null, para que se muestre el mensaje de error si no se selecciona ningun rol.


        [Display(Name = "contraseña")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "La {0} debe tener entre {2} y {1} carácteres.")]
        public string Contrasena { get; set; } = string.Empty;
        
        [Display(Name = "confirmar contraseña")]
        [Required(ErrorMessage = "Es obligatorio confirmar la contraseña.")]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarContrasena { get; set; } = string.Empty;
        

        public bool Activo { get; set; } = true; // Aqui no hay Required porque por defecto el usuario se crea como activo.
    }
}
