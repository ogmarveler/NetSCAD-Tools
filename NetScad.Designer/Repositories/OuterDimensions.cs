using Microsoft.Data.Sqlite;
using static NetScad.Core.Measurements.Conversion;
using Dapper;
using NetScad.Core.Interfaces;

namespace NetScad.Designer.Repositories
{
    // Entity for CAD outer dimensions
    public class OuterDimensions : IScadObject
    {
        public int Id { get; set; }
        public int? AxisDimensionsId { get; set; } // Foreign key to AxisDimensions
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Material { get; set; }
        public string OperationType { get; set; } = Core.Primitives.OperationType.Add.ToString(); // Add or Subtract
        public const int OpenSCAD_DecimalPlaces = 6; // High precision for 3D printing
        public double Length_MM { get; set; } // Millimeters (default)
        public double Width_MM { get; set; }
        public double Height_MM { get; set; }
        public double Thickness_MM { get; set; }
        public double XOffset_MM { get; set; } // X-axis translation offset
        public double YOffset_MM { get; set; } // Y-axis translation offset
        public double ZOffset_MM { get; set; } // Z-axis translation offset
        public double Round_r_MM  => Math.Round(RoundFromWidth(Width_MM), OpenSCAD_DecimalPlaces);
        public double Round_h_MM => Math.Round(RoundEdgeHeight(Round_r_MM), OpenSCAD_DecimalPlaces);
        public int Resolution { get; set; } = 180; // Default resolution for curves

