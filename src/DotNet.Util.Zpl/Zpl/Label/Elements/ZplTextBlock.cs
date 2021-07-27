using System;
using System.Collections.Generic;

namespace DotNet.Util.Zpl.Label.Elements
{
    /// <summary>
    /// The ^TB command prints a text block with defined width and height.
    /// The text block has an automatic word-wrap function. 
    /// If the text exceeds the block height, the text is truncated. Does not support \n
    /// </summary>
    public class ZplTextBlock : ZplTextField
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
        /// ZplTextBlock
        /// </summary>
        /// <param name="text"></param>
        /// <param name="positionX"></param>
        /// <param name="positionY"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="font"></param>
        /// <param name="newLineConversion"></param>
        /// <param name="useHexadecimalIndicator"></param>
        public ZplTextBlock(
            string text,
            int positionX,
            int positionY,
            int width,
            int height,
            ZplFont font,
            NewLineConversionMethod newLineConversion = NewLineConversionMethod.ToSpace,
            bool useHexadecimalIndicator = true)
            : base(text, positionX, positionY, font, newLineConversion, useHexadecimalIndicator)
        {
            Width = width;
            Height = height;

            if (newLineConversion == NewLineConversionMethod.ToZplNewLine)
            {
                throw new NotSupportedException("ToZplNewLine is not supported, use ZplFieldBlock");
            }
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            var result = new List<string>();
            result.AddRange(Font.Render(context));
            result.AddRange(Origin.Render(context));
            result.Add($"^TB{RenderFieldOrientation(Font.FieldOrientation)},{context.Scale(Width)},{context.Scale(Height)}");
            result.Add(RenderFieldDataSection());

            return result;
        }
    }
}
