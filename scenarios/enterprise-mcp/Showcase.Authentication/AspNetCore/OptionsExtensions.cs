using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showcase.Authentication.AspNetCore;
internal static class OptionsExtensions
{    
    
    /// <summary>
     /// Gets the options for the given service key, or the current value if the service key is <see langword="null"/>.
     /// </summary>
     /// <typeparam name="TOptions">The options type.</typeparam>
     /// <param name="optionsMonitor">The <see cref="IOptionsMonitor{TOptions}"/> to load the options object from.</param>
     /// <param name="serviceKey">An optional string that specifies the name of the options object to get.</param>
     /// <returns>The <typeparamref name="TOptions"/> instance.</returns>
    public static TOptions GetKeyedOrCurrent<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions>(
        this IOptionsMonitor<TOptions> optionsMonitor, string? serviceKey)
        where TOptions : class
    {
        if (serviceKey is null)
        {
            return optionsMonitor.CurrentValue;
        }

        return optionsMonitor.Get(serviceKey);
    }
}
