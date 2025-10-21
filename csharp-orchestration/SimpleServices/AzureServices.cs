using Microsoft.Extensions.Options;

namespace AgenticOrchestration.Services
{
    public class AzureConfiguration
    {
        public string AI_FOUNDRY_PROJECT_ENDPOINT { get; set; } = string.Empty;
        public string MODEL_DEPLOYMENT_NAME { get; set; } = string.Empty;
        public string AZURE_OPENAI_ENDPOINT { get; set; } = string.Empty;
        public string AZURE_OPENAI_KEY { get; set; } = string.Empty;
        public string COSMOS_DB_ENDPOINT { get; set; } = string.Empty;
        public string COSMOS_DB_DATABASE { get; set; } = string.Empty;
        public string COSMOS_DB_CONTAINER { get; set; } = string.Empty;
        public string COSMOS_KEY { get; set; } = string.Empty;
        public string SEARCH_ENDPOINT { get; set; } = string.Empty;
        public string SEARCH_INDEX { get; set; } = string.Empty;
        public string SEARCH_ADMIN_KEY { get; set; } = string.Empty;
        public string AZURE_STORAGE_CONNECTION_STRING { get; set; } = string.Empty;
        
        // Azure AD Configuration
        public string AZURE_TENANT_ID { get; set; } = string.Empty;
        public string AZURE_CLIENT_ID { get; set; } = string.Empty;
        public string AZURE_CLIENT_SECRET { get; set; } = string.Empty;
        
        // Legacy properties for compatibility
        public string COSMOSDB_CONNECTION_STRING { get; set; } = string.Empty;
        public string COSMOSDB_DATABASE_NAME { get; set; } = string.Empty;
        public string COSMOSDB_CONTAINER_NAME { get; set; } = string.Empty;
    }

    public interface IAzureServices
    {
        Task<object?> GetClaimDataAsync(string claimId);
        Task<List<string>> SearchPolicyDocumentsAsync(string policyNumber);
    }

    public class AzureServices : IAzureServices
    {
        private readonly ICosmosDbService _cosmosDbService;
        private readonly IAzureSearchService _searchService;
        private readonly ILogger<AzureServices> _logger;

        public AzureServices(ICosmosDbService cosmosDbService, IAzureSearchService searchService, ILogger<AzureServices> logger)
        {
     _cosmosDbService = cosmosDbService;
            _searchService = searchService;
     _logger = logger;
        }

        public async Task<object?> GetClaimDataAsync(string claimId)
        {
            try
            {
      return await _cosmosDbService.GetClaimByIdAsync(claimId);
        }
        catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve claim data for {ClaimId}", claimId);
       return null;
            }
        }

   public async Task<List<string>> SearchPolicyDocumentsAsync(string policyNumber)
        {
   try
     {
return await _searchService.SearchPolicyDocumentsAsync(policyNumber);
}
            catch (Exception ex)
       {
         _logger.LogWarning(ex, "Failed to search policy documents for {PolicyNumber}", policyNumber);
                return new List<string>();
        }
        }
  }

    public interface ICosmosDbService
    {
        Task<object?> GetClaimByIdAsync(string claimId);
    }

    public class CosmosDbService : ICosmosDbService
 {
        private readonly ILogger<CosmosDbService> _logger;

        public CosmosDbService(IOptions<AzureConfiguration> config, ILogger<CosmosDbService> logger)
        {
          _logger = logger;
  _logger.LogWarning("CosmosDB service initialized with mock data implementation");
        }

  public async Task<object?> GetClaimByIdAsync(string claimId)
   {
            try
            {
        // Return mock data for demonstration
         await Task.Delay(100); // Simulate async operation
     
         return new
   {
     id = claimId,
    claimant_name = "John Doe",
  incident_date = "2024-10-15",
     incident_description = "Vehicle collision at intersection",
 claim_amount = 15000,
         damage_description = "Front-end damage to vehicle",
       policy_number = "LIAB-AUTO-001",
        status = "Under Review"
           };
            }
            catch (Exception ex)
            {
       _logger.LogError(ex, "Error retrieving claim {ClaimId}", claimId);
     return null;
            }
   }
    }

    public interface IAzureSearchService
    {
      Task<List<string>> SearchPolicyDocumentsAsync(string policyNumber);
    }

  public class AzureSearchService : IAzureSearchService
    {
        private readonly ILogger<AzureSearchService> _logger;

        public AzureSearchService(IOptions<AzureConfiguration> config, ILogger<AzureSearchService> logger)
 {
   _logger = logger;
  _logger.LogInformation("Azure Search service initialized (using mock data)");
        }

        public async Task<List<string>> SearchPolicyDocumentsAsync(string policyNumber)
        {
            // Mock policy document search results
          await Task.Delay(100); // Simulate API call
            
     return new List<string>
       {
           $"Policy {policyNumber}: Liability Coverage - $100,000 per incident limit",
                $"Policy {policyNumber}: Deductible - $500 for collision claims",
       $"Policy {policyNumber}: Coverage includes property damage and bodily injury",
          $"Policy {policyNumber}: Valid from 01/01/2024 to 12/31/2024"
      };
        }
    }
}