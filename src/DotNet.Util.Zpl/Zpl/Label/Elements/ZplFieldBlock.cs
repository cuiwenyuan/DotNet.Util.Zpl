using System;
using System.Collections.Generic;

namespace DotNet.Util.Zpl.Label.Elements
{
    /// <summary>
    /// ZplFieldBlock ^FB – Field Block
    /// </summary>
    public class ZplFieldBlock : ZplTextField
    {
        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// MaxLineCount
        /// </summary>
        public int MaxLineCount { get; private set; }
        /// <summary>
        /// LineSpace
        /// </summary>
        public int LineSpace { get; private set; }
        /// <summary>
        /// TextJustification
        /// </summary>
        public TextJustification TextJustification { get; private set; }

        /// <summary>
        /// HangingIndent hanging indent (in dots) of the second and remaining lines
        /// </summary>
        public int HangingIndent { get; private set; }

        /// <summary>
        /// ZplFieldBlock
        /// </summary>
        /// <param name="text"></param>
        /// <param name="positionX"></param>
        /// <param name="positionY"></param>
        /// <param name="width"></param>
        /// <param name="font"></param>
        /// <param name="maxLineCount"></param>
        /// <param name="lineSpace"></param>
        /// <param name="textJustification"></param>
        /// <param name="hangingIndent"></param>
        /// <param name="newLineConversion"></param>
        /// <param name="useHexadecimalIndicator"></param>
        /// <param name="reversePrint"></param>
        public ZplFieldBlock(
            string text,
            int positionX,
            int positionY,
            int width,
            ZplFont font,
            int maxLineCount = 1,
            int lineSpace = 0,
            TextJustification textJustification = TextJustification.Left,
            int hangingIndent = 0,
            NewLineConversionMethod newLineConversion = NewLineConversionMethod.ToZplNewLine,
            bool useHexadecimalIndicator = true,
            bool reversePrint = false)
            : base(text, positionX, positionY, font, newLineConversion, useHexadecimalIndicator, reversePrint)
        {
            TextJustification = textJustification;
            Width = width;
            MaxLineCount = maxLineCount;
            LineSpace = lineSpace;
            HangingIndent = hangingIndent;
        }

        private string RenderTextJustification()
        {
            switch (TextJustification)
            {
                case TextJustification.Left:
                    return "L";
                case TextJustification.Center:
                    return "C";
                case TextJustification.Right:
                    return "R";
                case TextJustification.Justified:
                    return "J";
            }

            throw new NotImplementedException("Unknown Text Justification");
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^ XA
            //^ CF0,30,30 ^ FO25,50
            //   ^ FB250,4,,
            //^ FDFD command that IS\&
            // preceded by an FB \&command.
            // ^ FS
            // ^ XZ
            var result = new List<string>();
            result.AddRange(Font.Render(context));
            result.AddRange(Origin.Render(context));
            result.Add($"^FB{context.Scale(Width)},{MaxLineCount},{context.Scale(LineSpace)},{RenderTextJustification()},{context.Scale(HangingIndent)}");
            result.Add(RenderFieldDataSection());

            return result;
        }
    }
}
