# Render.com Deployment - Complete Step-by-Step Guide

## Before You Start

- Your app is built: `npm run build`
- Git is initialized: `git init` ✓
- Code is pushed to GitHub: `git push` ✓

---

## STEP 1: Create Render Account

1. **Go to**: https://render.com
2. **Click "Get Started"** (top right)
3. **Sign up with GitHub** (easiest option)
4. **Authorize** Render to access your GitHub
5. **Confirm email** if prompted
6. You're now on the Render dashboard

---

## STEP 2: Create MySQL Database

### 2A: Navigate to Databases

1. On Render dashboard, click **"New +"** button (top right)
2. Select **"MySQL"** from the list
3. Fill in the form:

| Field            | Value                        |
| ---------------- | ---------------------------- |
| **Database**     | `baigiamasis`                |
| **Username**     | `root`                       |
| **Password**     | `MyDb@Pass123!` (SAVE THIS!) |
| **Region**       | Select closest to you        |
| **Billing Plan** | Leave "Free" selected        |

4. **Click "Create Database"**
5. **WAIT 2-3 MINUTES** for database to be created
6. Once created, you'll see the connection details. **COPY and SAVE these**:
   - **Internal Database URL** (starts with `mysql://`)
   - **Host**: Something like `dpg-xxxx.render.internal`
   - **Port**: `3306`
   - **Database**: `baigiamasis`
   - **User**: `root`
   - **Password**: `MyDb@Pass123!`

### 2B: Verify Database Created

- You should see a green checkmark next to the database name
- Status should say "Available"

---

## STEP 3: Create Web Service (Your App)

### 3A: Go to Services

1. Click **"New +"** button again
2. Select **"Web Service"**

### 3B: Connect GitHub Repository

1. A list of your GitHub repos appears
2. Find **"LinasSatkauskas/Baigiamasis"**
3. Click **"Connect"** next to it
4. If repo doesn't appear, click **"Configure account"** to authorize more repos

### 3C: Configure Web Service

Fill in the form:

| Field             | Value                      |
| ----------------- | -------------------------- |
| **Name**          | `baigiamasis-app`          |
| **Environment**   | Select `.NET`              |
| **Region**        | Same as your database      |
| **Branch**        | `main`                     |
| **Build Command** | `npm run build`            |
| **Start Command** | `./out/ReactApp1.Server`   |
| **Plan**          | Free (or paid if you want) |

4. **Click "Advanced"** to expand more options
5. **Add Environment Variables** (scroll down, see Step 4)

---

## STEP 4: Add Environment Variables

### Before Creating: Get Database URL

From Step 2B, copy your **Internal Database URL**. It looks like:

```
mysql://root:MyDb@Pass123!@dpg-xxxx.render.internal:3306/baigiamasis
```

### Add Variables to Web Service

In the "Advanced" section, you'll see **"Environment"** field.

**Click "Add Environment Variable"** for each one:

| Key                      | Value                                           |
| ------------------------ | ----------------------------------------------- |
| `MySQL_Host`             | `dpg-xxxx.render.internal` (your database host) |
| `MySQL_Db`               | `baigiamasis`                                   |
| `MySQL_User`             | `root`                                          |
| `MySQL_Password`         | `MyDb@Pass123!`                                 |
| `Admin_Email`            | `admin@local`                                   |
| `Admin_Password`         | `Passw0rd!`                                     |
| `ASPNETCORE_ENVIRONMENT` | `Production`                                    |

---

## STEP 5: Deploy Your App

### 5A: Complete Web Service Creation

1. After adding all environment variables, scroll down
2. Click **"Create Web Service"** button
3. **WAIT 3-5 MINUTES** for build and deployment

### 5B: Watch the Logs

1. You'll see a "Build log" tab appear
2. Watch it build your app
3. Look for message like: **"Deployment live on https://baigiamasis-app.onrender.com"**

### 5C: Check Status

1. Once deployed, you should see a green "Running" status
2. Click the URL (like `https://baigiamasis-app.onrender.com`) to test your app

---

## STEP 6: Connect Database to App

This is important - the database needs to be linked to accept connections from your app:

1. Go back to your **MySQL database** service in Render
2. Look for **"Connections"** section
3. You should see your web service listed
4. If not, click **"Add Connection"** and select your web service

---

## STEP 7: Test Your App

1. **Open your app URL** in browser (from Step 5C)
2. You should see your app loading!
3. Try logging in with:
   - Email: `admin@local`
   - Password: `Passw0rd!`
4. If login works, the database is connected! ✅

---

## If You Get Errors

### Error: "Cannot connect to database"

- Go to web service **"Logs"** tab to see error messages
- Check that all environment variables match exactly
- Verify database is still running (check MySQL service status)

### Error: "Build failed"

- Check the build log for error details
- Common issue: Missing dependencies
- Run `npm run build` locally first to verify

### App crashes after deployment

- Go to **"Logs"** tab in Render
- Look for error messages
- Usually database connection issues - verify all credentials

---

## Update Your App (After Deployment)

Every time you make changes:

1. **Commit and push to GitHub**:

   ```bash
   git add .
   git commit -m "Your changes"
   git push
   ```

2. **Render auto-detects** the push and redeploys automatically
3. **Wait 2-3 minutes** for new deployment
4. Your changes are live!

---

## Your App URL

Once deployed, your app will be live at:

```
https://baigiamasis-app.onrender.com
```

(Replace `baigiamasis-app` with whatever name you chose in Step 3C)

---

## Free Tier Limits

✅ **What's included (free):**

- Unlimited deployments
- 1 database (500MB storage)
- Automatic SSL/HTTPS
- Auto-deploys on git push

⚠️ **Limitations:**

- App spins down after 15 min of inactivity (cold start)
- Database limited to 500MB
- Slower performance than paid tiers

---

## You're Done! 🎉

Your app is now deployed on Render and live online!

**Share your URL**: https://baigiamasis-app.onrender.com
