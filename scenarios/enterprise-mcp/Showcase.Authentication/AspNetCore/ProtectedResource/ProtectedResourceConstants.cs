using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showcase.Authentication.AspNetCore.ProtectedResource;
public static class ProtectedResourceConstants
{
    public const string DefaultOAuthProtectedResourceRoute = "/.well-known/oauth-protected-resource/{resource}";
    
    public const string JsonWebKeySetRoute = "/.well-known/jwks";

    public static class WWWAuthenticateKeys
    {
        public const string ResourceMetadata = "resource_metadata";
    } 

}
