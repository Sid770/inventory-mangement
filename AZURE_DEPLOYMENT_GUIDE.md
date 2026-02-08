# Azure Deployment Guide
## Angular + .NET Web API + Azure Table Storage (Free Tier Only)

---

## 1. Objective
This guide helps you deploy a full-stack web application on Microsoft Azure using only free services.

By the end of this guide, you will have:
- ✅ An Angular frontend deployed on Azure
- ✅ A .NET Web API deployed on Azure
- ✅ Data stored in Azure Table Storage
- ✅ A fully working end-to-end cloud application

---

## 2. Final Architecture
```
Browser
   |
Angular Frontend → Azure Static Web Apps (Free)
   |
.NET Web API → Azure App Service (F1 – Free)
   |
Azure Table Storage (Free)
```

---

## 3. Prerequisites

Before starting, ensure you have:
- ✅ Azure account (Azure for Students preferred)
- ✅ GitHub account
- ✅ Node.js and Angular CLI installed
- ✅ .NET SDK installed
- ✅ A working Angular project
- ✅ A working .NET Web API project

---

## 4. Backend Project Structure
Your .NET Web API project follows this structure:
```
InventoryAPI
│
├── Controllers
│   ├── DashboardController.cs
│   ├── InventoryController.cs
│   ├── StockController.cs
│   └── UsersController.cs
│
├── Models
│   ├── InventoryItem.cs
│   ├── StockTransaction.cs
│   └── UserEntity.cs
│
├── Data
│   └── InventoryDbContext.cs
│
├── DTOs
│   └── InventoryDtos.cs
│
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
└── InventoryAPI.csproj
```

---

## 5. Azure Portal Configuration Steps

### Step 1: Create Azure Storage Account

1. **Login to Azure Portal**
   - Go to: https://portal.azure.com
   - Sign in with your Azure account

2. **Create Storage Account**
   - Click **"Create a resource"**
   - Search for **"Storage Account"**
   - Click **"Create"**

3. **Configure Storage Account**
   - **Subscription**: Select your subscription (Azure for Students)
   - **Resource Group**: Create new → Name it: `inventory-app-rg`
   - **Storage Account Name**: `inventorystorage[yourname]` (must be globally unique, lowercase, no spaces)
   - **Region**: Choose closest region (e.g., East US)
   - **Performance**: **Standard**
   - **Redundancy**: **Locally-redundant storage (LRS)** (Free tier)
   
4. **Review and Create**
   - Click **"Review + Create"**
   - Click **"Create"**
   - Wait for deployment to complete
   - Click **"Go to resource"**

5. **Get Connection String**
   - In your Storage Account, navigate to **"Access keys"** (left menu under Security + networking)
   - Click **"Show"** next to Connection string
   - Copy **"Connection string"** for use later
   - **Save this securely** - you'll need it for your API configuration

---

### Step 2: Create Azure Table in Storage

1. **Open Storage Browser**
   - In your Storage Account, click **"Storage browser"** (left menu)
   
2. **Create Tables**
   - Expand **"Tables"**
   - Click **"+ Add table"**
   - Name: `Users`
   - Click **"OK"**
   
3. **Verify Table Creation**
   - You should see the "Users" table listed under Tables

---

### Step 3: Create Azure App Service (Backend API)

1. **Create App Service**
   - Click **"Create a resource"**
   - Search for **"Web App"**
   - Click **"Create"**

2. **Configure App Service Basics**
   - **Subscription**: Your subscription
   - **Resource Group**: Select `inventory-app-rg` (same as storage)
   - **Name**: `inventory-api-[yourname]` (must be globally unique)
   - **Publish**: **Code**
   - **Runtime stack**: **.NET 10** (or your version)
   - **Operating System**: **Linux** (cheaper) or **Windows**
   - **Region**: Same as your storage account

3. **Configure App Service Plan**
   - **Linux Plan**: Create new → Name: `inventory-plan`
   - **Pricing plan**: Click **"Change size"**
   - Select **"Dev/Test"** tab
   - Choose **"F1"** (Free tier)
   - Click **"Apply"**

4. **Review and Create**
   - Click **"Review + Create"**
   - Click **"Create"**
   - Wait for deployment
   - Click **"Go to resource"**

---

### Step 4: Configure App Service Settings

1. **Add Configuration Settings**
   - In your App Service, go to **"Configuration"** (left menu under Settings)
   - Click **"+ New application setting"**

2. **Add Storage Connection String**
   - **Name**: `StorageConnection`
   - **Value**: Paste your Storage Account connection string (from Step 1.5)
   - Click **"OK"**

