using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Clean Agentic AI Insurance Orchestration API", 
        Version = "v1",
        Description = "Minimal multi-agent orchestration system demo"
    });
});

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
 c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clean Agentic AI Orchestration API v1");
     c.RoutePrefix = string.Empty;
    });
}

app.UseCors();
app.UseHttpsRedirection();

// Minimal claim analysis endpoint
app.MapPost("/api/orchestration/analyze-claim", async (
    [FromBody] CleanClaimRequest request) =>
{
    var startTime = DateTime.UtcNow;
    
    try
    {
if (string.IsNullOrWhiteSpace(request.ClaimId))
     {
   return Results.BadRequest("ClaimId is required");
        }

        Console.WriteLine($"?? Processing claim {request.ClaimId}...");

        // Simulate the three agents running concurrently
        var claimReviewerTask = SimulateClaimReviewer(request);
      var riskAnalyzerTask = SimulateRiskAnalyzer(request);
        var policyCheckerTask = SimulatePolicyChecker(request);

        var results = await Task.WhenAll(claimReviewerTask, riskAnalyzerTask, policyCheckerTask);

var endTime = DateTime.UtcNow;
        var executionTime = (long)(endTime - startTime).TotalMilliseconds;

        Console.WriteLine($"? Completed orchestration for claim {request.ClaimId} in {executionTime}ms");

   var response = new CleanClaimResponse
  {
            ClaimId = request.ClaimId,
            PolicyNumber = request.PolicyNumber,
       ClaimReviewerResult = results[0],
            RiskAnalyzerResult = results[1],
    PolicyCheckerResult = results[2],
            ExecutionTimeMs = executionTime,
 Timestamp = endTime,
Summary = $"All three agents completed analysis in {executionTime}ms. Clean orchestration demo complete."
        };

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Error processing claim {request.ClaimId}: {ex.Message}");
        return Results.Problem($"Internal server error: {ex.Message}");
    }
})
.WithName("AnalyzeClaim")
.WithTags("Clean Orchestration")
.WithSummary("Analyze Insurance Claim (Clean Demo)")
.WithDescription("Simple demonstration of multi-agent orchestration");

app.MapGet("/api/health", () => 
{
    return Results.Ok(new 
    { 
 Status = "Healthy", 
        Timestamp = DateTime.UtcNow,
        Environment = app.Environment.EnvironmentName,
        Version = "1.0.0",
        Description = "Clean Agentic AI Insurance Orchestration API"
    });
})
.WithName("HealthCheck")
.WithTags("Health");

app.MapGet("/api/orchestration/sample-request", () =>
{
    var sampleRequest = new CleanClaimRequest
    {
        ClaimId = "CLM-2024-001",
   PolicyNumber = "POL-AUTO-12345",
        ClaimDate = DateTime.UtcNow.AddDays(-7),
        ClaimAmount = 15000.00m,
     Description = "Vehicle collision with significant front-end damage."
    };

    return Results.Ok(sampleRequest);
})
.WithName("GetSampleRequest")
.WithTags("Sample Data");

app.MapGet("/", () => Results.Redirect("/swagger"));

Console.WriteLine("?? Clean Agentic AI Insurance Orchestration API");
Console.WriteLine("===============================================");
Console.WriteLine("Minimal demo of agentic orchestration");
Console.WriteLine("===============================================");

await app.RunAsync();

// Agent simulation methods
static async Task<string> SimulateClaimReviewer(CleanClaimRequest request)
{
    Console.WriteLine("?? Claim Reviewer Agent starting...");
    await Task.Delay(400);
    
    return $@"CLAIM REVIEW - {request.ClaimId}
STATUS: VALID
Amount: ${request.ClaimAmount:N2}
Date: {request.ClaimDate:yyyy-MM-dd}
Confidence: 90%";
}

static async Task<string> SimulateRiskAnalyzer(CleanClaimRequest request)
{
    Console.WriteLine("?? Risk Analyzer Agent starting...");
    await Task.Delay(300);
    
    return $@"RISK ANALYSIS - {request.ClaimId}
Risk Level: LOW
Score: 2/10
Recommendation: Approve";
}

static async Task<string> SimulatePolicyChecker(CleanClaimRequest request)
{
    Console.WriteLine("?? Policy Checker Agent starting...");
    await Task.Delay(250);
    
    return $@"POLICY CHECK - {request.PolicyNumber}
Status: COVERED
Deductible: $500
Coverage: Active";
}

// Simple model classes
public class CleanClaimRequest
{
    [JsonPropertyName("claim_id")]
    public required string ClaimId { get; set; }

    [JsonPropertyName("policy_number")]
    public required string PolicyNumber { get; set; }

    [JsonPropertyName("claim_date")]
    public DateTime ClaimDate { get; set; }

 [JsonPropertyName("claim_amount")]
    public decimal ClaimAmount { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class CleanClaimResponse
{
    [JsonPropertyName("claim_id")]
    public required string ClaimId { get; set; }

    [JsonPropertyName("policy_number")]
    public required string PolicyNumber { get; set; }

    [JsonPropertyName("claim_reviewer_result")]
    public string ClaimReviewerResult { get; set; } = string.Empty;

    [JsonPropertyName("risk_analyzer_result")]
    public string RiskAnalyzerResult { get; set; } = string.Empty;

    [JsonPropertyName("policy_checker_result")]
    public string PolicyCheckerResult { get; set; } = string.Empty;

    [JsonPropertyName("execution_time_ms")]
    public long ExecutionTimeMs { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;
}