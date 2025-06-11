using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Showcase.Authentication.Core;


namespace Showcase.Authentication.AspNetCore.ResourceServer.Authentication;
internal sealed class ConfigureProtectedResourceOptions : IPostConfigureOptions<ProtectedResourceOptions>
{
    public string? Scheme { get; set; }

    public void PostConfigure(string? name, ProtectedResourceOptions options)
    {
        if(options.Metadata is null)
        {

        }
    }

}