3. **Add CORS Settings (if needed)**
   - Go to **"CORS"** (left menu under API)
   - Add your frontend URL (we'll get this in Step 6)
   - Or add `*` for testing (not recommended for production)

4. **Save Configuration**
   - Click **"Save"** at the top
   - Click **"Continue"** to confirm restart

---

### Step 5: Deploy Backend API to App Service

**Option A: Deploy via GitHub Actions (Recommended)**

1. **Enable GitHub Deployment**
   - In App Service, go to **"Deployment Center"** (left menu)
   - **Source**: Select **"GitHub"**
   - Click **"Authorize"** and login to GitHub
   - **Organization**: Your GitHub username
   - **Repository**: `inventory-mangement`
   - **Branch**: `master`
   - Click **"Save"**

2. **Verify Workflow**
   - Azure will create a workflow file in `.github/workflows/`
   - The deployment will start automatically
   - Monitor in **"Logs"** section

**Option B: Deploy via VS Code**

1. **Install Azure App Service Extension**
   - In VS Code, go to Extensions
   - Search for "Azure App Service"
   - Install it

2. **Deploy**
   - Right-click on `backend/InventoryAPI` folder
   - Select **"Deploy to Web App..."**
   - Select your subscription
   - Select your App Service: `inventory-api-[yourname]`
   - Confirm deployment

3. **Verify Deployment**
   - Once deployed, visit: `https://inventory-api-[yourname].azurewebsites.net`
   - You should see the API running
   - Visit: `https://inventory-api-[yourname].azurewebsites.net/swagger` for API docs

---

### Step 6: Create Azure Static Web App (Frontend)

1. **Create Static Web App**
   - Click **"Create a resource"**
   - Search for **"Static Web App"**
   - Click **"Create"**

2. **Configure Static Web App Basics**
   - **Subscription**: Your subscription
   - **Resource Group**: `inventory-app-rg`
   - **Name**: `inventory-frontend-[yourname]`
   - **Plan type**: **Free**
   - **Region**: Choose closest region
   - **Source**: **GitHub**

3. **Configure GitHub Integration**
   - Click **"Sign in with GitHub"**
   - **Organization**: Your GitHub username
   - **Repository**: `inventory-mangement`
   - **Branch**: `master`

4. **Build Configuration**
   - **Build Presets**: **Angular**
   - **App location**: `/` (root)
   - **Api location**: Leave empty
   - **Output location**: `dist/hcl3/browser` (check your angular.json for exact path)

5. **Review and Create**
   - Click **"Review + Create"**
   - Click **"Create"**
   - This will add a GitHub Actions workflow to your repository

6. **Get Frontend URL**
   - Once deployed, go to resource
   - Copy the **URL** (e.g., `https://[random].azurestaticapps.net`)

---

### Step 7: Update API Configuration with Frontend URL

1. **Update CORS Settings**
   - Go back to App Service (Backend API)
   - Navigate to **"Configuration"**
   - Update your Program.cs CORS policy or add in Azure:
   - Go to **"CORS"** menu
   - Add your Static Web App URL: `https://[your-frontend].azurestaticapps.net`
   - Click **"Save"**

---

### Step 8: Update Frontend to Use Azure API

1. **Update Angular Environment**
   - Create/update environment file with your API URL:
   ```typescript
   export const environment = {
     production: true,
     apiUrl: 'https://inventory-api-[yourname].azurewebsites.net/api'
   };
   ```

2. **Update Service Files**
   - Ensure your Angular services use `environment.apiUrl`

3. **Commit and Push**
   ```bash
   git add .
   git commit -m "Update API URL for Azure"
   git push origin master
   ```

4. **Monitor Deployment**
   - Go to your Static Web App → **"GitHub Action runs"**
   - Wait for build to complete

---

### Step 9: Test Your Application

1. **Test API Endpoints**
   - Visit: `https://inventory-api-[yourname].azurewebsites.net/swagger`
   - Test each endpoint
   - Verify data is being stored in Azure Table Storage

2. **Test Frontend**
   - Visit: `https://[your-frontend].azurestaticapps.net`
   - Test all features
   - Verify API calls are working

3. **Verify Azure Storage**
   - Go to Azure Portal → Your Storage Account
   - Navigate to **Storage browser → Tables → Users**
   - You should see data when you add users via API

---

## 6. Your Azure Resources Summary

After completion, you should have:

| Resource | Type | URL/Name | Cost |
|----------|------|----------|------|
| Storage Account | Azure Storage | `inventorystorage[yourname]` | **FREE** |
| Users Table | Table Storage | `Users` | **FREE** |
| Backend API | App Service (F1) | `https://inventory-api-[yourname].azurewebsites.net` | **FREE** |
| Frontend | Static Web App | `https://[random].azurestaticapps.net` | **FREE** |

---

## 7. Important URLs to Save

- **Azure Portal**: https://portal.azure.com
- **Frontend URL**: `https://[your-static-app].azurestaticapps.net`
- **Backend API URL**: `https://inventory-api-[yourname].azurewebsites.net`
- **Swagger Docs**: `https://inventory-api-[yourname].azurewebsites.net/swagger`
- **Storage Account**: In Azure Portal

---

## 8. Troubleshooting

### API Not Working
1. Check App Service logs: App Service → **"Log stream"**
2. Verify Configuration: App Service → **"Configuration"** → Check `StorageConnection`
3. Check CORS settings: App Service → **"CORS"**

### Frontend Not Loading
1. Check GitHub Actions: Static Web App → **"GitHub Action runs"**
2. Verify build output location in workflow file
3. Check browser console for errors

### Storage Connection Issues
1. Verify connection string is correct
2. Test connection string locally first
3. Check Storage Account firewall settings: Storage Account → **"Networking"**

### CORS Errors
1. Add frontend URL to CORS: App Service → **"CORS"**
2. Ensure CORS is configured in Program.cs
3. Clear browser cache and retry

---

## 9. Next Steps

- ✅ Monitor your application usage in Azure Portal
- ✅ Check GitHub Actions for successful deployments
- ✅ Add authentication (Azure AD B2C)
- ✅ Set up Application Insights for monitoring
- ✅ Configure custom domain (Optional)

---

## 10. Important Notes

⚠️ **Free Tier Limitations:**
- App Service F1: 60 CPU minutes/day
- Static Web Apps: 100 GB bandwidth/month
- Storage: 5 GB storage

⚠️ **Keep your Storage Connection String secure!**
- Never commit it to GitHub
- Use Azure App Service Configuration

---

**Your deployment is complete! 🎉**

Access your live application at: `https://[your-static-app].azurestaticapps.net`