        // Imperial conversions (computed)
        public double Length_IN => Math.Round(MillimeterToInches(Length_MM), OpenSCAD_DecimalPlaces);
        public double Width_IN => Math.Round(MillimeterToInches(Width_MM), OpenSCAD_DecimalPlaces);
        public double Height_IN => Math.Round(MillimeterToInches(Height_MM), OpenSCAD_DecimalPlaces);
        public double Thickness_IN => Math.Round(MillimeterToInches(Thickness_MM), OpenSCAD_DecimalPlaces);
        public double XOffset_IN => Math.Round(MillimeterToInches(XOffset_MM), OpenSCAD_DecimalPlaces);
        public double YOffset_IN => Math.Round(MillimeterToInches(YOffset_MM), OpenSCAD_DecimalPlaces);
        public double ZOffset_IN => Math.Round(MillimeterToInches(ZOffset_MM), OpenSCAD_DecimalPlaces);
        public double Round_r_IN => Math.Round(MillimeterToInches(RoundFromWidth(Width_MM)), OpenSCAD_DecimalPlaces);
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
            { "Length_MM", Length_MM },
            { "Width_MM", Width_MM },
            { "Height_MM", Height_MM },
            { "Thickness_MM", Thickness_MM },
            { "XOffset_MM", XOffset_MM },
            { "YOffset_MM", YOffset_MM },
            { "ZOffset_MM", ZOffset_MM },
            { "Round_r_MM", Round_r_MM },
            { "Length_IN", Length_IN },
            { "Width_IN", Width_IN },
            { "Height_IN", Height_IN },
            { "Thickness_IN", Thickness_IN },
            { "XOffset_IN", XOffset_IN },
            { "YOffset_IN", YOffset_IN },
            { "ZOffset_IN", ZOffset_IN },
            { "Round_r_IN", Round_r_IN },
            { "Resolution", Resolution },
            { "OSCADMethod", OSCADMethod },
            { "CreatedAt", CreatedAt }
        };
    }

    // Extension methods for OuterDimensions CRUD operations
    public static class OuterDimensionsExtensions
    {
        // Property mapping for OuterDimensions (AOT-safe)
        private static readonly (string Name, Type Type, bool IsNullable)[] Properties =
        [
            (nameof(OuterDimensions.Id), typeof(int), false),
            (nameof(OuterDimensions.AxisDimensionsId), typeof(int), true),
            (nameof(OuterDimensions.Name), typeof(string), true),
            (nameof(OuterDimensions.Description), typeof(string), true),
            (nameof(OuterDimensions.Material), typeof(string), true),
            (nameof(OuterDimensions.OperationType), typeof(string), false),
            (nameof(OuterDimensions.Length_MM), typeof(double), false),
            (nameof(OuterDimensions.Width_MM), typeof(double), false),
            (nameof(OuterDimensions.Height_MM), typeof(double), false),
            (nameof(OuterDimensions.Thickness_MM), typeof(double), false),
            (nameof(OuterDimensions.XOffset_MM), typeof(double), false),
            (nameof(OuterDimensions.YOffset_MM), typeof(double), false),
            (nameof(OuterDimensions.ZOffset_MM), typeof(double), false),
            (nameof(OuterDimensions.Round_r_MM), typeof(double), false),
            (nameof(OuterDimensions.Length_IN), typeof(double), false),
            (nameof(OuterDimensions.Width_IN), typeof(double), false),
            (nameof(OuterDimensions.Height_IN), typeof(double), false),
            (nameof(OuterDimensions.Thickness_IN), typeof(double), false),
            (nameof(OuterDimensions.XOffset_IN), typeof(double), false),
            (nameof(OuterDimensions.YOffset_IN), typeof(double), false),
            (nameof(OuterDimensions.ZOffset_IN), typeof(double), false),
            (nameof(OuterDimensions.Round_r_IN), typeof(double), false),
            (nameof(OuterDimensions.Resolution), typeof(int), false),
            (nameof(OuterDimensions.OSCADMethod), typeof(string), true),
            (nameof(OuterDimensions.CreatedAt), typeof(DateTime), false)
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
            string createTableSql = $"CREATE TABLE IF NOT EXISTS OuterDimensions ({string.Join(", ", columns)}, {foreignKeyConstraint})";
            
            await connection.ExecuteAsync(createTableSql);
        }

        // Insert a single OuterDimensions and return the new Id
        public static async Task<int> InsertAsync(this OuterDimensions entity, SqliteConnection connection)
        {
            IEnumerable<string> columns = Properties.Where(p => p.Name != "Id").Select(p => p.Name);
            IEnumerable<string> parameters = columns.Select(c => $"@{c}");
            string sql = $"INSERT INTO OuterDimensions ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)}); SELECT last_insert_rowid();";
            var id = await connection.ExecuteScalarAsync<int>(sql, entity);
            entity.Id = id; // Update the entity with the new Id
            return id;
        }

        // Upsert (INSERT OR REPLACE) and return the Id - Only updates if Name and Description match
        public static async Task<int> UpsertAsync(this OuterDimensions entity, SqliteConnection connection)
        {
            // First, try to find an existing record with matching Name and Description
            const string selectSql = "SELECT Id FROM OuterDimensions WHERE Name = @Name AND Description = @Description AND OperationType = @OperationType LIMIT 1";
            var existingId = await connection.QuerySingleOrDefaultAsync<int?>(selectSql, new { entity.Name, entity.Description, OperationType = entity.OperationType.ToString() });

            if (existingId.HasValue)
            {
                // Update the existing record
                entity.Id = existingId.Value;
                IEnumerable<string> setClause = Properties.Where(p => p.Name != "Id").Select(p => $"{p.Name} = @{p.Name}");
                string updateSql = $"UPDATE OuterDimensions SET {string.Join(", ", setClause)} WHERE Id = @Id";
                await connection.ExecuteAsync(updateSql, entity);
                return existingId.Value;
            }
            else
            {
                // Insert as a new record
                IEnumerable<string> columns = Properties.Where(p => p.Name != "Id").Select(p => p.Name);
                IEnumerable<string> parameters = columns.Select(c => $"@{c}");
                string insertSql = $"INSERT INTO OuterDimensions ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)}); SELECT last_insert_rowid();";
                var id = await connection.ExecuteScalarAsync<int>(insertSql, entity);
                entity.Id = id;
                return id;
            }
        }

        // Update
        public static async Task UpdateAsync(this OuterDimensions entity, SqliteConnection connection)
        {
            IEnumerable<string> setClause = Properties.Where(p => p.Name != "Id").Select(p => $"{p.Name} = @{p.Name}");
            string sql = $"UPDATE OuterDimensions SET {string.Join(", ", setClause)} WHERE Id = @Id";
            await connection.ExecuteAsync(sql, entity);
        }

        // Delete by Id
        public static async Task DeleteAsync(this OuterDimensions entity, SqliteConnection connection) => 
            await connection.ExecuteAsync("DELETE FROM OuterDimensions WHERE Id = @Id", new { entity.Id });

        // Select by Id
        public static async Task<OuterDimensions?> GetByIdAsync(this OuterDimensions _, SqliteConnection connection, int id) => 
            await connection.QuerySingleOrDefaultAsync<OuterDimensions>("SELECT * FROM OuterDimensions WHERE Id = @Id", new { Id = id });

        // Select all
        public static async Task<IEnumerable<OuterDimensions>> GetAllAsync(this OuterDimensions _, SqliteConnection connection) => 
            await connection.QueryAsync<OuterDimensions>("SELECT * FROM OuterDimensions");

        // Get by Material
        public static async Task<IEnumerable<OuterDimensions>> GetByMaterialAsync(this OuterDimensions _, SqliteConnection connection, string material) => 
            await connection.QueryAsync<OuterDimensions>(
                "SELECT * FROM OuterDimensions WHERE Material = @Material ORDER BY CreatedAt DESC",
                new { Material = material });

        // Get by AxisDimensionsId (Foreign Key)
        public static async Task<IEnumerable<OuterDimensions>> GetByAxisDimensionsIdAsync(this OuterDimensions _, SqliteConnection connection, int axisDimensionsId) => 
            await connection.QueryAsync<OuterDimensions>(
                "SELECT * FROM OuterDimensions WHERE AxisDimensionsId = @AxisDimensionsId ORDER BY CreatedAt DESC",
                new { AxisDimensionsId = axisDimensionsId });

        // Get all with no assigned axis
        public static async Task<IEnumerable<OuterDimensions>> GetUnassignedAsync(this OuterDimensions _, SqliteConnection connection) => 
            await connection.QueryAsync<OuterDimensions>(
                "SELECT * FROM OuterDimensions WHERE AxisDimensionsId IS NULL ORDER BY CreatedAt DESC");

        // Get all with joined AxisDimensions OSCADMethod
        public static async Task<IEnumerable<OuterDimensions>> GetAllWithAxisAsync(this OuterDimensions _, SqliteConnection connection)
        {
            const string sql = @"
                SELECT 
                    od.*,
                    ad.OSCADMethod as AxisOSCADMethod
                FROM OuterDimensions od
                LEFT JOIN AxisDimensions ad ON od.AxisDimensionsId = ad.Id
                ORDER BY od.CreatedAt DESC";
            
            var result = await connection.QueryAsync<OuterDimensions, string, OuterDimensions>(
                sql,
                (outerDim, axisMethod) =>
                {
                    outerDim.AxisOSCADMethod = axisMethod;
                    return outerDim;
                },
                splitOn: "AxisOSCADMethod"
            );
            
            return result;
        }

        // Get by Id with joined AxisDimensions OSCADMethod
        public static async Task<OuterDimensions?> GetByIdWithAxisAsync(this OuterDimensions _, SqliteConnection connection, int id)
        {
            const string sql = @"
                SELECT 
                    od.*,
                    ad.OSCADMethod as AxisOSCADMethod
                FROM OuterDimensions od
                LEFT JOIN AxisDimensions ad ON od.AxisDimensionsId = ad.Id
                WHERE od.Id = @Id";
            
            var result = await connection.QueryAsync<OuterDimensions, string, OuterDimensions>(
                sql,
                (outerDim, axisMethod) =>
                {
                    outerDim.AxisOSCADMethod = axisMethod;
                    return outerDim;
                },
                new { Id = id },
                splitOn: "AxisOSCADMethod"
            );
            
            return result.FirstOrDefault();
        }

        // Get by AxisDimensionsId with joined OSCADMethod
        public static async Task<IEnumerable<OuterDimensions>> GetByAxisDimensionsIdWithAxisAsync(this OuterDimensions _, SqliteConnection connection, int axisDimensionsId)
        {
            const string sql = @"
                SELECT 
                    od.*,
                    ad.OSCADMethod as AxisOSCADMethod
                FROM OuterDimensions od
                LEFT JOIN AxisDimensions ad ON od.AxisDimensionsId = ad.Id
                WHERE od.AxisDimensionsId = @AxisDimensionsId
                ORDER BY od.CreatedAt DESC";
            
            var result = await connection.QueryAsync<OuterDimensions, string, OuterDimensions>(
                sql,
                (outerDim, axisMethod) =>
                {
                    outerDim.AxisOSCADMethod = axisMethod;
                    return outerDim;
                },
                new { AxisDimensionsId = axisDimensionsId },
                splitOn: "AxisOSCADMethod"
            );
            
            return result;
        }

        // Get by Name with joined AxisDimensions OSCADMethod
        public static async Task<IEnumerable<OuterDimensions>> GetByNameWithAxisAsync(this OuterDimensions _, SqliteConnection connection, string name)
        {
            const string sql = @"
                SELECT 
                    od.*,
                    ad.OSCADMethod as AxisOSCADMethod
                FROM OuterDimensions od
                LEFT JOIN AxisDimensions ad ON od.AxisDimensionsId = ad.Id
                WHERE od.Name = @Name
                ORDER BY od.OperationType ASC, od.CreatedAt ASC";

            var result = await connection.QueryAsync<OuterDimensions, string, OuterDimensions>(
                sql,
                (outerDim, axisMethod) =>
                {
                    outerDim.AxisOSCADMethod = axisMethod;
                    return outerDim;
                },
                new { Name = name },
                splitOn: "AxisOSCADMethod"
            );
            
            return result;
        }

        // Get by OperationType
        public static async Task<IEnumerable<OuterDimensions>> GetByOperationTypeAsync(this OuterDimensions _, SqliteConnection connection, string operationType) => 
            await connection.QueryAsync<OuterDimensions>(
                "SELECT * FROM OuterDimensions WHERE OperationType = @OperationType ORDER BY CreatedAt DESC",
                new { OperationType = operationType });

        // Get by Name and OperationType with joined AxisDimensions OSCADMethod
        public static async Task<IEnumerable<OuterDimensions>> GetByNameAndOperationTypeWithAxisAsync(this OuterDimensions _, SqliteConnection connection, string name, string operationType)
        {
            const string sql = @"
                SELECT 
                    od.*,
                    ad.OSCADMethod as AxisOSCADMethod
                FROM OuterDimensions od
                LEFT JOIN AxisDimensions ad ON od.AxisDimensionsId = ad.Id
                WHERE od.Name = @Name AND od.OperationType = @OperationType
                ORDER BY od.OperationType ASC, od.CreatedAt ASC";

            var result = await connection.QueryAsync<OuterDimensions, string, OuterDimensions>(
                sql,
                (outerDim, axisMethod) =>
                {
                    outerDim.AxisOSCADMethod = axisMethod;
                    return outerDim;
                },
                new { Name = name, OperationType = operationType },
                splitOn: "AxisOSCADMethod"
            );
            
            return result;
        }
    }
}