using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Showcase.Authentication.AspNetCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showcase.Authentication.AspNetCore.ProtectedResource.Services;
public class ProtectedResourceAuthenticationHandler : IAuthenticationHandler //AuthenticationHandler<ProtectedResourceOptions>
{
    private readonly ProtectedResourceOptions _options;
    
    public ProtectedResourceAuthenticationHandler(ProtectedResourceOptions options)
    {
        _options = options;
    }

    public Task<AuthenticateResult> AuthenticateAsync()
    {
        // Implement authentication logic here
        throw new NotImplementedException();
    }

    public Task ChallengeAsync(AuthenticationProperties properties)
    {
        // Implement challenge logic here
        throw new NotImplementedException();
    }

    public Task ForbidAsync(AuthenticationProperties properties)
    {
        // Implement forbid logic here
        throw new NotImplementedException();
    }

    public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
    {
        throw new NotImplementedException();
    }
}
