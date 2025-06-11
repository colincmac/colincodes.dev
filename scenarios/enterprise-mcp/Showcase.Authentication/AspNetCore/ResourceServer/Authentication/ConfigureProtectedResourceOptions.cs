using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Showcase.Authentication.Core;


namespace Showcase.Authentication.AspNetCore.ResourceServer.Authentication;
internal sealed class ConfigureProtectedResourceOptions : IPostConfigureOptions<ProtectedResourceOptions>
{

    public void PostConfigure(string? name, ProtectedResourceOptions options)
    {
        ArgumentNullException.ThrowIfNull(name);
        var meta = options.Metadata;

        if (options.SigningKeyType is not ProtectedResourceMetadataSigningKeyType.None && meta.JwksUri is null)
        {
            meta.JwksUri = meta.Resource switch
            {
                { IsAbsoluteUri: true } => new Uri(meta.Resource, options.JwksDocumentPath),
                _ => options.JwksDocumentPath
            };
        }

    }
}
