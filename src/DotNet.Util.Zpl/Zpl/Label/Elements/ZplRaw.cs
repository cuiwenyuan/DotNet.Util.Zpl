using System.Collections.Generic;

namespace DotNet.Util.Zpl.Label.Elements
{
    /// <summary>
    /// ZplRaw
    /// </summary>
    public class ZplRaw : ZplElementBase
    {
        /// <summary>
        /// RawContent
        /// </summary>
        public string RawContent { get; private set; }
        /// <summary>
        /// ZplRaw
        /// </summary>
        /// <param name="rawZplCode"></param>
        public ZplRaw(string rawZplCode)
        {
            RawContent = rawZplCode;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            return new[] { RawContent };
        }
    }
}
