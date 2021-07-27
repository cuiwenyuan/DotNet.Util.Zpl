using System.Collections.Generic;

namespace DotNet.Util.Zpl.Label.Elements
{
    /// <summary>
    /// ZplGraphicEllipse
    /// </summary>
    public class ZplGraphicEllipse : ZplGraphicBox
    {
        /// <summary>
        /// ZplGraphicEllipse
        /// </summary>
        /// <param name="positionX"></param>
        /// <param name="positionY"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="borderThickness"></param>
        /// <param name="lineColor"></param>
        public ZplGraphicEllipse(
            int positionX,
            int positionY,
            int width,
            int height,
            int borderThickness = 1,
            LineColor lineColor = LineColor.Black)
            : base(positionX, positionY, width, height, borderThickness, lineColor, 0)
        {
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^ GE300,100,10,B ^ FS
            var result = new List<string>();
            result.AddRange(Origin.Render(context));
            result.Add($"^GE{context.Scale(Width)},{context.Scale(Height)},{context.Scale(BorderThickness)},{RenderLineColor(LineColor)}^FS");

            return result;
        }
    }
}
