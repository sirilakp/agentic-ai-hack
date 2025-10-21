using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AgenticOrchestration.Services
{
    public interface IUserAzureCredentialService
    {
        Task<TokenCredential> GetUserCredentialAsync(HttpContext httpContext);
        bool IsUserAuthenticated(HttpContext httpContext);
    }

    public class UserAzureCredentialService : IUserAzureCredentialService
    {
        private readonly ILogger<UserAzureCredentialService> _logger;
        private readonly AzureConfiguration _config;

        public UserAzureCredentialService(IOptions<AzureConfiguration> config, ILogger<UserAzureCredentialService> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public async Task<TokenCredential> GetUserCredentialAsync(HttpContext httpContext)
        {
            try
            {
                // Check if user is authenticated via Azure AD
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    _logger.LogInformation("User is authenticated via Azure AD, attempting to use user credentials");
                    
                    // For now, we'll use DefaultAzureCredential which can pick up user context
                    // In production, you'd want to implement proper token exchange
                    _logger.LogInformation("Using DefaultAzureCredential for authenticated user: {UserName}", 
                        httpContext.User.Identity.Name);
            
                    return new DefaultAzureCredential();
                }

                _logger.LogInformation("User not authenticated, falling back to DefaultAzureCredential");

                // Fallback to default credential (Azure CLI, VS, etc.)
                return new DefaultAzureCredential();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user credential, falling back to DefaultAzureCredential");
                return new DefaultAzureCredential();
            }
        }

        public bool IsUserAuthenticated(HttpContext httpContext)
        {
            return httpContext.User.Identity?.IsAuthenticated == true;
        }

        private string GetTenantId()
        {
            // Get from configuration or environment
            return Environment.GetEnvironmentVariable("AZURE_TENANT_ID") ?? 
                _config.AZURE_TENANT_ID ?? 
                "f46bbd44-80be-4615-a057-66c7caaaad3d"; // Your tenant ID from .env
        }

        private string GetClientId()
        {
            // Get from configuration or environment
            return Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") ?? 
                _config.AZURE_CLIENT_ID ?? 
                throw new InvalidOperationException("AZURE_CLIENT_ID not configured");
        }

        private string GetClientSecret()
        {
            // Get from configuration or environment
            return Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET") ?? 
                _config.AZURE_CLIENT_SECRET ?? 
                throw new InvalidOperationException("AZURE_CLIENT_SECRET not configured");
        }
    }
}