using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using AgenticOrchestration.Models;
using AgenticOrchestration.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

// Configure Azure AD Authentication
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
 .EnableTokenAcquisitionToCallDownstreamApi()
 .AddInMemoryTokenCaches();

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Agentic AI Insurance Orchestration API", 
        Version = "v1",
 Description = "Multi-agent orchestration system for insurance claim processing - Mock & Real Azure AI with Authentication"
    });

    // Add Azure AD OAuth2 configuration to Swagger
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
   Flows = new OpenApiOAuthFlows
        {
   AuthorizationCode = new OpenApiOAuthFlow
      {
   AuthorizationUrl = new Uri("https://login.microsoftonline.com/f46bbd44-80be-4615-a057-66c7caaaad3d/oauth2/v2.0/authorize"),
       TokenUrl = new Uri("https://login.microsoftonline.com/f46bbd44-80be-4615-a057-66c7caaaad3d/oauth2/v2.0/token"),
    Scopes = new Dictionary<string, string>
       {
      { "https://cognitiveservices.azure.com/.default", "Access Azure Cognitive Services" }
  }
       }
        }
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
   {
        {
    new OpenApiSecurityScheme
    {
             Reference = new OpenApiReference
        {
       Type = ReferenceType.SecurityScheme,
    Id = "oauth2"
     }
     },
     new[] { "https://cognitiveservices.azure.com/.default" }
        }
    });
});

// Configure Azure settings
builder.Services.Configure<AzureConfiguration>(
    builder.Configuration.GetSection("Azure"));

// Register MOCK services (existing)
builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<IAzureServices, AzureServices>();
builder.Services.AddScoped<ICosmosDbService, CosmosDbService>();
builder.Services.AddScoped<IAzureSearchService, AzureSearchService>();
builder.Services.AddScoped<IOrchestrationService, OrchestrationService>();

// Register REAL Azure AI services
builder.Services.AddScoped<IRealAgentService, RealAgentService>();
builder.Services.AddScoped<IRealOrchestrationService, RealOrchestrationService>();

// Register AUTHENTICATED Azure AI services (new)
builder.Services.AddScoped<IUserAzureCredentialService, UserAzureCredentialService>();
builder.Services.AddScoped<IAuthenticatedAgentService, AuthenticatedAgentService>();

// Add logging
builder.Services.AddLogging();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
        .AllowAnyMethod()
     .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
 {
 c.SwaggerEndpoint("/swagger/v1/swagger.json", "Agentic AI Orchestration API v1");
        c.RoutePrefix = string.Empty; // Makes Swagger UI available at root
        
        // Configure OAuth2 for Swagger
 c.OAuthClientId("00000000-0000-0000-0000-000000000000"); // Replace with your client ID
        c.OAuthUsePkce();
    c.OAuthScopeSeparator(" ");
 c.OAuthScopes("https://cognitiveservices.azure.com/.default");
    });
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// MOCK API endpoint (existing - no auth required)
app.MapPost("/api/orchestration/analyze-claim", async (
    [FromBody] OrchestrationRequest request,
    IOrchestrationService orchestrationService) =>
{
    try
 {
        if (string.IsNullOrWhiteSpace(request.ClaimId))
        {
  return Results.BadRequest("ClaimId is required");
     }

        if (string.IsNullOrWhiteSpace(request.PolicyNumber))
        {
        return Results.BadRequest("PolicyNumber is required");
     }

        var response = await orchestrationService.RunInsuranceClaimOrchestrationAsync(request);
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
    return Results.Problem($"Internal server error: {ex.Message}");
    }
})
.WithName("AnalyzeClaim")
.WithTags("Mock Orchestration")
.WithSummary("Analyze Insurance Claim (Mock)")
.WithDescription("Orchestrates multiple MOCK AI agents to analyze an insurance claim");

// REAL AZURE AI API endpoint (no auth required - uses service credentials)
app.MapPost("/api/orchestration/analyze-claim-real", async (
    [FromBody] OrchestrationRequest request,
    IRealOrchestrationService realOrchestrationService) =>
{
    try
    {
   if (string.IsNullOrWhiteSpace(request.ClaimId))
      {
        return Results.BadRequest("ClaimId is required");
        }

        if (string.IsNullOrWhiteSpace(request.PolicyNumber))
        {
   return Results.BadRequest("PolicyNumber is required");
    }

     var response = await realOrchestrationService.RunInsuranceClaimOrchestrationAsync(request);
      return Results.Ok(response);
    }
    catch (Exception ex)
    {
return Results.Problem($"Real Azure AI error: {ex.Message}");
    }
})
.WithName("AnalyzeClaimReal")
.WithTags("Real Azure AI Orchestration")
.WithSummary("Analyze Insurance Claim (Real Azure AI)")
.WithDescription("Orchestrates multiple REAL Azure AI Foundry agents using service credentials");

