using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class RoundedCylinder(Dictionary<string, object> parameters) : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters = parameters;

        public double Radius => (double)_parameters["r"];
        public double Height => (double)_parameters["h"];
        public double RoundRadius => (double)_parameters["round_r"];
        public double? Radius1 => _parameters.TryGetValue("r1", out var v) ? (double?)v : null;
        public double? Radius2 => _parameters.TryGetValue("r2", out var v) ? (double?)v : null;
        public int Resolution => _parameters.TryGetValue("resolution", out var v) ? (int)v : 360;
        public bool Center => _parameters.TryGetValue("center", out var c) && c is bool b && b;

        public string OSCADMethod
        {
            get
            {
                var fn = Resolution;
                var h = Height;
                var r = Radius;
                var rr = RoundRadius;
                var r1 = Radius1;
                var r2 = Radius2;

                // No rounding → just return normal cylinder
                if (rr <= 0)
                    return new Cylinder(_parameters).OSCADMethod;

                // Clamp rounding to prevent negative dimensions
                var clamped_rr = Math.Min(rr, Math.Min(h / 2 * 0.99, r * 0.99));

                // Inner (shrunk) height
                var inner_h = Math.Max(0.001, h - 2 * clamped_rr);

                // Is this a conical/frustum cylinder?
                bool isConical = (r1.HasValue && r1.Value > 0) || (r2.HasValue && r2.Value > 0);

                // Build inner cylinder (shrunk, bottom-aligned)
                var innerParams = new Dictionary<string, object>
        {
            { "h", inner_h },
            { "center", false },
            { "resolution", fn }
        };

                if (isConical)
                {
                    var inner_r1 = r1.HasValue ? Math.Max(0.001, r1.Value - clamped_rr) : (double?)null;
                    var inner_r2 = r2.HasValue ? Math.Max(0.001, r2.Value - clamped_rr) : (double?)null;

                    if (inner_r1.HasValue) innerParams["r1"] = inner_r1.Value;
                    if (inner_r2.HasValue) innerParams["r2"] = inner_r2.Value;
                }
                else
                {
                    var inner_r = Math.Max(0.001, r - clamped_rr);
                    innerParams["r"] = inner_r;
                }

                var innerCylinder = new Cylinder(innerParams);

                // ——— Create the correct rounding body (this is the key fix) ———
                IScadObject roundingBody;

                if (isConical)
                {
                    // For conical: use a short tapered cylinder (same taper, height ≈ 2×rr, centered)
                    var rbParams = new Dictionary<string, object>
            {
                { "h", Math.Max(0.002, 2 * clamped_rr) },  // tiny height to cover full fillet
                { "center", true },
                { "resolution", Math.Max(24, fn / 3) }
            };

                    // Match the taper: r1/r2 of rounding body = clamped_rr at both ends
                    if (r1.HasValue) rbParams["r1"] = clamped_rr;
                    if (r2.HasValue) rbParams["r2"] = clamped_rr;
                    // Fallback (should never happen due to isConical check)
                    if (!rbParams.ContainsKey("r1") && !rbParams.ContainsKey("r2"))
                        rbParams["r"] = clamped_rr;

                    roundingBody = new Cylinder(rbParams);
                }
                else
                {
                    // Straight cylinder → use perfect sphere
                    var sphereParams = new Dictionary<string, object>
            {
                { "r", clamped_rr },
                { "resolution", Math.Max(24, fn / 3) }
            };
                    roundingBody = new Sphere(sphereParams);
                }

                // Final minkowski — no translate here (you handle Z-offset externally)
                return new Minkowski(innerCylinder, roundingBody).OSCADMethod;
            }
        }

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "RoundedCylinder" },
            { "r", Radius },
            { "h", Height },
            { "round_r", RoundRadius },
            { "r1", Radius1 },
            { "r2", Radius2 },
            { "resolution", Resolution },
            { "center", Center }
        };
    }
}
