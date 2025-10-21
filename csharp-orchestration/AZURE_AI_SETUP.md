# ?? Agentic AI Insurance Orchestration - Mock & Real Azure AI

This project provides both **Mock** and **Real Azure AI Foundry** implementations for insurance claim analysis using multiple AI agents.

## ?? Available Endpoints

### ?? Mock Endpoint (Always Available)
- **URL**: `POST /api/orchestration/analyze-claim`
- **Description**: Uses simulated AI responses for testing and development
- **No Azure configuration required**

### ?? Real Azure AI Endpoint (Requires Configuration)
- **URL**: `POST /api/orchestration/analyze-claim-real`
- **Description**: Uses actual Azure AI Foundry models for real analysis
- **Requires Azure AI Foundry setup**

### ?? Configuration Status
- **URL**: `GET /api/config/azure-ai-status`
- **Description**: Check if Azure AI is properly configured

## ?? Azure AI Foundry Setup

### 1. Prerequisites
- Azure subscription
- Azure AI Foundry project (AI Studio)
- OpenAI model deployment (e.g., GPT-4o)

### 2. Configuration

Update `appsettings.json` or set environment variables:

```json
{
  "Azure": {
    "AI_FOUNDRY_PROJECT_ENDPOINT": "https://your-ai-foundry-project.eastus.models.ai.azure.com",
    "MODEL_DEPLOYMENT_NAME": "gpt-4o"
  }
}
```

### 3. Authentication
The application uses `DefaultAzureCredential` which supports:
- Azure CLI: `az login`
- Visual Studio authentication
- Environment variables (`AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET`, `AZURE_TENANT_ID`)
- Managed Identity (when deployed to Azure)

## ????? How to Run

### 1. Start the Application
```bash
dotnet run --project AgenticOrchestration.csproj
```

### 2. Access Swagger UI
Navigate to: `http://localhost:5000`

### 3. Test Both Endpoints

#### Mock Endpoint (Always Works)
```bash
curl -X POST "http://localhost:5000/api/orchestration/analyze-claim" \
  -H "Content-Type: application/json" \
  -d '{
    "claim_id": "CLM-2024-001",
    "policy_number": "POL-AUTO-12345",
    "claim_date": "2024-01-15T10:30:00Z",
    "claim_amount": 15000.00,
    "description": "Vehicle collision with significant front-end damage"
  }'
```

#### Real Azure AI Endpoint (Requires Setup)
```bash
curl -X POST "http://localhost:5000/api/orchestration/analyze-claim-real" \
  -H "Content-Type: application/json" \
  -d '{
    "claim_id": "CLM-2024-001",
    "policy_number": "POL-AUTO-12345",
    "claim_date": "2024-01-15T10:30:00Z",
    "claim_amount": 15000.00,
    "description": "Vehicle collision with significant front-end damage"
  }'
```

## ?? AI Agents

Both implementations orchestrate 3 specialized agents:

### 1. ?? Claim Reviewer Agent
- Analyzes claim completeness and validity
- Reviews documentation and evidence
- Provides recommendations for next steps

### 2. ?? Risk Analyzer Agent
- Evaluates fraud risk indicators
- Assigns risk scores (1-10 scale)
- Recommends investigation actions

### 3. ?? Policy Checker Agent
- Analyzes policy coverage applicability
- Identifies exclusions and deductibles
- Determines coverage decisions

## ?? Key Differences

| Feature | Mock Implementation | Real Azure AI Implementation |
|---------|-------------------|----------------------------|
| **Response Time** | 400-600ms (simulated) | 2-5 seconds (actual AI processing) |
| **Response Quality** | Static, predictable | Dynamic, intelligent analysis |
| **Cost** | Free | Azure OpenAI pricing applies |
| **Setup Required** | None | Azure AI Foundry configuration |
| **Internet Required** | No | Yes (Azure API calls) |
| **Use Case** | Development, testing, demos | Production, real analysis |

The dual implementation allows you to start development immediately while having a clear path to production-grade AI capabilities! ??