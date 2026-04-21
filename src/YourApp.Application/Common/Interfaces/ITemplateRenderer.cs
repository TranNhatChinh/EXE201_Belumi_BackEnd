namespace YourApp.Application.Common.Interfaces;

public interface ITemplateRenderer
{
    Task<string> RenderAsync(string templateName, object model);
}
