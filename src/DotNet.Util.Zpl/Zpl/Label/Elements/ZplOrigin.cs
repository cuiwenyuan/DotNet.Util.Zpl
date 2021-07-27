﻿using System.Collections.Generic;

namespace DotNet.Util.Zpl.Label.Elements
{
    /// <summary>
    /// Field Origin<br/>
    /// The ^FO command sets a field origin, relative to the label home (^LH) position.
    /// ^FO sets the upper-left corner of the field area by defining points along the x-axis and y-axis independent of the rotation.
    /// </summary>
    /// <remarks>
    /// Format:^FOx,y
    /// </remarks>
    public class ZplOrigin : ZplElementBase
    {
        public int PositionX { get; protected set; }
        public int PositionY { get; protected set; }

        /// <summary>
        /// ZplOrigin
        /// </summary>
        /// <param name="positionX">X Position (0-32000)</param>
        /// <param name="positionY">Y Position (0-32000)</param>
        public ZplOrigin(int positionX, int positionY)
        {
            PositionX = positionX;
            PositionY = positionY;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^ FO50,50
            return new string[] { $"^FO{context.Scale(PositionX)},{context.Scale(PositionY)}" };
        }

        /// <summary>
        /// Return a new instance with offset applied
        /// </summary>
        /// <param name="OffsetX"></param>
        /// <param name="OffsetY"></param>
        /// <returns></returns>
        public ZplOrigin Offset(int OffsetX, int OffsetY)
        {
            return new ZplOrigin(PositionX + OffsetX, PositionY + OffsetY);
        }
    }
}
