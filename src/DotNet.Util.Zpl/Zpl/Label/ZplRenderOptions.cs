namespace DotNet.Util.Zpl.Label
{
    /// <summary>
    /// Holding rendering settings
    /// </summary>
    public class ZplRenderOptions
    {
        /// <summary>
        /// Include ^XA and ^XZ
        /// </summary>
        public bool AddStartEndFormat { get; set; }
        /// <summary>
        /// Include ^LH0,0
        /// </summary>
        public bool AddDefaultLabelHome { get; set; }

        /// <summary>
        /// ^CI
        /// </summary>
        public string ChangeInternationalFontEncoding { get; set; }
        /// <summary>
        /// DisplayComments
        /// </summary>
        public bool DisplayComments { get; set; }
        /// <summary>
        /// AddEmptyLineBeforeElementStart
        /// </summary>
        public bool AddEmptyLineBeforeElementStart { get; set; }

        /// <summary>
        /// SourcePrint DPI
        /// </summary>
        /// <remarks>Default: 203</remarks>
        public int SourcePrintDpi { get; set; }

        /// <summary>
        /// TargetPrint DPI
        /// </summary>
        /// <remarks>Default: 203</remarks>
        public int TargetPrintDpi { get; set; }
        /// <summary>
        /// CompressedRendering
        /// </summary>
        public bool CompressedRendering { get; set; }
        /// <summary>
        /// ScaleFactor
        /// </summary>
        public double ScaleFactor { get { return (double)TargetPrintDpi / SourcePrintDpi; } }
        /// <summary>
        /// Scale
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public int Scale(int input)
        {
            return (int)(input * ScaleFactor);
        }
        /// <summary>
        /// Scale
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public double Scale(double input)
        {
            return input * ScaleFactor;
        }
        /// <summary>
        /// ZplRenderOptions
        /// </summary>
        public ZplRenderOptions()
        {
            AddStartEndFormat = true;
            AddDefaultLabelHome = true;
            ChangeInternationalFontEncoding = ZplConstants.InternationalFontEncoding.CI28;
            SourcePrintDpi = TargetPrintDpi = 203;
        }
    }
}
