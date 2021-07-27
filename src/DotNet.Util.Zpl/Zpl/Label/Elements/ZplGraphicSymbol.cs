using System.Collections.Generic;

namespace DotNet.Util.Zpl.Label.Elements
{
    /// <summary>
    /// ZplGraphicSymbol
    /// </summary>
    public class ZplGraphicSymbol : ZplPositionedElementBase
    {
        /// <summary>
        /// FieldOrientation
        /// </summary>
        public FieldOrientation FieldOrientation { get; private set; }
        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; private set; }
        
        /// <summary>
        /// Character
        /// </summary>
        public GraphicSymbolCharacter Character { get; private set; }

        string CharacterLetter
        {
            get
            {
                switch (Character)
                {
                    case GraphicSymbolCharacter.RegisteredTradeMark:
                        return "A";
                    case GraphicSymbolCharacter.Copyright:
                        return "B";
                    case GraphicSymbolCharacter.TradeMark:
                        return "C";
                    case GraphicSymbolCharacter.UnderwritersLaboratoriesApproval:
                        return "D";
                    case GraphicSymbolCharacter.CanadianStandardsAssociationApproval:
                        return "E";
                    default:
                        return "";
                }
            }
        }
        /// <summary>
        /// ZplGraphicSymbol
        /// </summary>
        /// <param name="character"></param>
        /// <param name="positionX"></param>
        /// <param name="positionY"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="fieldOrientation"></param>
        public ZplGraphicSymbol(
            GraphicSymbolCharacter character,
            int positionX,
            int positionY,
            int width,
            int height,
            FieldOrientation fieldOrientation = FieldOrientation.Normal)
            : base(positionX, positionY)
        {
            Character = character;
            FieldOrientation = fieldOrientation;
            Width = width;
            Height = height;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^GSo,h,w
            var result = new List<string>();
            result.AddRange(Origin.Render(context));
            result.Add($"^GS{RenderFieldOrientation(FieldOrientation)},{context.Scale(Height)},{context.Scale(Width)}^FD{CharacterLetter}^FS");

            return result;
        }
    }
}
