﻿using System.Collections.Generic;

namespace DotNet.Util.Zpl.Label.Elements
{
    /// <summary>
    /// All built-in fonts are referenced using a one-character identifier. The ^CW command assigns a single
    /// alphanumeric character to a font stored in DRAM, memory card, EPROM, or Flash
    /// </summary>
    public class ZplFontIdentifier : ZplElementBase
    {
        /// <summary>
        /// FontReplaceLetter
        /// </summary>
        public char FontReplaceLetter { get; private set; }
        /// <summary>
        /// Device
        /// </summary>
        public string Device { get; private set; }
        /// <summary>
        /// FontFileName
        /// </summary>
        public string FontFileName { get; private set; }

        /// <summary>
        /// All built-in fonts are referenced using a one-character identifier. The ^CW command assigns a single
        /// alphanumeric character to a font stored in DRAM, memory card, EPROM, or Flash
        /// </summary>
        public ZplFontIdentifier(char fontReplaceLetter, string device, string fontFileName)
        {
            FontReplaceLetter = fontReplaceLetter;
            Device = device;
            FontFileName = fontFileName;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^CWa,d:o.x
            var result = new List<string>
            {
                $"^CW{FontReplaceLetter},{Device}:{FontFileName}"
            };

            return result;
        }
    }
}
