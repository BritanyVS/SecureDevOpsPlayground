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

        var forwardedProto = httpContext.Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
        var forwardedHost = httpContext.Request.Headers["X-Forwarded-Host"].FirstOrDefault();

        var scheme = !string.IsNullOrEmpty(forwardedProto) ? forwardedProto : httpContext.Request.Scheme;
        var host = !string.IsNullOrEmpty(forwardedHost) ? forwardedHost : httpContext.Request.Host.Value;
        var baseUrl = $"{scheme}://{host}";

        swaggerDoc.Servers.Clear();
        swaggerDoc.Servers.Add(new OpenApiServer { Url = baseUrl });
    }
}