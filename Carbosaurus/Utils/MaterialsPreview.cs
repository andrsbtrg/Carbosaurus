using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carbosaurus.Utils
{
    public class MaterialsPreview
    {
        #region Singleton pattern
        private static MaterialsPreview instance = null;
        private static readonly object padlock = new object();

        public MaterialsPreview()
        {
            // Initialize previoys materials in case opening existing doc
            currentMaterials = new List<RhinoObject>();
            foreach (var docObject in Rhino.RhinoDoc.ActiveDoc.Objects)
            {
                if (!docObject.Attributes.HasUserData)
                    continue;
                var level = docObject.Attributes.GetUserString("Level");
                if (level != null)
                {
                    currentMaterials.Add(docObject);
                }
            }
        }

        public static MaterialsPreview Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new MaterialsPreview();
                    }
                    return instance;
                }
            }
        }
        #endregion

        public bool Active { get; private set; }
        public void SetPreview(bool state)
        {
            if (dotDisplays == null)
                return;
            foreach (var dot in dotDisplays)
            {
                dot.Enabled = state;
            }
            Active = state;
        }
        public List<RhinoObject> currentMaterials { get; private set; }

        public List<TextDotDisplay> dotDisplays { get; private set; }


        public void RemoveMaterial(List<RhinoObject> rhinoObjects)
        {
            foreach (var rhObj in rhinoObjects)
            {
                if (!currentMaterials.Contains(rhObj))
                    currentMaterials.Remove(rhObj);
            }
        }

        public void UpdatePreview()
        {
            SetPreview(false);
            dotDisplays = new List<TextDotDisplay>();
            AddToPreview(currentMaterials);
        }

        internal void AddToPreview(List<RhinoObject> surfaces)
        {
            if (currentMaterials == null)
                currentMaterials = new List<RhinoObject>();

            if (dotDisplays == null)
                dotDisplays = new List<TextDotDisplay>();

            //SetPreview(false);
            foreach (RhinoObject surface in surfaces)
            {
                if (!currentMaterials.Contains(surface))
                    currentMaterials.Add(surface);
                
            }
            PreviewMaterials();
            Active = true;
            Rhino.RhinoDoc.ActiveDoc.Views.Redraw();
        }

        private void PreviewMaterials()
        {
            SetPreview(false);
            dotDisplays = new List<TextDotDisplay>();
            foreach(var surface in currentMaterials)
            {
                string matName = surface.Attributes.GetUserString("Attribute");
                var geo = surface.Geometry as Brep;
                if (geo == null)
                {
                    var geo2 = surface.Geometry as Extrusion;
                    geo = geo2.ToBrep();
                }
                if (geo != null)
                {
                    Point3d centroid = AreaMassProperties.Compute(geo).Centroid;
                    dotDisplays.Add(new TextDotDisplay(new Rhino.Geometry.TextDot(matName, centroid)) { Enabled = true });
                }
            }
        }
    }

}
