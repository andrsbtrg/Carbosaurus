using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carbosaurus.Utils
{
    public static class Inventory
    {
        public static List<LCA.BillOfMaterials> BOM { get; set; }

        public static List <LCA.BillOfMaterials> GetBOM()
        {
            var bom = new List<LCA.BillOfMaterials>();
            var doc = Rhino.RhinoDoc.ActiveDoc;
            //var bom = new List<Dictionary<string, string>>();

            foreach (var docObject in doc.Objects)
            {
                if (!docObject.Attributes.HasUserData)
                    continue;
                var level = docObject.Attributes.GetUserString("Level");
                var attribute = docObject.Attributes.GetUserString("Attribute");
                var area = docObject.Attributes.GetUserString("unit");
                if (attribute == null | level == null)
                    continue;

                //var unit = surface.GetArea().ToString();
                string id = docObject.Id.ToString();
                //var amp = AreaMassProperties.Compute(surface);
                //Point3d centroid = amp.Centroid;

                bom.Add(new LCA.BillOfMaterials() { Attribute = attribute, Level = level, unit = area, id = id});
            }
            BOM = bom;
            return bom;

        }
    }
}
