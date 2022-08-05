using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Newtonsoft.Json;
using Rhino.Geometry;
using Eto.Drawing;
using Rhino.Display;

namespace Carbosaurus.Utils
{
    public class ResultsPreview
    {
        #region Singleton pattern
        private static ResultsPreview instance = null;
        private static readonly object padlock = new object();
        
        public ResultsPreview()
        {
            Color1 = Eto.Drawing.Color.FromArgb(50, 110, 220, (int)(0.8*255));
            Color2 = Color.FromArgb(220, 82, 50, (int)(0.8 * 255));
            Visible = false;
        }
        public static ResultsPreview Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new ResultsPreview();
                    }
                    return instance;
                }
            }
        }
        #endregion


        public bool Visible;
        // Default colours
        public Color Color1 { get; set; }
        public Color Color2 { get; set; }
        private DrawLegendConduit _conduit;

        public List<Utils.BrepDisplay> BrepDisplays { get; set; }

        private Interval m_z_range = new Interval(-10, 10);
        private Interval m_hue_range = new Interval(0, 0.3333333333333);
        private List<BrepDisplay> displays;
        private Dictionary<string, Dictionary<string, double?>> bom { get; set; }
        private List<double> MeanImpact { get; set; }
        public List<double> CoVar { get; private set; }
        public List<double> Std { get; private set; }
        public List<double> Corr { get; private set; }

        private string Json;
        private List<Brep> Breps { get; set; }
        public void SaveValues(string json)
        {
            Json = json;
            double total = 0;
            bom = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, double?>>>(json);

            MeanImpact = new List<double>();
            CoVar = new List<double>();
            Std = new List<double>();
            Corr = new List<double>();

            // Get totals
            foreach (string key in bom.Keys)
            {
                bom.TryGetValue(key, out var elem);
                if (elem == null)
                    continue;
                //double meanValue = elem["mean"];
                elem.TryGetValue("mean", out double? meanValue);
                //double stdValue = elem["std"];
                elem.TryGetValue("std", out double? stdValue);
                elem.TryGetValue("correlation", out double? corrValue);
                try
                {
                    meanValue = (double)meanValue;
                    stdValue = (double)stdValue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }
                if (corrValue == null)
                    corrValue = 0;


                MeanImpact.Add((double)meanValue);
                Std.Add((double)stdValue);
                CoVar.Add((double)(meanValue / stdValue));
                Corr.Add((double)corrValue);
                total += (double)meanValue;
            }
        }

        public void PreviewValues(string previewOption)
        {
            SetVisible(false);
            var legend = DrawLegend(50, 250);
            _conduit.Legend = legend;
            _conduit.Enabled = true;

            if (bom == null)
            {
                RhinoApp.WriteLine("No results to display. Please run a simulation first.");
                return;
            }

            var doc = RhinoDoc.ActiveDoc;

            displays = new List<BrepDisplay>();

            //m_z_range = new Interval(0, 360);
            //var coffRange = new Interval(0, 1);
            double [] vs = new double[Std.Count];
            for (int i = 0; i < Std.Count; i++)
            {
                vs.Append(Std[i] / MeanImpact[i]);
            }

            int j = 0;
            // Colors
            System.Drawing.Color color1 = System.Drawing.Color.FromArgb(Color1.ToArgb());
            System.Drawing.Color color2 = System.Drawing.Color.FromArgb(Color2.ToArgb());
            List<double> valueToPreview;
            switch (previewOption)
            {
                case "Contribution to variance":
                    valueToPreview = Corr;
                    break;
                case "Embodied Carbon":
                    valueToPreview = new List<double>();
                    for( int i =0; i< MeanImpact.Count; i++)
                    {
                        valueToPreview.Add(MeanImpact[i] / MeanImpact.Max());
                    }
                    break;
                default:
                    valueToPreview = Corr;
                    break;
            }

            foreach (string key in bom.Keys)
            {
                
                var rhobject = doc.Objects.FindId(Guid.Parse(key));
                if (rhobject != null)
                {
                    //if (rhobject.Geometry as Brep != null)
                    var geom = rhobject.Geometry as Brep;
                    if (geom == null)
                    {
                        var xtr = rhobject.Geometry as Extrusion;
                        if (xtr != null)
                        {
                            geom = xtr.ToBrep();
                        }

                    }

                    var coeff = valueToPreview[j];
                    //Console.WriteLine(coeff);
                    double s = RhinoMath.Clamp(coeff, 0, 1);
                    var color = ColorInterpolator.InterpolateBetween(color1, color2, s);
                    var display = new BrepDisplay(geom, 0.5, color);

                    //display.Enabled = true;
                    displays.Add(display);
                    j ++;
                }
            }
            BrepDisplays = displays;

            SetVisible(true);


        }

        private System.Drawing.Bitmap DrawLegend(int width, int height)
        {

            if (_conduit == null)
                _conduit = new DrawLegendConduit();

            var gradient = new Gradient(this.Color2, this.Color1); // Switch the order because the way rhino draws from top to bottom

            var legend = new System.Drawing.Bitmap(width, height);
            for (var x = 1; x < width; x++)
                for (var y = 1; y < height; y++)
                    legend.SetPixel(x, y, gradient.ColourAt((double)y / height));

            return legend;
        }

        private class DrawLegendConduit : Rhino.Display.DisplayConduit
        {
            public System.Drawing.Bitmap Legend;
            protected override void DrawForeground(DrawEventArgs e)
            {
                e.Display.DrawBitmap(new DisplayBitmap(Legend), 50, 50);

                // Draw text
                var bounds = e.Viewport.Bounds;
                var pt = new Rhino.Geometry.Point2d(bounds.Left + 140, bounds.Top + 60);
                e.Display.Draw2dText("High", System.Drawing.Color.Black, pt, true,18);

                var pt2 = new Rhino.Geometry.Point2d(pt.X, pt.Y + 230);
                e.Display.Draw2dText("Low", System.Drawing.Color.Black, pt2, true,18);
            }
        }

        public void SetVisible(bool state)
        {
            if (BrepDisplays == null)
            {
                return;
            }
            foreach (BrepDisplay display in BrepDisplays)
            {
                display.Enabled = state;
            }
            _conduit.Enabled = state;
            RhinoDoc.ActiveDoc.Views.Redraw();
            Visible = state;
        }
    }

    class Gradient
    {
        public Gradient(System.Drawing.Color color1, System.Drawing.Color color2)
        {
            Color1 = color1;
            Color2 = color2;

        }
        public Gradient(Color color1, Color color2)
        {
            Color1 = System.Drawing.Color.FromArgb(color1.ToArgb());
            Color2 = System.Drawing.Color.FromArgb(color2.ToArgb());
        }

        public System.Drawing.Color Color1 { get; private set; }
        public System.Drawing.Color Color2 { get; private set; }

        public System.Drawing.Color ColourAt(double pos)
        {

            var color = ColorInterpolator.InterpolateBetween(Color1, Color2, pos);
            return color;
        }
    }

    class ColorInterpolator
    {
        delegate byte ComponentSelector(System.Drawing.Color color);
        static ComponentSelector _redSelector = color => color.R;
        static ComponentSelector _greenSelector = color => color.G;
        static ComponentSelector _blueSelector = color => color.B;

        public static System.Drawing.Color InterpolateBetween(
            System.Drawing.Color endPoint1,
            System.Drawing.Color endPoint2,
            double lambda)
        {
            if (lambda < 0 || lambda > 1)
            {
                throw new ArgumentOutOfRangeException("lambda");
            }
            System.Drawing.Color color = System.Drawing.Color.FromArgb(
                InterpolateComponent(endPoint1, endPoint2, lambda, _redSelector),
                InterpolateComponent(endPoint1, endPoint2, lambda, _greenSelector),
                InterpolateComponent(endPoint1, endPoint2, lambda, _blueSelector)
            );

            return color;
        }

        static byte InterpolateComponent(
            System.Drawing.Color endPoint1,
            System.Drawing.Color endPoint2,
            double lambda,
            ComponentSelector selector)
        {
            return (byte)(selector(endPoint1)
                + (selector(endPoint2) - selector(endPoint1)) * lambda);
        }
    }

}
