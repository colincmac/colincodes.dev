namespace Showcase.Authentication.AspNetCore.ResourceServer.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ProtectedResourceAttribute : Attribute, IProtectedResourceAuthorizationMetadata
{
    /// <summary>
    /// Scopes accepted by this web API.
    /// </summary>
    public string[]? RequiredScopes { get; set; }
}
