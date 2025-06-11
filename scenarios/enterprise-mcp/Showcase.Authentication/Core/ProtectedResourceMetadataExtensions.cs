using Microsoft.Identity.Web;
using Microsoft.IdentityModel.JsonWebTokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Showcase.Authentication.Core;
public static class ProtectedResourceMetadataExtensions
{
    public static Claim[] ToClaims(this ProtectedResourceMetadata meta)
    {
        if (meta is null) throw new ArgumentNullException(nameof(meta));

        var claims = new List<Claim>();
        var props = typeof(ProtectedResourceMetadata)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props)
        {
            var rawValue = prop.GetValue(meta);
            if (rawValue == null) continue;

            // figure out the claim type/name
            var jsonAttr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
            var claimType = jsonAttr?.Name ?? prop.Name;

            switch (rawValue)
            {
                // handle lists of strings
                //case IEnumerable<string> stringList:
                //    foreach (var s in stringList.Where(x => x != null))
                //        claims.Add(new Claim(claimType, s, JsonClaimValueTypes.JsonArray));
                //    break;
                // handle lists of strings
                case IEnumerable<object> jsonArray:
                    
                    claims.Add(new Claim(claimType, JsonSerializer.Serialize(jsonArray), JsonClaimValueTypes.JsonArray));
                    break;

                // handle lists of Uri
                //case IEnumerable<Uri> uriList:
                //    foreach (var u in uriList.Where(x => x != null))
                //        claims.Add(new Claim(claimType, u!.ToString()!));
                //    break;

                // handle single Uri
                case Uri u:
                    claims.Add(new Claim(claimType, u.ToString()));
                    break;

                // handle boolean
                case bool b:
                    claims.Add(new Claim(claimType, b.ToString().ToLowerInvariant()));
                    break;

                // everything else (int, string, enums, etc.)
                default:
                    claims.Add(new Claim(claimType, rawValue.ToString()));
                    break;
            }
        }

        return claims.ToArray();
    }

    public static Claim[] ToClaimsViaJson(this ProtectedResourceMetadata meta)
    {
        if (meta is null) throw new ArgumentNullException(nameof(meta));

        JsonNode? root = JsonSerializer.SerializeToNode(meta, JsonContext.Default.Options);

        if (root is not JsonObject obj)
            return [];

        if (meta.Resource is not null) obj[JwtRegisteredClaimNames.Iss] = meta.Resource.ToString();

        var claims = new List<Claim>();

        // 2) Walk every property
        foreach (var kv in obj)
        {
            string claimType = kv.Key;
            JsonNode? valueNode = kv.Value;
            if(claimType is null || valueNode is null)
                continue; // skip null values
            claims.Add(new Claim(claimType, valueNode.ToJsonString(JsonContext.Default.Options), JsonClaimValueTypes.JsonArray)); // Changed to add entire array as a single claim

            //if (valueNode is JsonArray arr)
            //{
            //}
            //else
            //{
            //    // single value (string, bool, number, nested object…)
            //    claims.Add(new Claim(claimType, valueNode?.ToJsonString() ?? ""));
            //}
        }

        return [.. claims];
    }
}