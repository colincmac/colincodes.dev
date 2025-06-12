//using Microsoft.AspNetCore.DataProtection;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Identity.Client;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http.Headers;
//using System.Reflection;
//using System.Text;
//using System.Text.Json;
//using System.Threading;
//using System.Threading.Tasks;

//namespace TestApp;
//public static class EntraIntegrationImpl
//{
//    public static async Task Run()
//    {
//        var config = new ConfigurationBuilder()
//         .AddJsonFile("appsettings.Development.json", optional: false)
//         .AddEnvironmentVariables()
//         .Build();

//        string tenantId = config["AzureAd:TenantId"];
//        string clientId = config["AzureAd:ClientId"];
//        string clientSecret = config["AzureAd:ClientSecret"];
//        string apiUrl = "https://localhost:5001/api/values";

//        ConfidentialClientApplicationBuilder builder = ConfidentialClientApplicationBuilder
//            .Create(clientId)
//            .WithClientSecret(clientSecret)
//            .WithAuthority($"https://login.microsoftonline.com/{tenantId}").with;
//        using var httpClient = new HttpClient();
//        var initialResponse = await httpClient.GetAsync(apiUrl);
//        if (initialResponse.StatusCode != System.Net.HttpStatusCode.Unauthorized)
//        {
//            Console.WriteLine($"Unexpected status: {initialResponse.StatusCode}");
//            return;
//        }

//        // 4. Parse challenge to extract authority & scopes
//        //    This will send an unauthenticated request and read WWW-Authenticate
//        var authParams = await AuthenticationHeaderParser.ParseAuthenticationHeadersAsync(apiUrl, httpClient, CancellationToken.None);

//        string authority = authParams.Authority;
//        string[] scopes = authParams.Scopes;       // e.g. ["api://{resource}/.default"] :contentReference[oaicite:2]{index=2}

//        Console.WriteLine("Entra Integration");
//        Console.WriteLine();

//        var authority = $"https://login.microsoftonline.com/{tenantId}";

//        using var http = new HttpClient();
//        var authParams = await AuthenticationHeaderParser
//            .ParseAuthenticationHeadersAsync(resourceUrl, httpClient, cancellationToken);

//        var app = ConfidentialClientApplicationBuilder
//            .Create(clientId)
//            .WithAuthority(authParams.Authority)
//            .WithClientSecret(secret)
//            .Build();

//        // 1. Unauthenticated call
//        var initialResponse = await http.GetAsync(apiUrl);
//        if (initialResponse.StatusCode != System.Net.HttpStatusCode.Unauthorized)
//        {
//            Console.WriteLine("Expected 401; received " + initialResponse.StatusCode);
//            return;
//        }

//        // 2. Parse WWW-Authenticate header
//        var wwwAuth = initialResponse.Headers.WwwAuthenticate.FirstOrDefault();
//        Console.WriteLine("WWW-Authenticate: " + wwwAuth);
//        var metadataPart = wwwAuth!.Parameter.Split('=').Last().Trim('"');
//        Console.WriteLine("Resource metadata URL: " + metadataPart);

//        // 3. Fetch metadata
//        var metadataJson = await http.GetStringAsync(metadataPart);
//        using var doc = JsonDocument.Parse(metadataJson);
//        var resource = doc.RootElement.GetProperty("resource").GetString()!;
//        Console.WriteLine("Resource identifier: " + resource);

//        // 4. Acquire token
//        var app = ConfidentialClientApplicationBuilder
//            .Create(clientId)
//            .WithClientSecret(clientSecret)
//            .WithAuthority(authority)
//            .Build();
//        var scope = $"{resource}/.default";
//        var result = await app.AcquireTokenForClient(new[] { scope }).ExecuteAsync();
//        Console.WriteLine("Access token acquired.");

//        // 5. Call API with token
//        http.DefaultRequestHeaders.Authorization =
//            new AuthenticationHeaderValue("Bearer", result.AccessToken);
//        var apiResponse = await http.GetAsync(apiUrl);
//        Console.WriteLine("API response: " + await apiResponse.Content.ReadAsStringAsync());
//    }
//}
