using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace SoftDemat.Api.OpenApi;

public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private readonly IAuthenticationSchemeProvider _schemes;

    public BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider schemes)
    {
        _schemes = schemes;
    }

    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (!await HasBearerAsync())
            return;

        AddBearerScheme(document);
        ApplyRequirement(document);
    }

    private async Task<bool> HasBearerAsync()
    {
        var schemes = await _schemes.GetAllSchemesAsync();
        return schemes.Any(scheme => scheme.Name == "Bearer");
    }

    private static void AddBearerScheme(OpenApiDocument document)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Jeton d'accès JWT."
        };
    }

    private static void ApplyRequirement(OpenApiDocument document)
    {
        var requirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        };
        foreach (var path in document.Paths.Values)
        {
            if (path.Operations is not { } operations)
                continue;

            foreach (var operation in operations.Values)
            {
                operation.Security ??= [];
                operation.Security.Add(requirement);
            }
        }
    }
}
