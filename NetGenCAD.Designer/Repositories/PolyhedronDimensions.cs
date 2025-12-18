using Dapper;
using Microsoft.Data.Sqlite;
using NetGenCAD.Core.Interfaces;
using static NetGenCAD.Core.Measurements.Conversion;

namespace NetGenCAD.Designer.Repositories
{
    // Entity for CAD polyhedron dimensions
    public class PolyhedronDimensions : IScadObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string PolyhedronOperationType { get; set; } = Core.Primitives.PolyhedronOperationType.Points.ToString(); // Points or Faces
        public const int OpenSCAD_DecimalPlaces = 6; // High precision for 3D printing

        // Points properties
        public double PointX_MM { get; set; } // Millimeters (default)
        public double PointY_MM { get; set; } // Millimeters (default)
        public double PointZ_MM { get; set; } // Millimeters (default)
        
        // Imperial conversions (computed) - Points properties
        public double PointX_IN => Math.Round(MillimeterToInches(PointX_MM), OpenSCAD_DecimalPlaces);
        public double PointY_IN => Math.Round(MillimeterToInches(PointY_MM), OpenSCAD_DecimalPlaces);
        public double PointZ_IN => Math.Round(MillimeterToInches(PointZ_MM), OpenSCAD_DecimalPlaces);
        public int PointsId { get; set; } = 0; // Identifying the point set

        // Faces properties
        public string Face { get; set; } = string.Empty; // Points that make up the face
        public int FaceId { get; set; } = 0; // Identifying the face set       
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string OSCADMethod { get; set; } = string.Empty;

