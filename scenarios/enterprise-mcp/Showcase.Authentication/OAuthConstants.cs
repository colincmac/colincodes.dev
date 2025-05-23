using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showcase.Authentication;
public static class OAuthConstants
{
    public static class WellKnownUris
    {
        public const string OAuthProtectedResourceUri = "/.well-known/oauth-protected-resource";
    }

    public static class WWWAuthenticateKeys
    {
        public const string ResourceMetadata = "resource_metadata";
    } 

}
