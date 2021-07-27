namespace DotNet.Util.Zpl.Label.Elements
{
    /// <summary>
    /// ZplPositionedElementBase
    /// </summary>
    public abstract class ZplPositionedElementBase : ZplElementBase
    {
        /// <summary>
        /// Origin
        /// </summary>
        public ZplOrigin Origin { get; protected set; }
        /// <summary>
        /// ZplPositionedElementBase
        /// </summary>
        /// <param name="positionX"></param>
        /// <param name="positionY"></param>
        public ZplPositionedElementBase(int positionX, int positionY) : base()
        {
            Origin = new ZplOrigin(positionX, positionY);
        }
    }
}
