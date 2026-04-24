using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace EnflorarteTopiProyecto.Utils
{
    /// <summary>
    /// Helpers para las vistas que permiten preguntar si el usuario actual
    /// está autorizado para ejecutar una acción de un controlador concreto.
    /// Esto permite ocultar botones/links en las vistas de forma genérica
    /// sin codificar roles concretos.
    /// </summary>
    public static class HtmlAuthorizationExtensions
    {
        public static async Task<bool> UserCanAccessActionAsync(this IHtmlHelper htmlHelper, string actionName, string controllerName)
        {
            var httpContext = htmlHelper.ViewContext.HttpContext;
            var services = httpContext.RequestServices;

            var actionProvider = services.GetService<IActionDescriptorCollectionProvider>();
            if (actionProvider == null)
            {
                // Si no hay provider, no podemos evaluar; asumimos permitido para evitar romper vistas.
                return true;
            }

            var descriptors = actionProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>();
            var candidates = descriptors.Where(d => string.Equals(d.ActionName, actionName, StringComparison.OrdinalIgnoreCase)
                                                    && string.Equals(d.ControllerName, controllerName, StringComparison.OrdinalIgnoreCase))
                                        .ToList();

            if (candidates.Count == 0)
            {
                // Acción no encontrada: asumimos accesible (no queremos ocultar por error).
                return true;
            }

            var user = httpContext.User;
            var authorizationService = services.GetService<IAuthorizationService>();

            bool allowAnonymous = false;
            var authorizeAttributes = new List<AuthorizeAttribute>();

            foreach (var cad in candidates)
            {
                var controllerAttrs = cad.ControllerTypeInfo.GetCustomAttributes(true);
                var methodAttrs = cad.MethodInfo.GetCustomAttributes(true);

                if (controllerAttrs.OfType<AllowAnonymousAttribute>().Any() || methodAttrs.OfType<AllowAnonymousAttribute>().Any())
                {
                    allowAnonymous = true;
                    break;
                }

                authorizeAttributes.AddRange(controllerAttrs.OfType<AuthorizeAttribute>().Cast<AuthorizeAttribute>());
                authorizeAttributes.AddRange(methodAttrs.OfType<AuthorizeAttribute>().Cast<AuthorizeAttribute>());
            }

            if (allowAnonymous)
            {
                return true;
            }

            if (authorizeAttributes.Count == 0)
            {
                // Sin atributos Authorize: accesible por defecto.
                return true;
            }

            foreach (var attr in authorizeAttributes)
            {
                // Si se especifican roles, comprobar que el usuario esté en alguno.
                if (!string.IsNullOrWhiteSpace(attr.Roles))
                {
                    var roles = attr.Roles.Split(',').Select(r => r.Trim()).Where(r => !string.IsNullOrEmpty(r));
                    if (!roles.Any(r => user.IsInRole(r)))
                    {
                        return false;
                    }
                }

                // Si se especifica una policy, usar IAuthorizationService.
                if (!string.IsNullOrWhiteSpace(attr.Policy))
                {
                    if (authorizationService == null)
                    {
                        return false;
                    }

                    var result = await authorizationService.AuthorizeAsync(user, attr.Policy);
                    if (!result.Succeeded)
                    {
                        return false;
                    }
                }

                // Si no hay roles ni policy, el AuthorizeAttribute exige estar autenticado.
                if (string.IsNullOrWhiteSpace(attr.Roles) && string.IsNullOrWhiteSpace(attr.Policy))
                {
                    if (!(user?.Identity?.IsAuthenticated ?? false))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
