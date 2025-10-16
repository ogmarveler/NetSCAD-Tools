using Microsoft.Data.Sqlite;
using static NetScad.Core.Measurements.Conversion;
using Dapper;
using NetScad.Core.Interfaces;

namespace NetScad.Designer.Repositories
{
    // Entity for CAD cylinder dimensions
    public class CylinderDimensions : IScadObject
    {
        public int Id { get; set; }
        public int? AxisDimensionsId { get; set; } // Foreign key to AxisDimensions
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Material { get; set; }
        public string OperationType { get; set; } = Core.Primitives.OperationType.Add.ToString(); // Add or Subtract
        public const int OpenSCAD_DecimalPlaces = 6; // High precision for 3D printing
        public double Radius_MM { get; set; } // Millimeters (default)
        public double? Radius1_MM { get; set; } // Optional radius1 for cone
        public double? Radius2_MM { get; set; } // Optional radius2 for cone
        public double Height_MM { get; set; }
        public double Resolution => 180; // Default resolution for curves

        // Imperial conversions (computed)
        public double Radius_IN => Math.Round(MillimeterToInches(Radius_MM), OpenSCAD_DecimalPlaces);
        public double? Radius1_IN => Radius1_MM.HasValue ? Math.Round(MillimeterToInches(Radius1_MM.Value), OpenSCAD_DecimalPlaces) : null;
        public double? Radius2_IN => Radius2_MM.HasValue ? Math.Round(MillimeterToInches(Radius2_MM.Value), OpenSCAD_DecimalPlaces) : null;
        public double Height_IN => Math.Round(MillimeterToInches(Height_MM), OpenSCAD_DecimalPlaces);
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string OSCADMethod { get; set; } = string.Empty;

