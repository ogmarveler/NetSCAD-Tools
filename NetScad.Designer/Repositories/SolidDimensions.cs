using Microsoft.Data.Sqlite;
using NetScad.Core.Interfaces;
using static NetScad.Core.Measurements.Conversion;
using Dapper;

namespace NetScad.Designer.Repositories
{
    // Entity for CAD solid dimensions - supports both cubes and cylinders
    public class SolidDimensions : IScadObject
    {
        public int Id { get; set; }
        public int? AxisDimensionsId { get; set; } // Foreign key to AxisDimensions
        public int? ModuleDimensionsId { get; set; } // Foreign key to ModuleDimensions
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Material { get; set; }
        public string OperationType { get; set; } = Core.Primitives.OperationType.Union.ToString(); // Add or Subtract
        public string SolidType { get; set; } = "Cube"; // "Cube", "RoundCube", or "Cylinder"
        public const int OpenSCAD_DecimalPlaces = 6; // High precision for 3D printing

        // Cube/RoundCube properties
        public double Length_MM { get; set; } // Millimeters (default)
        public double Width_MM { get; set; }
        public double Height_MM { get; set; }
        public double Thickness_MM { get; set; } = 0; // For hollow objects

        // Cylinder properties
        public double Radius_MM { get; set; } = 0; // Millimeters (default)
        public double Radius1_MM { get; set; } = 0; // Optional radius1 for cone
        public double Radius2_MM { get; set; } = 0; // Optional radius2 for cone
        public double CylinderHeight_MM { get; set; } = 0; // Cylinder-specific height

        // Common transformation properties
        public double XOffset_MM { get; set; } // X-axis translation offset
        public double YOffset_MM { get; set; } // Y-axis translation offset
        public double ZOffset_MM { get; set; } // Z-axis translation offset
        public double XRotate { get; set; } = 0;
        public double YRotate { get; set; } = 0;
        public double ZRotate { get; set; } = 0;

        // Rounding properties (for RoundCube)
        public double Round_r_MM { get; set; } = 0; // Rounding radius (default 0mm)
        public double Round_h_MM { get; set; } = 0; // Rounding height (minkowski offset)
        public int Resolution { get; set; } = 180; // Default resolution for curves

        // Imperial conversions (computed) - Cube properties
        public double Length_IN => Math.Round(MillimeterToInches(Length_MM), OpenSCAD_DecimalPlaces);
        public double Width_IN => Math.Round(MillimeterToInches(Width_MM), OpenSCAD_DecimalPlaces);
        public double Height_IN => Math.Round(MillimeterToInches(Height_MM), OpenSCAD_DecimalPlaces);
        public double Thickness_IN => Math.Round(MillimeterToInches(Thickness_MM), OpenSCAD_DecimalPlaces);

        // Imperial conversions - Cylinder properties
        public double Radius_IN => Math.Round(MillimeterToInches(Radius_MM), OpenSCAD_DecimalPlaces);
        public double Radius1_IN => Math.Round(MillimeterToInches(Radius1_MM), OpenSCAD_DecimalPlaces);
        public double Radius2_IN => Math.Round(MillimeterToInches(Radius2_MM), OpenSCAD_DecimalPlaces);
        public double CylinderHeight_IN => Math.Round(MillimeterToInches(CylinderHeight_MM), OpenSCAD_DecimalPlaces);

        // Imperial conversions - Common properties
        public double XOffset_IN => Math.Round(MillimeterToInches(XOffset_MM), OpenSCAD_DecimalPlaces);
        public double YOffset_IN => Math.Round(MillimeterToInches(YOffset_MM), OpenSCAD_DecimalPlaces);
        public double ZOffset_IN => Math.Round(MillimeterToInches(ZOffset_MM), OpenSCAD_DecimalPlaces);
        public double Round_r_IN => Math.Round(MillimeterToInches(Round_r_MM), OpenSCAD_DecimalPlaces);
        public double Round_h_IN => Math.Round(MillimeterToInches(Round_h_MM), OpenSCAD_DecimalPlaces);

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

