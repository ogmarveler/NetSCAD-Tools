using NetScad.Core.Interfaces;
using NetScad.Core.Models;
using System;
using System.Collections.Generic;


namespace NetScad.Core.Primitives
{
    public enum OScad3D { Cube, Cylinder, Sphere, Polyhedron, Surface, RoundedCube, RoundedCylinder, RoundedSphere, RoundedPolyhedron, RoundedSurface }
    public enum OScad2D { Square, Circle, Polygon }
    public enum OScad1D { Line, Arc }
    public enum OScadSpecial { Text, Import }
    public enum OScadDimension { D1, D2, D3, Special }
    public enum OScadTransform { Translate, Resize, Multmatrix }
    public enum OScadModify { Rotate, Scale, Mirror }
    public enum OScadBooleanOperation { Union, Difference, Intersection, Minkowski, Hull }
    public enum OScadIteration { For }

    public static partial class ScadExtensions
    {
        private static void ValidateKeys(Dictionary<string, object> parameters, string[] requiredKeys, string primitive)
        {
            foreach (var key in requiredKeys)
                if (!parameters.ContainsKey(key))
                    throw new ArgumentException($"{primitive} requires key: {key}");
        }

        public static IScadObject ToScadObject(this OScad3D self, Dictionary<string, object> parameters)
        {
            switch (self)
            {
                case OScad3D.Cube:
                    ValidateKeys(parameters, new[] { "size_x", "size_y", "size_z" }, "Cube");
                    return new Cube(parameters);

                case OScad3D.Cylinder:
                    ValidateKeys(parameters, new[] { "r", "h" }, "Cylinder");
                    return new Cylinder(parameters);

                case OScad3D.Sphere:
                    ValidateKeys(parameters, new[] { "r" }, "Sphere");
                    return new Sphere(parameters);

                case OScad3D.Polyhedron:
                    ValidateKeys(parameters, new[] { "points", "faces" }, "Polyhedron");
                    return new Polyhedron(
                        (List<List<double>>)parameters["points"],
                        (List<List<int>>)parameters["faces"],
                        parameters.ContainsKey("convexity") ? (int)parameters["convexity"] : 1);

                case OScad3D.Surface:
                    ValidateKeys(parameters, new[] { "file" }, "Surface");
                    return new Surface(
                        (string)parameters["file"],
                        parameters.ContainsKey("center") ? (bool)parameters["center"] : false,
                        parameters.ContainsKey("convexity") ? (int)parameters["convexity"] : 1);

                case OScad3D.RoundedCube:
                    ValidateKeys(parameters, new[] { "size_x", "size_y", "size_z", "round_r" }, "RoundedCube");
                    return new RoundedCube(parameters);

                case OScad3D.RoundedCylinder:
                    ValidateKeys(parameters, new[] { "r", "h", "round_r" }, "RoundedCylinder");
                    return new RoundedCylinder(parameters);

                case OScad3D.RoundedSphere:
                    ValidateKeys(parameters, new[] { "r", "round_r" }, "RoundedSphere");
                    return new RoundedSphere(parameters);

                case OScad3D.RoundedPolyhedron:
                    ValidateKeys(parameters, new[] { "points", "faces", "round_r" }, "RoundedPolyhedron");
                    return new RoundedPolyhedron(
                        (List<List<double>>)parameters["points"],
                        (List<List<int>>)parameters["faces"],
                        (double)parameters["round_r"],
                        parameters.ContainsKey("round_h") ? (double)parameters["round_h"] : 0.001,
                        parameters.ContainsKey("resolution") ? (double)parameters["resolution"] : 200);

                case OScad3D.RoundedSurface:
                    ValidateKeys(parameters, new[] { "file", "round_r" }, "RoundedSurface");
                    return new RoundedSurface(
                        (string)parameters["file"],
                        (double)parameters["round_r"],
                        parameters.ContainsKey("center") ? (bool)parameters["center"] : false,
                        parameters.ContainsKey("round_h") ? (double)parameters["round_h"] : 0.001,
                        parameters.ContainsKey("resolution") ? (double)parameters["resolution"] : 200);

                default:
                    throw new ArgumentException("Unknown OScad3D type");
            }
        }

        public static IScadObject ToScadObject(this OScad2D self, Dictionary<string, object> parameters)
        {
            switch (self)
            {
                case OScad2D.Square:
                    ValidateKeys(parameters, new[] { "size_x", "size_y" }, "Square");
                    return new Square(parameters);

                case OScad2D.Circle:
                    ValidateKeys(parameters, new[] { "r" }, "Circle");
                    return new Circle(parameters);

                case OScad2D.Polygon:
                    ValidateKeys(parameters, new[] { "points" }, "Polygon");
                    return new Polygon(
                        (List<List<double>>)parameters["points"],
                        parameters.ContainsKey("paths") ? (List<List<int>>?)parameters["paths"] : null,
                        parameters.ContainsKey("convexity") ? (int)parameters["convexity"] : 1);

                default:
                    throw new ArgumentException("Unknown OScad2D type");
            }
        }

