# PetFeeder API

API REST (ASP.NET Core 8) para la app PetFeeder — autenticación (registro, verificación por código OTP vía correo y login) sobre SQL Server.

App Android (cliente): https://github.com/IDGS-905-23001546/PetFeeder

---

## ✅ Requisitos previos

- **.NET 8 SDK**
- **SQL Server** (instancia local; el proyecto usa `localhost` con autenticación de Windows)
- **Visual Studio 2022** (o `dotnet` CLI)
- Una cuenta de **Gmail** con **verificación en 2 pasos** y una **contraseña de aplicación** (para enviar los correos OTP)

---

## 🚀 Cómo levantar el proyecto (paso a paso)

### 1. Clonar el repositorio
```bash
git clone https://github.com/IDGS-905-23001546/PetFeeder.API.git
cd PetFeeder.API
```

### 2. Crear tu `appsettings.json`
El archivo `appsettings.json` **NO está en el repo** (contiene secretos). Hay una plantilla:
`PetFeeder.API/appsettings.example.json`.

Cópiala y renómbrala a `appsettings.json` en la misma carpeta (`PetFeeder.API/`), luego llena tus valores:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=petfeeder_db;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "FromEmail": "TU-CORREO@gmail.com",
    "FromName": "PetFeeder",
    "AppPassword": "TU_APP_PASSWORD_DE_GMAIL_DE_16_CARACTERES"
  },
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*"
}
```

> **¿Cómo sacar la App Password de Gmail?**
> 1. Activa la verificación en 2 pasos: https://myaccount.google.com/security
> 2. Genera la contraseña de app: https://myaccount.google.com/apppasswords
> 3. Pega los 16 caracteres **sin espacios** en `AppPassword`.

### 3. Crear la base de datos
Ejecuta el script **`database/petfeeder_db_sqlserver.sql`** en tu SQL Server
(ábrelo en SSMS y dale Execute, o `sqlcmd -S localhost -E -i database/petfeeder_db_sqlserver.sql`).

El script crea la base `petfeeder_db` con sus 9 tablas (`usuarios`, `otp_verificacion`,
`sesiones`, `mascotas`, `dispensadores`, `horarios`, `dispensaciones`,
`telemetria_dispensador`, `notificaciones`), 3 vistas y datos de prueba.

Incluye un **usuario admin de prueba** ya listo para iniciar sesión:
- correo: `carlosriosrmz17@gmail.com`
- contraseña: `CarlosRMZ17`

> El script es re-ejecutable (borra y recrea las tablas en cada corrida).

### 4. Ejecutar la API
- En Visual Studio: abre `PetFeeder.API.sln` y presiona **F5** (perfil **https**).
- O por CLI:
  ```bash
  cd PetFeeder.API
  dotnet run --launch-profile https
  ```

La API queda en:
- HTTPS: `https://localhost:7127`
- HTTP:  `http://localhost:5172`
- Swagger (pruebas): `https://localhost:7127/swagger`

---

## 🔌 Endpoints de autenticación

| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/api/auth/registro`  | Crea usuario (contraseña encriptada con BCrypt) y envía código OTP al correo |
| `POST` | `/api/auth/verificar` | Valida el código OTP y marca la cuenta como verificada |
| `POST` | `/api/auth/login`     | Inicia sesión validando correo + contraseña |

---

## 📱 Conexión con la app Android

- La app apunta a la API mediante la URL en `ApiConfig.kt` del proyecto Android.
- **Emulador:** `http://10.0.2.2:5172/`
- **Teléfono físico:** `http://IP_DE_TU_PC:5172/` (misma WiFi + firewall abierto para el puerto 5172).
- Cada integrante corre **su propia** API local; cada quien apunta su app a su propia máquina.

---

## 🔒 Notas de seguridad (desarrollo)

- `appsettings.json` está en `.gitignore` — **nunca** subas tu App Password al repo.
- En `Program.cs`, `app.UseHttpsRedirection()` está comentado para permitir las pruebas por HTTP desde el emulador/teléfono. En producción debe reactivarse y usar HTTPS.
