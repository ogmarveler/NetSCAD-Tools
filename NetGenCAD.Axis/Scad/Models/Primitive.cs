namespace NetGenCAD.Axis.SCAD.Models
{
    public class Primitive
    {
        public enum OpenScad3D
        {
            Cube,       // Rectangular prism
            Cylinder,   // Cylinders, cones
            Sphere,     // Spherical solid
            Polyhedron, // Custom 3D shape from points and faces
            Surface     // 3D heightmap from 2D data
        }

        public enum OpenScad2D
        {
            Square,    // Rectangular 2D shape
            Circle,    // Circular 2D shape
            Polygon    // Custom 2D shape from points and paths
        }

        public enum OpenScad1D
        {
            Line,      // Straight line between two points
            Arc        // Curved line segment
        }

        public enum OpenScadSpecial
        {
            Text,      // 2D or 3D text
            Import     // Import external 2D/3D model files
        }

        public enum Dimension
        {
            D1,
            D2,
            D3,
            Special
        }

        public enum Transform
        {
            Translate,
            Rotate,
            Scale,
            Mirror,
            Resize,
            Multmatrix
        }

        public enum BooleanOperation
        {
            Union,
            Difference,
            Intersection,
            Minkowski,
            Hull
        }

        public enum Iteration
        {
            For
        }
    }
}