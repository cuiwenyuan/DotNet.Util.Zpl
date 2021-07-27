﻿using System;
using System.Collections.Generic;

namespace DotNet.Util.Zpl.Label.Elements
{
    /// <summary>
    /// ^XGd:o.x,mx,my
    /// </summary>
    public class ZplRecallGraphic : ZplPositionedElementBase
    {
        public char StorageDevice { get; private set; }
        public string ImageName { get; private set; }
        private string _extension { get; set; }
        public int MagnificationFactorX { get; private set; }
        public int MagnificationFactorY { get; private set; }

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
