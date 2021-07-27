using System.Collections.Generic;

namespace DotNet.Util.Zpl.Label.Elements
{
    /// <summary>
    /// ^A – Scalable/Bitmapped Font
    /// </summary>
    public class ZplFont : ZplElementBase
    {
        /// <summary>
        /// Any font in the printer (downloaded, EPROM, stored fonts, fonts A through Z and 0 to 9).
        /// </summary>
        public string FontName { get; private set; }
        /// <summary>
        /// FieldOrientation
        /// </summary>
        public FieldOrientation FieldOrientation { get; private set; }
        /// <summary>
        /// FontWidth
        /// </summary>
        public int FontWidth { get; private set; }
        /// <summary>
        /// FontHeight
        /// </summary>
        public int FontHeight { get; private set; }
        /// <summary>
        /// ZplFont
        /// </summary>
        /// <param name="fontWidth"></param>
        /// <param name="fontHeight"></param>
        /// <param name="fontName"></param>
        /// <param name="fieldOrientation"></param>
        public ZplFont(
            int fontWidth = 30,
            int fontHeight = 30,
            string fontName = "0",
            FieldOrientation fieldOrientation = FieldOrientation.Normal)
        {
            FontName = fontName;
            FieldOrientation = fieldOrientation;
            FontWidth = fontWidth;
            FontHeight = fontHeight;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            return new[] { $"^A{FontName}{RenderFieldOrientation(FieldOrientation)},{context.Scale(FontHeight)},{context.Scale(FontWidth)}" };
        }
    }
}
