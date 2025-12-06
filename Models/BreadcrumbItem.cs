namespace WebDienThoai.Models
{
    public class BreadcrumbItem
    {
        public string Text { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public object RouteValues { get; set; }
        public bool IsLink => !string.IsNullOrEmpty(Action) && !string.IsNullOrEmpty(Controller);
    }
}