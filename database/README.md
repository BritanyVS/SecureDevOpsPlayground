# Base de datos

SQLite mediante EF Core. Sin servidor externo: el archivo vive en un **volumen Docker**
(`sqlite-data`, en `/app/data/SecureDevOpsDb.db` dentro del contenedor backend).

- En desarrollo local se usa `SecureDevOpsDb.db` en la raíz del proyecto (ver
  `SecureDevOps.API/appsettings.json`, ignorado por `.gitignore`).
- El seed crea los usuarios demo y ninguna tarea inicial.
- Para resetear en Docker: `docker compose down -v` y volver a levantar.

La BD es la sustrato de demostraciones dinámicas: la búsqueda con `%` en
`/api/taskitem/search` y `/api/lab/search` es la que permite probar **inyección SQL**.