// AUTHENTICATED AZURE AI API endpoint (NEW - requires user authentication)
app.MapPost("/api/orchestration/analyze-claim-authenticated", [Authorize] async (
    [FromBody] OrchestrationRequest request,
    IAuthenticatedAgentService authenticatedAgentService,
    HttpContext httpContext) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.ClaimId))
        {
 return Results.BadRequest("ClaimId is required");
        }

        if (string.IsNullOrWhiteSpace(request.PolicyNumber))
        {
            return Results.BadRequest("PolicyNumber is required");
     }

      // Prepare data for agents
        var claimData = new
        {
            ClaimId = request.ClaimId,
   PolicyNumber = request.PolicyNumber,
            ClaimDate = request.ClaimDate,
       ClaimAmount = request.ClaimAmount,
      Description = request.Description,
     AdditionalData = request.AdditionalData,
      Source = "API Request",
ProcessingType = "Authenticated Azure AI Analysis",
            Timestamp = DateTime.UtcNow,
   AuthenticatedUser = httpContext.User.Identity?.Name
 };

        var policyDocuments = new List<string>
        {
        $"Policy Number: {request.PolicyNumber}",
     "Coverage Type: Comprehensive Auto Insurance",
      "Deductible: $500",
            "Coverage Limit: $50,000",
            "Premium: $1,200 annually",
  "Effective Date: 2024-01-01",
            "Expiration Date: 2024-12-31"
        };

        // Execute all three agents concurrently using user authentication
        var tasks = new[]
   {
            authenticatedAgentService.ExecuteClaimReviewerAsync(request.ClaimId, claimData, httpContext),
 authenticatedAgentService.ExecuteRiskAnalyzerAsync(request.ClaimId, claimData, httpContext),
   authenticatedAgentService.ExecutePolicyCheckerAsync(request.PolicyNumber, policyDocuments, httpContext)
 };

     var results = await Task.WhenAll(tasks);

        var response = new OrchestrationResponse
    {
     ClaimId = request.ClaimId,
    PolicyNumber = request.PolicyNumber,
            ClaimReviewerResult = results[0],
            RiskAnalyzerResult = results[1],
 PolicyCheckerResult = results[2],
     ExecutionTimeMs = 0, // Will be calculated by the individual agents
  Timestamp = DateTime.UtcNow,
 Summary = $"🔐 AUTHENTICATED Azure AI Analysis completed for user: {httpContext.User.Identity?.Name}\n\nAll agents executed using the authenticated user's Azure credentials."
        };

    return Results.Ok(response);
    }
  catch (Exception ex)
    {
   return Results.Problem($"Authenticated Azure AI error: {ex.Message}");
    }
})
.WithName("AnalyzeClaimAuthenticated")
.WithTags("Authenticated Azure AI Orchestration")
.WithSummary("Analyze Insurance Claim (User Authenticated)")
.WithDescription("Orchestrates multiple Azure AI agents using the authenticated user's Azure credentials")
.RequireAuthorization();

// Authentication endpoints
app.MapGet("/api/auth/login", () => 
{
    return Results.Challenge(
        properties: new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
            RedirectUri = "/"
        },
      authenticationSchemes: new[] { OpenIdConnectDefaults.AuthenticationScheme });
})
.WithName("Login")
.WithTags("Authentication")
.WithSummary("Login with Azure AD")
.WithDescription("Redirects to Azure AD login");

app.MapGet("/api/auth/logout", [Authorize] () => 
{
    return Results.SignOut(
        properties: new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
            RedirectUri = "/"
        },
        authenticationSchemes: new[] { OpenIdConnectDefaults.AuthenticationScheme });
})
.WithName("Logout")
.WithTags("Authentication")
.WithSummary("Logout from Azure AD")
.WithDescription("Signs out from Azure AD")
.RequireAuthorization();

app.MapGet("/api/auth/user", [Authorize] (HttpContext httpContext) => 
{
    var userInfo = new
    {
        IsAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false,
        Name = httpContext.User.Identity?.Name,
        Claims = httpContext.User.Claims.Select(c => new { c.Type, c.Value }).ToList()
    };
    return Results.Ok(userInfo);
})
.WithName("GetUserInfo")
.WithTags("Authentication")
.WithSummary("Get Current User Info")
.WithDescription("Returns information about the currently authenticated user")
.RequireAuthorization();

