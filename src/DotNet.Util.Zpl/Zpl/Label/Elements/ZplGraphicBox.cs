using System.Collections.Generic;

namespace DotNet.Util.Zpl.Label.Elements
{
    /// <summary>
    /// ZplGraphicBox
    /// </summary>
    public class ZplGraphicBox : ZplGraphicElement
    {
        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// CornerRounding 0~8
        /// </summary>
        public int CornerRounding { get; private set; }
        /// <summary>
        /// ZplGraphicBox
        /// </summary>
        /// <param name="positionX"></param>
        /// <param name="positionY"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="borderThickness"></param>
        /// <param name="lineColor"></param>
        /// <param name="cornerRounding"></param>
        public ZplGraphicBox(
            int positionX,
            int positionY,
            int width,
            int height,
            int borderThickness = 1,
            LineColor lineColor = LineColor.Black,
            int cornerRounding = 0)
            : base(positionX, positionY, borderThickness, lineColor)
        {
            Width = width;
            Height = height;

            CornerRounding = cornerRounding;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^ FO50,50
            //^ GB300,200,10 ^ FS
            var result = new List<string>();
            result.AddRange(Origin.Render(context));
            result.Add($"^GB{context.Scale(Width)},{context.Scale(Height)},{context.Scale(BorderThickness)},{RenderLineColor(LineColor)},{context.Scale(CornerRounding)}^FS");

            return result;
        }
    }
}
