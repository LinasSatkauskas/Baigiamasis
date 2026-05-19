# Complete Azure Deployment Guide

## Prerequisites

- Azure Free Account (sign up at https://azure.microsoft.com/free)
- Your app built: `npm run build`
- Git installed

## Part 1: Setup on Azure Portal

### Step 1: Create a Resource Group

1. Go to https://portal.azure.com
2. In the search bar at top, search for **"Resource groups"**
3. Click **"+ Create"** button
4. Fill in:
   - **Subscription**: Select your free subscription
   - **Resource group name**: `baigiamasis-rg`
   - **Region**: Select closest to you (e.g., `North Europe`)
5. Click **"Review + create"** → **"Create"**
6. Wait for it to complete

### Step 2: Create MySQL Database

1. Search for **"Azure Database for MySQL"** in top search bar
2. Click **"Create"** → Select **"Single Server"** (free tier)
3. Fill in the form:
   - **Resource group**: Select `baigiamasis-rg` (created above)
   - **Server name**: `baigiamasis-db` (must be unique, Azure will tell you)
   - **Data source**: "None"
   - **Location**: Same as resource group
   - **Version**: 8.0 (latest)
   - **Compute + storage**: Click it, select **"Burstable B1s"** (cheapest)
   - **Admin username**: `root`
   - **Password**: `YourSecurePassword123!` (save this!)
4. Click **"Review + create"** → **"Create"**
5. Wait 5-10 minutes for database creation

### Step 3: Configure MySQL Database

1. Once MySQL is created, open it
2. Click **"Connection security"** in left menu
3. Under **"Firewall rules"**:
   - Turn **"Allow access to Azure services"** to **ON**
   - Add rule: Name: `AllowAll`, Start IP: `0.0.0.0`, End IP: `255.255.255.255`
   - Click **"Save"**
4. Click **"Databases"** in left menu
5. Click **"+ Add"** to create database
   - **Database name**: `baigiamasis`
   - Click **"OK"**

### Step 4: Create App Service Plan

1. Search for **"App Service plans"** in top search
2. Click **"+ Create"**
3. Fill in:
   - **Resource group**: `baigiamasis-rg`
   - **Name**: `baigiamasis-plan`
   - **Operating System**: Windows
   - **Region**: Same as MySQL
   - **Pricing tier**: Click it, select **B1 (Free tier)** - $0/month
4. Click **"Review + create"** → **"Create"**

### Step 5: Create App Service (Your Web App)

1. Search for **"App Services"** in top search
2. Click **"+ Create"**
3. Fill in:
   - **Resource group**: `baigiamasis-rg`
   - **Name**: `baigiamasis-app` (this becomes your URL: baigiamasis-app.azurewebsites.net)
   - **Publish**: Code
   - **Runtime stack**: .NET 8 (LTS)
   - **Operating System**: Windows
   - **App Service Plan**: Select `baigiamasis-plan` (created above)
4. Click **"Review + create"** → **"Create"**
5. Wait for deployment to complete

### Step 6: Configure App Service Environment Variables

1. Open your App Service (`baigiamasis-app`)
2. Click **"Configuration"** in left menu
3. Click **"+ New application setting"** and add these (one by one):

| Name                     | Value                                               |
| ------------------------ | --------------------------------------------------- |
| `MySQL:Db`               | `baigiamasis`                                       |
| `MySQL:User`             | `root@baigiamasis-db`                               |
| `MySQL:Password`         | `YourSecurePassword123!` (the password from Step 2) |
| `Admin:Email`            | `admin@local`                                       |
| `Admin:Password`         | `Passw0rd!`                                         |
| `ASPNETCORE_ENVIRONMENT` | `Production`                                        |

4. Click **"Save"** at top
5. When prompted to reload, click **"Continue"**

---

## Part 2: Deploy Your Code

### Step 1: Setup Git Deployment

1. In your App Service, click **"Deployment Center"** in left menu
2. Under **Source**, select **"Local Git"**
3. Click **"Save"** at top
4. Under **Local Git/FTPS credentials**, copy the Git clone URL (looks like: `https://baigiamasis-app.scm.azurewebsites.net/baigiamasis-app.git`)
5. Copy the **username** and **password** shown (save them!)

### Step 2: Deploy from Your Computer

Open terminal in your project folder and run:

```bash
# First time setup
git remote add azure <your-git-clone-url>

# Example:
git remote add azure https://baigiamasis-app.scm.azurewebsites.net/baigiamasis-app.git

# Push to deploy
git push azure main

# (Enter the username and password from Step 1)
```

Wait 3-5 minutes for deployment to complete. You'll see:

```
remote: Finished successfully.
remote: Running post deployment command(s)...
remote: Deployment successful.
```

### Step 3: Verify Deployment

1. Go to your App Service in Azure portal
2. Click the URL at top (https://baigiamasis-app.azurewebsites.net)
3. You should see your app loading!

---

## Part 3: Troubleshooting

### If App Shows 404 Error:

1. Go back to App Service
2. Click **"Logs"** or **"SSH"** in left menu to check error logs
3. Common issues:
   - Database connection string wrong (check Configuration settings)
   - Frontend not built (run `npm run build` locally first)

### If Database Connection Fails:

1. Go to MySQL database in Azure
2. Click **"Connection security"**
3. Verify firewall rules allow Azure services
4. Test connection from your computer using MySQL client

### Check Logs:

1. In App Service, click **"App Service logs"**
2. Enable **"Application logging"** → **"Save"**
3. Click **"Log stream"** to see real-time errors

---

## Part 4: Updates & Redeployment

Every time you make changes:

```bash
# Build locally
npm run build

# Commit changes
git add .
git commit -m "Your changes"

# Deploy
git push azure main
```

That's it! Azure automatically redeploys your app.

---

## Free Tier Limitations

- **App Service B1**: 1 GB RAM, enough for small apps
- **MySQL**: 5 GB storage
- **Total**: $0/month (paid tiers available if you scale)

---

## Support

- Azure Portal: https://portal.azure.com
- Docs: https://docs.microsoft.com/azure
- If stuck, check App Service Logs (Menu → Logs)
