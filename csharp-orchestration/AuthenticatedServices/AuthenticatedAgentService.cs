using Azure.Identity;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using System.Text.Json;
using AgenticOrchestration.Models;
using OpenAI.Chat;

namespace AgenticOrchestration.Services
{
    public interface IAuthenticatedAgentService
    {
        Task<string> ExecuteClaimReviewerAsync(string claimId, object? claimData, HttpContext httpContext);
        Task<string> ExecuteRiskAnalyzerAsync(string claimId, object? claimData, HttpContext httpContext);
        Task<string> ExecutePolicyCheckerAsync(string policyNumber, List<string> policyDocuments, HttpContext httpContext);
    }

    public class AuthenticatedAgentService : IAuthenticatedAgentService
  {
        private readonly AzureConfiguration _config;
        private readonly ILogger<AuthenticatedAgentService> _logger;
        private readonly IUserAzureCredentialService _credentialService;

        public AuthenticatedAgentService(
     IOptions<AzureConfiguration> config, 
            ILogger<AuthenticatedAgentService> logger,
     IUserAzureCredentialService credentialService)
        {
            _config = config.Value;
            _logger = logger;
       _credentialService = credentialService;
        }

        private async Task<AzureOpenAIClient> CreateClientForUserAsync(HttpContext httpContext)
        {
            try
      {
                var credential = await _credentialService.GetUserCredentialAsync(httpContext);
   var client = new AzureOpenAIClient(new Uri(_config.AI_FOUNDRY_PROJECT_ENDPOINT), credential);
   
     var userInfo = _credentialService.IsUserAuthenticated(httpContext) 
             ? $"authenticated user {httpContext.User.Identity?.Name}" 
     : "default credential";
             
 _logger.LogInformation("Created Azure OpenAI client for {UserInfo}", userInfo);
                return client;
    }
        catch (Exception ex)
        {
        _logger.LogError(ex, "Failed to create Azure OpenAI client for user");
          throw new InvalidOperationException($"Failed to create Azure OpenAI client: {ex.Message}", ex);
       }
   }

   public async Task<string> ExecuteClaimReviewerAsync(string claimId, object? claimData, HttpContext httpContext)
  {
     try
     {
                var claimJson = JsonSerializer.Serialize(claimData, new JsonSerializerOptions { WriteIndented = true });
  
          var systemPrompt = @"You are an expert Insurance Claim Reviewer Agent specialized in analyzing and validating insurance claims.

Your primary responsibilities include:
1. Review all claim details (dates, amounts, descriptions)
2. Verify completeness of documentation and supporting evidence
3. Analyze damage assessments and cost estimates for reasonableness
4. Validate claim details against policy requirements
5. Identify inconsistencies, missing info, or red flags
6. Provide a detailed assessment with specific recommendations

Response Format:
- CLAIM STATUS: VALID / QUESTIONABLE / INVALID
- Analysis: Summary of findings by component
- Missing Info / Concerns: List of issues or gaps
- Next Steps: Clear, actionable recommendations
- Reviewer Confidence: Percentage (0-100%)

Be thorough, objective, and professional in your analysis.";

    var userPrompt = $@"Please analyze the following insurance claim:

Claim ID: {claimId}
Claim Data:
{claimJson}

Provide a comprehensive review based on the claim data above.";

        var openAIClient = await CreateClientForUserAsync(httpContext);
      var chatClient = openAIClient.GetChatClient(_config.MODEL_DEPLOYMENT_NAME);
  
   var authInfo = _credentialService.IsUserAuthenticated(httpContext)
        ? $"Authenticated user: {httpContext.User.Identity?.Name}"
 : "Using default Azure credentials";

       var response = await chatClient.CompleteChatAsync(
           [
           new SystemChatMessage(systemPrompt),
  new UserChatMessage(userPrompt)
      ],
          new ChatCompletionOptions
 {
            Temperature = 0.1f,
           MaxOutputTokenCount = 1000,
         FrequencyPenalty = 0,
 PresencePenalty = 0
   });

          var result = response.Value.Content[0].Text ?? "No response from Azure AI agent";
         
         // Add authentication info to the result
        var authenticatedResult = $@"{result}

---
?? Authentication Info: {authInfo}
?? Processed via: Azure AI Foundry with user authentication";

   _logger.LogInformation("ClaimReviewer agent completed successfully for claim {ClaimId} with user auth", claimId);
       return authenticatedResult;
         }
   catch (Exception ex)
            {
    _logger.LogError(ex, "Error in Authenticated ClaimReviewer agent for claim {ClaimId}", claimId);
                return $"Authenticated Azure AI Error in claim review: {ex.Message}";
       }
        }

