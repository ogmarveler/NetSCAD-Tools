using Microsoft.Data.Sqlite;
using Dapper;


namespace NetScad.Core.Repositories
{
    // Entity for CAD outer dimensions
    public class OuterDimensions
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Material { get; set; }
        public const int OpenSCAD_DecimalPlaces = 6; // Set to 6 for higher precision
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Thickness { get; set; }
        public double Round_r { get; set; }
        public double Round_h { get; set; }
        public double Resolution { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Extension methods for OuterDimensions CRUD operations
    public static class OuterDimensionsExtensions
    {
        // Property mapping for OuterDimensions (AOT-safe)
        private static readonly (string Name, Type Type, bool IsNullable)[] Properties =
        [
            (nameof(OuterDimensions.Id), typeof(int), false),
            (nameof(OuterDimensions.Name), typeof(string), false),
            (nameof(OuterDimensions.Description), typeof(string), true),
            (nameof(OuterDimensions.Material), typeof(string), true),
            (nameof(OuterDimensions.Length), typeof(double), false),
            (nameof(OuterDimensions.Width), typeof(double), false),
            (nameof(OuterDimensions.Height), typeof(double), false),
            (nameof(OuterDimensions.Thickness), typeof(double), false),
            (nameof(OuterDimensions.Round_r), typeof(double), false),
            (nameof(OuterDimensions.Round_h), typeof(double), false),
            (nameof(OuterDimensions.Resolution), typeof(double), false),
            (nameof(OuterDimensions.CreatedAt), typeof(DateTime), false)
        ];

        // Create table
        public static async Task CreateTable(this SqliteConnection connection)
        {
            var columns = Properties.Select(p =>
                $"{p.Name} {(p.Type == typeof(int) ? "INTEGER" : p.Type == typeof(string) ? "TEXT" : p.Type == typeof(double) ? "REAL" : "TEXT")} " +
                (p.Name == "Id" ? "PRIMARY KEY AUTOINCREMENT" : p.IsNullable ? "" : "NOT NULL")
            );
            connection.Execute($"CREATE TABLE IF NOT EXISTS OuterDimensions ({string.Join(", ", columns)})");
        }

        // Insert a single OuterDimensions
        public static async Task InsertAsync(this OuterDimensions entity, SqliteConnection connection)
        {
            var columns = Properties.Where(p => p.Name != "Id").Select(p => p.Name);
            var parameters = columns.Select(c => $"@{c}");
            var sql = $"INSERT INTO OuterDimensions ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)})";
            await connection.ExecuteAsync(sql, entity);
        }

        // Upsert (INSERT OR REPLACE)
        public static async Task UpsertAsync(this OuterDimensions entity, SqliteConnection connection)
        {
            var columns = Properties.Select(p => p.Name);
            var parameters = columns.Select(c => $"@{c}");
            var sql = $"INSERT OR REPLACE INTO OuterDimensions ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)})";
            await connection.ExecuteAsync(sql, entity);
        }

        // Update
        public static async Task UpdateAsync(this OuterDimensions entity, SqliteConnection connection)
        {
            var setClause = Properties.Where(p => p.Name != "Id").Select(p => $"{p.Name} = @{p.Name}");
            var sql = $"UPDATE OuterDimensions SET {string.Join(", ", setClause)} WHERE Id = @Id";
            await connection.ExecuteAsync(sql, entity);
        }

        // Delete by Id
        public static async Task DeleteAsync(this OuterDimensions entity, SqliteConnection connection)
        {
            await connection.ExecuteAsync("DELETE FROM OuterDimensions WHERE Id = @Id", new { entity.Id });
        }

        // Select by Id
        public static async Task<OuterDimensions?> GetByIdAsync(this OuterDimensions _, SqliteConnection connection, int id)
        {
            return await connection.QuerySingleOrDefaultAsync<OuterDimensions>("SELECT * FROM OuterDimensions WHERE Id = @Id", new { Id = id });
        }

        // Select all
        public static async Task<IEnumerable<OuterDimensions>> GetAllAsync(this OuterDimensions _, SqliteConnection connection)
        {
            return await connection.QueryAsync<OuterDimensions>("SELECT * FROM OuterDimensions");
        }

        // Complex query example (mimics stored procedure)
        public static async Task<IEnumerable<OuterDimensions>> GetByMaterialAsync(this OuterDimensions _, SqliteConnection connection, string material)
        {
            return await connection.QueryAsync<OuterDimensions>(
                "SELECT * FROM OuterDimensions WHERE Material = @Material ORDER BY CreatedAt DESC",
                new { Material = material });
        }
    }
}
