using Microsoft.Extensions.Options;
using Showcase.Authentication.Core;


namespace Showcase.Authentication.AspNetCore.ResourceServer.Authentication;
internal sealed class ConfigureProtectedResourceOptions : IPostConfigureOptions<ProtectedResourceOptions>
{

    public void PostConfigure(string? name, ProtectedResourceOptions options)
    {
        ArgumentNullException.ThrowIfNull(name);
        var meta = options.Metadata;

        if (options.SigningKeyType is not ProtectedResourceMetadataSigningKeyType.None
            && meta.JwksUri is null)
        {
            meta.JwksUri = meta.Resource switch
            {
                { IsAbsoluteUri: true, AbsolutePath: var absolutePath } when absolutePath != "/" => new Uri(new Uri($"${meta.Resource.Scheme}://{meta.Resource.Host}{ProtectedResourceConstants.JsonWebKeySetPathSuffix}"), absolutePath),
                { IsAbsoluteUri: true } => new Uri(meta.Resource, ProtectedResourceConstants.JsonWebKeySetPathSuffix),
                _ => new Uri(ProtectedResourceConstants.JsonWebKeySetPathSuffix, UriKind.Relative)
            };
        }
    }
}
