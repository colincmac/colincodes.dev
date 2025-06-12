namespace Showcase.Authentication.Core;
public static class ProtectedResourceConstants
{
    public const string DefaultOAuthProtectedResourcePathSuffix = "/.well-known/oauth-protected-resource";

    public const string JsonWebKeySetPathSuffix = "/.well-known/jwks";

    public static class WWWAuthenticateKeys
    {
        public const string UnsignedResourceMetadata = "resource_metadata";
        public const string SignedResourceMetadata = "signed_metadata";
    }
    public static class AuthenticationSchemes
    {
        public const string DPoP = "DPoP";
    }
}
