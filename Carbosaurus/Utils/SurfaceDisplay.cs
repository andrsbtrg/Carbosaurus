using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using Rhino.Geometry;
using Rhino.Display;

namespace Carbosaurus.Utils
{
    class SurfaceDisplay : DisplayConduit
    {
        private Surface surface { get; set; }
        private Color color { get; set; } = Color.Black;
        private Mesh _mesh;
        private DisplayMaterial displayMaterial;

        public SurfaceDisplay(Surface surface)
        {
            this.surface = surface;
            this._mesh = Mesh.CreateFromSurface(surface);
            this.displayMaterial = new DisplayMaterial();
        }

        public SurfaceDisplay(Surface surface, Color color)
        {
            this.surface = surface;
            this.color = color;
            _mesh = Mesh.CreateFromSurface(surface);
            displayMaterial = new DisplayMaterial(color,0);

        }
        public SurfaceDisplay(Surface surface, Color color, double transparency)
        {
            this.surface = surface;
            this.color = color;
            _mesh = Mesh.CreateFromSurface(surface);
            displayMaterial = new DisplayMaterial(color, transparency);

        }

        //protected override void PostDrawObjects(DrawEventArgs e)
        //{
        //    e.Display.DrawMeshShaded(_mesh, displayMaterial);
            
        //    //e.Display.PushDepthWriting(false);
        //    //e.Display.PushDepthTesting(false);

        //    e.Display.DrawMeshWires(_mesh, Color.Black);
        //}
        protected override void DrawOverlay(DrawEventArgs e) // Using drawing overlaw and transparency to show plane on top of other objects
        {
            e.Display.DrawMeshShaded(_mesh, displayMaterial);

            //e.Display.PushDepthWriting(false);
            //e.Display.PushDepthTesting(false);

            e.Display.DrawMeshWires(_mesh, Color.Black);
        }
        // this is called every frame inside the drawing code so try to do as little as possible
        // in order to not degrade display speed. Don't create new objects if you don't have to as this
        // will incur an overhead on the heap and garbage collection. 
        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
            e.IncludeBoundingBox(surface.GetBoundingBox(true));
        }

    }
}
