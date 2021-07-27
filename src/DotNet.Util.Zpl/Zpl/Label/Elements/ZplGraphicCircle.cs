using System.Collections.Generic;

namespace DotNet.Util.Zpl.Label.Elements
{
    /// <summary>
    /// Graphic Circle<br/>
    /// The ^GC command produces a circle on the printed label.
    /// The command parameters specify the diameter(width) of the circle, outline thickness, and color.
    /// Thickness extends inward from the outline.
    /// </summary>
    /// <remarks>
    /// Format:^GCd,t,c
    /// d = circle diameter
    /// t = border thickness
    /// c = line color
    /// </remarks>
    public class ZplGraphicCircle : ZplGraphicElement
    {
        /// <summary>
        /// Diameter
        /// </summary>
        public int Diameter { get; private set; }
        /// <summary>
        /// ZplGraphicCircle
        /// </summary>
        /// <param name="positionX"></param>
        /// <param name="positionY"></param>
        /// <param name="diameter"></param>
        /// <param name="borderThickness"></param>
        /// <param name="lineColor"></param>
        public ZplGraphicCircle(
            int positionX,
            int positionY,
            int diameter,
            int borderThickness = 1,
            LineColor lineColor = LineColor.Black)
            : base(positionX, positionY, borderThickness, lineColor)
        {
            Diameter = diameter;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^GCd,t,c
            var result = new List<string>();
            result.AddRange(Origin.Render(context));
            result.Add($"^GC{context.Scale(Diameter)},{context.Scale(BorderThickness)},{RenderLineColor(LineColor)}^FS");

            return result;
        }
    }
}
