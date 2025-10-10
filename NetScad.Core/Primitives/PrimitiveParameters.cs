using NetScad.Core.Interfaces;
using NetScad.Core.Models;
using System;
using System.Collections.Generic;

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
                        new ParameterInfo("size_x", typeof(double)),
                        new ParameterInfo("size_y", typeof(double)),
                        new ParameterInfo("size_z", typeof(double))
                    }
                },
                {
                    OScad3D.Cylinder, new List<ParameterInfo>
                    {
                        new ParameterInfo("r", typeof(double)),
                        new ParameterInfo("h", typeof(double)),
                        new ParameterInfo("r1", typeof(double), null),
                        new ParameterInfo("r2", typeof(double), null),
                        new ParameterInfo("resolution", typeof(double), 100.0)
                    }
                },
                {
                    OScad3D.Sphere, new List<ParameterInfo>
                    {
                        new ParameterInfo("r", typeof(double)),
                        new ParameterInfo("resolution", typeof(double), 100.0)
                    }
                },
                {
                    OScad3D.Polyhedron, new List<ParameterInfo>
                    {
                        new ParameterInfo("points", typeof(List<List<double>>)),
                        new ParameterInfo("faces", typeof(List<List<int>>)),
                        new ParameterInfo("convexity", typeof(int), 1)
                    }
                },
                {
                    OScad3D.Surface, new List<ParameterInfo>
                    {
                        new ParameterInfo("file", typeof(string)),
                        new ParameterInfo("center", typeof(bool), false),
                        new ParameterInfo("convexity", typeof(int), 1)
                    }
                },
                {
                    OScad3D.RoundedCube, new List<ParameterInfo>
                    {
                        new ParameterInfo("size_x", typeof(double)),
                        new ParameterInfo("size_y", typeof(double)),
                        new ParameterInfo("size_z", typeof(double)),
                        new ParameterInfo("round_r", typeof(double)),
                        new ParameterInfo("round_h", typeof(double), 0.001),
                        new ParameterInfo("resolution", typeof(double), 200.0)
                    }
                },
                {
                    OScad3D.RoundedCylinder, new List<ParameterInfo>
                    {
                        new ParameterInfo("r", typeof(double)),
                        new ParameterInfo("h", typeof(double)),
                        new ParameterInfo("round_r", typeof(double)),
                        new ParameterInfo("round_h", typeof(double), 0.001),
                        new ParameterInfo("r1", typeof(double), null),
                        new ParameterInfo("r2", typeof(double), null),
                        new ParameterInfo("resolution", typeof(double), 200.0)
                    }
                },
                {
                    OScad3D.RoundedSphere, new List<ParameterInfo>
                    {
                        new ParameterInfo("r", typeof(double)),
                        new ParameterInfo("round_r", typeof(double)),
                        new ParameterInfo("round_h", typeof(double), 0.001),
                        new ParameterInfo("resolution", typeof(double), 200.0)
                    }
                },
                {
                    OScad3D.RoundedPolyhedron, new List<ParameterInfo>
                    {
                        new ParameterInfo("points", typeof(List<List<double>>)),
                        new ParameterInfo("faces", typeof(List<List<int>>)),
                        new ParameterInfo("round_r", typeof(double)),
                        new ParameterInfo("round_h", typeof(double), 0.001),
                        new ParameterInfo("resolution", typeof(double), 200.0)
                    }
                },
                {
                    OScad3D.RoundedSurface, new List<ParameterInfo>
                    {
                        new ParameterInfo("file", typeof(string)),
                        new ParameterInfo("round_r", typeof(double)),
                        new ParameterInfo("center", typeof(bool), false),
                        new ParameterInfo("round_h", typeof(double), 0.001),
                        new ParameterInfo("resolution", typeof(double), 200.0)
                    }
                },

                // OScad2D
                {
                    OScad2D.Square, new List<ParameterInfo>
                    {
                        new ParameterInfo("size_x", typeof(double)),
                        new ParameterInfo("size_y", typeof(double)),
                        new ParameterInfo("center", typeof(bool), false)
                    }
                },
                {
                    OScad2D.Circle, new List<ParameterInfo>
                    {
                        new ParameterInfo("r", typeof(double)),
                        new ParameterInfo("resolution", typeof(double), 100.0)
                    }
                },
                {
                    OScad2D.Polygon, new List<ParameterInfo>
                    {
                        new ParameterInfo("points", typeof(List<List<double>>)),
                        new ParameterInfo("paths", typeof(List<List<int>>), null),
                        new ParameterInfo("convexity", typeof(int), 1)
                    }
                },

                // OScad1D
                {
                    OScad1D.Line, new List<ParameterInfo>
                    {
                        new ParameterInfo("x1", typeof(double)),
                        new ParameterInfo("y1", typeof(double)),
                        new ParameterInfo("x2", typeof(double)),
                        new ParameterInfo("y2", typeof(double))
                    }
                },
                {
                    OScad1D.Arc, new List<ParameterInfo>
                    {
                        new ParameterInfo("r", typeof(double)),
                        new ParameterInfo("start", typeof(double)),
                        new ParameterInfo("end", typeof(double)),
                        new ParameterInfo("resolution", typeof(double), 100.0)
                    }
                },

                // OScadSpecial
                {
                    OScadSpecial.Text, new List<ParameterInfo>
                    {
                        new ParameterInfo("text", typeof(string)),
                        new ParameterInfo("size", typeof(double), 10.0),
                        new ParameterInfo("font", typeof(string), null),
                        new ParameterInfo("halign", typeof(string), "left"),
                        new ParameterInfo("valign", typeof(string), "baseline"),
                        new ParameterInfo("spacing", typeof(double), 1.0),
                        new ParameterInfo("direction", typeof(string), "ltr"),
                        new ParameterInfo("language", typeof(string), null),
                        new ParameterInfo("script", typeof(string), null),
                        new ParameterInfo("resolution", typeof(double), 200.0)
                    }
                },
                {
                    OScadSpecial.Import, new List<ParameterInfo>
                    {
                        new ParameterInfo("file", typeof(string)),
                        new ParameterInfo("convexity", typeof(int), 1)
                    }
                },

                // OScadTransform
                {
                    OScadTransform.Translate, new List<ParameterInfo>
                    {
                        new ParameterInfo("x", typeof(double)),
                        new ParameterInfo("y", typeof(double)),
                        new ParameterInfo("z", typeof(double)),
                        new ParameterInfo("children", typeof(IScadObject[]), null)
                    }
                },
                {
                    OScadModify.Rotate, new List<ParameterInfo>
                    {
                        new ParameterInfo("ax", typeof(double)),
                        new ParameterInfo("ay", typeof(double)),
                        new ParameterInfo("az", typeof(double)),
                        new ParameterInfo("children", typeof(IScadObject[]), null)
                    }
                },
                {
                    OScadModify.Scale, new List<ParameterInfo>
                    {
                        new ParameterInfo("sx", typeof(double)),
                        new ParameterInfo("sy", typeof(double)),
                        new ParameterInfo("sz", typeof(double)),
                        new ParameterInfo("children", typeof(IScadObject[]), null)
                    }
                },
                {
                    OScadModify.Mirror, new List<ParameterInfo>
                    {
                        new ParameterInfo("mx", typeof(double)),
                        new ParameterInfo("my", typeof(double)),
                        new ParameterInfo("mz", typeof(double)),
                        new ParameterInfo("children", typeof(IScadObject[]), null)
                    }
                },
                {
                    OScadTransform.Resize, new List<ParameterInfo>
                    {
                        new ParameterInfo("rx", typeof(double)),
                        new ParameterInfo("ry", typeof(double)),
                        new ParameterInfo("rz", typeof(double)),
                        new ParameterInfo("auto", typeof(bool), false),
                        new ParameterInfo("children", typeof(IScadObject[]), null)
                    }
                },
                {
                    OScadTransform.Multmatrix, new List<ParameterInfo>
                    {
                        new ParameterInfo("matrix", typeof(List<List<double>>)),
                        new ParameterInfo("children", typeof(IScadObject[]), null)
                    }
                },

                // OScadBooleanOperation
                {
                    OScadBooleanOperation.Union, new List<ParameterInfo>
                    {
                        new ParameterInfo("children", typeof(IScadObject[]))
                    }
                },
                {
                    OScadBooleanOperation.Difference, new List<ParameterInfo>
                    {
                        new ParameterInfo("children", typeof(IScadObject[]))
                    }
                },
                {
                    OScadBooleanOperation.Intersection, new List<ParameterInfo>
                    {
                        new ParameterInfo("children", typeof(IScadObject[]))
                    }
                },
                {
                    OScadBooleanOperation.Minkowski, new List<ParameterInfo>
                    {
                        new ParameterInfo("children", typeof(IScadObject[]))
                    }
                },
                {
                    OScadBooleanOperation.Hull, new List<ParameterInfo>
                    {
                        new ParameterInfo("children", typeof(IScadObject[]))
                    }
                },

                // OScadIteration
                {
                    OScadIteration.For, new List<ParameterInfo>
                    {
                        new ParameterInfo("loopExpression", typeof(string)),
                        new ParameterInfo("body", typeof(IScadObject[]), null)
                    }
                },

                // ScrewPrimitives
                {
                    ScrewPrimitives.ScrewHole, new List<ParameterInfo>
                    {
                        new ParameterInfo("screwSize", typeof(ScrewSize)),
                        new ParameterInfo("h", typeof(double)),
                        new ParameterInfo("resolution", typeof(double), 100.0)
                    }
                },
                {
                    ScrewPrimitives.ScrewBoss, new List<ParameterInfo>
                    {
                        new ParameterInfo("screwSize", typeof(ScrewSize)),
                        new ParameterInfo("h", typeof(double)),
                        new ParameterInfo("resolution", typeof(double), 100.0)
                    }
                },
                {
                    ScrewPrimitives.ScrewHeadCountersink, new List<ParameterInfo>
                    {
                        new ParameterInfo("screwSize", typeof(ScrewSize)),
                        new ParameterInfo("resolution", typeof(double), 100.0)
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