        // Join property for ModuleDimensions name
        private string? _moduleName;
        public string? ModuleName
        {
            get => _moduleName ?? (ModuleDimensionsId.HasValue ? $"Module: {ModuleDimensionsId.Value}" : "No Module Assigned");
            set => _moduleName = value;
        }

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "Id", Id },
            { "AxisDimensionsId", AxisDimensionsId ?? (object)DBNull.Value },
            { "ModuleDimensionsId", ModuleDimensionsId ?? (object)DBNull.Value },
            { "Name", Name },
            { "Description", Description ?? (object)DBNull.Value },
            { "Material", Material ?? (object)DBNull.Value },
            { "OperationType", OperationType },
            { "SolidType", SolidType },
            { "Length_MM", Length_MM },
            { "Width_MM", Width_MM },
            { "Height_MM", Height_MM },
            { "Thickness_MM", Thickness_MM },
            { "Radius_MM", Radius_MM },
            { "Radius1_MM", Radius1_MM },
            { "Radius2_MM", Radius2_MM },
            { "CylinderHeight_MM", CylinderHeight_MM },
            { "XOffset_MM", XOffset_MM },
            { "YOffset_MM", YOffset_MM },
            { "ZOffset_MM", ZOffset_MM },
            { "XRotate", XRotate },
            { "YRotate", YRotate },
            { "ZRotate", ZRotate },
            { "Length_IN", Length_IN },
            { "Width_IN", Width_IN },
            { "Height_IN", Height_IN },
            { "Thickness_IN", Thickness_IN },
            { "Radius_IN", Radius_IN },
            { "Radius1_IN", Radius1_IN },
            { "Radius2_IN", Radius2_IN },
            { "CylinderHeight_IN", CylinderHeight_IN },
            { "XOffset_IN", XOffset_IN },
            { "YOffset_IN", YOffset_IN },
            { "ZOffset_IN", ZOffset_IN },
            { "Round_r_MM", Round_r_MM },
            { "Round_h_MM", Round_h_MM },
            { "Round_r_IN", Round_r_IN },
            { "Round_h_IN", Round_h_IN },
            { "Resolution", Resolution },
            { "OSCADMethod", OSCADMethod },
            { "CreatedAt", CreatedAt }
        };
    }

    // Extension methods for SolidDimensions CRUD operations
    public static class SolidDimensionsExtensions
    {
        // Property mapping for SolidDimensions (AOT-safe)
        private static readonly (string Name, Type Type, bool IsNullable)[] Properties =
        [
            (nameof(SolidDimensions.Id), typeof(int), false),
            (nameof(SolidDimensions.AxisDimensionsId), typeof(int), true),
            (nameof(SolidDimensions.ModuleDimensionsId), typeof(int), true),
            (nameof(SolidDimensions.Name), typeof(string), true),
            (nameof(SolidDimensions.Description), typeof(string), true),
            (nameof(SolidDimensions.Material), typeof(string), true),
            (nameof(SolidDimensions.OperationType), typeof(string), false),
            (nameof(SolidDimensions.SolidType), typeof(string), false),
            (nameof(SolidDimensions.Length_MM), typeof(double), false),
            (nameof(SolidDimensions.Width_MM), typeof(double), false),
            (nameof(SolidDimensions.Height_MM), typeof(double), false),
            (nameof(SolidDimensions.Thickness_MM), typeof(double), false),
            (nameof(SolidDimensions.Radius_MM), typeof(double), false),
            (nameof(SolidDimensions.Radius1_MM), typeof(double), false),
            (nameof(SolidDimensions.Radius2_MM), typeof(double), false),
            (nameof(SolidDimensions.CylinderHeight_MM), typeof(double), false),
            (nameof(SolidDimensions.XOffset_MM), typeof(double), false),
            (nameof(SolidDimensions.YOffset_MM), typeof(double), false),
            (nameof(SolidDimensions.ZOffset_MM), typeof(double), false),
            (nameof(SolidDimensions.XRotate), typeof(double), false),
            (nameof(SolidDimensions.YRotate), typeof(double), false),
            (nameof(SolidDimensions.ZRotate), typeof(double), false),
            (nameof(SolidDimensions.Length_IN), typeof(double), false),
            (nameof(SolidDimensions.Width_IN), typeof(double), false),
            (nameof(SolidDimensions.Height_IN), typeof(double), false),
            (nameof(SolidDimensions.Thickness_IN), typeof(double), false),
            (nameof(SolidDimensions.Radius_IN), typeof(double), false),
            (nameof(SolidDimensions.Radius1_IN), typeof(double), false),
            (nameof(SolidDimensions.Radius2_IN), typeof(double), false),
            (nameof(SolidDimensions.CylinderHeight_IN), typeof(double), false),
            (nameof(SolidDimensions.XOffset_IN), typeof(double), false),
            (nameof(SolidDimensions.YOffset_IN), typeof(double), false),
            (nameof(SolidDimensions.ZOffset_IN), typeof(double), false),
            (nameof(SolidDimensions.Round_r_MM), typeof(double), false),
            (nameof(SolidDimensions.Round_h_MM), typeof(double), false),
            (nameof(SolidDimensions.Round_r_IN), typeof(double), false),
            (nameof(SolidDimensions.Round_h_IN), typeof(double), false),
            (nameof(SolidDimensions.Resolution), typeof(int), false),
            (nameof(SolidDimensions.OSCADMethod), typeof(string), true),
            (nameof(SolidDimensions.CreatedAt), typeof(DateTime), false)
        ];

        // Create table
        public static async Task CreateTable(this SqliteConnection connection)
        {
            IEnumerable<string> columns = Properties.Select(p =>
                $"{p.Name} {(p.Type == typeof(int) ? "INTEGER" : p.Type == typeof(string) ? "TEXT" : p.Type == typeof(double) ? "REAL" : "TEXT")} " +
                (p.Name == "Id" ? "PRIMARY KEY AUTOINCREMENT" : p.IsNullable ? "" : "NOT NULL")
            );

            // Add foreign key constraints
            string axisFK = "FOREIGN KEY (AxisDimensionsId) REFERENCES AxisDimensions(Id) ON DELETE SET NULL";
            string moduleFK = "FOREIGN KEY (ModuleDimensionsId) REFERENCES ModuleDimensions(Id) ON DELETE SET NULL";
            string createTableSql = $"CREATE TABLE IF NOT EXISTS SolidDimensions ({string.Join(", ", columns)}, {axisFK}, {moduleFK})";

            await connection.ExecuteAsync(createTableSql);
        }

        // Insert a single SolidDimensions and return the new Id
        public static async Task<int> InsertAsync(this SolidDimensions entity, SqliteConnection connection)
        {
            IEnumerable<string> columns = Properties.Where(p => p.Name != "Id").Select(p => p.Name);
            IEnumerable<string> parameters = columns.Select(c => $"@{c}");
            string sql = $"INSERT INTO SolidDimensions ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)}); SELECT last_insert_rowid();";
            var id = await connection.ExecuteScalarAsync<int>(sql, entity);
            entity.Id = id; // Update the entity with the new Id
            return id;
        }

        // Upsert (INSERT OR REPLACE) and return the Id - Only updates if Name and Description match
        public static async Task<int> UpsertAsync(this SolidDimensions entity, SqliteConnection connection)
        {
            // First, try to find an existing record with matching Name and Description
            const string selectSql = "SELECT Id FROM SolidDimensions WHERE Name = @Name AND Description = @Description AND OperationType = @OperationType LIMIT 1";
            var existingId = await connection.QuerySingleOrDefaultAsync<int?>(selectSql, new { entity.Name, entity.Description, OperationType = entity.OperationType.ToString() });

            if (existingId.HasValue)
            {
                // Update the existing record
                entity.Id = existingId.Value;
                IEnumerable<string> setClause = Properties.Where(p => p.Name != "Id").Select(p => $"{p.Name} = @{p.Name}");
                string updateSql = $"UPDATE SolidDimensions SET {string.Join(", ", setClause)} WHERE Id = @Id";
                await connection.ExecuteAsync(updateSql, entity);
                return existingId.Value;
            }
            else
            {
                // Insert as a new record
                IEnumerable<string> columns = Properties.Where(p => p.Name != "Id").Select(p => p.Name);
                IEnumerable<string> parameters = columns.Select(c => $"@{c}");
                string insertSql = $"INSERT INTO SolidDimensions ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)}); SELECT last_insert_rowid();";
                var id = await connection.ExecuteScalarAsync<int>(insertSql, entity);
                entity.Id = id;
                return id;
            }
        }

        // Update
        public static async Task UpdateAsync(this SolidDimensions entity, SqliteConnection connection)
        {
            IEnumerable<string> setClause = Properties.Where(p => p.Name != "Id").Select(p => $"{p.Name} = @{p.Name}");
            string sql = $"UPDATE SolidDimensions SET {string.Join(", ", setClause)} WHERE Id = @Id";
            await connection.ExecuteAsync(sql, entity);
        }

        // Update ModuleDimensionsId for multiple SolidDimensions
        public static async Task UpdateModuleDimensionsIdAsync(this SqliteConnection connection, IEnumerable<int> solidIds, int? moduleDimensionsId)
        {
            string sql = "UPDATE SolidDimensions SET ModuleDimensionsId = @ModuleDimensionsId WHERE Id = @Id";
            foreach (var solidId in solidIds)
            {
                await connection.ExecuteAsync(sql, new { ModuleDimensionsId = moduleDimensionsId, Id = solidId });
            }
        }

        // Delete by Id
        public static async Task DeleteAsync(this SolidDimensions entity, SqliteConnection connection) =>
            await connection.ExecuteAsync("DELETE FROM SolidDimensions WHERE Id = @Id", new { entity.Id });

        // Select by Id
        public static async Task<SolidDimensions?> GetByIdAsync(this SolidDimensions _, SqliteConnection connection, int id) =>
            await connection.QuerySingleOrDefaultAsync<SolidDimensions>("SELECT * FROM SolidDimensions WHERE Id = @Id", new { Id = id });

        // Select all
        public static async Task<IEnumerable<SolidDimensions>> GetAllAsync(this SolidDimensions _, SqliteConnection connection) =>
            await connection.QueryAsync<SolidDimensions>("SELECT * FROM SolidDimensions");

        // Get by Material
        public static async Task<IEnumerable<SolidDimensions>> GetByMaterialAsync(this SolidDimensions _, SqliteConnection connection, string material) =>
            await connection.QueryAsync<SolidDimensions>(
                "SELECT * FROM SolidDimensions WHERE Material = @Material ORDER BY CreatedAt DESC",
                new { Material = material });

        // Get by SolidType
        public static async Task<IEnumerable<SolidDimensions>> GetBySolidTypeAsync(this SolidDimensions _, SqliteConnection connection, string solidType) =>
            await connection.QueryAsync<SolidDimensions>(
                "SELECT * FROM SolidDimensions WHERE SolidType = @SolidType ORDER BY CreatedAt DESC",
                new { SolidType = solidType });

        // Get by AxisDimensionsId (Foreign Key)
        public static async Task<IEnumerable<SolidDimensions>> GetByAxisDimensionsIdAsync(this SolidDimensions _, SqliteConnection connection, int axisDimensionsId) =>
            await connection.QueryAsync<SolidDimensions>(
                "SELECT * FROM SolidDimensions WHERE AxisDimensionsId = @AxisDimensionsId ORDER BY CreatedAt DESC",
                new { AxisDimensionsId = axisDimensionsId });

        // Get by ModuleDimensionsId (Foreign Key)
        public static async Task<IEnumerable<SolidDimensions>> GetByModuleDimensionsIdAsync(this SolidDimensions _, SqliteConnection connection, int moduleDimensionsId) =>
            await connection.QueryAsync<SolidDimensions>(
                "SELECT * FROM SolidDimensions WHERE ModuleDimensionsId = @ModuleDimensionsId ORDER BY CreatedAt DESC",
                new { ModuleDimensionsId = moduleDimensionsId });

        // Get all with no assigned axis
        public static async Task<IEnumerable<SolidDimensions>> GetUnassignedAsync(this SolidDimensions _, SqliteConnection connection) =>
            await connection.QueryAsync<SolidDimensions>(
                "SELECT * FROM SolidDimensions WHERE AxisDimensionsId IS NULL ORDER BY CreatedAt DESC");

        // Get all with joined AxisDimensions OSCADMethod
        public static async Task<IEnumerable<SolidDimensions>> GetAllWithAxisAsync(this SolidDimensions _, SqliteConnection connection)
        {
            const string sql = @"
                SELECT 
                    sd.*,
                    ad.OSCADMethod as AxisOSCADMethod
                FROM SolidDimensions sd
                LEFT JOIN AxisDimensions ad ON sd.AxisDimensionsId = ad.Id
                ORDER BY sd.CreatedAt DESC";

            var result = await connection.QueryAsync<SolidDimensions, string, SolidDimensions>(
                sql,
                (solidDim, axisMethod) =>
                {
                    solidDim.AxisOSCADMethod = axisMethod;
                    return solidDim;
                },
                splitOn: "AxisOSCADMethod"
            );

            return result;
        }

        // Get by Id with joined AxisDimensions OSCADMethod
        public static async Task<SolidDimensions?> GetByIdWithAxisAsync(this SolidDimensions _, SqliteConnection connection, int id)
        {
            const string sql = @"
                SELECT 
                    sd.*,
                    ad.OSCADMethod as AxisOSCADMethod
                FROM SolidDimensions sd
                LEFT JOIN AxisDimensions ad ON sd.AxisDimensionsId = ad.Id
                WHERE sd.Id = @Id";

            var result = await connection.QueryAsync<SolidDimensions, string, SolidDimensions>(
                sql,
                (solidDim, axisMethod) =>
                {
                    solidDim.AxisOSCADMethod = axisMethod;
                    return solidDim;
                },
                new { Id = id },
                splitOn: "AxisOSCADMethod"
            );

            return result.FirstOrDefault();
        }

        // Get by AxisDimensionsId with joined OSCADMethod
        public static async Task<IEnumerable<SolidDimensions>> GetByAxisDimensionsIdWithAxisAsync(this SolidDimensions _, SqliteConnection connection, int axisDimensionsId)
        {
            const string sql = @"
                SELECT 
                    sd.*,
                    ad.OSCADMethod as AxisOSCADMethod
                FROM SolidDimensions sd
                LEFT JOIN AxisDimensions ad ON sd.AxisDimensionsId = ad.Id
                WHERE sd.AxisDimensionsId = @AxisDimensionsId
                ORDER BY sd.CreatedAt DESC";

            var result = await connection.QueryAsync<SolidDimensions, string, SolidDimensions>(
                sql,
                (solidDim, axisMethod) =>
                {
                    solidDim.AxisOSCADMethod = axisMethod;
                    return solidDim;
                },
                new { AxisDimensionsId = axisDimensionsId },
                splitOn: "AxisOSCADMethod"
            );

            return result;
        }

        // Get by Name with joined AxisDimensions OSCADMethod
        public static async Task<IEnumerable<SolidDimensions>> GetByNameWithAxisAsync(this SolidDimensions _, SqliteConnection connection, string name)
        {
            const string sql = @"
                SELECT 
                    sd.*,
                    ad.OSCADMethod as AxisOSCADMethod
                FROM SolidDimensions sd
                LEFT JOIN AxisDimensions ad ON sd.AxisDimensionsId = ad.Id
                WHERE sd.Name = @Name
                ORDER BY sd.OperationType ASC, sd.CreatedAt ASC";

            var result = await connection.QueryAsync<SolidDimensions, string, SolidDimensions>(
                sql,
                (solidDim, axisMethod) =>
                {
                    solidDim.AxisOSCADMethod = axisMethod;
                    return solidDim;
                },
                new { Name = name },
                splitOn: "AxisOSCADMethod"
            );

            return result;
        }

        // Get by Name with joined AxisDimensions OSCADMethod AND ModuleDimensions Name
        public static async Task<IEnumerable<SolidDimensions>> GetByNameWithAxisAndModuleAsync(this SolidDimensions _, SqliteConnection connection, string name)
        {
            const string sql = @"
                SELECT 
                    sd.*,
                    ad.OSCADMethod as AxisOSCADMethod,
                    md.Name as ModuleName
                FROM SolidDimensions sd
                LEFT JOIN AxisDimensions ad ON sd.AxisDimensionsId = ad.Id
                LEFT JOIN ModuleDimensions md ON sd.ModuleDimensionsId = md.Id
                WHERE sd.Name = @Name
                ORDER BY sd.OperationType ASC, sd.CreatedAt ASC";

            var result = await connection.QueryAsync<SolidDimensions, string, string, SolidDimensions>(
                sql,
                (solidDim, axisMethod, moduleName) =>
                {
                    solidDim.AxisOSCADMethod = axisMethod;
                    solidDim.ModuleName = moduleName;
                    return solidDim;
                },
                new { Name = name },
                splitOn: "AxisOSCADMethod,ModuleName"
            );

            return result;
        }

        // Get by OperationType
        public static async Task<IEnumerable<SolidDimensions>> GetByOperationTypeAsync(this SolidDimensions _, SqliteConnection connection, string operationType) =>
            await connection.QueryAsync<SolidDimensions>(
                "SELECT * FROM SolidDimensions WHERE OperationType = @OperationType ORDER BY CreatedAt DESC",
                new { OperationType = operationType });

        // Get by Name and OperationType with joined AxisDimensions OSCADMethod
        public static async Task<IEnumerable<SolidDimensions>> GetByNameAndOperationTypeWithAxisAsync(this SolidDimensions _, SqliteConnection connection, string name, string operationType)
        {
            const string sql = @"
                SELECT 
                    sd.*,
                    ad.OSCADMethod as AxisOSCADMethod
                FROM SolidDimensions sd
                LEFT JOIN AxisDimensions ad ON sd.AxisDimensionsId = ad.Id
                WHERE sd.Name = @Name AND sd.OperationType = @OperationType
                ORDER BY sd.CreatedAt DESC";

            var result = await connection.QueryAsync<SolidDimensions, string, SolidDimensions>(
                sql,
                (solidDim, axisMethod) =>
                {
                    solidDim.AxisOSCADMethod = axisMethod;
                    return solidDim;
                },
                new { Name = name, OperationType = operationType },
                splitOn: "AxisOSCADMethod"
            );

            return result;
        }

        // Get by Name and SolidType with joined AxisDimensions OSCADMethod
        public static async Task<IEnumerable<SolidDimensions>> GetByNameAndSolidTypeWithAxisAsync(this SolidDimensions _, SqliteConnection connection, string name, string solidType)
        {
            const string sql = @"
                SELECT 
                    sd.*,
                    ad.OSCADMethod as AxisOSCADMethod
                FROM SolidDimensions sd
                LEFT JOIN AxisDimensions ad ON sd.AxisDimensionsId = ad.Id
                WHERE sd.Name = @Name AND sd.SolidType = @SolidType
                ORDER BY sd.CreatedAt DESC";

            var result = await connection.QueryAsync<SolidDimensions, string, SolidDimensions>(
                sql,
                (solidDim, axisMethod) =>
                {
                    solidDim.AxisOSCADMethod = axisMethod;
                    return solidDim;
                },
                new { Name = name, SolidType = solidType },
                splitOn: "AxisOSCADMethod"
            );

            return result;
        }
    }
}