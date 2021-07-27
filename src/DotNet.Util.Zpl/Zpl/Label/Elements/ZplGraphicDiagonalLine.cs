using System.Collections.Generic;

namespace DotNet.Util.Zpl.Label.Elements
{
    /// <summary>
    /// ZplGraphicDiagonalLine
    /// </summary>
    public class ZplGraphicDiagonalLine : ZplGraphicBox
    {
        /// <summary>
        /// RightLeaningDiagonal
        /// </summary>
        public bool RightLeaningDiagonal { get; private set; }
        /// <summary>
        /// ZplGraphicDiagonalLine
        /// </summary>
        /// <param name="positionX"></param>
        /// <param name="positionY"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="borderThickness"></param>
        /// <param name="rightLeaningDiagonal"></param>
        /// <param name="lineColor"></param>
        public ZplGraphicDiagonalLine(
            int positionX,
            int positionY,
            int width,
            int height,
            int borderThickness = 1,
            bool rightLeaningDiagonal = false,
            LineColor lineColor = LineColor.Black)
            : base(positionX, positionY, width, height, borderThickness, lineColor, 0)
        {
            RightLeaningDiagonal = rightLeaningDiagonal;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^GDw,h,t,c,o
            var result = new List<string>();
            result.AddRange(Origin.Render(context));
            result.Add($"^GD{context.Scale(Width)},{context.Scale(Height)},{context.Scale(BorderThickness)},{RenderLineColor(LineColor)},{(RightLeaningDiagonal ? "R" : "L")}^FS");

            return result;
        }
    }
}
