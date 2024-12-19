using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Controllers;
using Groepsreizen_team_tet.Attributes;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace Groepsreizen_team_tet.Filters
{
    public class BreadcrumbActionFilter : IActionFilter
    {
        private readonly IUrlHelperFactory _urlHelperFactory;

        public BreadcrumbActionFilter(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (descriptor == null)
                return;

            var user = context.HttpContext.User;

            if (descriptor.ControllerName.Equals("Home", StringComparison.OrdinalIgnoreCase) &&
                descriptor.ActionName.Equals("Index", StringComparison.OrdinalIgnoreCase))
            {
                // Geen breadcrumbs tonen op de Home-pagina
                if (context.Controller is Controller controllerss)
                {
                    controllerss.ViewBag.Breadcrumbs = null; // Of een lege lijst: new List<BreadcrumbItem>()
                }
                return;
            }


            // Check for [AllowAnonymous]
            bool allowAnonymous = descriptor.MethodInfo.IsDefined(typeof(AllowAnonymousAttribute), inherit: true) ||
                                   descriptor.ControllerTypeInfo.IsDefined(typeof(AllowAnonymousAttribute), inherit: true);

            if (!allowAnonymous && !user.Identity.IsAuthenticated)
            {
                // Alleen "Home" breadcrumb weergeven
                var breadcrumbss = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem
                    {
                        Title = "Home",
                        Url = context.HttpContext.Request.PathBase.HasValue ? context.HttpContext.Request.PathBase.Value : "/",
                        IsActive = false
                    }
                };

                // Stel de breadcrumbs in via ViewBag
                if (context.Controller is Controller controllers)
                {
                    controllers.ViewBag.Breadcrumbs = breadcrumbss;
                }

                // Stop verdere verwerking aangezien de gebruiker niet is ingelogd
                return;
            }

            // Verzamel BreadcrumbAttributes van de controller en de actie
            var breadcrumbAttributes = new List<BreadcrumbAttribute>();

            if (!allowAnonymous)
            {
                // Voeg controller-level breadcrumbs alleen toe als niet AllowAnonymous
                var classAttributes = descriptor.ControllerTypeInfo.GetCustomAttributes(typeof(BreadcrumbAttribute), false)
                                                              .Cast<BreadcrumbAttribute>();
                breadcrumbAttributes.AddRange(classAttributes);
            }

            // Method-level attributes
            var methodAttributes = descriptor.MethodInfo.GetCustomAttributes(typeof(BreadcrumbAttribute), false)
                                                           .Cast<BreadcrumbAttribute>();
            breadcrumbAttributes.AddRange(methodAttributes);

            if (!breadcrumbAttributes.Any())
                return;

            var breadcrumbs = new List<BreadcrumbItem>();

            // Voeg Home toe als eerste breadcrumb
            breadcrumbs.Add(new BreadcrumbItem
            {
                Title = "Home",
                Url = context.HttpContext.Request.PathBase.HasValue ? context.HttpContext.Request.PathBase.Value : "/",
                IsActive = false
            });

            // Voeg de breadcrumbs van de attributen toe, behalve het laatste item
            foreach (var attribute in breadcrumbAttributes.Take(breadcrumbAttributes.Count - 1))
            {
                string url = "";
                if (!string.IsNullOrEmpty(attribute.Controller) && !string.IsNullOrEmpty(attribute.Action))
                {
                    var urlHelper = _urlHelperFactory.GetUrlHelper(context);
                    url = urlHelper.Action(attribute.Action, attribute.Controller);
                }

                breadcrumbs.Add(new BreadcrumbItem
                {
                    Title = attribute.Title,
                    Url = url,
                    IsActive = false
                });
            }

            // Voeg het laatste breadcrumb-item toe en markeer het als actief
            var lastAttribute = breadcrumbAttributes.Last();
            string lastUrl = "";
            if (!string.IsNullOrEmpty(lastAttribute.Controller) && !string.IsNullOrEmpty(lastAttribute.Action))
            {
                var urlHelper = _urlHelperFactory.GetUrlHelper(context);
                lastUrl = urlHelper.Action(lastAttribute.Action, lastAttribute.Controller);
            }

            breadcrumbs.Add(new BreadcrumbItem
            {
                Title = lastAttribute.Title,
                Url = "", // Actief item heeft geen URL
                IsActive = true
            });

            // **Hier voegen we de rolgebaseerde aanpassing toe**
            if (user.IsInRole("Beheerder") || user.IsInRole("Verantwoordelijke"))
            {
                // Zoek het "Groepsreizen" breadcrumb-item en pas de URL aan naar "Beheer"
                var groepsreizenBreadcrumb = breadcrumbs.FirstOrDefault(b => b.Title == "Groepsreizen");
                if (groepsreizenBreadcrumb != null)
                {
                    var urlHelper = _urlHelperFactory.GetUrlHelper(context);
                    groepsreizenBreadcrumb.Url = urlHelper.Action("Beheer", "Groepsreis");
                }
            }
            else
            {
                // Voor andere rollen, verwijst "Groepsreizen" naar "Index"
                var groepsreizenBreadcrumb = breadcrumbs.FirstOrDefault(b => b.Title == "Groepsreizen");
                if (groepsreizenBreadcrumb != null)
                {
                    var urlHelper = _urlHelperFactory.GetUrlHelper(context);
                    groepsreizenBreadcrumb.Url = urlHelper.Action("Index", "Groepsreis");
                }
            }


            // Cast context.Controller naar Controller om ViewBag te kunnen gebruiken
            if (context.Controller is Controller controller)
            {
                controller.ViewBag.Breadcrumbs = breadcrumbs;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Niet nodig voor breadcrumbs
        }
    }
}
