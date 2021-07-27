namespace DotNet.Util.Zpl.Label.Elements
{
    /// <summary>
    /// ZplGraphicElement
    /// </summary>
    public abstract class ZplGraphicElement : ZplPositionedElementBase
    {
        /// <summary>
        /// Line color
        /// </summary>
        public LineColor LineColor { get; protected set; }
        /// <summary>
        /// BorderThickness
        /// </summary>
        public int BorderThickness { get; protected set; }
        /// <summary>
        /// ZplGraphicElement
        /// </summary>
        /// <param name="positionX"></param>
        /// <param name="positionY"></param>
        /// <param name="borderThickness"></param>
        /// <param name="lineColor"></param>
        public ZplGraphicElement(int positionX, int positionY, int borderThickness = 1, LineColor lineColor = LineColor.Black) : base(positionX, positionY)
        {
            BorderThickness = borderThickness;
            LineColor = lineColor;
        }
    }
}
