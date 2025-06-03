using Microsoft.Extensions.DependencyInjection;
using Showcase.Authentication.AspNetCore.ProtectedResource.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showcase.Authentication.AspNetCore.ProtectedResource.Extensions;
public static class ProtectedResourceServiceCollectionExtensions
{
    public static IServiceCollection AddProtectedResource(this IServiceCollection services, string? hostedResource = null)
    {

        if(!string.IsNullOrEmpty(hostedResource))
        {
            services.AddSingleton(new NamedService<ProtectedResourceService>(hostedResource));
        }

        return services;
    }
}
