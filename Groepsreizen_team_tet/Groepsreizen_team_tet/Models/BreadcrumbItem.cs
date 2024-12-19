namespace Groepsreizen_team_tet.Models
{
    public class BreadcrumbItem
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = "#";
        public bool IsActive { get; set; } = false;
    }
}
