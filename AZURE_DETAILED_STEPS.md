# Azure Deployment - STEP BY STEP (Ultra Detailed)

## Before You Start

- Open https://portal.azure.com in your browser
- Have your password/credentials ready
- Have this guide open in another window

---

## SECTION 1: CREATE RESOURCE GROUP (Container for everything)

### Step 1A: Navigate to Resource Groups

1. You're now at Azure Portal home page
2. **Look for the search bar** at the very top of the page (magnifying glass 🔍)
3. **Click the search bar** and type: `resource groups` (exactly)
4. **Press Enter** or click on the first result that says "Resource groups"

### Step 1B: Create New Resource Group

1. You're now on the "Resource groups" page
2. **Look for a blue button** on the left side that says **"+ Create"**
3. **Click it**
4. You'll see a form with these fields:

```
Subscription: [dropdown] - leave as default
Resource group name: [text field] - TYPE: baigiamasis-rg
Region: [dropdown] - SELECT: North Europe (or your closest)
```

5. **Click "Review + create"** button (bottom of page)
6. **Click "Create"** button (final confirmation)
7. **WAIT** - You'll see a loading spinner. Watch for "Deployment complete" message
8. **Click "Go to resource group"** when it appears

✅ **You now have a Resource Group!**

---

## SECTION 2: CREATE MYSQL DATABASE

### Step 2A: Find MySQL

1. **Go back to home page**: Click "Home" link (top left)
2. **Use search bar** again: type `Azure Database for MySQL`
3. **Click the result** (should say "Azure Database for MySQL")

### Step 2B: Create MySQL Server

