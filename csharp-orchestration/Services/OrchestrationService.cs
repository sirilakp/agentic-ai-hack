using Microsoft.Extensions.Options;
using AgenticOrchestration.Models;
using AgenticOrchestration.Services;
using System.Text.Json;
using System.Diagnostics;

namespace AgenticOrchestration.Services
{
    public interface IOrchestrationService
    {
        Task<OrchestrationResponse> RunInsuranceClaimOrchestrationAsync(OrchestrationRequest request);
    }

    public class OrchestrationService : IOrchestrationService
    {
        private readonly IAgentService _agentService;
        private readonly IAzureServices _azureServices;
        private readonly ILogger<OrchestrationService> _logger;

        public OrchestrationService(
            IAgentService agentService,
            IAzureServices azureServices,
            ILogger<OrchestrationService> logger)
        {
            _agentService = agentService;
            _azureServices = azureServices;
            _logger = logger;
        }

        public async Task<OrchestrationResponse> RunInsuranceClaimOrchestrationAsync(OrchestrationRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Starting insurance claim orchestration for claim {ClaimId}", request.ClaimId);

            try
            {
                // Prepare data for agents
                var claimData = await PrepareClaimDataAsync(request);
                var policyDocuments = await GetPolicyDocumentsAsync(request.PolicyNumber);

                // Execute all three agents concurrently
                var tasks = new[]
                {
                    ExecuteAgentWithLogging("ClaimReviewer", () => 
                        _agentService.ExecuteClaimReviewerAsync(request.ClaimId, claimData)),
                    
                    ExecuteAgentWithLogging("RiskAnalyzer", () => 
                        _agentService.ExecuteRiskAnalyzerAsync(request.ClaimId, claimData)),
                    
                    ExecuteAgentWithLogging("PolicyChecker", () => 
                        _agentService.ExecutePolicyCheckerAsync(request.PolicyNumber, policyDocuments))
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
                    Summary = GenerateSummary(results)
                };

                _logger.LogInformation("Orchestration completed for claim {ClaimId} in {ElapsedMs}ms", 
                    request.ClaimId, stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Orchestration failed for claim {ClaimId} after {ElapsedMs}ms", 
                    request.ClaimId, stopwatch.ElapsedMilliseconds);
                
                return new OrchestrationResponse
                {
                    ClaimId = request.ClaimId,
                    PolicyNumber = request.PolicyNumber,
                    ClaimReviewerResult = $"Error: {ex.Message}",
                    RiskAnalyzerResult = $"Error: {ex.Message}",
                    PolicyCheckerResult = $"Error: {ex.Message}",
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                    Timestamp = DateTime.UtcNow,
                    Summary = $"Orchestration failed: {ex.Message}"
                };
            }
        }

        private async Task<string> ExecuteAgentWithLogging(string agentName, Func<Task<string>> agentExecutor)
        {
            var agentStopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Starting {AgentName} agent", agentName);

            try
            {
                var result = await agentExecutor();
                agentStopwatch.Stop();
                
                _logger.LogInformation("{AgentName} completed in {ElapsedMs}ms", 
                    agentName, agentStopwatch.ElapsedMilliseconds);
                
                return result;
            }
            catch (Exception ex)
            {
                agentStopwatch.Stop();
                _logger.LogError(ex, "{AgentName} failed after {ElapsedMs}ms", 
                    agentName, agentStopwatch.ElapsedMilliseconds);
                
                return $"{agentName} Error: {ex.Message}";
            }
        }

        private async Task<object> PrepareClaimDataAsync(OrchestrationRequest request)
        {
            try
            {
                // Try to get claim data from Cosmos DB first
                var claimFromDb = await _azureServices.GetClaimDataAsync(request.ClaimId);
                if (claimFromDb != null)
                {
                    return claimFromDb;
                }

                // Fallback to request data
                return new
                {
                    ClaimId = request.ClaimId,
                    PolicyNumber = request.PolicyNumber,
                    ClaimDate = request.ClaimDate,
                    ClaimAmount = request.ClaimAmount,
                    Description = request.Description,
                    AdditionalData = request.AdditionalData,
                    Source = "API Request"
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not retrieve claim data from database, using request data");
                return new
                {
                    ClaimId = request.ClaimId,
                    PolicyNumber = request.PolicyNumber,
                    ClaimDate = request.ClaimDate,
                    ClaimAmount = request.ClaimAmount,
                    Description = request.Description,
                    AdditionalData = request.AdditionalData,
                    Source = "API Request (DB unavailable)"
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

                // Fallback to mock policy documents
                return GetMockPolicyDocuments(policyNumber);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not retrieve policy documents from search, using mock data");
                return GetMockPolicyDocuments(policyNumber);
            }
        }

        private static List<string> GetMockPolicyDocuments(string policyNumber)
        {
            return new List<string>
            {
                $"Policy Number: {policyNumber}",
                "Coverage Type: Comprehensive Auto Insurance",
                "Deductible: $500",
                "Coverage Limit: $50,000",
                "Premium: $1,200 annually",
                "Effective Date: 2024-01-01",
                "Expiration Date: 2024-12-31",
                "Covered Perils: Collision, Comprehensive, Liability",
                "Exclusions: Racing, Commercial Use, Intentional Damage",
                "Special Conditions: Must report claims within 30 days"
            };
        }

        private static string GenerateSummary(string[] results)
        {
            var summary = "Orchestration Analysis Summary:\n\n";
            
            summary += "1. CLAIM REVIEW:\n";
            summary += ExtractKeyPoints(results[0], "claim review") + "\n\n";
            
            summary += "2. RISK ANALYSIS:\n";
            summary += ExtractKeyPoints(results[1], "risk analysis") + "\n\n";
            
            summary += "3. POLICY CHECK:\n";
            summary += ExtractKeyPoints(results[2], "policy check") + "\n\n";
            
            summary += "All agents have completed their analysis. Review detailed results above for comprehensive insights.";
            
            return summary;
        }

        private static string ExtractKeyPoints(string agentResult, string agentType)
        {
            // Simple extraction - in production, you might use more sophisticated NLP
            var lines = agentResult.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var keyLines = lines.Take(3).Where(line => !string.IsNullOrWhiteSpace(line));
            
            if (keyLines.Any())
            {
                return string.Join("\n", keyLines);
            }
            
            return $"Agent completed {agentType} - see detailed results above.";
        }
    }
}