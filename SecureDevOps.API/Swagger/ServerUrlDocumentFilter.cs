using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SecureDevOps.API.Swagger;

public class ServerUrlDocumentFilter : IDocumentFilter
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServerUrlDocumentFilter(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return;

        var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";

        swaggerDoc.Servers.Clear();
        swaggerDoc.Servers.Add(new OpenApiServer { Url = baseUrl });
    }
}