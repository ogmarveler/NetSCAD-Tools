using Dapper;
using Microsoft.Data.Sqlite;
using NetGenCAD.Core.Interfaces;
using static NetGenCAD.Core.Measurements.Conversion;

namespace NetGenCAD.Designer.Repositories
{
    // Entity for CAD custom shape dimensions, reusable shapes across various objects
    public class ShapeDimensions : IScadObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Should match the PolyhedronDimensions Shape Name
        public string? Description { get; set; }
        public const int OpenSCAD_DecimalPlaces = 6; // High precision for 3D printing
        
        // Bounding box properties (calculated from polyhedron bounds)
        public double BoxLength_MM { get; set; } = 0; // X-axis length in millimeters
        public double BoxWidth_MM { get; set; } = 0;  // Y-axis width in millimeters
        public double BoxHeight_MM { get; set; } = 0; // Z-axis height in millimeters
        
        // Imperial conversions (computed) - Bounding box properties
        public double BoxLength_IN => Math.Round(MillimeterToInches(BoxLength_MM), OpenSCAD_DecimalPlaces);
        public double BoxWidth_IN => Math.Round(MillimeterToInches(BoxWidth_MM), OpenSCAD_DecimalPlaces);
        public double BoxHeight_IN => Math.Round(MillimeterToInches(BoxHeight_MM), OpenSCAD_DecimalPlaces);

