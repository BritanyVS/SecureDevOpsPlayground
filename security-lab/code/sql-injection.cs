using System.Data.SqlClient;

namespace SnykLabCode.Scenarios;

// ⚠️ LABORATORIO: SQL INJECTION
// Código INTENCIONALMENTE vulnerable para que Snyk Code lo detecte.

public class SqlInjectionLab
{
    private readonly string _connString;

    public SqlInjectionLab(string connString)
    {
        _connString = connString;
    }

    // VULNERABLE: el input del usuario se concatena directamente en la consulta SQL.
    // GET /tasks?q=abc' UNION SELECT username,password FROM users--
    public List<string> SearchUsers(string q)
    {
        var results = new List<string>();
        using var conn = new SqlConnection(_connString);
        conn.Open();

        // FALLO: concatenación insegura → SQL Injection.
        using var cmd = new SqlCommand($"SELECT Username FROM Users WHERE Username LIKE '%{q}%'", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(reader.GetString(0));
        }
        return results;
    }

    // ✅ CORREGIDO: consulta parametrizada. El input NUNCA se interpreta como SQL.
    public List<string> SearchUsersSecure(string q)
    {
        var results = new List<string>();
        using var conn = new SqlConnection(_connString);
        conn.Open();

        using var cmd = new SqlCommand(
            "SELECT Username FROM Users WHERE Username LIKE @pattern", conn);
        cmd.Parameters.AddWithValue("@pattern", $"%{q}%");

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(reader.GetString(0));
        }
        return results;
    }
}