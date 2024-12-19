namespace Groepsreizen_team_tet.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class BreadcrumbAttribute : Attribute
    {
        public string Title { get; }
        public string? Controller { get; }
        public string? Action { get; }

        public BreadcrumbAttribute(string title, string? controller = null, string? action = null)
        {
            Title = title;
            Controller = controller;
            Action = action;
        }
    }
}
