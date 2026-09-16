# database/ — Persistencia

El backend usa **SQLite** (`SecureDevOps.API/SecureDevOpsDb.db`) con EF Core 8.

## Detalles

- El archivo de BD se crea y migra automáticamente al arrancar (`db.Database.Migrate()`).
- En desarrollo: `SecureDevOps.API/SecureDevOpsDb.db` (en `.gitignore` vía `*.db`).
- En docker-compose: el archivo vive en el **volumen `sqlite-data`** montado en
  `/app/data` (variable `ConnectionStrings__DefaultConnection`).
- Existencia de modelo: `User` y `TaskItem` (ver `SecureDevOps.API/Migrations/`).

## Seed (usuarios demo)

Se siembran 3 usuarios si la tabla está vacía:
`juan@gmail.com` / `prueba@gmail.com` / `admin@gmail.com` (password `contra1234`, admin
`Admin123!`) — SOLO para demos; en entornos reales elimina el seed o protégele con
variable de entorno.

## Migraciones

```bash
cd SecureDevOps.API
dotnet ef migrations add NombreDeMigracion
dotnet ef database update
```

> Scripts SQL específicos (init) no son necesarios porque EF Core genera el esquema.
> Si migras a Postgres/MySQL, cambiar el provider en `Program.cs` y la cadena de conexión;
> el modelo no cambia.