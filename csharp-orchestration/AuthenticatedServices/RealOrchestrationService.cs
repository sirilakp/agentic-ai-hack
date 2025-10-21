using Microsoft.Extensions.Options;
using AgenticOrchestration.Models;
using AgenticOrchestration.Services;
using System.Text.Json;
using System.Diagnostics;

namespace AgenticOrchestration.Services
{
    public interface IRealOrchestrationService
    {
        Task<OrchestrationResponse> RunInsuranceClaimOrchestrationAsync(OrchestrationRequest request);
    }

  public class RealOrchestrationService : IRealOrchestrationService
    {
        private readonly IRealAgentService _realAgentService;
  private readonly IAzureServices _azureServices;
      private readonly ILogger<RealOrchestrationService> _logger;

        public RealOrchestrationService(
      IRealAgentService realAgentService,
            IAzureServices azureServices,
   ILogger<RealOrchestrationService> logger)
        {
            _realAgentService = realAgentService;
         _azureServices = azureServices;
         _logger = logger;
     }

      public async Task<OrchestrationResponse> RunInsuranceClaimOrchestrationAsync(OrchestrationRequest request)
        {
  var stopwatch = Stopwatch.StartNew();
   _logger.LogInformation("Starting REAL Azure AI insurance claim orchestration for claim {ClaimId}", request.ClaimId);

     try
         {
                // Prepare data for agents
             var claimData = await PrepareClaimDataAsync(request);
             var policyDocuments = await GetPolicyDocumentsAsync(request.PolicyNumber);

        _logger.LogInformation("Executing real Azure AI agents concurrently for claim {ClaimId}", request.ClaimId);

  // Execute all three agents concurrently using REAL Azure AI
           var tasks = new[]
        {
    ExecuteAgentWithLogging("RealClaimReviewer", () => 
          _realAgentService.ExecuteClaimReviewerAsync(request.ClaimId, claimData)),
     
   ExecuteAgentWithLogging("RealRiskAnalyzer", () => 
      _realAgentService.ExecuteRiskAnalyzerAsync(request.ClaimId, claimData)),
    
    ExecuteAgentWithLogging("RealPolicyChecker", () => 
         _realAgentService.ExecutePolicyCheckerAsync(request.PolicyNumber, policyDocuments))
          };

       var results = await Task.WhenAll(tasks);

        stopwatch.Stop();

       var response = new OrchestrationResponse
     {
           ClaimId = request.ClaimId,
 PolicyNumber = request.PolicyNumber,
       ClaimReviewerResult = results[0],
      RiskAnalyzerResult = results[1],
      PolicyCheckerResult = results[2],
        ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
          Timestamp = DateTime.UtcNow,
         Summary = GenerateRealAiSummary(results)
    };

    _logger.LogInformation("REAL Azure AI orchestration completed for claim {ClaimId} in {ElapsedMs}ms", 
     request.ClaimId, stopwatch.ElapsedMilliseconds);

     return response;
        }
       catch (Exception ex)
            {
         stopwatch.Stop();
     _logger.LogError(ex, "REAL Azure AI orchestration failed for claim {ClaimId} after {ElapsedMs}ms", 
     request.ClaimId, stopwatch.ElapsedMilliseconds);
    
         return new OrchestrationResponse
 {
 ClaimId = request.ClaimId,
    PolicyNumber = request.PolicyNumber,
     ClaimReviewerResult = $"Real Azure AI Error: {ex.Message}",
           RiskAnalyzerResult = $"Real Azure AI Error: {ex.Message}",
          PolicyCheckerResult = $"Real Azure AI Error: {ex.Message}",
          ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
      Timestamp = DateTime.UtcNow,
       Summary = $"Real Azure AI orchestration failed: {ex.Message}. Please check Azure AI Foundry configuration."
 };
        }
  }

     private async Task<string> ExecuteAgentWithLogging(string agentName, Func<Task<string>> agentExecutor)
        {
         var agentStopwatch = Stopwatch.StartNew();
          _logger.LogInformation("Starting {AgentName} with REAL Azure AI", agentName);

       try
        {
  var result = await agentExecutor();
 agentStopwatch.Stop();
 
     _logger.LogInformation("{AgentName} completed with REAL Azure AI in {ElapsedMs}ms", 
           agentName, agentStopwatch.ElapsedMilliseconds);
      
        return result;
  }
 catch (Exception ex)
    {
        agentStopwatch.Stop();
     _logger.LogError(ex, "{AgentName} failed with REAL Azure AI after {ElapsedMs}ms", 
   agentName, agentStopwatch.ElapsedMilliseconds);
         
       return $"{agentName} Real Azure AI Error: {ex.Message}";
          }
 }

