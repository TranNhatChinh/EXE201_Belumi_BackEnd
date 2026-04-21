using Microsoft.AspNetCore.Hosting;
using Scriban;
using YourApp.Application.Common.Interfaces;

namespace YourApp.Infrastructure.Services;

public class ScribanTemplateRenderer : ITemplateRenderer
{
    public ScribanTemplateRenderer()
    {
    }

    public async Task<string> RenderAsync(string templateName, object model)
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", $"{templateName}.html");

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Không tìm thấy template tại đường dẫn: {templatePath}");
        }

        var templateContent = await File.ReadAllTextAsync(templatePath);
        var template = Template.Parse(templateContent);
        
        return await template.RenderAsync(model);
    }
}
