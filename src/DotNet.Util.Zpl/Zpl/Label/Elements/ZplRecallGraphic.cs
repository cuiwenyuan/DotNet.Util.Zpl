using System;
using System.Collections.Generic;

namespace DotNet.Util.Zpl.Label.Elements
{
    /// <summary>
    /// ^XGd:o.x,mx,my
    /// </summary>
    public class ZplRecallGraphic : ZplPositionedElementBase
    {
        /// <summary>
        /// StorageDevice
        /// </summary>
        public char StorageDevice { get; private set; }
        /// <summary>
        /// ImageName
        /// </summary>
        public string ImageName { get; private set; }
        private string _extension { get; set; }
        /// <summary>
        /// MagnificationFactorX
        /// </summary>
        public int MagnificationFactorX { get; private set; }
        /// <summary>
        /// MagnificationFactorY
        /// </summary>
        public int MagnificationFactorY { get; private set; }
        /// <summary>
        /// ZplRecallGraphic
        /// </summary>
        /// <param name="positionX"></param>
        /// <param name="positionY"></param>
        /// <param name="storageDevice"></param>
        /// <param name="imageName"></param>
        /// <param name="magnificationFactorX"></param>
        /// <param name="magnificationFactorY"></param>
        public ZplRecallGraphic(
            int positionX,
            int positionY,
            char storageDevice,
            string imageName,
            int magnificationFactorX = 1,
            int magnificationFactorY = 1)
            : base(positionX, positionY)
        {
            if (imageName.Length > 8)
            {
                new ArgumentException("Maximum length of 8 characters exceeded", nameof(imageName));
            }

            _extension = "GRF";

            StorageDevice = storageDevice;
            ImageName = imageName;
            MagnificationFactorX = magnificationFactorX;
            MagnificationFactorY = magnificationFactorY;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            var result = new List<string>();
            result.AddRange(Origin.Render(context));
            result.Add($"^XG{StorageDevice}:{ImageName}.{_extension},{MagnificationFactorX},{MagnificationFactorY}^FS");

            return result;
        }
    }
}