        public static IScadObject ToScadObject(this OScad1D self, Dictionary<string, object> parameters)
        {
            switch (self)
            {
                case OScad1D.Line:
                    ValidateKeys(parameters, new[] { "x1", "y1", "x2", "y2" }, "Line");
                    return new Line(parameters);

                case OScad1D.Arc:
                    ValidateKeys(parameters, new[] { "r", "start", "end" }, "Arc");
                    return new Arc(parameters);

                default:
                    throw new ArgumentException("Unknown OScad1D type");
            }
        }

        public static IScadObject ToScadObject(this OScadSpecial self, Dictionary<string, object> parameters)
        {
            switch (self)
            {
                case OScadSpecial.Text:
                    ValidateKeys(parameters, new[] { "text" }, "Text");
                    return new Text(
                        (string)parameters["text"],
                        parameters.ContainsKey("size") ? (double)parameters["size"] : 10,
                        parameters.ContainsKey("font") ? (string?)parameters["font"] : null,
                        parameters.ContainsKey("halign") ? (string)parameters["halign"] : "left",
                        parameters.ContainsKey("valign") ? (string)parameters["valign"] : "baseline",
                        parameters.ContainsKey("spacing") ? (double)parameters["spacing"] : 1,
                        parameters.ContainsKey("direction") ? (string)parameters["direction"] : "ltr",
                        parameters.ContainsKey("language") ? (string?)parameters["language"] : null,
                        parameters.ContainsKey("script") ? (string?)parameters["script"] : null,
                        parameters.ContainsKey("resolution") ? (double)parameters["resolution"] : 200);

                case OScadSpecial.Import:
                    ValidateKeys(parameters, new[] { "file" }, "Import");
                    return new Import(
                        (string)parameters["file"],
                        parameters.ContainsKey("convexity") ? (int)parameters["convexity"] : 1);

                default:
                    throw new ArgumentException("Unknown OScadSpecial type");
            }
        }

        public static IScadObject ToScadObject(this OScadTransform self, Dictionary<string, object> parameters)
        {
            switch (self)
            {
                case OScadTransform.Translate:
                    ValidateKeys(parameters, new[] { "x", "y", "z" }, "Translate");
                    return new Translate(parameters);

                case OScadTransform.Resize:
                    ValidateKeys(parameters, new[] { "rx", "ry", "rz" }, "Resize");
                    return new Resize(parameters);

                case OScadTransform.Multmatrix:
                    ValidateKeys(parameters, new[] { "matrix" }, "Multmatrix");
                    return new Multmatrix(
                        (List<List<double>>)parameters["matrix"],
                        parameters.ContainsKey("children") ? (IScadObject[])parameters["children"] : Array.Empty<IScadObject>());

                default:
                    throw new ArgumentException("Unknown OScadTransform type");
            }
        }

        public static IScadObject ToScadObject(this OScadModify self, Dictionary<string, object> parameters)
        {
            switch (self)
            {
                case OScadModify.Rotate:
                    ValidateKeys(parameters, new[] { "ax", "ay", "az" }, "Rotate");
                    return new Rotate(parameters);

                case OScadModify.Scale:
                    ValidateKeys(parameters, new[] { "sx", "sy", "sz" }, "Scale");
                    return new Scale(parameters);

                case OScadModify.Mirror:
                    ValidateKeys(parameters, new[] { "mx", "my", "mz" }, "Mirror");
                    return new Mirror(parameters);

                default:
                    throw new ArgumentException("Unknown OScadTransform type");
            }
        }

        public static IScadObject ToScadObject(this OScadBooleanOperation self, Dictionary<string, object> parameters)
        {
            ValidateKeys(parameters, new[] { "children" }, self.ToString());
            var children = (IScadObject[])parameters["children"];
            return self switch
            {
                OScadBooleanOperation.Union => new Union(children),
                OScadBooleanOperation.Difference => new Difference(children),
                OScadBooleanOperation.Intersection => new Intersection(children),
                OScadBooleanOperation.Minkowski => new Minkowski(children),
                OScadBooleanOperation.Hull => new Hull(children),
                _ => throw new ArgumentException("Unknown OScadBooleanOperation type")
            };
        }

        public static IScadObject ToScadObject(this OScadIteration self, Dictionary<string, object> parameters)
        {
            ValidateKeys(parameters, new[] { "loopExpression" }, "For");
            return self switch
            {
                OScadIteration.For => new ForLoop(
                    (string)parameters["loopExpression"],
                    parameters.ContainsKey("body") ? (IScadObject[])parameters["body"] : Array.Empty<IScadObject>()),
                _ => throw new ArgumentException("Unknown OScadIteration type")
            };
        }
    }
}