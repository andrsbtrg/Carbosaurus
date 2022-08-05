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
    public class TextDotDisplay : DisplayConduit
    {
        public TextDot textDot { get; private set; }
        Color color { get; set; } = Color.Gray;

        public TextDotDisplay(TextDot textDot)
        {
            this.textDot = textDot;
        }

        public TextDotDisplay(TextDot textDot, Color color)
        {
            this.textDot = textDot;
            this.color = color;
        }

        protected override void PostDrawObjects(DrawEventArgs e)
        {
            //e.Display.DrawDot(textDot.Point, textDot.Text);
            e.Display.PushDepthWriting(false);
            e.Display.PushDepthTesting(false);
            e.Display.DrawDot(textDot, color, Color.Black, color);

        }
        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
            e.IncludeBoundingBox(textDot.GetBoundingBox(true));
        }
    }
}
