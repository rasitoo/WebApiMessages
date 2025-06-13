# WebApiMessages

**WebApiMessages** es una API RESTful construida con ASP.NET Core (.NET 8) que permite la gestión de chats y mensajes en tiempo real utilizando SignalR. Incluye autenticación JWT, documentación Swagger y persistencia en PostgreSQL mediante Entity Framework Core.

## Características principales

- **Gestión de chats:** Crear, consultar, actualizar y eliminar chats.
- **Mensajería en tiempo real:** Notificaciones instantáneas de creación, actualización y eliminación de chats y mensajes usando SignalR.
- **Autenticación JWT:** Acceso seguro a los endpoints mediante tokens JWT.
- **Swagger:** Documentación interactiva de la API.
- **Persistencia:** Almacenamiento de datos en PostgreSQL usando Entity Framework Core.

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/)
- (Opcional) Docker

## Configuración

1. **Clona el repositorio:**

2. **Configura las variables de entorno** en `appsettings.json` o mediante variables de entorno para la conexión a la base de datos y la clave JWT:
   
```json
   "DbSettings": {
     "Host": "localhost",
     "Port": "5432",
     "Username": "usuario",
     "Password": "contraseña",
     "Database": "WebApiMessagesDb"
   },
   "JwtSettings": {
     "SecretKey": "clave-secreta-segura"
   }
   
```

3. **Aplica las migraciones de la base de datos:**
   
```shell
   dotnet ef migrations add InitialCreate
   dotnet ef database update
```

4. **Ejecuta la aplicación:**
   
```shell
   dotnet run
```

## Uso

- Accede a la documentación Swagger en: `http://localhost:<puerto>/swagger`
- Los endpoints principales se encuentran bajo `/api/Chats` y `/api/Messages`.
- Para acceder a los endpoints protegidos, debes autenticarte y proporcionar el token JWT en el header `Authorization` como `Bearer <token>`.

## Tecnologías utilizadas

- ASP.NET Core 8
- Entity Framework Core
- SignalR
- PostgreSQL
- Swagger (Swashbuckle)
- Autenticación JWT

## Docker

Puedes construir y ejecutar el proyecto usando Docker:


```shell
docker build -t webapimessages .
docker run -p 5000:80 --env-file .env webapimessages

```

## Licencia

Este proyecto está bajo la licencia MIT.
