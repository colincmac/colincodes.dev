# ProtectedResourceMetadataLibrary

This library enables ASP.NET Core API servers to dynamically fetch and use OAuth 2.0 Protected Resource Metadata (RFC 9728) for client authentication and authorization.

## Why use `AddProtectedResourceJwtBearer` vs a custom AuthenticationHandler

- **Leverages built-in JWT Bearer**: Extending the existing `JwtBearerHandler` preserves ASP.NET Core’s proven token validation, audience checks, and pipeline behavior.
- **Minimal custom code**: Hooks only the `OnChallenge` event to inject the `resource_metadata` parameter rather than reimplementing the full handler pipeline.
- **Easier maintenance**: Aligns with Microsoft’s authentication abstractions, benefiting from ongoing security patches, features, and documentation.
- **Interceptor vs full handler**: Using an extension method is less invasive, keeps your codebase smaller, and reduces duplication of core JWT logic.

## Supporting Signed Protected Resource Metadata

RFC 9728 allows metadata to be returned as a signed JWT (JWS). To enforce integrity and authenticity:

1. **Enable signature validation** in configuration (`ValidateSignature: true`).
2. **Fetch the JWS** from the well-known endpoint.
3. **Retrieve JWKS** from `jwks_uri` in an initial unsigned metadata fetch (or configuration).
4. **Validate the JWS** signature before deserializing the payload.
5. **Cache both keys and metadata** per configured durations.

### How it works
- On first call, the library retrieves the raw JWS string.
- It loads the signing keys from the `jwks_uri`.
- Uses `System.IdentityModel.Tokens.Jwt` to validate the JWS (issuer, signature).
- Upon successful validation, extracts the JSON payload and deserializes into `ProtectedResourceMetadata`.


## Integration with Microsoft Identity SDK and Entra ID

The easiest way to combine this library with Azure AD (Entra ID) is via Microsoft.Identity.Web:

1. **Add NuGet packages** to your `.csproj`:
   ```xml
   <PackageReference Include="Microsoft.Identity.Web" Version="2.0.0" />
   <PackageReference Include="Microsoft.Identity.Web.MicrosoftGraph" Version="2.0.0" />
   ```

2. **Configure services** in `Program.cs` (for minimal hosting):
   ```csharp
   using Microsoft.Identity.Web;
   using Microsoft.AspNetCore.Authentication.JwtBearer;
   using MyCompany.ProtectedResourceMetadata.Extensions;

   var builder = WebApplication.CreateBuilder(args);

   // Register our metadata fetcher
   builder.Services.AddProtectedResourceMetadata();

   // Configure Microsoft Identity / Entra ID
   builder.Services.AddMicrosoftIdentityWebApiAuthentication(
           builder.Configuration, "AzureAd")
       .EnableTokenAcquisitionToCallDownstreamApi()
       .AddInMemoryTokenCaches();

   // Replace default JWT bearer to inject resource_metadata on challenges
   builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddProtectedResourceJwtBearer(
           JwtBearerDefaults.AuthenticationScheme,
           JwtBearerDefaults.AuthenticationScheme,
           options => {
               // Additional JwtBearerOptions configuration if needed
               // e.g. options.Audience = builder.Configuration["AzureAd:Audience"];
           });

   var app = builder.Build();
   app.UseAuthentication();
   app.UseAuthorization();
   app.MapControllers();
   app.Run();
   ```

3. **appsettings.json** example:
   ```json
   {
     "AzureAd": {
       "Instance": "https://login.microsoftonline.com/",
       "Domain": "yourtenant.onmicrosoft.com",
       "TenantId": "<TENANT_ID>",
       "ClientId": "<API_CLIENT_ID>",
       "Audience": "<API_CLIENT_ID>"
     },
     "ProtectedResource": {
       "ValidateSignature": true,
       "JwksRefreshIntervalHours": 24
     }
   }
   ```

4. After this setup, any 401 responses from your API will include:
   ```
   WWW-Authenticate: Bearer resource_metadata="https://api.yourdomain.com/.well-known/oauth-protected-resource"
   ```
