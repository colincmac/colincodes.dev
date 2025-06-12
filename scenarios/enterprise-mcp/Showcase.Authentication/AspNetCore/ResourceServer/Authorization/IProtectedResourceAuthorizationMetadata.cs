namespace Showcase.Authentication.AspNetCore.ResourceServer.Authorization;
public interface IProtectedResourceAuthorizationMetadata
{
    public string[]? RequiredScopes { get; }
}