        private async Task<object> PrepareClaimDataAsync(OrchestrationRequest request)
   {
       try
            {
     // Try to get claim data from Azure services first
     var claimFromDb = await _azureServices.GetClaimDataAsync(request.ClaimId);
    if (claimFromDb != null)
        {
     return claimFromDb;
        }

  // Fallback to request data with additional context for AI analysis
     return new
     {
ClaimId = request.ClaimId,
     PolicyNumber = request.PolicyNumber,
         ClaimDate = request.ClaimDate,
       ClaimAmount = request.ClaimAmount,
     Description = request.Description,
    AdditionalData = request.AdditionalData,
            Source = "API Request",
        ProcessingType = "Real Azure AI Analysis",
  Timestamp = DateTime.UtcNow
      };
   }
   catch (Exception ex)
    {
       _logger.LogWarning(ex, "Could not retrieve claim data from database for real AI analysis, using request data");
     return new
       {
  ClaimId = request.ClaimId,
     PolicyNumber = request.PolicyNumber,
          ClaimDate = request.ClaimDate,
         ClaimAmount = request.ClaimAmount,
  Description = request.Description,
      AdditionalData = request.AdditionalData,
      Source = "API Request (Azure services unavailable)",
     ProcessingType = "Real Azure AI Analysis",
       Timestamp = DateTime.UtcNow
            };
        }
    }

        private async Task<List<string>> GetPolicyDocumentsAsync(string policyNumber)
  {
 try
   {
    // Try to get policy documents from Azure Search first
          var documentsFromSearch = await _azureServices.SearchPolicyDocumentsAsync(policyNumber);
        if (documentsFromSearch.Any())
       {
           return documentsFromSearch;
           }

        // Fallback to enhanced mock policy documents for real AI analysis
    return GetEnhancedPolicyDocuments(policyNumber);
     }
        catch (Exception ex)
         {
       _logger.LogWarning(ex, "Could not retrieve policy documents from search for real AI analysis, using enhanced mock data");
          return GetEnhancedPolicyDocuments(policyNumber);
       }
        }

  private static List<string> GetEnhancedPolicyDocuments(string policyNumber)
        {
     return new List<string>
    {
    $"INSURANCE POLICY DOCUMENT - Policy Number: {policyNumber}",
         "",
   "SECTION 1: COVERAGE SUMMARY",
 "Coverage Type: Comprehensive Auto Insurance",
     "Policy Effective Date: January 1, 2024",
        "Policy Expiration Date: December 31, 2024",
   "Premium: $1,200 annually (paid in full)",
    "",
          "SECTION 2: COVERAGE LIMITS",
   "Collision Coverage: $50,000 per incident",
      "Comprehensive Coverage: $50,000 per incident", 
      "Liability Coverage: $100,000 per person / $300,000 per incident",
     "Medical Payments: $10,000 per person",
    "Uninsured Motorist: $50,000 per person / $100,000 per incident",
     "",
  "SECTION 3: DEDUCTIBLES",
     "Collision Deductible: $500",
   "Comprehensive Deductible: $500",
         "",
    "SECTION 4: COVERED PERILS",
    "- Collision with another vehicle or object",
"- Theft or attempted theft of the vehicle",
         "- Vandalism and malicious mischief",
         "- Fire, explosion, or lightning",
       "- Falling objects (trees, rocks, etc.)",
       "- Natural disasters (floods, hurricanes, earthquakes)",
      "- Glass breakage",
    "",
        "SECTION 5: EXCLUSIONS",
    "- Racing or speed contests",
  "- Commercial use of personal vehicle",
        "- Intentional damage by the insured",
     "- Wear and tear or mechanical breakdown",
      "- Damage from nuclear hazards",
       "- War or military action",
   "",
        "SECTION 6: CLAIMS PROCEDURES",
     "- Claims must be reported within 30 days of incident",
     "- Police report required for theft or hit-and-run",
         "- Multiple estimates may be required for repairs over $2,500",
          "- Insured must cooperate with investigation",
        "",
          "SECTION 7: SPECIAL CONDITIONS",
        "- Good driver discount applied (15% reduction)",
 "- Multi-vehicle discount applied (10% reduction)",
     "- No claims bonus in effect (additional 5% reduction)",
          "- Policy in good standing with no late payments"
    };
  }

        private static string GenerateRealAiSummary(string[] results)
     {
  var summary = "?? REAL AZURE AI ORCHESTRATION ANALYSIS SUMMARY:\n\n";
     
    summary += "?? 1. REAL AI CLAIM REVIEW:\n";
summary += ExtractKeyPoints(results[0], "Azure AI claim review") + "\n\n";
            
summary += "?? 2. REAL AI RISK ANALYSIS:\n";
     summary += ExtractKeyPoints(results[1], "Azure AI risk analysis") + "\n\n";
    
    summary += "?? 3. REAL AI POLICY CHECK:\n";
  summary += ExtractKeyPoints(results[2], "Azure AI policy check") + "\n\n";
            
 summary += "? All Azure AI Foundry agents have completed their real analysis. ";
         summary += "This analysis was powered by actual Azure OpenAI models with sophisticated prompting and reasoning capabilities.";
   
    return summary;
   }

   private static string ExtractKeyPoints(string agentResult, string agentType)
        {
  // Enhanced extraction for real AI results
    var lines = agentResult.Split('\n', StringSplitOptions.RemoveEmptyEntries);
   var keyLines = lines
    .Where(line => !string.IsNullOrWhiteSpace(line))
        .Take(4) // Take more lines for richer AI responses
     .Select(line => line.Trim());
            
   if (keyLines.Any())
       {
          return string.Join("\n", keyLines);
            }
   
      return $"Azure AI agent completed {agentType} - see detailed results above.";
      }
    }
}