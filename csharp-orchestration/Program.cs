using Microsoft.AspNetCore.Mvc;
using AgenticOrchestration.Models;
using AgenticOrchestration.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Agentic AI Insurance Orchestration API", 
        Version = "v1",
        Description = "Multi-agent orchestration system for insurance claim processing"
    });
});

// Configure Azure settings
builder.Services.Configure<AzureConfiguration>(
    builder.Configuration.GetSection("Azure"));

// Register services
builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<IAzureServices, AzureServices>();
builder.Services.AddScoped<ICosmosDbService, CosmosDbService>();
builder.Services.AddScoped<IAzureSearchService, AzureSearchService>();
builder.Services.AddScoped<IOrchestrationService, OrchestrationService>();

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
    });
}

app.UseCors();
app.UseHttpsRedirection();

// API endpoints
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
.WithTags("Orchestration")
.WithSummary("Analyze Insurance Claim")
.WithDescription("Orchestrates multiple AI agents to analyze an insurance claim comprehensively");

app.MapGet("/api/health", () => 
{
    return Results.Ok(new HealthResponse
    { 
        Status = "Healthy", 
        Service = "Agentic AI Orchestration",
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
.WithTags("Orchestration")
.WithSummary("Get Sample Request")
.WithDescription("Returns a sample orchestration request for testing purposes");

app.MapGet("/", () => Results.Redirect("/swagger"));

Console.WriteLine("🚀 Agentic AI Insurance Orchestration API");
Console.WriteLine("=====================================");
Console.WriteLine("Orchestrating 3 specialized AI agents:");
Console.WriteLine("1. 🔍 Claim Reviewer Agent");
Console.WriteLine("2. ⚠️  Risk Analyzer Agent"); 
Console.WriteLine("3. 📋 Policy Checker Agent");
Console.WriteLine("=====================================");
Console.WriteLine($"🌐 Swagger UI: Navigate to the displayed URL");

await app.RunAsync();