        // Join statement to include axis if available
        // Backing field for AxisOSCADMethod
        private string? _axisOSCADMethod;
        public string? AxisOSCADMethod 
        { 
            get => _axisOSCADMethod ?? (AxisDimensionsId.HasValue ? $"Axis: {AxisDimensionsId.Value}" : "No Axis Assigned");
            set => _axisOSCADMethod = value?.Replace("use <Axes/axes.scad>; ", "").Replace("Get_", "").Replace("_", " ");
        }

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "Id", Id },
            { "AxisDimensionsId", AxisDimensionsId ?? (object)DBNull.Value },
            { "Name", Name },
            { "Description", Description ?? (object)DBNull.Value },
            { "Material", Material ?? (object)DBNull.Value },
            { "OperationType", OperationType },
            { "Radius_MM", Radius_MM },
            { "Radius1_MM", Radius1_MM ?? (object)DBNull.Value },
            { "Radius2_MM", Radius2_MM ?? (object)DBNull.Value },
            { "Height_MM", Height_MM },
            { "Radius_IN", Radius_IN },
            { "Radius1_IN", Radius1_IN ?? (object)DBNull.Value },
            { "Radius2_IN", Radius2_IN ?? (object)DBNull.Value },
            { "Height_IN", Height_IN },
            { "Resolution", Resolution },
            { "OSCADMethod", OSCADMethod },
            { "CreatedAt", CreatedAt }
        };
    }

    // Extension methods for CylinderDimensions CRUD operations
    public static class CylinderDimensionsExtensions
    {
        // Property mapping for CylinderDimensions (AOT-safe)
        private static readonly (string Name, Type Type, bool IsNullable)[] Properties =
        [
            (nameof(CylinderDimensions.Id), typeof(int), false),
            (nameof(CylinderDimensions.AxisDimensionsId), typeof(int), true),
            (nameof(CylinderDimensions.Name), typeof(string), true),
            (nameof(CylinderDimensions.Description), typeof(string), true),
            (nameof(CylinderDimensions.Material), typeof(string), true),
            (nameof(CylinderDimensions.OperationType), typeof(string), false),
            (nameof(CylinderDimensions.Radius_MM), typeof(double), false),
            (nameof(CylinderDimensions.Radius1_MM), typeof(double), true),
            (nameof(CylinderDimensions.Radius2_MM), typeof(double), true),
            (nameof(CylinderDimensions.Height_MM), typeof(double), false),
            (nameof(CylinderDimensions.Radius_IN), typeof(double), false),
            (nameof(CylinderDimensions.Radius1_IN), typeof(double), true),
            (nameof(CylinderDimensions.Radius2_IN), typeof(double), true),
            (nameof(CylinderDimensions.Height_IN), typeof(double), false),
            (nameof(CylinderDimensions.Resolution), typeof(double), false),
            (nameof(CylinderDimensions.OSCADMethod), typeof(string), true),
            (nameof(CylinderDimensions.CreatedAt), typeof(DateTime), false)
        ];

        // Create table
        public static async Task CreateTable(this SqliteConnection connection)
        {
            IEnumerable<string> columns = Properties.Select(p =>
                $"{p.Name} {(p.Type == typeof(int) ? "INTEGER" : p.Type == typeof(string) ? "TEXT" : p.Type == typeof(double) ? "REAL" : "TEXT")} " +
                (p.Name == "Id" ? "PRIMARY KEY AUTOINCREMENT" : p.IsNullable ? "" : "NOT NULL")
            );
            
            // Add foreign key constraint
            string foreignKeyConstraint = "FOREIGN KEY (AxisDimensionsId) REFERENCES AxisDimensions(Id) ON DELETE SET NULL";
            string createTableSql = $"CREATE TABLE IF NOT EXISTS CylinderDimensions ({string.Join(", ", columns)}, {foreignKeyConstraint})";
            
            await connection.ExecuteAsync(createTableSql);
        }

        // Insert a single CylinderDimensions and return the new Id
        public static async Task<int> InsertAsync(this CylinderDimensions entity, SqliteConnection connection)
        {
            IEnumerable<string> columns = Properties.Where(p => p.Name != "Id").Select(p => p.Name);
            IEnumerable<string> parameters = columns.Select(c => $"@{c}");
            string sql = $"INSERT INTO CylinderDimensions ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)}); SELECT last_insert_rowid();";
            var id = await connection.ExecuteScalarAsync<int>(sql, entity);
            entity.Id = id; // Update the entity with the new Id
            return id;
        }

        // Upsert (INSERT OR REPLACE) and return the Id - Only updates if Name and Description match
        public static async Task<int> UpsertAsync(this CylinderDimensions entity, SqliteConnection connection)
        {
            // First, try to find an existing record with matching Name and Description
            const string selectSql = "SELECT Id FROM CylinderDimensions WHERE Name = @Name AND Description = @Description LIMIT 1";
            var existingId = await connection.QuerySingleOrDefaultAsync<int?>(selectSql, new { entity.Name, entity.Description });
            
            if (existingId.HasValue)
            {
                // Update the existing record
                entity.Id = existingId.Value;
                IEnumerable<string> setClause = Properties.Where(p => p.Name != "Id").Select(p => $"{p.Name} = @{p.Name}");
                string updateSql = $"UPDATE CylinderDimensions SET {string.Join(", ", setClause)} WHERE Id = @Id";
                await connection.ExecuteAsync(updateSql, entity);
                return existingId.Value;
            }
            else
            {
                // Insert as a new record
                IEnumerable<string> columns = Properties.Where(p => p.Name != "Id").Select(p => p.Name);
                IEnumerable<string> parameters = columns.Select(c => $"@{c}");
                string insertSql = $"INSERT INTO CylinderDimensions ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)}); SELECT last_insert_rowid();";
                var id = await connection.ExecuteScalarAsync<int>(insertSql, entity);
                entity.Id = id;
                return id;
            }
        }

        // Update
        public static async Task UpdateAsync(this CylinderDimensions entity, SqliteConnection connection)
        {
            IEnumerable<string> setClause = Properties.Where(p => p.Name != "Id").Select(p => $"{p.Name} = @{p.Name}");
            string sql = $"UPDATE CylinderDimensions SET {string.Join(", ", setClause)} WHERE Id = @Id";
            await connection.ExecuteAsync(sql, entity);
        }

        // Delete by Id
        public static async Task DeleteAsync(this CylinderDimensions entity, SqliteConnection connection) => 
            await connection.ExecuteAsync("DELETE FROM CylinderDimensions WHERE Id = @Id", new { entity.Id });

        // Select by Id
        public static async Task<CylinderDimensions?> GetByIdAsync(this CylinderDimensions _, SqliteConnection connection, int id) => 
            await connection.QuerySingleOrDefaultAsync<CylinderDimensions>("SELECT * FROM CylinderDimensions WHERE Id = @Id", new { Id = id });

        // Select all
        public static async Task<IEnumerable<CylinderDimensions>> GetAllAsync(this CylinderDimensions _, SqliteConnection connection) => 
            await connection.QueryAsync<CylinderDimensions>("SELECT * FROM CylinderDimensions");

        // Get by Material
        public static async Task<IEnumerable<CylinderDimensions>> GetByMaterialAsync(this CylinderDimensions _, SqliteConnection connection, string material) => 
            await connection.QueryAsync<CylinderDimensions>(
                "SELECT * FROM CylinderDimensions WHERE Material = @Material ORDER BY CreatedAt DESC",
                new { Material = material });

        // Get by AxisDimensionsId (Foreign Key)
        public static async Task<IEnumerable<CylinderDimensions>> GetByAxisDimensionsIdAsync(this CylinderDimensions _, SqliteConnection connection, int axisDimensionsId) => 
            await connection.QueryAsync<CylinderDimensions>(
                "SELECT * FROM CylinderDimensions WHERE AxisDimensionsId = @AxisDimensionsId ORDER BY CreatedAt DESC",
                new { AxisDimensionsId = axisDimensionsId });

        // Get all with no assigned axis
        public static async Task<IEnumerable<CylinderDimensions>> GetUnassignedAsync(this CylinderDimensions _, SqliteConnection connection) => 
            await connection.QueryAsync<CylinderDimensions>(
                "SELECT * FROM CylinderDimensions WHERE AxisDimensionsId IS NULL ORDER BY CreatedAt DESC");

        // Get all with joined AxisDimensions OSCADMethod
        public static async Task<IEnumerable<CylinderDimensions>> GetAllWithAxisAsync(this CylinderDimensions _, SqliteConnection connection)
        {
            const string sql = @"
                SELECT 
                    cd.*,
                    ad.OSCADMethod as AxisOSCADMethod
                FROM CylinderDimensions cd
                LEFT JOIN AxisDimensions ad ON cd.AxisDimensionsId = ad.Id
                ORDER BY cd.CreatedAt DESC";
            
            var result = await connection.QueryAsync<CylinderDimensions, string, CylinderDimensions>(
                sql,
                (cylinderDim, axisMethod) =>
                {
                    cylinderDim.AxisOSCADMethod = axisMethod;
                    return cylinderDim;
                },
                splitOn: "AxisOSCADMethod"
            );
            
            return result;
        }

        // Get by Id with joined AxisDimensions OSCADMethod
        public static async Task<CylinderDimensions?> GetByIdWithAxisAsync(this CylinderDimensions _, SqliteConnection connection, int id)
        {
            const string sql = @"
                SELECT 
                    cd.*,
                    ad.OSCADMethod as AxisOSCADMethod
                FROM CylinderDimensions cd
                LEFT JOIN AxisDimensions ad ON cd.AxisDimensionsId = ad.Id
                WHERE cd.Id = @Id";
            
            var result = await connection.QueryAsync<CylinderDimensions, string, CylinderDimensions>(
                sql,
                (cylinderDim, axisMethod) =>
                {
                    cylinderDim.AxisOSCADMethod = axisMethod;
                    return cylinderDim;
                },
                new { Id = id },
                splitOn: "AxisOSCADMethod"
            );
            
            return result.FirstOrDefault();
        }

        // Get by AxisDimensionsId with joined OSCADMethod
        public static async Task<IEnumerable<CylinderDimensions>> GetByAxisDimensionsIdWithAxisAsync(this CylinderDimensions _, SqliteConnection connection, int axisDimensionsId)
        {
            const string sql = @"
                SELECT 
                    cd.*,
                    ad.OSCADMethod as AxisOSCADMethod
                FROM CylinderDimensions cd
                LEFT JOIN AxisDimensions ad ON cd.AxisDimensionsId = ad.Id
                WHERE cd.AxisDimensionsId = @AxisDimensionsId
                ORDER BY cd.CreatedAt DESC";
            
            var result = await connection.QueryAsync<CylinderDimensions, string, CylinderDimensions>(
                sql,
                (cylinderDim, axisMethod) =>
                {
                    cylinderDim.AxisOSCADMethod = axisMethod;
                    return cylinderDim;
                },
                new { AxisDimensionsId = axisDimensionsId },
                splitOn: "AxisOSCADMethod"
            );
            
            return result;
        }

        // Get by Name with joined AxisDimensions OSCADMethod
        public static async Task<IEnumerable<CylinderDimensions>> GetByNameWithAxisAsync(this CylinderDimensions _, SqliteConnection connection, string name)
        {
            const string sql = @"
                SELECT 
                    cd.*,
                    ad.OSCADMethod as AxisOSCADMethod
                FROM CylinderDimensions cd
                LEFT JOIN AxisDimensions ad ON cd.AxisDimensionsId = ad.Id
                WHERE cd.Name = @Name
                ORDER BY cd.CreatedAt DESC";
            
            var result = await connection.QueryAsync<CylinderDimensions, string, CylinderDimensions>(
                sql,
                (cylinderDim, axisMethod) =>
                {
                    cylinderDim.AxisOSCADMethod = axisMethod;
                    return cylinderDim;
                },
                new { Name = name },
                splitOn: "AxisOSCADMethod"
            );
            
            return result;
        }

        // Get by OperationType
        public static async Task<IEnumerable<CylinderDimensions>> GetByOperationTypeAsync(this CylinderDimensions _, SqliteConnection connection, string operationType) => 
            await connection.QueryAsync<CylinderDimensions>(
                "SELECT * FROM CylinderDimensions WHERE OperationType = @OperationType ORDER BY CreatedAt DESC",
                new { OperationType = operationType });

        // Get by Name and OperationType with joined AxisDimensions OSCADMethod
        public static async Task<IEnumerable<CylinderDimensions>> GetByNameAndOperationTypeWithAxisAsync(this CylinderDimensions _, SqliteConnection connection, string name, string operationType)
        {
            const string sql = @"
                SELECT 
                    cd.*,
                    ad.OSCADMethod as AxisOSCADMethod
                FROM CylinderDimensions cd
                LEFT JOIN AxisDimensions ad ON cd.AxisDimensionsId = ad.Id
                WHERE cd.Name = @Name AND cd.OperationType = @OperationType
                ORDER BY cd.CreatedAt DESC";
            
            var result = await connection.QueryAsync<CylinderDimensions, string, CylinderDimensions>(
                sql,
                (cylinderDim, axisMethod) =>
                {
                    cylinderDim.AxisOSCADMethod = axisMethod;
                    return cylinderDim;
                },
                new { Name = name, OperationType = operationType },
                splitOn: "AxisOSCADMethod"
            );
            
            return result;
        }
    }
}
