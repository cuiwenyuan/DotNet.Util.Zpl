namespace DotNet.Util.Zpl.Label.Elements
{
    /// <summary>
    /// public partial class Label
    /// </summary>
    public abstract class ZplBarcode : ZplPositionedElementBase
    {
        /// <summary>
        /// ZplBarcode
        /// </summary>
        /// <param name="content"></param>
        /// <param name="positionX"></param>
        /// <param name="positionY"></param>
        /// <param name="height"></param>
        /// <param name="fieldOrientation"></param>
        /// <param name="printInterpretationLine"></param>
        /// <param name="printInterpretationLineAboveCode"></param>
        public ZplBarcode(
            string content,
            int positionX,
            int positionY,
            int height,
            FieldOrientation fieldOrientation,
            bool printInterpretationLine,
            bool printInterpretationLineAboveCode) 
            : base(positionX, positionY)
        {
            Origin = new ZplOrigin(positionX, positionY);
            Content = content;
            Height = height;
            FieldOrientation = fieldOrientation;
            PrintInterpretationLine = printInterpretationLine;
            PrintInterpretationLineAboveCode = printInterpretationLineAboveCode;
        }
        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; protected set; }
        /// <summary>
        /// FieldOrientation
        /// </summary>
        public FieldOrientation FieldOrientation { get; protected set; }

        public string Content { get; protected set; }
        public bool PrintInterpretationLine { get; protected set; }
        public bool PrintInterpretationLineAboveCode { get; protected set; }

        public string RenderPrintInterpretationLine()
        {
            return PrintInterpretationLine ? "Y" : "N";
        }

        public string RenderPrintInterpretationLineAboveCode()
        {
            return PrintInterpretationLineAboveCode ? "Y" : "N";
        }

        protected string RenderFieldOrientation()
        {
            return RenderFieldOrientation(FieldOrientation);
        }

        protected bool IsDigitsOnly(string text)
        {
            foreach (char c in text)
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }
            }

            return true;
        }
    }
}
