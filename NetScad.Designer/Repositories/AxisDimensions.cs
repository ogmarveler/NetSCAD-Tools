using Microsoft.Data.Sqlite;
using Dapper;
using NetScad.Core.Interfaces;
using static NetScad.Core.Measurements.Selector;

namespace NetScad.Designer.Repositories
{
    // Entity for CAD axis dimensions
    public class AxisDimensions : IScadObject
    {
        public int Id { get; set; }
        public string Theme { get; set; }
        public string Unit { get; set; }
        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }
        public double MinZ { get; set; }
        public double MaxZ { get; set; }
        public string OSCADMethod { get; set; }
        public string IncludeMethod => "use <Axes/axes.scad>;";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "Id", Id },
            { "Theme", Theme },
            { "Unit", Unit },
            { "MinX", MinX },
            { "MaxX", MaxX },
            { "MinY", MinY },
            { "MaxY", MaxY },
            { "MinZ", MinZ },
            { "MaxZ", MaxZ },
            { "IncludeMethod", IncludeMethod },
            { "OSCADMethod", OSCADMethod },
            { "CreatedAt", CreatedAt }
        };
    }

    // Extension methods for AxisDimensions CRUD operations
    public static class AxisDimensionsExtensions
    {
        // Property mapping for AxisDimensions (AOT-safe)
        private static readonly (string Name, Type Type, bool IsNullable)[] Properties =
        [
            (nameof(AxisDimensions.Id), typeof(int), false),
            (nameof(AxisDimensions.Theme), typeof(string), false),
            (nameof(AxisDimensions.Unit), typeof(string), false),
            (nameof(AxisDimensions.MinX), typeof(double), false),
            (nameof(AxisDimensions.MaxX), typeof(double), false),
            (nameof(AxisDimensions.MinY), typeof(double), false),
            (nameof(AxisDimensions.MaxY), typeof(double), false),
            (nameof(AxisDimensions.MinZ), typeof(double), false),
            (nameof(AxisDimensions.MaxZ), typeof(double), false),
            (nameof(AxisDimensions.IncludeMethod), typeof(string), false),
            (nameof(AxisDimensions.OSCADMethod), typeof(string), true),
            (nameof(AxisDimensions.CreatedAt), typeof(DateTime), false)
        ];

        // Create table
        public static async Task CreateTable(this SqliteConnection connection)
        {
            IEnumerable<string> columns = Properties.Select(p =>
                $"{p.Name} {(p.Type == typeof(int) ? "INTEGER" : p.Type == typeof(string) ? "TEXT" : p.Type == typeof(double) ? "REAL" : "TEXT")} " +
                (p.Name == "Id" ? "PRIMARY KEY AUTOINCREMENT" : p.IsNullable ? "" : "NOT NULL")
            );
            await connection.ExecuteAsync($"CREATE TABLE IF NOT EXISTS AxisDimensions ({string.Join(", ", columns)})");
        }

        // Insert a single AxisDimensions and return the new Id
        public static async Task<int> InsertAsync(this AxisDimensions entity, SqliteConnection connection)
        {
            IEnumerable<string> columns = Properties.Where(p => p.Name != "Id").Select(p => p.Name);
            IEnumerable<string> parameters = columns.Select(c => $"@{c}");
            string sql = $"INSERT INTO AxisDimensions ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)}); SELECT last_insert_rowid();";
            var id = await connection.ExecuteScalarAsync<int>(sql, entity);
            entity.Id = id; // Update the entity with the new Id
            return id;
        }

        // Upsert (INSERT OR REPLACE) and return the Id - Only inserts new row if data differs
        public static async Task<int> UpsertAsync(this AxisDimensions entity, SqliteConnection connection)
        {
            // First, try to find an existing record with matching data (excluding Id and CreatedAt)
            const string selectSql = @"
                SELECT Id FROM AxisDimensions 
                WHERE Theme = @Theme 
                AND Unit = @Unit 
                AND MinX = @MinX 
                AND MaxX = @MaxX 
                AND MinY = @MinY 
                AND MaxY = @MaxY 
                AND MinZ = @MinZ 
                AND MaxZ = @MaxZ 
                AND IncludeMethod = @IncludeMethod 
                AND (OSCADMethod = @OSCADMethod OR (OSCADMethod IS NULL AND @OSCADMethod IS NULL))
                LIMIT 1";
            
            var existingId = await connection.QuerySingleOrDefaultAsync<int?>(
                selectSql, 
                new 
                { 
                    entity.Theme, 
                    entity.Unit, 
                    entity.MinX, 
                    entity.MaxX, 
                    entity.MinY, 
                    entity.MaxY, 
                    entity.MinZ, 
                    entity.MaxZ, 
                    entity.IncludeMethod, 
                    entity.OSCADMethod 
                });
            
            if (existingId.HasValue)
            {
                // Record with same data already exists, return existing Id
                entity.Id = existingId.Value;
                return existingId.Value;
            }
            else
            {
                // Insert as a new record (data is different)
                IEnumerable<string> columns = Properties.Where(p => p.Name != "Id").Select(p => p.Name);
                IEnumerable<string> parameters = columns.Select(c => $"@{c}");
                string insertSql = $"INSERT INTO AxisDimensions ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)}); SELECT last_insert_rowid();";
                var id = await connection.ExecuteScalarAsync<int>(insertSql, entity);
                entity.Id = id;
                return id;
            }
        }

        // Update
        public static async Task UpdateAsync(this AxisDimensions entity, SqliteConnection connection)
        {
            IEnumerable<string> setClause = Properties.Where(p => p.Name != "Id").Select(p => $"{p.Name} = @{p.Name}");
            string sql = $"UPDATE AxisDimensions SET {string.Join(", ", setClause)} WHERE Id = @Id";
            await connection.ExecuteAsync(sql, entity);
        }

        // Delete by Id
        public static async Task DeleteAsync(this AxisDimensions entity, SqliteConnection connection) => 
            await connection.ExecuteAsync("DELETE FROM AxisDimensions WHERE Id = @Id", new { entity.Id });

        // Select by Id
        public static async Task<AxisDimensions?> GetByIdAsync(this AxisDimensions _, SqliteConnection connection, int id) => 
            await connection.QuerySingleOrDefaultAsync<AxisDimensions>("SELECT * FROM AxisDimensions WHERE Id = @Id", new { Id = id });

        // Select all
        public static async Task<IEnumerable<AxisDimensions>> GetAllAsync(this AxisDimensions _, SqliteConnection connection) => 
            await connection.QueryAsync<AxisDimensions>("SELECT * FROM AxisDimensions");

        // Complex query example (mimics stored procedure)
        public static async Task<IEnumerable<AxisDimensions>> GetByUnitAsync(this AxisDimensions _, SqliteConnection connection, string unit) => 
            await connection.QueryAsync<AxisDimensions>(
                "SELECT * FROM AxisDimensions WHERE Unit = @Unit ORDER BY CreatedAt DESC",
                new { Unit = unit });

        // Get by Theme
        public static async Task<IEnumerable<AxisDimensions>> GetByThemeAsync(this AxisDimensions _, SqliteConnection connection, string theme) => 
            await connection.QueryAsync<AxisDimensions>(
                "SELECT * FROM AxisDimensions WHERE Theme = @Theme ORDER BY CreatedAt DESC",
                new { Theme = theme });
    }
}
