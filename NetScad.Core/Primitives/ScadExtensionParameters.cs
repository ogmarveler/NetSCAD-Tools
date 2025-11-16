using NetScad.Core.Interfaces;
using NetScad.Core.Models;

namespace NetScad.Core.Primitives
{
    public static partial class ScadExtensions
    {
        // OScad3D Extensions
        public static Dictionary<string, object> CubeParams(this OScad3D self, double sizeX, double sizeY, double sizeZ)
        {
            if (self != OScad3D.Cube)
                throw new ArgumentException("SetParams with size_x, size_y, size_z is only valid for Cube");
            return new Dictionary<string, object>
            {
                { "size_x", sizeX },
                { "size_y", sizeY },
                { "size_z", sizeZ }
            };
        }

        public static Dictionary<string, object> CylinderParams(this OScad3D self, double r, double h, double? r1 = null, double? r2 = null, double? resolution = null)
        {
            if (self != OScad3D.Cylinder)
                throw new ArgumentException("SetParams with r, h, r1, r2, resolution is only valid for Cylinder");
            var dict = new Dictionary<string, object> { { "r", r }, { "h", h } };
            if (r1.HasValue) dict["r1"] = r1.Value;
            if (r2.HasValue) dict["r2"] = r2.Value;
            if (resolution.HasValue) dict["resolution"] = resolution.Value;
            return dict;
        }

        public static Dictionary<string, object> SphereParams(this OScad3D self, double r, double? resolution = null)
        {
            if (self != OScad3D.Sphere)
                throw new ArgumentException("SetParams with r, resolution is only valid for Sphere");
            var dict = new Dictionary<string, object> { { "r", r } };
            if (resolution.HasValue) dict["resolution"] = resolution.Value;
            return dict;
        }

        public static Dictionary<string, object> PolyhedronParams(this OScad3D self, List<List<double>> points, List<List<int>> faces, int convexity = 1)
        {
            if (self != OScad3D.Polyhedron)
                throw new ArgumentException("SetParams with points, faces, convexity is only valid for Polyhedron");
            return new Dictionary<string, object>
            {
                { "points", points },
                { "faces", faces },
                { "convexity", convexity }
            };
        }

        public static Dictionary<string, object> SurfaceParams(this OScad3D self, string file, bool center = false, bool invert = false, int convexity = 1, double scaleX = 1, double scaleY = 1, double scaleZ = 1)
        {
            if (self != OScad3D.Surface)
                throw new ArgumentException("SetParams with file, center, invert, convexity, scale is only valid for Surface");
            return new Dictionary<string, object>
            {
                { "file", file },
                { "center", center },
                { "invert", invert },
                { "convexity", convexity },
                { "scaleX", scaleX },
                { "scaleY", scaleY },
                { "scaleZ", scaleZ }
            };
        }

        public static Dictionary<string, object> RoundedCubeParams(this OScad3D self, double sizeX, double sizeY, double sizeZ, double roundR, double? roundH = null, double? resolution = null)
        {
            if (self != OScad3D.RoundedCube)
                throw new ArgumentException("SetParams with size_x, size_y, size_z, round_r, round_h, resolution is only valid for RoundedCube");
            var dict = new Dictionary<string, object>
            {
                { "size_x", sizeX },
                { "size_y", sizeY },
                { "size_z", sizeZ },
                { "round_r", roundR }
            };
            if (roundH.HasValue) dict["round_h"] = roundH.Value;
            if (resolution.HasValue) dict["resolution"] = resolution.Value;
            return dict;
        }

        public static Dictionary<string, object> RoundedCylinderParams(this OScad3D self, double r, double h, double roundR, double? roundH = null, double? r1 = null, double? r2 = null, double? resolution = null)
        {
            if (self != OScad3D.RoundedCylinder)
                throw new ArgumentException("SetParams with r, h, round_r, round_h, r1, r2, resolution is only valid for RoundedCylinder");
            var dict = new Dictionary<string, object> { { "r", r }, { "h", h }, { "round_r", roundR } };
            if (roundH.HasValue) dict["round_h"] = roundH.Value;
            if (r1.HasValue) dict["r1"] = r1.Value;
            if (r2.HasValue) dict["r2"] = r2.Value;
            if (resolution.HasValue) dict["resolution"] = resolution.Value;
            return dict;
        }

        public static Dictionary<string, object> RoundedSphereParams(this OScad3D self, double r, double roundR, double? roundH = null, double? resolution = null)
        {
            if (self != OScad3D.RoundedSphere)
                throw new ArgumentException("SetParams with r, round_r, round_h, resolution is only valid for RoundedSphere");
            var dict = new Dictionary<string, object> { { "r", r }, { "round_r", roundR } };
            if (roundH.HasValue) dict["round_h"] = roundH.Value;
            if (resolution.HasValue) dict["resolution"] = resolution.Value;
            return dict;
        }

