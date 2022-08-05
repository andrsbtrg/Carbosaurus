using Rhino.Display;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carbosaurus.Utils
{
    public class BrepDisplay : DisplayConduit
    {
        List<Brep> breps { get; set; }
        Brep brep { get; set; }

        public double Alpha { get; set; } = 0.6;

        public Color Color { get; set; } = Color.Red;

        public BrepDisplay(List<Brep> breps)
        {
            this.breps = breps;
        }

        public BrepDisplay(List<Brep> breps, double alpha, Color color)
        {
            this.breps = breps;
            this.Alpha = alpha;
            this.Color = color;
        }
        public BrepDisplay(Brep brep, double alpha, Color color)
        {
            this.brep = brep;
            this.Alpha = alpha;
            this.Color = color;
        }

        public void AddBrep(Brep brep)
        {
            if(this.Enabled)
            {
                this.Enabled = false;
                breps.Add(brep);
                this.Enabled = true;
            }

            else
            {
                breps.Add(brep);
            }
        }

        public void RemoveCurve(Brep brep)
        {
            if (this.Enabled)
            {
                this.Enabled = false;
                breps.Remove(brep);
                this.Enabled = true;
            }
        }

        public void Empty()
        {
            this.Enabled = false;
            breps = new List<Brep>();
        }

        // Intead of using Postdraw Objects, using DrawOverlay so that the Shaded viewport and the Display Conduit don't interferfere and cause visual artifacts

        //protected override void PostDrawObjects(DrawEventArgs e)
        //{
        //    if (this.breps != null)
        //    {
        //        foreach (Brep brep in this.breps)
        //        {
        //            e.Display.DrawBrepShaded(brep, new DisplayMaterial(Color, Alpha));
        //        }
        //    }
        //    else if (this.brep != null)
        //        e.Display.DrawBrepShaded(brep, new DisplayMaterial(Color, Alpha));
        //}

        protected override void DrawOverlay(DrawEventArgs e)
        {
            if (this.breps != null)
            {
                foreach (Brep brep in this.breps)
                {
                    e.Display.DrawBrepShaded(brep, new DisplayMaterial(Color, Alpha));
                }
            }

            else if (this.brep != null)
                e.Display.DrawBrepShaded(brep, new DisplayMaterial(Color, Alpha));
        }

        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
            if (this.breps != null)
            {
                foreach (Brep brep in this.breps)
                {
                    e.IncludeBoundingBox(brep.GetBoundingBox(true));
                }
            }
            else if (this.brep != null)
            {
                e.IncludeBoundingBox(brep.GetBoundingBox(true));
            }

        }


    }
}
