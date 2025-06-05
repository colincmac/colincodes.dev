using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showcase.Authentication.AspNetCore.ResourceServer.Authorization;
public interface IProtectedResourceAuthorizationMetadata
{
    public string[]? RequiredScopes { get; }
}
