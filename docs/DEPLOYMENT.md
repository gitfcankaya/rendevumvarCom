# Deployment Guide - RendevumVar

## Overview

This guide covers the deployment process for the RendevumVar SaaS Salon Appointment System. The application consists of:
- **Backend**: .NET 9.0 Web API
- **Frontend**: React 19 + TypeScript + Vite
- **Database**: Microsoft SQL Server 2022

## Prerequisites

### For Docker Deployment
- Docker Engine 20.10+
- Docker Compose 2.0+

### For Manual Deployment
- .NET 9.0 SDK
- Node.js 20+
- SQL Server 2019+ (or SQL Server Express)
- nginx (for production frontend)

## Environment Configuration

### Backend Configuration

Create or update `src/RendevumVar.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=RendevumVar;User Id=your-user;Password=your-password;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Frontend Configuration

The frontend uses environment variables for configuration. Create the appropriate `.env` file:

#### Development (.env.development)
```bash
VITE_API_URL=http://localhost:5000
```

#### Production (.env.production)
```bash
VITE_API_URL=/api
```

**Note**: In production, nginx proxies `/api` requests to the backend service.

## Docker Deployment (Recommended)

### Using Docker Compose

1. **Clone the repository**:
```bash
git clone https://github.com/gitfcankaya/rendevumvarCom.git
cd rendevumvarCom
```

2. **Update environment variables** in `docker-compose.yml`:
```yaml
# Update the SQL Server password
SA_PASSWORD: "YourStrong@Password123"

# Update the connection string in the API service
ConnectionStrings__DefaultConnection: "Server=sqlserver;Database=RendevumVar;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True"
```

3. **Start all services**:
```bash
docker-compose up -d
```

4. **Verify services are running**:
```bash
docker-compose ps
```

5. **Access the application**:
- Frontend: http://localhost:3000
- Backend API: http://localhost:5000
- SQL Server: localhost:1433

6. **View logs**:
```bash
docker-compose logs -f
```

7. **Stop services**:
```bash
docker-compose down
```

### Building Individual Docker Images

#### Backend
```bash
docker build -f Dockerfile.api -t rendevumvar-api:latest .
docker run -p 5000:80 -e ConnectionStrings__DefaultConnection="your-connection-string" rendevumvar-api:latest
```

#### Frontend
```bash
docker build -f Dockerfile.frontend -t rendevumvar-frontend:latest .
docker run -p 3000:80 rendevumvar-frontend:latest
```

## Manual Deployment

### Backend Deployment

1. **Build the backend**:
```bash
cd src/RendevumVar.API
dotnet build -c Release
```

2. **Run database migrations**:
```bash
cd ../RendevumVar.Infrastructure
dotnet ef database update --startup-project ../RendevumVar.API
```

3. **Publish the backend**:
```bash
cd ../RendevumVar.API
dotnet publish -c Release -o /path/to/publish
```

4. **Run the backend**:
```bash
cd /path/to/publish
dotnet RendevumVar.API.dll
```

### Frontend Deployment

1. **Install dependencies**:
```bash
cd frontend
npm install
```

2. **Build for production**:
```bash
npm run build
```

3. **Serve with nginx**:
```bash
# Copy build output to nginx directory
sudo cp -r dist/* /var/www/html/

# Copy nginx configuration
sudo cp nginx.conf /etc/nginx/sites-available/rendevumvar
sudo ln -s /etc/nginx/sites-available/rendevumvar /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

## Azure Deployment

### Prerequisites
- Azure subscription
- Azure CLI installed

### Deploy Backend to Azure App Service

1. **Create App Service**:
```bash
az webapp create \
  --name rendevumvar-api \
  --resource-group YourResourceGroup \
  --plan YourAppServicePlan \
  --runtime "DOTNET|9.0"
```

2. **Create SQL Database**:
```bash
az sql db create \
  --name RendevumVar \
  --server your-sql-server \
  --resource-group YourResourceGroup \
  --service-objective S0
```

3. **Configure connection string**:
```bash
az webapp config connection-string set \
  --name rendevumvar-api \
  --resource-group YourResourceGroup \
  --settings DefaultConnection="your-connection-string" \
  --connection-string-type SQLAzure
```

4. **Deploy from GitHub**:
```bash
az webapp deployment source config \
  --name rendevumvar-api \
  --resource-group YourResourceGroup \
  --repo-url https://github.com/gitfcankaya/rendevumvarCom \
  --branch main \
  --manual-integration
```

### Deploy Frontend to Azure Static Web Apps

1. **Create Static Web App**:
```bash
az staticwebapp create \
  --name rendevumvar-frontend \
  --resource-group YourResourceGroup \
  --source https://github.com/gitfcankaya/rendevumvarCom \
  --location "West Europe" \
  --branch main \
  --app-location "frontend" \
  --api-location "" \
  --output-location "dist"
```

2. **Configure environment variables** in Azure Portal:
- Navigate to Static Web App > Configuration
- Add: `VITE_API_URL=/api`

## Health Checks

### Backend Health Check
```bash
curl http://localhost:5000/weatherforecast
```

### Frontend Health Check
```bash
curl http://localhost:3000
```

### Database Health Check
```bash
sqlcmd -S localhost -U sa -P "YourPassword" -Q "SELECT 1"
```

## Troubleshooting

### Backend Issues

**Problem**: Build warnings about obsolete Entity Framework methods
**Solution**: Ensure you're using the latest code with updated `HasCheckConstraint` syntax.

**Problem**: Database connection fails
**Solution**: Verify connection string and ensure SQL Server is accessible.

### Frontend Issues

**Problem**: Build fails with TypeScript errors
**Solution**: Run `npm install` to ensure all dependencies are installed.

**Problem**: API calls fail in production
**Solution**: Verify nginx is properly configured to proxy `/api` requests to the backend.

### Docker Issues

**Problem**: SSL certificate errors during build
**Solution**: This is usually an environment-specific issue. Try:
```bash
docker build --network=host -f Dockerfile.api -t rendevumvar-api .
```

**Problem**: Container exits immediately
**Solution**: Check logs with `docker logs container-name`.

## Monitoring and Logging

### Application Insights (Azure)
Configure Application Insights key in `appsettings.json`:
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-key-here"
  }
}
```

### Log Files
- Backend logs: Check console output or configured log files
- Frontend logs: Browser developer console
- nginx logs: `/var/log/nginx/error.log` and `/var/log/nginx/access.log`

## Security Considerations

1. **Always use HTTPS in production**
2. **Change default SQL Server password**
3. **Use environment variables for secrets**
4. **Enable Azure Active Directory authentication** (recommended)
5. **Implement rate limiting**
6. **Keep dependencies updated**

## Backup and Recovery

### Database Backup
```bash
# Manual backup
sqlcmd -S localhost -U sa -P "YourPassword" \
  -Q "BACKUP DATABASE RendevumVar TO DISK = '/backup/RendevumVar.bak'"
```

### Automated Backups (Azure)
- Azure SQL Database provides automated backups
- Retention: 7-35 days (configurable)
- Point-in-time restore available

## CI/CD Pipeline

A GitHub Actions workflow can be set up for automated deployments. See `docs/SDD.md` for the recommended workflow configuration.

## Support

For deployment issues or questions:
- Open an issue on GitHub
- Email: support@rendevumvar.com
- Documentation: Check README.md and other docs in the `/docs` folder
