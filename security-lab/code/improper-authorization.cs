using Microsoft.EntityFrameworkCore;

namespace SnykLabCode.Scenarios;

// ⚠️ LABORATORIO: IMPROPER AUTHORIZATION (IDOR / BOLA)
// Código INTENCIONALMENTE vulnerable para que Snyk Code lo detecte.

public class ImproperAuthorizationLab
{
    // Este escenario refleja el bug REAL que existía en `TaskItemService` antes de esta
    // fase: GetById/Update/Delete no validaban que la tarea perteneciera al usuario del token.

    // VULNERABLE: se busca por id SIN comprobar el propietario.
    public async Task<TaskItem?> GetByIdAsync(DbSet<TaskItem> tasks, Guid id, Guid currentUserId)
    {
        // FALLO: un usuario podría pasar el id de una tarea AJENA y verla.
        return await tasks.FirstOrDefaultAsync(t => t.Id == id);
    }

    // ✅ CORREGIDO: el filtro incluye CreatedByUserId == currentUserId.
    public async Task<TaskItem?> GetByIdAsyncSecure(DbSet<TaskItem> tasks, Guid id, Guid currentUserId)
    {
        return await tasks.FirstOrDefaultAsync(t => t.Id == id && t.CreatedByUserId == currentUserId);
    }

    // VULNERABLE: delete sin comprobar propietario.
    public async Task<bool> DeleteAsync(DbSet<TaskItem> tasks, Guid id)
    {
        var task = await tasks.FirstOrDefaultAsync(t => t.Id == id);
        if (task is null) return false;

        tasks.Remove(task);
        await tasks.ToListAsync(); // simplificación: en producción se usa SaveChangesAsync
        return true;
    }
}

// Compat: tipo mínimo solo para que el ejemplo compile conceptualmente en la demo.
// En la app real es SecureDevOps.API.Models.TaskItem.
public class TaskItem
{
    public Guid Id { get; set; }
    public Guid CreatedByUserId { get; set; }
}