        // Metadata
        public int NumberOfVertices { get; set; } = 0;
        public int NumberOfFaces { get; set; } = 0;
        public int NumberOfEdges { get; set; } = 0;
        public int Convexity { get; set; } = 1;
        public double Volume_CM3 { get; set; } = 0; // Volume in cubic centimeters
        public double Volume_IN3 { get; set; } = 0; // Volume in cubic inches
        public double SurfaceArea_CM2 { get; set; } = 0; // Surface area in square centimeters
        public double SurfaceArea_IN2 { get; set; } = 0; // Surface area in square inches
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Output OpenSCAD method for rendering this shape
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
            { "BoxLength_MM", BoxLength_MM },
            { "BoxWidth_MM", BoxWidth_MM },
            { "BoxHeight_MM", BoxHeight_MM },
            { "BoxLength_IN", BoxLength_IN },
            { "BoxWidth_IN", BoxWidth_IN },
            { "BoxHeight_IN", BoxHeight_IN },
            { "NumberOfVertices", NumberOfVertices },
            { "NumberOfFaces", NumberOfFaces },
            { "NumberOfEdges", NumberOfEdges },
            { "Convexity", Convexity },
            { "Volume_CM3", Volume_CM3 },
            { "Volume_IN3", Volume_IN3 },
            { "SurfaceArea_CM2", SurfaceArea_CM2 },
            { "SurfaceArea_IN2", SurfaceArea_IN2 },
            { "OSCADMethod", OSCADMethod },
            { "CreatedAt", CreatedAt },
        };
    }

    // Extension methods for ShapeDimensions CRUD operations
    public static class ShapeDimensionsExtensions
    {
        // Property mapping for ShapeDimensions (AOT-safe)
        private static readonly (string Name, Type Type, bool IsNullable)[] Properties =
        [
            (nameof(ShapeDimensions.Id), typeof(int), false),
            (nameof(ShapeDimensions.Name), typeof(string), true),
            (nameof(ShapeDimensions.Description), typeof(string), true),
            (nameof(ShapeDimensions.BoxLength_MM), typeof(double), false),
            (nameof(ShapeDimensions.BoxWidth_MM), typeof(double), false),
            (nameof(ShapeDimensions.BoxHeight_MM), typeof(double), false),
            (nameof(ShapeDimensions.BoxLength_IN), typeof(double), false),
            (nameof(ShapeDimensions.BoxWidth_IN), typeof(double), false),
            (nameof(ShapeDimensions.BoxHeight_IN), typeof(double), false),
            (nameof(ShapeDimensions.NumberOfVertices), typeof(int), false),
            (nameof(ShapeDimensions.NumberOfFaces), typeof(int), false),
            (nameof(ShapeDimensions.NumberOfEdges), typeof(int), false),
            (nameof(ShapeDimensions.Convexity), typeof(int), false),
            (nameof(ShapeDimensions.Volume_CM3), typeof(double), false),
            (nameof(ShapeDimensions.Volume_IN3), typeof(double), false),
            (nameof(ShapeDimensions.SurfaceArea_CM2), typeof(double), false),
            (nameof(ShapeDimensions.SurfaceArea_IN2), typeof(double), false),
            (nameof(ShapeDimensions.OSCADMethod), typeof(string), true),
            (nameof(ShapeDimensions.CreatedAt), typeof(DateTime), false),
        ];

        // Create table with unique constraint on Name
        public static async Task CreateTable(this SqliteConnection connection)
        {
            IEnumerable<string> columns = Properties.Select(p =>
                $"{p.Name} {(p.Type == typeof(int) ? "INTEGER" : p.Type == typeof(string) ? "TEXT" : p.Type == typeof(double) ? "REAL" : "TEXT")} " +
                (p.Name == "Id" ? "PRIMARY KEY AUTOINCREMENT" : p.IsNullable ? "" : "NOT NULL")
            );

            string createTableSql = $"CREATE TABLE IF NOT EXISTS ShapeDimensions ({string.Join(", ", columns)}, UNIQUE(Name))";

            await connection.ExecuteAsync(createTableSql);
        }

        // Insert a single ShapeDimensions and return the new Id
        public static async Task<int> InsertAsync(this ShapeDimensions entity, SqliteConnection connection)
        {
            IEnumerable<string> columns = Properties.Where(p => p.Name != "Id").Select(p => p.Name);
            IEnumerable<string> parameters = columns.Select(c => $"@{c}");
            string sql = $"INSERT INTO ShapeDimensions ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)}); SELECT last_insert_rowid();";
            var id = await connection.ExecuteScalarAsync<int>(sql, entity);
            entity.Id = id;
            return id;
        }

        // Upsert (INSERT OR REPLACE) based on unique Name constraint and return the Id
        public static async Task<int> UpsertAsync(this ShapeDimensions entity, SqliteConnection connection)
        {
            const string selectSql = "SELECT Id FROM ShapeDimensions WHERE Name = @Name LIMIT 1";
            var existingId = await connection.QuerySingleOrDefaultAsync<int?>(selectSql, new { entity.Name });

            if (existingId.HasValue)
            {
                entity.Id = existingId.Value;
                IEnumerable<string> setClause = Properties.Where(p => p.Name != "Id").Select(p => $"{p.Name} = @{p.Name}");
                string updateSql = $"UPDATE ShapeDimensions SET {string.Join(", ", setClause)} WHERE Id = @Id";
                await connection.ExecuteAsync(updateSql, entity);
                return existingId.Value;
            }
            else
            {
                IEnumerable<string> columns = Properties.Where(p => p.Name != "Id").Select(p => p.Name);
                IEnumerable<string> parameters = columns.Select(c => $"@{c}");
                string insertSql = $"INSERT INTO ShapeDimensions ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)}); SELECT last_insert_rowid();";
                var id = await connection.ExecuteScalarAsync<int>(insertSql, entity);
                entity.Id = id;
                return id;
            }
        }

        // Update
        public static async Task UpdateAsync(this ShapeDimensions entity, SqliteConnection connection)
        {
            IEnumerable<string> setClause = Properties.Where(p => p.Name != "Id").Select(p => $"{p.Name} = @{p.Name}");
            string sql = $"UPDATE ShapeDimensions SET {string.Join(", ", setClause)} WHERE Id = @Id";
            await connection.ExecuteAsync(sql, entity);
        }

        // Delete by Id
        public static async Task DeleteAsync(this ShapeDimensions entity, SqliteConnection connection) =>
            await connection.ExecuteAsync("DELETE FROM ShapeDimensions WHERE Id = @Id", new { entity.Id });

        // Select by Id
        public static async Task<ShapeDimensions?> GetByIdAsync(this ShapeDimensions _, SqliteConnection connection, int id) =>
            await connection.QuerySingleOrDefaultAsync<ShapeDimensions>("SELECT * FROM ShapeDimensions WHERE Id = @Id", new { Id = id });

        // Select all
        public static async Task<IEnumerable<ShapeDimensions>> GetAllAsync(this ShapeDimensions _, SqliteConnection connection) =>
            await connection.QueryAsync<ShapeDimensions>("SELECT * FROM ShapeDimensions");

        // Get by Name
        public static async Task<IEnumerable<ShapeDimensions>> GetByNameAsync(this ShapeDimensions _, SqliteConnection connection, string name) =>
            await connection.QueryAsync<ShapeDimensions>(
                "SELECT * FROM ShapeDimensions WHERE Name = @Name ORDER BY CreatedAt ASC",
                new { Name = name });

        // Copy shape
        public static async Task CopyShapeAsync(this ShapeDimensions source, SqliteConnection connection)
        {
            const string sql = @"
            INSERT INTO ShapeDimensions
            SELECT NULL,
                   Name || '_' || 'copy' AS Name,
                   Description,
                   BoxLength_MM,
                   BoxWidth_MM,
                   BoxHeight_MM,
                   BoxLength_IN,
                   BoxWidth_IN,
                   BoxHeight_IN,
                   NumberOfVertices,
                   NumberOfFaces,
                   NumberOfEdges,
                   Convexity,
                   Volume_CM3,
                   Volume_IN3,
                   SurfaceArea_CM2,
                   SurfaceArea_IN2,
                   OSCADMethod,
                   CreatedAt
                   FROM ShapeDimensions
                   WHERE Name = @Name";
            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@Name", source.Name);
            await cmd.ExecuteNonQueryAsync();
        }

        // Get shapes from Db for use in ScadObjectViewModel
        public static async Task<IEnumerable<ShapeDimensions>> GetShapesList(SqliteConnection connection) => await connection.QueryAsync<ShapeDimensions>("SELECT * FROM ShapeDimensions");
    }
}