app.MapGet("/api/health", () => 
{
    return Results.Ok(new HealthResponse
  { 
 Status = "Healthy", 
        Service = "Agentic AI Orchestration - Mock, Real & Authenticated",
        Timestamp = DateTime.UtcNow,
   Version = "1.0.0"
    });
})
.WithName("HealthCheck")
.WithTags("Health");

app.MapGet("/api/orchestration/sample-request", () =>
{
var sampleRequest = new OrchestrationRequest
    {
        ClaimId = "CLM-2024-001",
  PolicyNumber = "POL-AUTO-12345",
        ClaimDate = DateTime.UtcNow.AddDays(-7),
        ClaimAmount = 15000.00m,
     Description = "Vehicle collision with significant front-end damage. Driver reports accident occurred during morning commute on Interstate 95."
 };

    return Results.Ok(sampleRequest);
})
.WithName("GetSampleRequest")
.WithTags("Sample Data")
.WithSummary("Get Sample Request")
.WithDescription("Returns a sample orchestration request for testing all endpoints");

// Configuration info endpoint
app.MapGet("/api/config/azure-ai-status", (IOptions<AzureConfiguration> config) =>
{
    var azureConfig = config.Value;
    var status = new
    {
     HasAzureAIEndpoint = !string.IsNullOrWhiteSpace(azureConfig.AI_FOUNDRY_PROJECT_ENDPOINT),
        HasModelDeployment = !string.IsNullOrWhiteSpace(azureConfig.MODEL_DEPLOYMENT_NAME),
      HasAzureAdConfig = !string.IsNullOrWhiteSpace(azureConfig.AZURE_TENANT_ID),
        Endpoint = string.IsNullOrWhiteSpace(azureConfig.AI_FOUNDRY_PROJECT_ENDPOINT) ? 
            "Not configured" : 
    azureConfig.AI_FOUNDRY_PROJECT_ENDPOINT,
    ModelDeployment = string.IsNullOrWhiteSpace(azureConfig.MODEL_DEPLOYMENT_NAME) ? 
    "Not configured" : 
    azureConfig.MODEL_DEPLOYMENT_NAME,
        TenantId = string.IsNullOrWhiteSpace(azureConfig.AZURE_TENANT_ID) ? 
    "Not configured" : 
    azureConfig.AZURE_TENANT_ID,
     ReadyForRealAI = !string.IsNullOrWhiteSpace(azureConfig.AI_FOUNDRY_PROJECT_ENDPOINT) && 
            !string.IsNullOrWhiteSpace(azureConfig.MODEL_DEPLOYMENT_NAME),
        ReadyForAuthenticatedAI = !string.IsNullOrWhiteSpace(azureConfig.AI_FOUNDRY_PROJECT_ENDPOINT) && 
        !string.IsNullOrWhiteSpace(azureConfig.MODEL_DEPLOYMENT_NAME) &&
   !string.IsNullOrWhiteSpace(azureConfig.AZURE_TENANT_ID),
   Instructions = "Configure Azure AD app registration for authenticated AI endpoints"
    };

    return Results.Ok(status);
})
.WithName("AzureAIStatus")
.WithTags("Configuration")
.WithSummary("Check Azure AI Configuration")
.WithDescription("Returns the current Azure AI Foundry and authentication configuration status");

app.MapGet("/", () => Results.Redirect("/swagger"));

Console.WriteLine("🚀 Agentic AI Insurance Orchestration API with Authentication");
Console.WriteLine("=============================================================");
Console.WriteLine("Available endpoints:");
Console.WriteLine("📋 MOCK: /api/orchestration/analyze-claim");
Console.WriteLine("🤖 REAL: /api/orchestration/analyze-claim-real");
Console.WriteLine("🔐 AUTH: /api/orchestration/analyze-claim-authenticated");
Console.WriteLine("🔑 LOGIN: /api/auth/login");
Console.WriteLine("🚪 LOGOUT: /api/auth/logout");
Console.WriteLine("👤 USER: /api/auth/user");
Console.WriteLine("⚙️  CONFIG: /api/config/azure-ai-status");
Console.WriteLine("=============================================================");
Console.WriteLine("Orchestrating AI agents:");
Console.WriteLine("1. 🔍 Claim Reviewer Agent");
Console.WriteLine("2. ⚠️  Risk Analyzer Agent"); 
Console.WriteLine("3. 📋 Policy Checker Agent");
Console.WriteLine("=============================================================");
Console.WriteLine($"🌐 Swagger UI: Navigate to the displayed URL");
Console.WriteLine($"🔐 Authentication: Login via Azure AD for authenticated endpoints");

await app.RunAsync();