        public async Task<string> RenderToStlAsync(string outputPath)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "Id", Id },
            { "Name", Name },
            { "Description", Description ?? (object)DBNull.Value },
            { "PolyhedronOperationType", PolyhedronOperationType },
            { "PointX_MM", PointX_MM },
            { "PointY_MM", PointY_MM },
            { "PointZ_MM", PointZ_MM },
            { "PointX_IN", PointX_IN },
            { "PointY_IN", PointY_IN },
            { "PointZ_IN", PointZ_IN },
            { "PointsId", PointsId },
            { "Face", Face },
            { "FaceId", FaceId },
            { "OSCADMethod", OSCADMethod },
            { "CreatedAt", CreatedAt },
        };
    }

    // Extension methods for PolyhedronDimensions CRUD operations
    public static class PolyhedronDimensionsExtensions
    {
        // Property mapping for PolyhedronDimensions (AOT-safe)
        private static readonly (string Name, Type Type, bool IsNullable)[] Properties =
        [
            (nameof(PolyhedronDimensions.Id), typeof(int), false),
            (nameof(PolyhedronDimensions.Name), typeof(string), true),
            (nameof(PolyhedronDimensions.Description), typeof(string), true),
            (nameof(PolyhedronDimensions.PolyhedronOperationType), typeof(string), false),
            (nameof(PolyhedronDimensions.PointX_MM), typeof(double), false),
            (nameof(PolyhedronDimensions.PointY_MM), typeof(double), false),
            (nameof(PolyhedronDimensions.PointZ_MM), typeof(double), false),
            (nameof(PolyhedronDimensions.PointX_IN), typeof(double), false),
            (nameof(PolyhedronDimensions.PointY_IN), typeof(double), false),
            (nameof(PolyhedronDimensions.PointZ_IN), typeof(double), false),
            (nameof(PolyhedronDimensions.PointsId), typeof(int), true),
            (nameof(PolyhedronDimensions.Face), typeof(string), true),
            (nameof(PolyhedronDimensions.FaceId), typeof(int), true),
            (nameof(PolyhedronDimensions.OSCADMethod), typeof(string), true),
            (nameof(PolyhedronDimensions.CreatedAt), typeof(DateTime), false),
        ];

        // Create table
        public static async Task CreateTable(this SqliteConnection connection)
        {
            IEnumerable<string> columns = Properties.Select(p =>
                $"{p.Name} {(p.Type == typeof(int) ? "INTEGER" : p.Type == typeof(string) ? "TEXT" : p.Type == typeof(double) ? "REAL" : "TEXT")} " +
                (p.Name == "Id" ? "PRIMARY KEY AUTOINCREMENT" : p.IsNullable ? "" : "NOT NULL")
            );

            string createTableSql = $"CREATE TABLE IF NOT EXISTS PolyhedronDimensions ({string.Join(", ", columns)})";

            await connection.ExecuteAsync(createTableSql);
        }

        // Insert a single PolyhedronDimensions and return the new Id
        public static async Task<int> InsertAsync(this PolyhedronDimensions entity, SqliteConnection connection)
        {
            IEnumerable<string> columns = Properties.Where(p => p.Name != "Id").Select(p => p.Name);
            IEnumerable<string> parameters = columns.Select(c => $"@{c}");
            string sql = $"INSERT INTO PolyhedronDimensions ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)}); SELECT last_insert_rowid();";
            var id = await connection.ExecuteScalarAsync<int>(sql, entity);
            entity.Id = id;
            return id;
        }

        // Upsert (INSERT OR REPLACE) and return the Id
        public static async Task<int> UpsertAsync(this PolyhedronDimensions entity, SqliteConnection connection)
        {
            const string selectSql = "SELECT Id FROM PolyhedronDimensions WHERE Name = @Name AND Description = @Description AND PolyhedronOperationType = @PolyhedronOperationType LIMIT 1";
            var existingId = await connection.QuerySingleOrDefaultAsync<int?>(selectSql, new { entity.Name, entity.Description, entity.PolyhedronOperationType });

            if (existingId.HasValue)
            {
                entity.Id = existingId.Value;
                IEnumerable<string> setClause = Properties.Where(p => p.Name != "Id").Select(p => $"{p.Name} = @{p.Name}");
                string updateSql = $"UPDATE PolyhedronDimensions SET {string.Join(", ", setClause)} WHERE Id = @Id";
                await connection.ExecuteAsync(updateSql, entity);
                return existingId.Value;
            }
            else
            {
                IEnumerable<string> columns = Properties.Where(p => p.Name != "Id").Select(p => p.Name);
                IEnumerable<string> parameters = columns.Select(c => $"@{c}");
                string insertSql = $"INSERT INTO PolyhedronDimensions ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)}); SELECT last_insert_rowid();";
                var id = await connection.ExecuteScalarAsync<int>(insertSql, entity);
                entity.Id = id;
                return id;
            }
        }

        // Update
        public static async Task UpdateAsync(this PolyhedronDimensions entity, SqliteConnection connection)
        {
            IEnumerable<string> setClause = Properties.Where(p => p.Name != "Id").Select(p => $"{p.Name} = @{p.Name}");
            string sql = $"UPDATE PolyhedronDimensions SET {string.Join(", ", setClause)} WHERE Id = @Id";
            await connection.ExecuteAsync(sql, entity);
        }

        // Delete by Id
        public static async Task DeleteAsync(this PolyhedronDimensions entity, SqliteConnection connection) =>
            await connection.ExecuteAsync("DELETE FROM PolyhedronDimensions WHERE Id = @Id", new { entity.Id });

        // Select by Id
        public static async Task<PolyhedronDimensions?> GetByIdAsync(this PolyhedronDimensions _, SqliteConnection connection, int id) =>
            await connection.QuerySingleOrDefaultAsync<PolyhedronDimensions>("SELECT * FROM PolyhedronDimensions WHERE Id = @Id", new { Id = id });

        // Select all
        public static async Task<IEnumerable<PolyhedronDimensions>> GetAllAsync(this PolyhedronDimensions _, SqliteConnection connection) =>
            await connection.QueryAsync<PolyhedronDimensions>("SELECT * FROM PolyhedronDimensions");

        // Get by Name
        public static async Task<IEnumerable<PolyhedronDimensions>> GetByObjectNameAsync(this PolyhedronDimensions _, SqliteConnection connection, string name) =>
            await connection.QueryAsync<PolyhedronDimensions>(
                "SELECT * FROM PolyhedronDimensions WHERE Name = @Name ORDER BY CreatedAt ASC",
                new { Name = name });

        // Get by PolyhedronOperationType
        public static async Task<IEnumerable<PolyhedronDimensions>> GetByOperationTypeAsync(this PolyhedronDimensions _, SqliteConnection connection, string operationType) =>
            await connection.QueryAsync<PolyhedronDimensions>(
                "SELECT * FROM PolyhedronDimensions WHERE PolyhedronOperationType = @PolyhedronOperationType ORDER BY CreatedAt DESC",
                new { PolyhedronOperationType = operationType });

        // Copy object
        public static async Task CopyObjectAsync(this PolyhedronDimensions source, SqliteConnection connection)
        {
            const string sql = @"
            INSERT INTO PolyhedronDimensions
            SELECT NULL,
                   Name || '_' || 'copy' AS Name,
                   Description,
                   PolyhedronOperationType,
                   PointX_MM,
                   PointY_MM,
                   PointZ_MM,
                   PointX_IN,
                   PointY_IN,
                   PointZ_IN,
                   PointsId,
                   Face,
                   FaceId,
                   OSCADMethod,
                   CreatedAt
                   FROM PolyhedronDimensions
                   WHERE Name = @Name";
            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@Name", source.Name);
            await cmd.ExecuteNonQueryAsync();
        }

        // Get polyhedron names from Db for use in ScadObjectViewModel
        public static async Task<IEnumerable<string>> GetPolyhedronNamesList(SqliteConnection connection) => await connection.QueryAsync<string>("SELECT DISTINCT Name FROM PolyhedronDimensions");
    }
}