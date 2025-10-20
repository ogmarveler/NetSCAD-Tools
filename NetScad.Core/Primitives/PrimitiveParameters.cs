using NetScad.Core.Interfaces;
using NetScad.Core.Models;

namespace NetScad.Core.Primitives
{
    public class PrimitiveParameters
    {
        public Dictionary<object, List<ParameterInfo>> ParameterMap { get; }

        public PrimitiveParameters()
        {
            ParameterMap = new Dictionary<object, List<ParameterInfo>>
            {
                // OScad3D
                {
                    OScad3D.Cube, new List<ParameterInfo>
                    {
                        new("size_x", typeof(double)),
                        new("size_y", typeof(double)),
                        new("size_z", typeof(double))
                    }
                },
                {
                    OScad3D.Cylinder, new List<ParameterInfo>
                    {
                        new("r", typeof(double)),
                        new("h", typeof(double)),
                        new("r1", typeof(double), null),
                        new("r2", typeof(double), null),
                        new("resolution", typeof(double), 100.0)
                    }
                },
                {
                    OScad3D.Sphere, new List<ParameterInfo>
                    {
                        new("r", typeof(double)),
                        new("resolution", typeof(double), 100.0)
                    }
                },
                {
                    OScad3D.Polyhedron, new List<ParameterInfo>
                    {
                        new("points", typeof(List<List<double>>)),
                        new("faces", typeof(List<List<int>>)),
                        new("convexity", typeof(int), 1)
                    }
                },
                {
                    OScad3D.Surface, new List<ParameterInfo>
                    {
                        new("file", typeof(string)),
                        new("center", typeof(bool), false),
                        new("convexity", typeof(int), 1)
                    }
                },
                {
                    OScad3D.RoundedCube, new List<ParameterInfo>
                    {
                        new("size_x", typeof(double)),
                        new("size_y", typeof(double)),
                        new("size_z", typeof(double)),
                        new("round_r", typeof(double)),
                        new("round_h", typeof(double), 0.001),
                        new("resolution", typeof(double), 200.0)
                    }
                },
                {
                    OScad3D.RoundedCylinder, new List<ParameterInfo>
                    {
                        new("r", typeof(double)),
                        new("h", typeof(double)),
                        new("round_r", typeof(double)),
                        new("round_h", typeof(double), 0.001),
                        new("r1", typeof(double), null),
                        new("r2", typeof(double), null),
                        new("resolution", typeof(double), 200.0)
                    }
                },
                {
                    OScad3D.RoundedSphere, new List<ParameterInfo>
                    {
                        new("r", typeof(double)),
                        new("round_r", typeof(double)),
                        new("round_h", typeof(double), 0.001),
                        new("resolution", typeof(double), 200.0)
                    }
                },
                {
                    OScad3D.RoundedPolyhedron, new List<ParameterInfo>
                    {
                        new("points", typeof(List<List<double>>)),
                        new("faces", typeof(List<List<int>>)),
                        new("round_r", typeof(double)),
                        new("round_h", typeof(double), 0.001),
                        new("resolution", typeof(double), 200.0)
                    }
                },
                {
                    OScad3D.RoundedSurface, new List<ParameterInfo>
                    {
                        new("file", typeof(string)),
                        new("round_r", typeof(double)),
                        new("center", typeof(bool), false),
                        new("round_h", typeof(double), 0.001),
                        new("resolution", typeof(double), 200.0)
                    }
                },

                // OScad2D
                {
                    OScad2D.Square, new List<ParameterInfo>
                    {
                        new("size_x", typeof(double)),
                        new("size_y", typeof(double)),
                        new("center", typeof(bool), false)
                    }
                },
                {
                    OScad2D.Circle, new List<ParameterInfo>
                    {
                        new("r", typeof(double)),
                        new("resolution", typeof(double), 100.0)
                    }
                },
                {
                    OScad2D.Polygon, new List<ParameterInfo>
                    {
                        new("points", typeof(List<List<double>>)),
                        new("paths", typeof(List<List<int>>), null),
                        new("convexity", typeof(int), 1)
                    }
                },

                // OScad1D
                {
                    OScad1D.Line, new List<ParameterInfo>
                    {
                        new("x1", typeof(double)),
                        new("y1", typeof(double)),
                        new("x2", typeof(double)),
                        new("y2", typeof(double))
                    }
                },
                {
                    OScad1D.Arc, new List<ParameterInfo>
                    {
                        new("r", typeof(double)),
                        new("start", typeof(double)),
                        new("end", typeof(double)),
                        new("resolution", typeof(double), 100.0)
                    }
                },

                // OScadSpecial
                {
                    OScadSpecial.Text, new List<ParameterInfo>
                    {
                        new("text", typeof(string)),
                        new("size", typeof(double), 10.0),
                        new("font", typeof(string), null),
                        new("halign", typeof(string), "left"),
                        new("valign", typeof(string), "baseline"),
                        new("spacing", typeof(double), 1.0),
                        new("direction", typeof(string), "ltr"),
                        new("language", typeof(string), null),
                        new("script", typeof(string), null),
                        new("resolution", typeof(double), 200.0)
                    }
                },
                {
                    OScadSpecial.Import, new List<ParameterInfo>
                    {
                        new("file", typeof(string)),
                        new("convexity", typeof(int), 1)
                    }
                },

                // OScadTransform
                {
                    OScadTransform.Translate, new List<ParameterInfo>
                    {
                        new("x", typeof(double)),
                        new("y", typeof(double)),
                        new("z", typeof(double)),
                        new("children", typeof(IScadObject[]), null)
                    }
                },
                {
                    OScadModify.Rotate, new List<ParameterInfo>
                    {
                        new("ax", typeof(double)),
                        new("ay", typeof(double)),
                        new("az", typeof(double)),
                        new("children", typeof(IScadObject[]), null)
                    }
                },
                {
                    OScadModify.Scale, new List<ParameterInfo>
                    {
                        new("sx", typeof(double)),
                        new("sy", typeof(double)),
                        new("sz", typeof(double)),
                        new("children", typeof(IScadObject[]), null)
                    }
                },
                {
                    OScadModify.Mirror, new List<ParameterInfo>
                    {
                        new("mx", typeof(double)),
                        new("my", typeof(double)),
                        new("mz", typeof(double)),
                        new("children", typeof(IScadObject[]), null)
                    }
                },
                {
                    OScadTransform.Resize, new List<ParameterInfo>
                    {
                        new("rx", typeof(double)),
                        new("ry", typeof(double)),
                        new("rz", typeof(double)),
                        new("auto", typeof(bool), false),
                        new("children", typeof(IScadObject[]), null)
                    }
                },
                {
                    OScadTransform.Multmatrix, new List<ParameterInfo>
                    {
                        new("matrix", typeof(List<List<double>>)),
                        new("children", typeof(IScadObject[]), null)
                    }
                },

                // OScadBooleanOperation
                {
                    OScadBooleanOperation.Union, new List<ParameterInfo>
                    {
                        new("children", typeof(IScadObject[]))
                    }
                },
                {
                    OScadBooleanOperation.Difference, new List<ParameterInfo>
                    {
                        new("children", typeof(IScadObject[]))
                    }
                },
                {
                    OScadBooleanOperation.Intersection, new List<ParameterInfo>
                    {
                        new("children", typeof(IScadObject[]))
                    }
                },
                {
                    OScadBooleanOperation.Minkowski, new List<ParameterInfo>
                    {
                        new("children", typeof(IScadObject[]))
                    }
                },
                {
                    OScadBooleanOperation.Hull, new List<ParameterInfo>
                    {
                        new("children", typeof(IScadObject[]))
                    }
                },

                // OScadIteration
                {
                    OScadIteration.For, new List<ParameterInfo>
                    {
                        new("loopExpression", typeof(string)),
                        new("body", typeof(IScadObject[]), null)
                    }
                },

                // ScrewPrimitives
                {
                    ScrewPrimitives.ScrewHole, new List<ParameterInfo>
                    {
                        new("screwSize", typeof(ScrewSize)),
                        new("h", typeof(double)),
                        new("resolution", typeof(double), 100.0)
                    }
                },
                {
                    ScrewPrimitives.ScrewBoss, new List<ParameterInfo>
                    {
                        new("screwSize", typeof(ScrewSize)),
                        new("h", typeof(double)),
                        new("resolution", typeof(double), 100.0)
                    }
                },
                {
                    ScrewPrimitives.ScrewHeadCountersink, new List<ParameterInfo>
                    {
                        new("screwSize", typeof(ScrewSize)),
                        new("resolution", typeof(double), 100.0)
                    }
                }
            };
        }

        public Dictionary<string, object> BuildParameters(object primitive, Dictionary<string, object> userInputs)
        {
            if (!ParameterMap.TryGetValue(primitive, out var paramsInfo))
                throw new ArgumentException($"Unknown primitive: {primitive}");

            var result = new Dictionary<string, object>();

            foreach (var param in paramsInfo)
            {
                if (userInputs.TryGetValue(param.Name, out var value))
                {
                    try
                    {
                        result[param.Name] = Convert.ChangeType(value, param.Type);
                    }
                    catch
                    {
                        throw new ArgumentException($"Invalid type for {param.Name}: expected {param.Type.Name}");
                    }
                }
                else if (param.DefaultValue != null)
                {
                    result[param.Name] = param.DefaultValue;
                }
                else
                {
                    throw new KeyNotFoundException($"Missing required parameter: {param.Name}");
                }
            }

            return result;
        }
    }
}
