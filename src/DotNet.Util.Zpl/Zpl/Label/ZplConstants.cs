using DotNet.Util.Zpl.Label.Elements;

namespace DotNet.Util.Zpl.Label
{
    /// <summary>
    /// ZplConstants
    /// </summary>
    public static class ZplConstants
    {
        /// <summary>
        /// InternationalFontEncoding
        /// </summary>
        public static class InternationalFontEncoding
        {
            /// <summary>
            /// Unicode (UTF-8 encoding) - Unicode Character Set
            /// </summary>
            public static readonly string CI28 = "^CI28";
            /// <summary>
            /// 13 = Zebra Code Page 850 (see page 1194)
            /// </summary>
            public static readonly string CI13 = "^CI13";
        }
        /// <summary>
        /// Font
        /// </summary>
        public static class Font
        {
            /// <summary>
            /// Default ZplFont
            /// </summary>
            public static readonly ZplFont Default = new ZplFont(30, 30, "0", FieldOrientation.Normal);
        }
    }
}