        public async Task<string> ExecuteRiskAnalyzerAsync(string claimId, object? claimData, HttpContext httpContext)
        {
 try
      {
      var claimJson = JsonSerializer.Serialize(claimData, new JsonSerializerOptions { WriteIndented = true });
   
    var systemPrompt = @"You are a Risk Analysis Agent specialized in evaluating insurance claims for authenticity and detecting potential fraud.

Core Functions:
1. Analyze claim data for suspicious patterns or inconsistencies
2. Identify potential fraud indicators based on timing, amounts, and descriptions
3. Assess claim credibility and assign a risk score
4. Recommend appropriate follow-up actions

Fraud Indicators to Consider:
- Claims with irregular timing patterns
- Contradictory or vague damage descriptions
- Unusual or repetitive claim amounts
- Multiple recent claims from same entity
- Geographic or temporal clustering of incidents
- Inconsistent witness statements or documentation

Output Format:
- Risk Level: LOW / MEDIUM / HIGH
- Risk Analysis: Brief summary of findings
- Fraud Indicators: List of specific signals detected (if any)
- Risk Score: 1–10 scale (1=lowest risk, 10=highest risk)
- Recommendation: Investigate / Monitor / No action needed
- Confidence Level: Percentage (0-100%)

Be analytical, objective, and focus on factual evidence.";

     var userPrompt = $@"Please analyze the following claim for fraud risk:

Claim ID: {claimId}
Claim Data:
{claimJson}

Provide a detailed risk assessment based on the claim data above.";

                var openAIClient = await CreateClientForUserAsync(httpContext);
    var chatClient = openAIClient.GetChatClient(_config.MODEL_DEPLOYMENT_NAME);
            
       var authInfo = _credentialService.IsUserAuthenticated(httpContext)
    ? $"Authenticated user: {httpContext.User.Identity?.Name}"
               : "Using default Azure credentials";

     var response = await chatClient.CompleteChatAsync(
      [
       new SystemChatMessage(systemPrompt),
       new UserChatMessage(userPrompt)
     ],
              new ChatCompletionOptions
     {
      Temperature = 0.2f,
         MaxOutputTokenCount = 1000,
           FrequencyPenalty = 0,
 PresencePenalty = 0
   });

        var result = response.Value.Content[0].Text ?? "No response from Azure AI agent";
    
      // Add authentication info to the result
          var authenticatedResult = $@"{result}

---
?? Authentication Info: {authInfo}
?? Processed via: Azure AI Foundry with user authentication";

       _logger.LogInformation("RiskAnalyzer agent completed successfully for claim {ClaimId} with user auth", claimId);
           return authenticatedResult;
            }
     catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Authenticated RiskAnalyzer agent for claim {ClaimId}", claimId);
         return $"Authenticated Azure AI Error in risk analysis: {ex.Message}";
            }
        }

   public async Task<string> ExecutePolicyCheckerAsync(string policyNumber, List<string> policyDocuments, HttpContext httpContext)
        {
   try
          {
          var policyInfo = string.Join("\n", policyDocuments);
           
   var systemPrompt = @"You are a Policy Checker Agent specialized in analyzing insurance policies and determining coverage applicability.

Your task is to:
1. Analyze policy documents and extract key coverage information
2. Identify relevant exclusions, limits, and deductibles
3. Determine coverage applicability for specific scenarios
4. Provide clear explanations based on policy language

Instructions:
- Base your determination solely on the provided policy documents
- Quote specific policy sections that support your conclusions
- Identify any ambiguities or areas requiring clarification
- Be precise and objective in your analysis

Output Format:
- Policy Number: [Policy number]
- Coverage Status: COVERED / NOT COVERED / REQUIRES REVIEW
- Coverage Details: Main coverage types and limits
- Relevant Exclusions: Any applicable exclusions
- Deductibles: Applicable deductible amounts
- Policy Sections: Quote specific sections supporting determination
- Recommendation: Clear next steps or actions needed
- Confidence Level: Percentage (0-100%)

Focus on factual policy interpretation and avoid speculation.";

       var userPrompt = $@"Please analyze the following policy:

Policy Number: {policyNumber}
Policy Documents:
{policyInfo}

Provide a comprehensive policy analysis based on the documents above.";

      var openAIClient = await CreateClientForUserAsync(httpContext);
      var chatClient = openAIClient.GetChatClient(_config.MODEL_DEPLOYMENT_NAME);
       
        var authInfo = _credentialService.IsUserAuthenticated(httpContext)
        ? $"Authenticated user: {httpContext.User.Identity?.Name}"
  : "Using default Azure credentials";

      var response = await chatClient.CompleteChatAsync(
      [
            new SystemChatMessage(systemPrompt),
             new UserChatMessage(userPrompt)
    ],
     new ChatCompletionOptions
        {
               Temperature = 0.1f,
     MaxOutputTokenCount = 1000,
        FrequencyPenalty = 0,
                   PresencePenalty = 0
        });

      var result = response.Value.Content[0].Text ?? "No response from Azure AI agent";
          
     // Add authentication info to the result
          var authenticatedResult = $@"{result}

---
?? Authentication Info: {authInfo}
?? Processed via: Azure AI Foundry with user authentication";

          _logger.LogInformation("PolicyChecker agent completed successfully for policy {PolicyNumber} with user auth", policyNumber);
                return authenticatedResult;
 }
          catch (Exception ex)
    {
 _logger.LogError(ex, "Error in Authenticated PolicyChecker agent for policy {PolicyNumber}", policyNumber);
                return $"Authenticated Azure AI Error in policy check: {ex.Message}";
            }
  }
    }
}