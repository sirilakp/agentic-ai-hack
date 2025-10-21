# ?? Azure AD Authentication Setup Guide

## Overview

Your application now supports three types of Azure AI orchestration:

1. **?? Mock** - No authentication required
2. **?? Real Azure AI** - Uses service credentials (API keys)
3. **?? Authenticated Azure AI** - Uses user's Azure login credentials

## ?? Quick Setup for Azure AD Authentication

### Step 1: Create Azure AD App Registration

1. **Go to Azure Portal** ? Azure Active Directory ? App registrations
2. **Click "New registration"**
3. **Configure the app:**
   - **Name**: `Agentic-AI-Orchestration-Swagger`
   - **Supported account types**: Accounts in this organizational directory only
- **Redirect URI**: 
     - Type: `Web`
     - URI: `https://localhost:5001/signin-oidc`

### Step 2: Configure Authentication

1. **In your app registration**, go to **Authentication**
2. **Add platform** ? **Web**
3. **Add redirect URIs:**
   - `https://localhost:5001/signin-oidc`
   - `https://localhost:5001/swagger/oauth2-redirect.html`
4. **Logout URL**: `https://localhost:5001/signout-callback-oidc`
5. **Enable**: "Access tokens" and "ID tokens"

### Step 3: Get Configuration Values

From your app registration, copy these values:

- **Application (client) ID**: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
- **Directory (tenant) ID**: `f46bbd44-80be-4615-a057-66c7caaaad3d` (already configured)

### Step 4: Create Client Secret (Optional)

1. Go to **Certificates & secrets**
2. **New client secret**
3. **Copy the secret value** (you won't see it again!)

### Step 5: Configure API Permissions

1. Go to **API permissions**
2. **Add a permission** ? **Azure Service Management** ? **Delegated permissions**
3. **Select**: `user_impersonation`
4. **Add a permission** ? **Microsoft Graph** ? **Delegated permissions**
5. **Select**: `User.Read`
6. **Grant admin consent** for your tenant

## ?? Configuration Update

Update your `appsettings.json`:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "your-domain.onmicrosoft.com",
    "TenantId": "f46bbd44-80be-4615-a057-66c7caaaad3d",
    "ClientId": "YOUR-CLIENT-ID-HERE",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc"
  }
}
```

## ?? Testing the Authentication

### 1. Start the Application
```bash
dotnet run --project AgenticOrchestration.csproj --urls "https://localhost:5001"
```

### 2. Access Swagger UI
Navigate to: `https://localhost:5001`

### 3. Test Authentication Flow

#### **Test Mock Endpoint (No Auth Required)**
- Use `/api/orchestration/analyze-claim`
- Works immediately without login

#### **Test Real Azure AI (No Auth Required)**
- Use `/api/orchestration/analyze-claim-real`
- Uses service credentials (API keys)

#### **Test Authenticated Endpoint (Requires Login)**
1. **Click "Authorize" in Swagger UI**
2. **Login with your Azure credentials**
3. **Use `/api/orchestration/analyze-claim-authenticated`**
4. **See your username in the response!**

## ?? Available Endpoints

### Authentication Endpoints
- `GET /api/auth/login` - Redirect to Azure AD login
- `GET /api/auth/logout` - Sign out from Azure AD
- `GET /api/auth/user` - Get current user info (requires auth)

### Orchestration Endpoints
- `POST /api/orchestration/analyze-claim` - Mock (no auth)
- `POST /api/orchestration/analyze-claim-real` - Real Azure AI (no auth)
- `POST /api/orchestration/analyze-claim-authenticated` - User authenticated Azure AI

### Utility Endpoints
- `GET /api/config/azure-ai-status` - Check configuration
- `GET /api/health` - Health check

## ?? Troubleshooting

### Common Issues

1. **"AADSTS50011: The reply URL specified in the request does not match"**
   - Add `https://localhost:5001/signin-oidc` to redirect URIs

2. **"AADSTS7000215: Invalid client secret is provided"**
   - Check client secret in configuration

3. **"User not found" in authenticated endpoint**
   - Ensure user has access to Azure AI resources

### Quick Fixes

```bash
# Check if app is running on correct port
dotnet run --urls "https://localhost:5001"

# Verify configuration
curl "https://localhost:5001/api/config/azure-ai-status"
```

## ?? Expected Results

### **Mock Endpoint Response**: 
- Fast (500ms)
- Static responses
- No authentication info

### **Real Azure AI Response**: 
- Slower (2-5s)
- AI-generated content
- Uses service account

### **Authenticated Response**: 
- Slower (2-5s)
- AI-generated content
- Shows authenticated user name
- Uses user's Azure permissions

## ?? Next Steps

1. **Deploy to Azure**: Use managed identity for production
2. **Configure RBAC**: Set up proper permissions for users
3. **Monitor Usage**: Track token consumption per user
4. **Add More Scopes**: Request additional Azure permissions as needed

Your application now supports secure, user-authenticated Azure AI calls through Swagger! ??