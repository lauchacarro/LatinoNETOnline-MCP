using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.AspNetCore.Authentication;


using ProtectedMcpServer.Tools;
using System.Net.Http.Headers;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
var serverUrl = builder.Configuration["ServerConfiguration:BaseUrl"] ?? "http://localhost:7071/";
var oAuthServerUrl = builder.Configuration["ServerConfiguration:OAuthServerUrl"] ?? "https://ids.latinonet.online";

// Ensure serverUrl ends with /
if (!serverUrl.EndsWith("/"))
{
    serverUrl += "/";
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = oAuthServerUrl;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = serverUrl, // Validate that the audience matches the resource metadata as suggested in RFC 8707
        ValidIssuer = oAuthServerUrl,
        NameClaimType = "name",
        RoleClaimType = "roles"
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
 
            Console.WriteLine($"Token validated!");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"Challenging client to authenticate with Entra ID");
            return Task.CompletedTask;
        }
    };
})
.AddMcp(options =>
{
    options.ResourceMetadata = new()
    {
        ResourceName = "Latino .Net Online MCP",
        Resource = new Uri(serverUrl),
        AuthorizationServers = { new Uri(oAuthServerUrl) },
        ScopesSupported = ["latinonetonline_api", "openid", "profile"],
    };
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

// Configure forwarded headers for cloud deployment
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddMcpServer()
    .WithTools<WebinarTools>()
    .WithHttpTransport();



// Configure HttpClientFactory for LatinoNet Webinar API
var webinarConfig = builder.Configuration.GetSection("ExternalApis:WebinarApi");
builder.Services.AddHttpClient("WebinarApi", client =>
{
    client.BaseAddress = new Uri(webinarConfig["BaseUrl"] ?? "https://api.latinonet.online");

    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    
    if (int.TryParse(webinarConfig["TimeoutSeconds"], out var webinarTimeout))
    {
        client.Timeout = TimeSpan.FromSeconds(webinarTimeout);
    }
});

var app = builder.Build();

// Configure for cloud deployment
if (!builder.Environment.IsDevelopment())
{
    app.UseForwardedHeaders();
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();

// Use the default MCP policy name that we've configured
app.MapMcp().RequireAuthorization();

// Add health check endpoint for cloud deployment
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow })).AllowAnonymous();

Console.WriteLine($"Starting MCP server with authorization at {serverUrl}");
Console.WriteLine($"Using Identity Server (OpenID Connect) at {oAuthServerUrl}");
Console.WriteLine($"Protected Resource Metadata URL: {serverUrl}.well-known/oauth-protected-resource");
Console.WriteLine("Press Ctrl+C to stop the server");

// Use PORT environment variable for cloud deployment, fallback to configured URL
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    app.Run($"http://0.0.0.0:{port}");
}
else
{
    app.Run(serverUrl);
}
