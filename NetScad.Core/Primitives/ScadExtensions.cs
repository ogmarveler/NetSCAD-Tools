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
                    ValidateKeys(parameters, ["size_x", "size_y", "size_z"], "Cube");
                    return new Cube(parameters);

                case OScad3D.Cylinder:
                    ValidateKeys(parameters, ["r", "h"], "Cylinder");
                    return new Cylinder(parameters);

                case OScad3D.Sphere:
                    ValidateKeys(parameters, ["r"], "Sphere");
                    return new Sphere(parameters);

                case OScad3D.Polyhedron:
                    ValidateKeys(parameters, ["points", "faces"], "Polyhedron");
                    return new Polyhedron(
                        (List<List<double>>)parameters["points"],
                        (List<List<int>>)parameters["faces"],
                        parameters.TryGetValue("convexity", out object? value) ? (int)value : 1);

                case OScad3D.Surface:
                    ValidateKeys(parameters, ["file"], "Surface");
                    return new Surface(
                        (string)parameters["file"],
                        parameters.ContainsKey("center") && (bool)parameters["center"],
                        parameters.TryGetValue("convexity", out object? value1) ? (int)value1 : 1);

                case OScad3D.RoundedCube:
                    ValidateKeys(parameters, ["size_x", "size_y", "size_z", "round_r"], "RoundedCube");
                    return new RoundedCube(parameters);

                case OScad3D.RoundedCylinder:
                    ValidateKeys(parameters, ["r", "h", "round_r"], "RoundedCylinder");
                    return new RoundedCylinder(parameters);

                case OScad3D.RoundedSphere:
                    ValidateKeys(parameters, ["r", "round_r"], "RoundedSphere");
                    return new RoundedSphere(parameters);

                case OScad3D.RoundedPolyhedron:
                    ValidateKeys(parameters, ["points", "faces", "round_r"], "RoundedPolyhedron");
                    return new RoundedPolyhedron(
                        (List<List<double>>)parameters["points"],
                        (List<List<int>>)parameters["faces"],
                        (double)parameters["round_r"],
                        parameters.TryGetValue("round_h", out object? value2) ? (double)value2 : 0.001,
                        parameters.TryGetValue("resolution", out object? value3) ? (double)value3 : 200);

                case OScad3D.RoundedSurface:
                    ValidateKeys(parameters, ["file", "round_r"], "RoundedSurface");
                    return new RoundedSurface(
                        (string)parameters["file"],
                        (double)parameters["round_r"],
                        parameters.ContainsKey("center") && (bool)parameters["center"],
                        parameters.TryGetValue("round_h", out object? value4) ? (double)value4 : 0.001,
                        parameters.TryGetValue("resolution", out object? value5) ? (double)value5 : 200);

                default:
                    throw new ArgumentException("Unknown OScad3D type");
            }
        }

        public static IScadObject ToScadObject(this OScad2D self, Dictionary<string, object> parameters)
        {
            switch (self)
            {
                case OScad2D.Square:
                    ValidateKeys(parameters, ["size_x", "size_y"], "Square");
                    return new Square(parameters);

                case OScad2D.Circle:
                    ValidateKeys(parameters, ["r"], "Circle");
                    return new Circle(parameters);

                case OScad2D.Polygon:
                    ValidateKeys(parameters, ["points"], "Polygon");
                    return new Polygon(
                        (List<List<double>>)parameters["points"],
                        parameters.TryGetValue("paths", out object? value) ? (List<List<int>>?)value : null,
                        parameters.TryGetValue("convexity", out object? value1) ? (int)value1 : 1);

                default:
                    throw new ArgumentException("Unknown OScad2D type");
            }
        }

        public static IScadObject ToScadObject(this OScad1D self, Dictionary<string, object> parameters)
        {
            switch (self)
            {
                case OScad1D.Line:
                    ValidateKeys(parameters, ["x1", "y1", "x2", "y2"], "Line");
                    return new Line(parameters);

                case OScad1D.Arc:
                    ValidateKeys(parameters, ["r", "start", "end"], "Arc");
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
                    ValidateKeys(parameters, ["text"], "Text");
                    return new Text(
                        (string)parameters["text"],
                        parameters.TryGetValue("size", out object? value) ? (double)value : 10,
                        parameters.TryGetValue("font", out object? value1) ? (string?)value1 : null,
                        parameters.TryGetValue("halign", out object? value2) ? (string)value2 : "left",
                        parameters.TryGetValue("valign", out object? value3) ? (string)value3 : "baseline",
                        parameters.TryGetValue("spacing", out object? value4) ? (double)value4 : 1,
                        parameters.TryGetValue("direction", out object? value5) ? (string)value5 : "ltr",
                        parameters.TryGetValue("language", out object? value6) ? (string?)value6 : null,
                        parameters.TryGetValue("script", out object? value7) ? (string?)value7 : null,
                        parameters.TryGetValue("resolution", out object? value8) ? (double)value8 : 200);

                case OScadSpecial.Import:
                    ValidateKeys(parameters, ["file"], "Import");
                    return new Import(
                        (string)parameters["file"],
                        parameters.TryGetValue("convexity", out object? value9) ? (int)value9 : 1);

                default:
                    throw new ArgumentException("Unknown OScadSpecial type");
            }
        }

        public static IScadObject ToScadObject(this OScadTransform self, Dictionary<string, object> parameters)
        {
            switch (self)
            {
                case OScadTransform.Translate:
                    ValidateKeys(parameters, ["x", "y", "z"], "Translate");
                    return new Translate(parameters);

                case OScadTransform.Resize:
                    ValidateKeys(parameters, ["rx", "ry", "rz"], "Resize");
                    return new Resize(parameters);

                case OScadTransform.Multmatrix:
                    ValidateKeys(parameters, ["matrix"], "Multmatrix");
                    return new Multmatrix(
                        (List<List<double>>)parameters["matrix"],
                        parameters.TryGetValue("children", out object? value) ? (IScadObject[])value : []);

                default:
                    throw new ArgumentException("Unknown OScadTransform type");
            }
        }

        public static IScadObject ToScadObject(this OScadModify self, Dictionary<string, object> parameters)
        {
            switch (self)
            {
                case OScadModify.Rotate:
                    ValidateKeys(parameters, ["ax", "ay", "az"], "Rotate");
                    return new Rotate(parameters);

                case OScadModify.Scale:
                    ValidateKeys(parameters, ["sx", "sy", "sz"], "Scale");
                    return new Scale(parameters);

                case OScadModify.Mirror:
                    ValidateKeys(parameters, ["mx", "my", "mz"], "Mirror");
                    return new Mirror(parameters);

                default:
                    throw new ArgumentException("Unknown OScadTransform type");
            }
        }

        public static IScadObject ToScadObject(this OScadBooleanOperation self, Dictionary<string, object> parameters)
        {
            ValidateKeys(parameters, ["children"], self.ToString());
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
            ValidateKeys(parameters, ["loopExpression"], "For");
            return self switch
            {
                OScadIteration.For => new ForLoop(
                    (string)parameters["loopExpression"],
                    parameters.TryGetValue("body", out object? value) ? (IScadObject[])value : []),
                _ => throw new ArgumentException("Unknown OScadIteration type")
            };
        }
    }
}