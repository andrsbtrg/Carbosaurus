using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carbosaurus.Utils
{
    public static class AssignMaterial
    {
        public static List<Surface> SelectSurfaces()
        {
            var gc = new GetObject();
            gc.GeometryFilter = Rhino.DocObjects.ObjectType.Surface;
            gc.EnablePreSelect(true, true);
            gc.GetMultiple(1, 0);
            if (gc.CommandResult() != Result.Success)
            {
                return null;
            }

            var surfaces = new List<Surface>();
            foreach (var surfaceRef in gc.Objects())
            {
                var surface = surfaceRef.Surface();
                if (surface == null)
                    continue;
                if (surface.IsPlanar() == false)
                    continue;
                surfaces.Add(surface);
            }
            return surfaces;
        }

        public static List<Rhino.DocObjects.RhinoObject> SelectSurfaceRef(out List<string> areas)
        {
            areas = new List<string>();
            var gc = new GetObject();
            gc.GeometryFilter = Rhino.DocObjects.ObjectType.Surface;
            gc.EnablePreSelect(true, true);
            gc.GetMultiple(1, 0);
            if (gc.CommandResult() != Result.Success)
            {
                return null;
            }

            var surfaces = new List<Rhino.DocObjects.RhinoObject>();
            foreach (var surfaceRef in gc.Objects())
            {
                var face = surfaceRef.Face();
                if (face != null)
                {
                    areas.Add(AreaMassProperties.Compute(face).Area.ToString());
                    var surfaceObj = surfaceRef.Object();
                    var surface = surfaceRef.Surface();
                    if (surface == null)
                        continue;
                    // Turned off as we want to use also nonplanar surfaces
                    //if (surface.IsPlanar() == false)
                    //    continue;
                    surfaces.Add(surfaceObj);
                }
                else
                {
                    var surfaceObj = surfaceRef.Object();
                    var surface = surfaceRef.Surface();
                    if (surface == null)
                        continue;
                    if (surface.IsPlanar() == false)
                        continue;
                    surfaces.Add(surfaceObj);
                    var Brep = surfaceRef.Brep();
                    areas.Add(Brep.GetArea().ToString());
                }

            }
            return surfaces;
        }

        internal static bool WriteCustomDict(List<Rhino.DocObjects.RhinoObject> surfaces, List<string> areas, string level, string attribute)
        {
            var doc = Rhino.RhinoDoc.ActiveDoc;
            if (doc == null)
                return false;
            if (surfaces == null)
                return false;
            int i = 0;
            foreach (var surface in surfaces)
            {

                var surfaceObj = doc.Objects.FindId(surface.Id);
                if (surfaceObj == null)
                    continue;

                surfaceObj.Attributes.SetUserString("Level", level);
                surfaceObj.Attributes.SetUserString("Attribute", attribute);
                surfaceObj.Attributes.SetUserString("unit", areas[i]);

                surfaceObj.CommitChanges();
                i++;
            }
            return true;
        }

        internal static void RemoveMaterials(List<Rhino.DocObjects.RhinoObject> surfaces)
        {
            foreach(var surface in surfaces)
            {
                surface.Attributes.DeleteAllUserStrings();
                surface.CommitChanges();
            }
        }
    }
}
