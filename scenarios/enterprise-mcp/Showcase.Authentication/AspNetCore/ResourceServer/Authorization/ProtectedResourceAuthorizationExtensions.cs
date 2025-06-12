using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Showcase.Authentication.AspNetCore.ResourceServer.Authorization;
public static class ProtectedResourceAuthorizationExtensions
{
    /// <summary>
    /// This method adds support for the required scope attribute. It adds a default policy that
    /// adds a scope requirement. This requirement looks for IAuthRequiredScopeMetadata on the current endpoint.
    /// </summary>
    /// <param name="services">The services being configured.</param>
    /// <returns>Services.</returns>
    public static IServiceCollection AddProtectedResourceAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();

        //services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<AuthorizationOptions>, RequireScopeOptions>());
        //services.TryAddEnumerable(ServiceDescriptor.Singleton<IAuthorizationHandler, ScopeAuthorizationHandler>());
        return services;
    }

    /// <summary>
    /// This method adds metadata to route endpoint to describe required scopes. It's the imperative version of
    /// the [RequiredScope] attribute.
    /// </summary>
    /// <typeparam name="TBuilder">Class implementing <see cref="IEndpointConventionBuilder"/>.</typeparam>
    /// <param name="endpointConventionBuilder">To customize the endpoints.</param>
    /// <param name="scope">Scope.</param>
    /// <returns>Builder.</returns>
    public static TBuilder RequireScope<TBuilder>(this TBuilder endpointConventionBuilder, params string[] scope)
        where TBuilder : IEndpointConventionBuilder
    {
        return endpointConventionBuilder.WithMetadata(new ProtectedResourceEndpointMetadata(scope));
    }

    private sealed class ProtectedResourceEndpointMetadata(string[]? scope) : IProtectedResourceAuthorizationMetadata
    {
        public string[]? RequiredScopes { get; } = scope;
    }
}
