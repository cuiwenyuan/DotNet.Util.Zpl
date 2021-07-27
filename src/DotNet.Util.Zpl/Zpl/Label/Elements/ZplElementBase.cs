using System;
using System.Collections.Generic;

namespace DotNet.Util.Zpl.Label.Elements
{
    /// <summary>
    /// ZplElementBase
    /// </summary>
    public abstract class ZplElementBase
    {
        /// <summary>
        /// Comments
        /// </summary>
        public List<string> Comments { get; protected set; }

        /// <summary>
        /// Indicate the rendering process whether this elemenet can be skipped
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Optionally identify the element for future lookup/manipulation
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// ZplElementBase
        /// </summary>
        public ZplElementBase()
        {
            Comments = new List<string>();
            IsEnabled = true;
        }

        /// <summary>
        /// Render Zpl data
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> Render()
        {
            return Render(new ZplRenderOptions());
        }
        /// <summary>
        /// RenderToString
        /// </summary>
        /// <returns></returns>
        public string RenderToString()
        {
            return string.Join(" ", Render());
        }

        /// <summary>
        /// Render Zpl data
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public abstract IEnumerable<string> Render(ZplRenderOptions context);
        /// <summary>
        /// RenderFieldOrientation
        /// </summary>
        /// <param name="fieldOrientation"></param>
        /// <returns></returns>
        public string RenderFieldOrientation(FieldOrientation fieldOrientation)
        {
            switch (fieldOrientation)
            {
                case FieldOrientation.Normal:
                    return "N";
                case FieldOrientation.Rotated90:
                    return "R";
                case FieldOrientation.Rotated180:
                    return "I";
                case FieldOrientation.Rotated270:
                    return "B";
            }

            throw new NotImplementedException("Unknown Field Orientation");
        }
        /// <summary>
        /// RenderLineColor
        /// </summary>
        /// <param name="lineColor"></param>
        /// <returns></returns>
        public string RenderLineColor(LineColor lineColor)
        {
            switch (lineColor)
            {
                case LineColor.Black:
                    return "B";
                case LineColor.White:
                    return "W";
            }

            throw new NotImplementedException("Unknown Line Color");
        }
        /// <summary>
        /// RenderErrorCorrectionLevel
        /// </summary>
        /// <param name="errorCorrectionLevel"></param>
        /// <returns></returns>
        public string RenderErrorCorrectionLevel(ErrorCorrectionLevel errorCorrectionLevel)
        {
            switch (errorCorrectionLevel)
            {
                case ErrorCorrectionLevel.UltraHighReliability:
                    return "H";
                case ErrorCorrectionLevel.HighReliability:
                    return "Q";
                case ErrorCorrectionLevel.Standard:
                    return "M";
                case ErrorCorrectionLevel.HighDensity:
                    return "L";
            }

            throw new NotImplementedException("Unknown Error Correction Level");
        }
        /// <summary>
        /// ToZplString
        /// </summary>
        /// <returns></returns>
        public string ToZplString()
        {
            return ToZplString(new ZplRenderOptions());
        }
        /// <summary>
        /// ToZplString
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string ToZplString(ZplRenderOptions context)
        {
            return string.Join("\n", Render(context));
        }
    }
}
