using System.Data;
using Microsoft.Data.Sqlite;

namespace SnykLab.Code
{
    // ⚠️ LABORATORIO: SQL Injection intencional (Snyk Code / SAST)
    public class InjectionExample
    {
        // VULNERABILIDAD: concatenación de SQL sin parametrizar
        public List<string> SearchUsers(string connectionString, string searchTerm)
        {
            var results = new List<string>();

            // FALLO: input del usuario concatenado directamente en SQL
            string query = "SELECT Username, Email FROM Users WHERE Username LIKE '%" + searchTerm + "%'";

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                results.Add(reader.GetString(0));
            }

            return results;
        }

        // VULNERABILIDAD: método ejecuta código sin validar
        public object ExecuteRawSql(string connectionString, string rawSql)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            var command = new SqliteCommand(rawSql, connection);
            return command.ExecuteScalar();
        }
    }
}