using Microsoft.Data.Sqlite;
using Dapper;
using NetScad.Core.Interfaces;

namespace NetScad.Designer.Repositories
{
    // Entity for CAD module dimensions (union/difference operations)
    public class ModuleDimensions : IScadObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ModuleType { get; set; } = string.Empty; // "Union" or "Difference"
        public string SolidType { get; set; } = string.Empty; // "Cube" or "Cylinder"
        public string ObjectName { get; set; } = string.Empty; // Reference to Object Name
        public string ObjectDescription { get; set; } = string.Empty; // Reference to Object Description
        public double XOffset_MM { get; set; } // X-axis translation offset
        public double YOffset_MM { get; set; } // Y-axis translation offset
        public double ZOffset_MM { get; set; } // Z-axis translation offset
        public double XOffset_IN { get; set; } // X-axis translation offset
        public double YOffset_IN { get; set; } // Y-axis translation offset
        public double ZOffset_IN { get; set; } // Z-axis translation offset
        public double XRotate { get; set; } = 0;
        public double YRotate { get; set; } = 0;
        public double ZRotate { get; set; } = 0;
        public string IncludeMethod { get; set; } = string.Empty;
        public string OSCADMethod { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string ModuleColor { get; set; } = string.Empty; // New property for module color

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "Id", Id },
            { "Name", Name },
            { "ModuleType", ModuleType },
            { "SolidType", SolidType },
            { "ObjectName", ObjectName },
            { "ObjectDescription", ObjectDescription },
            { "XOffset_MM", XOffset_MM },
            { "YOffset_MM", YOffset_MM },
            { "ZOffset_MM", ZOffset_MM },
            { "XOffset_MM", XOffset_IN },
            { "YOffset_MM", YOffset_IN },
            { "ZOffset_MM", ZOffset_IN },
            { "XRotate", XRotate },
            { "YRotate", YRotate },
            { "ZRotate", ZRotate },
            { "OSCADMethod", OSCADMethod },
            { "IncludeMethod", IncludeMethod },
            { "CreatedAt", CreatedAt },
            { "ModuleColor", ModuleColor },
        };
    }

    // Extension methods for ModuleDimensions CRUD operations
    public static class ModuleDimensionsExtensions
    {
        // Property mapping for ModuleDimensions (AOT-safe)
        private static readonly (string Name, Type Type, bool IsNullable)[] Properties =
        [
            (nameof(ModuleDimensions.Id), typeof(int), false),
            (nameof(ModuleDimensions.Name), typeof(string), false),
            (nameof(ModuleDimensions.ModuleType), typeof(string), false),
            (nameof(ModuleDimensions.SolidType), typeof(string), false),
            (nameof(ModuleDimensions.ObjectName), typeof(string), false),
            (nameof(ModuleDimensions.ObjectDescription), typeof(string), false),
            (nameof(ModuleDimensions.XOffset_MM), typeof(double), false),
            (nameof(ModuleDimensions.YOffset_MM), typeof(double), false),
            (nameof(ModuleDimensions.ZOffset_MM), typeof(double), false),
            (nameof(ModuleDimensions.XOffset_IN), typeof(double), false),
            (nameof(ModuleDimensions.YOffset_IN), typeof(double), false),
            (nameof(ModuleDimensions.ZOffset_IN), typeof(double), false),
            (nameof(ModuleDimensions.XRotate), typeof(double), false),
            (nameof(ModuleDimensions.YRotate), typeof(double), false),
            (nameof(ModuleDimensions.ZRotate), typeof(double), false),
            (nameof(ModuleDimensions.OSCADMethod), typeof(string), false),
            (nameof(ModuleDimensions.IncludeMethod), typeof(string), false),
            (nameof(ModuleDimensions.CreatedAt), typeof(DateTime), false),
            (nameof(ModuleDimensions.ModuleColor), typeof(string), false)
        ];

        // Create table
        public static async Task CreateTable(this SqliteConnection connection)
        {
            IEnumerable<string> columns = Properties.Select(p =>
                $"{p.Name} {(p.Type == typeof(int) ? "INTEGER" : p.Type == typeof(string) ? "TEXT" : p.Type == typeof(DateTime) ? "TEXT" : "TEXT")} " +
                (p.Name == "Id" ? "PRIMARY KEY AUTOINCREMENT" : p.IsNullable ? "" : "NOT NULL")
            );
            await connection.ExecuteAsync($"CREATE TABLE IF NOT EXISTS ModuleDimensions ({string.Join(", ", columns)})");
        }

        // Insert a single ModuleDimensions and return the new Id
        public static async Task<int> InsertAsync(this ModuleDimensions entity, SqliteConnection connection)
        {
            IEnumerable<string> columns = Properties.Where(p => p.Name != "Id").Select(p => p.Name);
            IEnumerable<string> parameters = columns.Select(c => $"@{c}");
            string sql = $"INSERT INTO ModuleDimensions ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)}); SELECT last_insert_rowid();";
            var id = await connection.ExecuteScalarAsync<int>(sql, entity);
            entity.Id = id; // Update the entity with the new Id
            return id;
        }

        // Upsert (INSERT OR REPLACE) and return the Id - Only updates if Name, SolidType, and ModuleType match
        public static async Task<int> UpsertAsync(this ModuleDimensions entity, SqliteConnection connection)
        {
            // First, try to find an existing record with matching Name, SolidType, and ModuleType
            const string selectSql = "SELECT Id FROM ModuleDimensions WHERE Name = @Name AND SolidType = @SolidType AND ModuleType = @ModuleType LIMIT 1";
            var existingId = await connection.QuerySingleOrDefaultAsync<int?>(selectSql, new { entity.Name, entity.SolidType, entity.ModuleType });
            
            if (existingId.HasValue)
            {
                // Update the existing record
                entity.Id = existingId.Value;
                IEnumerable<string> setClause = Properties.Where(p => p.Name != "Id").Select(p => $"{p.Name} = @{p.Name}");
                string updateSql = $"UPDATE ModuleDimensions SET {string.Join(", ", setClause)} WHERE Id = @Id";
                await connection.ExecuteAsync(updateSql, entity);
                return existingId.Value;
            }
            else
            {
                // Insert as a new record
                IEnumerable<string> columns = Properties.Where(p => p.Name != "Id").Select(p => p.Name);
                IEnumerable<string> parameters = columns.Select(c => $"@{c}");
                string insertSql = $"INSERT INTO ModuleDimensions ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)}); SELECT last_insert_rowid();";
                var id = await connection.ExecuteScalarAsync<int>(insertSql, entity);
                entity.Id = id;
                return id;
            }
        }

        // Update
        public static async Task UpdateAsync(this ModuleDimensions entity, SqliteConnection connection)
        {
            IEnumerable<string> setClause = Properties.Where(p => p.Name != "Id").Select(p => $"{p.Name} = @{p.Name}");
            string sql = $"UPDATE ModuleDimensions SET {string.Join(", ", setClause)} WHERE Id = @Id";
            await connection.ExecuteAsync(sql, entity);
        }

        public static async Task UpdateIncludeMethodByNameDescriptionSolidTypeAsync(this ModuleDimensions entity, SqliteConnection connection)
        {
            string updateSql = $"UPDATE ModuleDimensions SET IncludeMethod = @IncludeMethod WHERE ObjectName = @ObjectName AND SolidType = @SolidType AND ObjectDescription = @ObjectDescription";
            await connection.ExecuteAsync(updateSql, entity);
        }

        // Delete by Id
        public static async Task DeleteAsync(this ModuleDimensions entity, SqliteConnection connection) => 
            await connection.ExecuteAsync("DELETE FROM ModuleDimensions WHERE Id = @Id", new { entity.Id });

        // Select by Id
        public static async Task<ModuleDimensions?> GetByIdAsync(this ModuleDimensions _, SqliteConnection connection, int id) => 
            await connection.QuerySingleOrDefaultAsync<ModuleDimensions>("SELECT * FROM ModuleDimensions WHERE Id = @Id", new { Id = id });

        // Select all
        public static async Task<IEnumerable<ModuleDimensions>> GetAllAsync(this ModuleDimensions _, SqliteConnection connection) => 
            await connection.QueryAsync<ModuleDimensions>("SELECT * FROM ModuleDimensions ORDER BY CreatedAt DESC");

        // Get by Name
        public static async Task<ModuleDimensions?> GetByNameAsync(this ModuleDimensions _, SqliteConnection connection, string name) => 
            await connection.QuerySingleOrDefaultAsync<ModuleDimensions>("SELECT * FROM ModuleDimensions WHERE Name = @Name", new { Name = name });

        // Get by ModuleType
        public static async Task<IEnumerable<ModuleDimensions>> GetByModuleTypeAsync(this ModuleDimensions _, SqliteConnection connection, string moduleType) => 
            await connection.QueryAsync<ModuleDimensions>(
                "SELECT * FROM ModuleDimensions WHERE ModuleType = @ModuleType ORDER BY CreatedAt DESC",
                new { ModuleType = moduleType });

        // Get by SolidType
        public static async Task<IEnumerable<ModuleDimensions>> GetBySolidTypeAsync(this ModuleDimensions _, SqliteConnection connection, string solidType) => 
            await connection.QueryAsync<ModuleDimensions>(
                "SELECT * FROM ModuleDimensions WHERE SolidType = @SolidType ORDER BY CreatedAt DESC",
                new { SolidType = solidType });

        // Get by ObjectName
        public static async Task<IEnumerable<ModuleDimensions>> GetByObjectNameAsync(this ModuleDimensions _, SqliteConnection connection, string objectName) => 
            await connection.QueryAsync<ModuleDimensions>(
                "SELECT * FROM ModuleDimensions WHERE ObjectName = @ObjectName ORDER BY ModuleType ASC, SolidType ASC",
                new { ObjectName = objectName });

        // Get by Name and ModuleType
        public static async Task<IEnumerable<ModuleDimensions>> GetByNameAndModuleTypeAsync(this ModuleDimensions _, SqliteConnection connection, string name, string moduleType) => 
            await connection.QueryAsync<ModuleDimensions>(
                "SELECT * FROM ModuleDimensions WHERE Name = @Name AND ModuleType = @ModuleType ORDER BY ModuleType ASC, SolidType ASC",
                new { Name = name, ModuleType = moduleType });

// Add this extension method to the ModuleDimensionsExtensions class
public static async Task UpdateColorAsync(this ModuleDimensions module, SqliteConnection connection, string color)
        {
            const string sql = @"
            UPDATE ModuleDimensions 
            SET ModuleColor = @ModuleColor 
            WHERE Id = @Id";

            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@ModuleColor", color);
            cmd.Parameters.AddWithValue("@Id", module.Id);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}