1. You're on the "Azure Database for MySQL" page
2. **Click blue "+ Create"** button
3. **Important**: A popup might appear asking to choose "Single Server" or "Flexible Server"
   - **SELECT: "Single Server"** (it's free tier friendly)
4. **You'll see a form**. Fill in EXACTLY:

| Field             | Value                                              |
| ----------------- | -------------------------------------------------- |
| Subscription      | (leave default)                                    |
| Resource group    | **baigiamasis-rg** (click dropdown, search for it) |
| Server name       | **baigiamasis-db**                                 |
| Data source       | Select: **None**                                   |
| Location          | **North Europe** (same as resource group)          |
| Version           | **8.0** (latest)                                   |
| Compute + Storage | Click the text field, select **B1s - Burstable**   |
| Admin username    | **root**                                           |
| Password          | **MyDb@Pass123!** (SAVE THIS!)                     |
| Confirm password  | **MyDb@Pass123!** (same)                           |

5. **Scroll down** and check box: **"Allow access to Azure services"**
6. **Click "Review + create"**
7. **Click "Create"**
8. **WAIT 5-10 MINUTES** - This takes a while. You'll see "Deployment in progress"
9. When done, **click "Go to resource"**

✅ **You now have MySQL Server!**

---

## SECTION 3: CONFIGURE MYSQL DATABASE

### Step 3A: Create the Database

1. You're now on your MySQL server page (you should see "baigiamasis-db" in the title)
2. **Look at left side menu** - find option that says **"Databases"**
3. **Click "Databases"**
4. **Click "+ Add"** button (blue, top right)
5. **In the popup**:
   - Database name: type `baigiamasis`
   - Click **"OK"**

✅ **Database created!**

### Step 3B: Configure Firewall

1. Still on MySQL server page
2. **In left menu**, find **"Connection security"**
3. **Click it**
4. Look for toggle labeled **"Allow access to Azure services"**
   - Make sure it's **ON** (toggle should be blue)
5. Below that, you'll see **"Firewall rules"** section
6. **Click "+ Add current client IP"** button
   - This adds your home computer's IP
7. **Add another rule manually**:
   - Click **"+ Add firewall rule"** button
   - Rule name: `AllowAll`
   - Start IP: `0.0.0.0`
   - End IP: `255.255.255.255`
   - Click **"OK"**
8. **Click "Save"** button (top blue button)

✅ **MySQL is configured!**

---

## SECTION 4: CREATE APP SERVICE PLAN

### Step 4A: Navigate to App Service Plans

1. **Home button** (top left) → go to home
2. **Search bar**: type `App Service plans`
3. **Click result** that says "App Service plans"

### Step 4B: Create Plan

1. Click blue **"+ Create"** button
2. **Fill in the form**:

| Field            | Value                          |
| ---------------- | ------------------------------ |
| Subscription     | (default)                      |
| Resource group   | **baigiamasis-rg**             |
| Name             | **baigiamasis-plan**           |
| Operating System | **Windows**                    |
| Region           | **North Europe**               |
| Pricing tier     | Click it, select **B1 (Free)** |

3. **Click "Review + create"** → **Click "Create"**
4. **WAIT** for "Deployment complete"
5. **Click "Go to resource"**

✅ **App Service Plan created!**

---

## SECTION 5: CREATE APP SERVICE (Your Web App)

### Step 5A: Navigate to App Services

1. **Home** → Home page
2. **Search bar**: type `App Services`
3. **Click result** that says "App Services"

### Step 5B: Create App Service

1. Click blue **"+ Create"** button
2. **Fill in the form**:

| Field            | Value                                       |
| ---------------- | ------------------------------------------- |
| Subscription     | (default)                                   |
| Resource group   | **baigiamasis-rg**                          |
| Name             | **baigiamasis-app**                         |
| Publish          | Select: **Code**                            |
| Runtime stack    | Select: **.NET 8 (LTS)**                    |
| Operating System | **Windows**                                 |
| App Service Plan | Click dropdown, select **baigiamasis-plan** |

3. **Click "Review + create"** → **Click "Create"**
4. **WAIT** for completion
5. **Click "Go to resource"**

✅ **App Service created!**

---

## SECTION 6: SET ENVIRONMENT VARIABLES (Database Connection)

### Step 6A: Open Configuration

1. You're on App Service page (title shows "baigiamasis-app")
2. **In left menu**, find **"Configuration"**
3. **Click it**
4. You'll see a page with "Application settings" tab

### Step 6B: Add Settings

For each setting below, **click "+ New application setting"** and fill in:

**Setting 1:**

- Name: `MySQL:Db`
- Value: `baigiamasis`
- Click **"OK"**

**Setting 2:**

- Name: `MySQL:User`
- Value: `root@baigiamasis-db`
- Click **"OK"**

**Setting 3:**

- Name: `MySQL:Password`
- Value: `MyDb@Pass123!` (the password from Step 2B)
- Click **"OK"**

**Setting 4:**

- Name: `Admin:Email`
- Value: `admin@local`
- Click **"OK"**

**Setting 5:**

- Name: `Admin:Password`
- Value: `Passw0rd!`
- Click **"OK"**

**Setting 6:**

- Name: `ASPNETCORE_ENVIRONMENT`
- Value: `Production`
- Click **"OK"**

### Step 6C: Save Settings

1. **Click "Save"** button (blue, top of page)
2. A popup appears asking "Are you sure?"
3. **Click "Continue"**
4. **WAIT** for settings to apply (takes 30 seconds)

✅ **Environment variables set!**

---

## SECTION 7: DEPLOY YOUR CODE

### Step 7A: Setup Git Deployment

1. Still on App Service page
2. **In left menu**, find **"Deployment Center"**
3. **Click it**
4. Under "Source", select **"Local Git"** (radio button)
5. **Click "Save"** button (top)
6. **WAIT** 30 seconds for setup

### Step 7B: Get Git Credentials

1. You're still on Deployment Center page
2. **Scroll down** to find section labeled **"Local Git/FTPS credentials"**
3. You'll see:
   - **Git Clone Url**: looks like `https://baigiamasis-app.scm.azurewebsites.net/baigiamasis-app.git`
   - **Username**: looks like `baigiamasis-app\deploymentuser`
   - **Password**: a long random string

4. **COPY and SAVE all three** (open Notepad and paste them!)

### Step 7C: Deploy from Your Computer

1. **Open terminal/command prompt** on your computer
2. **Navigate to your project folder**:
   ```
   cd C:\Users\robot\Desktop\Baigiamasis-master
   ```
3. **Add Azure as remote** (paste your Git Clone Url):
   ```
   git remote add azure https://baigiamasis-app.scm.azurewebsites.net/baigiamasis-app.git
   ```
4. **Make sure your app is built**:
   ```
   npm run build
   ```
5. **Deploy to Azure**:
   ```
   git push azure main
   ```
6. **When prompted**:
   - Username: Paste from Step 7B
   - Password: Paste from Step 7B
7. **WAIT 3-5 MINUTES** - You'll see messages like:
   ```
   Compressing objects: 100%
   Writing objects: 100%
   remote: Deployment successful.
   ```

✅ **Your app is deployed!**

---

## SECTION 8: VERIFY IT WORKS

### Step 8A: Check App URL

1. Go back to your App Service in Azure
2. **At the top**, you'll see a URL like: `https://baigiamasis-app.azurewebsites.net`
3. **Click that URL** (or copy-paste in browser)
4. **You should see your app loading!** 🎉

### Step 8B: Verify Database Connection

1. If app loads but shows errors:
   - Click **"Log stream"** in left menu
   - You'll see real-time logs
   - Look for error messages about database
2. Common error: "Access denied" = database credentials are wrong
   - Go back to Step 6B and verify credentials

✅ **Deployment complete!**

---

## SECTION 9: MAKE UPDATES

Every time you change code:

1. **On your computer**:

   ```
   npm run build
   git add .
   git commit -m "Your changes"
   git push azure main
   ```

2. **Wait 2-3 minutes** for deployment
3. **Refresh your app URL** in browser

That's it! Changes go live automatically.

---

## Troubleshooting

### App shows 404 or "Cannot find module"

- Check logs: Deployment Center → Log stream
- Usually means build failed
- Make sure `npm run build` works on your computer first

### Database connection error

- Check Configuration (Step 6)
- Verify credentials match MySQL setup (Step 2B)
- Go to MySQL → Connection Security → verify firewall rules

### Deployment fails

- Go to Deployment Center → Logs
- Check error message
- Common: Git credentials wrong (recopy from Step 7B)

---

## You're Done! 🎉

Your app is now live at: **https://baigiamasis-app.azurewebsites.net**

Everything is free (Azure gives you $200 free credit).
