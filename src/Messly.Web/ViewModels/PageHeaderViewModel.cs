namespace Messly.Web.ViewModels;

public class PageHeaderViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? ActionText { get; set; }
    public string? ActionUrl { get; set; }
}
