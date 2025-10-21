# C# Agentic AI Insurance Orchestration API

This is a C# ASP.NET Core implementation of the multi-agent orchestration system for insurance claim processing, equivalent to the Python version in challenge-5.

## Overview

The API orchestrates three specialized AI agents to analyze insurance claims:

1. **Claim Reviewer Agent**: Validates claim details, documentation, and completeness
2. **Risk Analyzer Agent**: Analyzes for fraud indicators and assigns risk scores  
3. **Policy Checker Agent**: Reviews policy documents and coverage details

All agents run concurrently using Azure OpenAI services with DefaultAzureCredential authentication.

## Architecture

```
AgenticOrchestration/
├── Models/
│   └── ApiModels.cs          # Request/Response models
├── Services/
│   ├── AzureServices.cs      # Azure integrations (Cosmos DB, Search)
│   ├── AgentService.cs       # Individual AI agents
│   └── OrchestrationService.cs # Main orchestration logic
├── Program.cs                # API configuration and endpoints
├── appsettings.json          # Configuration
└── AgenticOrchestration.csproj
```

## Features

- **Minimal API**: Clean REST endpoints with Swagger documentation
- **Concurrent Execution**: All three agents run in parallel for optimal performance
- **Azure Integration**: Uses Azure OpenAI, Cosmos DB, and AI Search
- **Fallback Data**: Mock data when Azure services are unavailable
- **Comprehensive Logging**: Detailed execution tracking and error handling
- **Health Checks**: API health monitoring endpoint

## Prerequisites

1. **.NET 8 SDK** installed
2. **Azure CLI** logged in with appropriate permissions
3. **Azure AI Foundry project** with OpenAI deployment
4. Environment variables configured (see Configuration)

## Configuration

Update `appsettings.json` with your Azure settings:

```json
{
  "Azure": {
    "AI_FOUNDRY_PROJECT_ENDPOINT": "https://your-project.openai.azure.com/",
    "MODEL_DEPLOYMENT_NAME": "gpt-4o",
    "COSMOS_DB_ENDPOINT": "https://your-cosmosdb.documents.azure.com:443/",
    "COSMOS_DB_DATABASE": "insurance_db",
    "COSMOS_DB_CONTAINER": "claims",
    "SEARCH_ENDPOINT": "https://your-search.search.windows.net",
    "SEARCH_INDEX": "policies"
  }
}
```

## Running the API

1. **Restore packages**:
   ```bash
   dotnet restore
   ```

2. **Build the project**:
   ```bash
   dotnet build
   ```

3. **Run the API**:
   ```bash
   dotnet run
   ```

4. **Access Swagger UI**: Navigate to `https://localhost:7000` (or the displayed URL)

## API Endpoints

### `POST /api/orchestration/analyze-claim`
Orchestrates all three agents to analyze an insurance claim.

**Request Body**:
```json
{
  "claimId": "CLM-2024-001",
  "policyNumber": "POL-AUTO-12345",
  "claimDate": "2024-01-15T10:30:00Z",
  "claimAmount": 15000.00,
  "description": "Vehicle collision with significant damage",
  "additionalData": {
    "accidentLocation": "Interstate 95",
    "weatherConditions": "Clear",
    "policeReportNumber": "PR-2024-789"
  }
}
```

**Response**:
```json
{
  "claimId": "CLM-2024-001",
  "policyNumber": "POL-AUTO-12345",
  "claimReviewerResult": "Detailed claim review analysis...",
  "riskAnalyzerResult": "Risk assessment with fraud indicators...",
  "policyCheckerResult": "Policy coverage analysis...",
  "executionTimeMs": 2500,
  "timestamp": "2024-01-15T10:35:00Z",
  "summary": "Orchestration Analysis Summary..."
}
```

### `GET /api/orchestration/sample-request`
Returns a sample request for testing purposes.

### `GET /api/health`
Returns API health status.

## Authentication

The API uses **DefaultAzureCredential** which attempts authentication in this order:
1. Environment variables
2. Azure CLI credentials
3. Managed Identity (when deployed to Azure)

Ensure you're logged in via Azure CLI:
```bash
az login
```

## Key Differences from Python Version

1. **Concurrent Execution**: Uses `Task.WhenAll()` instead of Python's `asyncio.gather()`
2. **Dependency Injection**: ASP.NET Core DI container manages service lifecycles
3. **Structured Logging**: Uses `ILogger<T>` for comprehensive logging
4. **Configuration**: Uses ASP.NET Core configuration system
5. **API Documentation**: Swagger/OpenAPI automatically generated
6. **Error Handling**: Comprehensive exception handling with proper HTTP status codes

## Testing

1. **Use Swagger UI**: Navigate to the root URL to access interactive API documentation
2. **Get Sample Request**: Use `/api/orchestration/sample-request` to get test data
3. **Health Check**: Use `/api/health` to verify the service is running

## Production Considerations

- Configure proper Azure authentication (Managed Identity recommended)
- Set up Azure Application Insights for monitoring
- Configure proper CORS policies for web applications
- Use Azure App Service or Container Apps for hosting
- Set up proper logging and alerting
- Consider implementing rate limiting and caching

## Troubleshooting

**Authentication Issues**:
- Ensure Azure CLI is logged in: `az account show`
- Verify correct Azure subscription is selected
- Check Azure AI Foundry project permissions

**Configuration Issues**:
- Verify all Azure endpoints in `appsettings.json`
- Ensure model deployment name matches your Azure OpenAI deployment
- Check Azure resource names and regions

**Performance Issues**:
- Monitor execution times in logs
- Check Azure OpenAI quota and rate limits
- Consider implementing caching for policy documents