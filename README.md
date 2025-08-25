# Protected MCP Server - Webinar Tools

Un servidor MCP (Model Context Protocol) protegido con autenticación JWT que proporciona herramientas para obtener información de webinars y buscar speakers desde la API de LatinoNet.

## 🚀 Características

- **Autenticación JWT**: Protegido con tokens JWT de LatinoNet OAuth Server
- **Herramientas de Webinars**: Obtener próximos webinars de la plataforma LatinoNet
- **Búsqueda de Speakers**: Buscar speakers en la base de datos de LatinoNet
- **Herramientas del Clima**: Incluye herramientas de ejemplo para pronósticos del clima (Weather.gov)
- **Escalable para la Nube**: Configurado para deploy en Azure, AWS, GCP
- **Contenedorizado**: Incluye Dockerfile para deployment con Docker

## 🛠️ Tools Disponibles

### WebinarTools

1. **GetUpcomingWebinars**
   - Obtiene todos los próximos webinars de LatinoNet
   - Requiere autenticación JWT
   - Endpoint: `GET /api/v1/webinars-module/Proposals`

2. **SearchSpeakers**
   - Busca speakers en la base de datos de LatinoNet
   - Parámetros: `search` (término de búsqueda), `take` (máximo 50 resultados)
   - Requiere autenticación JWT
   - Endpoint: `GET /api/v1/webinars-module/Speakers/Search`

### WeatherTools (Ejemplo)

1. **GetAlerts** - Obtiene alertas meteorológicas por estado (US)
2. **GetForecast** - Obtiene pronóstico por coordenadas geográficas

## 🔧 Configuración

### Desarrollo Local

1. **Prerrequisitos**
   - .NET 9.0 SDK
   - Visual Studio 2022 o VS Code

2. **Configuración**
   ```bash
   git clone <repository-url>
   cd ProtectedMcpServer
   dotnet restore
   dotnet run
   ```

3. **Variables de Configuración**
   Edita `appsettings.json` para configuración local:
   ```json
   {
     "ServerConfiguration": {
       "BaseUrl": "http://localhost:7071/",
       "OAuthServerUrl": "https://ids.latinonet.online"
     },
     "ExternalApis": {
       "WebinarApi": {
         "BaseUrl": "https://api.latinonet.online",
         "TimeoutSeconds": 30
       }
     }
   }
   ```

### 🐳 Docker

1. **Build imagen**
   ```bash
   docker build -t protected-mcp-server .
   ```

2. **Ejecutar con Docker Compose**
   ```bash
   docker-compose up -d
   ```

### ☁️ Deploy en la Nube

#### Azure App Service

1. **Configurar variables de entorno**
   ```bash
   ASPNETCORE_ENVIRONMENT=Production
   ServerConfiguration__BaseUrl=https://your-app.azurewebsites.net/
   ServerConfiguration__OAuthServerUrl=https://ids.latinonet.online
   ```

2. **Deploy con GitHub Actions**
   - Configurar secretos: `AZURE_WEBAPP_PUBLISH_PROFILE_PRODUCTION`
   - Push a `main` branch activa el deployment

#### AWS/GCP

- Usar Dockerfile para deployment en ECS/Cloud Run
- Configurar variables de entorno según la plataforma

## 🔐 Autenticación

El servidor requiere un token JWT válido de LatinoNet OAuth Server:

1. **OAuth Server**: `https://ids.latinonet.online`
2. **Scopes**: `["latinonetonline_api", "openid", "profile"]`
3. **Headers requeridos**: `Authorization: Bearer <jwt-token>`

### Ejemplo de uso con curl

```bash
# Obtener webinars
curl -X GET \
  'http://localhost:7071/mcp/tools/GetUpcomingWebinars' \
  -H 'Authorization: Bearer <your-jwt-token>'

# Buscar speakers
curl -X GET \
  'http://localhost:7071/mcp/tools/SearchSpeakers?search=jorge&take=5' \
  -H 'Authorization: Bearer <your-jwt-token>'
```

## 📁 Estructura del Proyecto

```
ProtectedMcpServer/
├── Program.cs                 # Configuración principal y startup
├── Tools/
│   ├── WeatherTools.cs       # Herramientas del clima (ejemplo)
│   └── WebinarTools.cs       # Herramientas de webinars (principal)
├── appsettings.json          # Configuración desarrollo
├── appsettings.Production.json # Configuración producción
├── Dockerfile                # Imagen Docker
├── docker-compose.yml        # Orquestación local
└── .github/workflows/        # CI/CD GitHub Actions
```

## 🌐 Endpoints

- **MCP Server**: `http://localhost:7071/mcp`
- **Health Check**: `http://localhost:7071/health`
- **OAuth Metadata**: `http://localhost:7071/.well-known/oauth-protected-resource`

## 🧪 Testing

```bash
# Ejecutar tests
dotnet test

# Verificar health check
curl http://localhost:7071/health
```

## 📊 Monitoreo

El servidor incluye:
- Health checks para monitoreo de infraestructura
- Logging configurable por ambiente
- Timeouts configurables para APIs externas
- Manejo de errores escalable

## 🤝 Contribución

1. Fork el proyecto
2. Crea una feature branch (`git checkout -b feature/nueva-feature`)
3. Commit tus cambios (`git commit -am 'Add nueva feature'`)
4. Push a la branch (`git push origin feature/nueva-feature`)
5. Abre un Pull Request

## 📄 Licencia

Este proyecto está bajo la licencia MIT. Ver `LICENSE` para más detalles.