        public static Dictionary<string, object> RoundedPolyhedronParams(this OScad3D self, List<List<double>> points, List<List<int>> faces, double roundR, double? roundH = null, double? resolution = null)
        {
            if (self != OScad3D.RoundedPolyhedron)
                throw new ArgumentException("SetParams with points, faces, round_r, round_h, resolution is only valid for RoundedPolyhedron");
            var dict = new Dictionary<string, object> { { "points", points }, { "faces", faces }, { "round_r", roundR } };
            if (roundH.HasValue) dict["round_h"] = roundH.Value;
            if (resolution.HasValue) dict["resolution"] = resolution.Value;
            return dict;
        }

        public static Dictionary<string, object> RoundedSurfaceParams(this OScad3D self, string file, double roundR, bool center = false, bool invert = false, int convexity = 1, double scaleX = 1, double scaleY = 1, double scaleZ = 1, double? roundH = null, double? resolution = null)
        {
            if (self != OScad3D.RoundedSurface)
                throw new ArgumentException("SetParams with file, round_r, center, invert, convexity, scale, round_h, resolution is only valid for RoundedSurface");
            var dict = new Dictionary<string, object> 
            { 
                { "file", file }, 
                { "round_r", roundR }, 
                { "center", center },
                { "invert", invert },
                { "convexity", convexity },
                { "scaleX", scaleX },
                { "scaleY", scaleY },
                { "scaleZ", scaleZ }
            };
            if (roundH.HasValue) dict["round_h"] = roundH.Value;
            if (resolution.HasValue) dict["resolution"] = resolution.Value;
            return dict;
        }

        // OScad2D Extensions
        public static Dictionary<string, object> SquareParams(this OScad2D self, double sizeX, double sizeY, bool center = false)
        {
            if (self != OScad2D.Square)
                throw new ArgumentException("SetParams with size_x, size_y, center is only valid for Square");
            return new Dictionary<string, object>
            {
                { "size_x", sizeX },
                { "size_y", sizeY },
                { "center", center }
            };
        }

        public static Dictionary<string, object> CircleParams(this OScad2D self, double r, double? resolution = null)
        {
            if (self != OScad2D.Circle)
                throw new ArgumentException("SetParams with r, resolution is only valid for Circle");
            var dict = new Dictionary<string, object> { { "r", r } };
            if (resolution.HasValue) dict["resolution"] = resolution.Value;
            return dict;
        }

        public static Dictionary<string, object> PolygonParams(this OScad2D self, List<List<double>> points, List<List<int>>? paths = null, int convexity = 1)
        {
            if (self != OScad2D.Polygon)
                throw new ArgumentException("SetParams with points, paths, convexity is only valid for Polygon");
            var dict = new Dictionary<string, object> { { "points", points } };
            if (paths != null) dict["paths"] = paths;
            dict["convexity"] = convexity;
            return dict;
        }

        // OScad1D Extensions
        public static Dictionary<string, object> LineParams(this OScad1D self, double x1, double y1, double x2, double y2)
        {
            if (self != OScad1D.Line)
                throw new ArgumentException("SetParams with x1, y1, x2, y2 is only valid for Line");
            return new Dictionary<string, object>
            {
                { "x1", x1 },
                { "y1", y1 },
                { "x2", x2 },
                { "y2", y2 }
            };
        }

        public static Dictionary<string, object> ArcParams(this OScad1D self, double r, double start, double end, double? resolution = null)
        {
            if (self != OScad1D.Arc)
                throw new ArgumentException("SetParams with r, start, end, resolution is only valid for Arc");
            var dict = new Dictionary<string, object> { { "r", r }, { "start", start }, { "end", end } };
            if (resolution.HasValue) dict["resolution"] = resolution.Value;
            return dict;
        }

        // OScadSpecial Extensions
        public static Dictionary<string, object> TextParams(this OScadSpecial self, string text, double size = 10, string? font = null, string halign = "left", string valign = "baseline", double spacing = 1, string direction = "ltr", string? language = null, string? script = null, double? resolution = null)
        {
            if (self != OScadSpecial.Text)
                throw new ArgumentException("SetParams with text, size, font, etc. is only valid for Text");
            var dict = new Dictionary<string, object>
            {
                { "text", text },
                { "size", size },
                { "halign", halign },
                { "valign", valign },
                { "spacing", spacing },
                { "direction", direction }
            };
            if (font != null) dict["font"] = font;
            if (language != null) dict["language"] = language;
            if (script != null) dict["script"] = script;
            if (resolution.HasValue) dict["resolution"] = resolution.Value;
            return dict;
        }

        public static Dictionary<string, object> ImportParams(this OScadSpecial self, string file, int convexity = 1)
        {
            if (self != OScadSpecial.Import)
                throw new ArgumentException("SetParams with file, convexity is only valid for Import");
            return new Dictionary<string, object>
            {
                { "file", file },
                { "convexity", convexity }
            };
        }

        // OScadTransform Extensions
        public static Dictionary<string, object> TranslateParams(this OScadTransform self, double x, double y, double z, IScadObject[]? children = null)
        {
            if (self != OScadTransform.Translate)
                throw new ArgumentException("SetParams with x, y, z, children is only valid for Translate");
            var dict = new Dictionary<string, object> { { "x", x }, { "y", y }, { "z", z } };
            if (children != null) dict["children"] = children;
            return dict;
        }

        public static Dictionary<string, object> RotateParams(this OScadModify self, double ax, double ay, double az, IScadObject[]? children = null)
        {
            if (self != OScadModify.Rotate)
                throw new ArgumentException("SetParams with ax, ay, az, children is only valid for Rotate");
            var dict = new Dictionary<string, object> { { "ax", ax }, { "ay", ay }, { "az", az } };
            if (children != null) dict["children"] = children;
            return dict;
        }

        public static Dictionary<string, object> ScaleParams(this OScadModify self, double sx, double sy, double sz, IScadObject[]? children = null)
        {
            if (self != OScadModify.Scale)
                throw new ArgumentException("SetParams with sx, sy, sz, children is only valid for Scale");
            var dict = new Dictionary<string, object> { { "sx", sx }, { "sy", sy }, { "sz", sz } };
            if (children != null) dict["children"] = children;
            return dict;
        }

        public static Dictionary<string, object> MirrorParams(this OScadModify self, double mx, double my, double mz, IScadObject[]? children = null)
        {
            if (self != OScadModify.Mirror)
                throw new ArgumentException("SetParams with mx, my, mz, children is only valid for Mirror");
            var dict = new Dictionary<string, object> { { "mx", mx }, { "my", my }, { "mz", mz } };
            if (children != null) dict["children"] = children;
            return dict;
        }

        public static Dictionary<string, object> Params(this OScadTransform self, double rx, double ry, double rz, bool auto = false, IScadObject[]? children = null)
        {
            if (self != OScadTransform.Resize)
                throw new ArgumentException("SetParams with rx, ry, rz, auto, children is only valid for Resize");
            var dict = new Dictionary<string, object> { { "rx", rx }, { "ry", ry }, { "rz", rz }, { "auto", auto } };
            if (children != null) dict["children"] = children;
            return dict;
        }

        public static Dictionary<string, object> MultiMatrixParams(this OScadTransform self, List<List<double>> matrix, IScadObject[]? children = null)
        {
            if (self != OScadTransform.Multmatrix)
                throw new ArgumentException("SetParams with matrix, children is only valid for Multmatrix");
            var dict = new Dictionary<string, object> { { "matrix", matrix } };
            if (children != null) dict["children"] = children;
            return dict;
        }

        // OScadBooleanOperation Extensions
        public static Dictionary<string, object> Params(this OScadBooleanOperation self, IScadObject[] children)
        {
            return new Dictionary<string, object> { { "children", children } };
        }

        // OScadIteration Extensions
        public static Dictionary<string, object> ForParams(this OScadIteration self, string loopExpression, IScadObject[]? body = null)
        {
            if (self != OScadIteration.For)
                throw new ArgumentException("SetParams with loopExpression, body is only valid for For");
            var dict = new Dictionary<string, object> { { "loopExpression", loopExpression } };
            if (body != null) dict["body"] = body;
            return dict;
        }

        // ScrewPrimitives Extensions
        public static Dictionary<string, object> ScrewHoleParams(this ScrewPrimitives self, ScrewSize screwSize, double h, double? resolution = null)
        {
            if (self != ScrewPrimitives.ScrewHole && self != ScrewPrimitives.ScrewBoss)
                throw new ArgumentException("SetParams with screwSize, h, resolution is only valid for ScrewHole or ScrewBoss");
            var dict = new Dictionary<string, object> { { "screwSize", screwSize }, { "h", h } };
            if (resolution.HasValue) dict["resolution"] = resolution.Value;
            return dict;
        }

        public static Dictionary<string, object> ScrewHeadCountersinkParams(this ScrewPrimitives self, ScrewSize screwSize, double? resolution = null)
        {
            if (self != ScrewPrimitives.ScrewHeadCountersink)
                throw new ArgumentException("SetParams with screwSize, resolution is only valid for ScrewHeadCountersink");
            var dict = new Dictionary<string, object> { { "screwSize", screwSize } };
            if (resolution.HasValue) dict["resolution"] = resolution.Value;
            return dict;
        }
    }
}
