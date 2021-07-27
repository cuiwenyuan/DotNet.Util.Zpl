# DotNet.Util.Zpl
DotNet.Util.Zpl a set of .net libraries to design label & generate ZPL data easily, it supports 4 ways (LPT/COM/USB/TCP) to print label (ZPL data) to Zebra printers.
ZPL is a printer description language from Zebra Technologies. ZPL II is now emulated by many label printers from different manufacturers. So with this implementation you can print labels to most printers.

For ZPL string please refer to the Programming Guide for raw ZPL code definitaion, [ZPL Documentation](https://www.zebra.com/content/dam/zebra/manuals/printers/common/programming/zpl-zbi2-pm-en.pdf)

## Supported Printer

This library supports following connected printer:

| Connection | Informations |
| ------------- | ------------- |
| COM | COM |
| LPT | connects to PC/Laptop by print the port LPT |
| USB | USB  |
| TCP | Network printers/Shared Printers |

## BinaryKits.Zpl
Generate ZPL label data is based on [BinaryKits.Zpl](https://github.com/BinaryKits/BinaryKits.Zpl), thanks [BinaryKits.Zpl](https://github.com/BinaryKits/BinaryKits.Zpl).

## Supported Label Elements

This library supports following elements:

| Element | Informations |
| ------------- | ------------- |
| Barcode | ANSI Codabar, Code 30, Code 128, EAN-13, Interleaved 2 of 5 |
| QR-Code | - |
| Image | DownloadObjects, DownloadGraphics  |
| Text | TextBlock, TextField, FieldBlock, SingleLineFieldBlock |
| Drawing | GraphicBox, DiagonalLine, Circle, Ellipse |

## Is there a way to generate a preview?

Yes, you can test the generated ZPL code via http://labelary.com/viewer.html

## How can I send the generated data to my printer?

For example, the data can be transmitted to the printer IpAddress on port 9100.

```cs
var zplData = @"^XA^MMP^PW300^LS0^LT0^FT10,60^APN,30,30^FH\^FDSAMPLE TEXT^FS^XZ";
ZebraPrinterUtil.PrinterProgrammingLanguage = ProgrammingLanguage.ZPL;
ZebraPrinterUtil.PrinterType = DeviceType.TCP;
ZebraPrinterUtil.TcpIpAddress = "10.10.5.85";
ZebraPrinterUtil.TcpPort = 9100;
ZebraPrinterUtil.PrintCommand(zplData);
```

Also, a Virutal Printer for Zebra is available as [Chrome Plugin](https://chrome.google.com/webstore/detail/zpl-printer/phoidlklenidapnijkabnfdgmadlcmjo)

## Examples

### Using statement

```cs
using DotNet.Util.Zpl.Label;
using DotNet.Util.Zpl.Label.Elements;
```

### Single element

```cs
var output = new ZplGraphicBox(100, 100, 100, 100).ToZplString();
Console.WriteLine(output);
```

### Barcode

```cs
var output = new ZplBarcode128("123ABC", 10, 50).ToZplString();
Console.WriteLine(output);
```

![Barcode 128](doc/preview-barcode128.png)

### Whole label

```cs
var sampleText = "[_~^][LineBreak\n][The quick fox jumps over the lazy dog.]";
var font = new ZplFont(fontWidth: 50, fontHeight: 50);
var elements = new List<ZplElementBase>();
elements.Add(new ZplTextField(sampleText, 50, 100, font));
elements.Add(new ZplGraphicBox(400, 700, 100, 100, 5));
elements.Add(new ZplGraphicBox(450, 750, 100, 100, 50, LineColor.White));
elements.Add(new ZplGraphicCircle(400, 700, 100, 5));
elements.Add(new ZplGraphicDiagonalLine(400, 700, 100, 50, 5));
elements.Add(new ZplGraphicDiagonalLine(400, 700, 50, 100, 5));
elements.Add(new ZplGraphicSymbol(GraphicSymbolCharacter.Copyright, 600, 600, 50, 50));

// Add raw Zpl code
elements.Add(new ZplRaw("^FO200, 200^GB300, 200, 10 ^FS"));

var renderEngine = new ZplEngine(elements);
var output = renderEngine.ToZplString(new ZplRenderOptions { AddEmptyLineBeforeElementStart = true });

Console.WriteLine(output);
```

![Whole label](doc/preview-whole.label.png)

### Simple layout

```cs
var elements = new List<ZplElementBase>();

var origin = new ZplOrigin(100, 100);
for (int i = 0; i < 3; i++)
{
    for (int j = 0; j < 3; j++)
    {
        elements.Add(new ZplGraphicBox(origin.PositionX, origin.PositionY, 50, 50));
        origin = origin.Offset(0, 100);
    }
    origin = origin.Offset(100, -300);
}

var options = new ZplRenderOptions();
var output = new ZplEngine(elements).ToZplString(options);

Console.WriteLine(output);
```
![Simple layout](doc/preview-simple-layout.png)

### Auto scale based on DPI

```cs
var elements = new List<ZplElementBase>();
elements.Add(new ZplGraphicBox(400, 700, 100, 100, 5));

var options = new ZplRenderOptions { SourcePrintDpi = 203, TargetPrintDpi = 300 };
var output = new ZplEngine(elements).ToZplString(options);

Console.WriteLine(output);
```
### Render with comment for easy debugging

```cs
var elements = new List<ZplElementBase>();

var textField = new ZplTextField("AAA", 50, 100, ZplConstants.Font.Default);
textField.Comments.Add("An important field");
elements.Add(textField);

var renderEngine = new ZplEngine(elements);
var output = renderEngine.ToZplString(new ZplRenderOptions { DisplayComments = true });

Console.WriteLine(output);
```

### Different text field type

```cs
var sampleText = "[_~^][LineBreak\n][The quick fox jumps over the lazy dog.]";
var font = new ZplFont(fontWidth: 50, fontHeight: 50);

var elements = new List<ZplElementBase>();
// Special character is repalced with space
elements.Add(new ZplextField(sampleText, 10, 10, font, useHexadecimalIndicator: false));
// Special character is repalced Hex value using ^FH
elements.Add(new ZplTextField(sampleText, 10, 50, font, useHexadecimalIndicator: true));
// Only the first line is displayed
elements.Add(new ZplSingleLineFieldBlock(sampleText, 10, 150, 500, font));
// Max 2 lines, text exceeding the maximum number of lines overwrites the last line.
elements.Add(new ZplFieldBlock(sampleText, 10, 300, 400, font, 2));
// Multi - line text within a box region
elements.Add(new ZplTextBlock(sampleText, 10, 600, 400, 100, font));

var renderEngine = new ZplEngine(elements);
var output = renderEngine.ToZplString(new ZplRenderOptions { AddEmptyLineBeforeElementStart = true });

Console.WriteLine(output);
```

### Draw pictures

> For the best image result, first convert your graphic to black and white. The library auto resize the image based on DPI.

You have 2 possibilities to transfer the graphic to the printer:

#### 1. ZplDownloadObjects (Use ~DY and ^IM)
With this option, the image is sent to the printer in the original graphic format and the printer converts the graphic to a black and white graphic

```cs
var elements = new List<ZplElementBase>();
elements.Add(new ZplDownloadObjects('R', "SAMPLE.BMP", System.IO.File.ReadAllBytes("sample.bmp")));
elements.Add(new ZplImageMove(100, 100, 'R', "SAMPLE", "BMP"));

var renderEngine = new ZplEngine(elements);
var output = renderEngine.ToZplString(new ZplRenderOptions { AddEmptyLineBeforeElementStart = true, TargetPrintDpi = 300, SourcePrintDpi = 200 });

Console.WriteLine(output);
```

#### 2. ZplDownloadGraphics (Use ~DG and ^XG)
With this option, the image is converted from the library into a black and white graphic and the printer already receives the finished print data

```cs
var elements = new List<ZplElementBase>();
elements.Add(new ZplDownloadGraphics('R', "SAMPLE", System.IO.File.ReadAllBytes("sample.bmp")));
elements.Add(new ZplRecallGraphic(100, 100, 'R', "SAMPLE"));

var renderEngine = new ZplEngine(elements);
var output = renderEngine.ToZplString(new ZplRenderOptions { AddEmptyLineBeforeElementStart = true, TargetPrintDpi = 600, SourcePrintDpi = 200 });

Console.WriteLine(output);
```

## Printer manufacturers that support zpl

| Manufacturer | Simulator |
| ------------- | ------------- |
| [Zebra Technologies](https://www.zebra.com) | - |
| [Honeywell International Inc](https://sps.honeywell.com) | ZSIM |
| [Avery Dennison](https://www.averydennison.com) | MLI (Monarch Language Interpreter) |
| [cab Produkttechnik GmbH & Co. KG](https://www.cab.de) | |
| [AirTrack](https://airtrack.com) | |
| [SATO](https://www.satoeurope.com) | SZPL |
| [printronix](https://www.printronix.com) | ZGL |
| [Toshiba Tec](https://www.toshibatec.eu) | |
| [GoDEX](https://www.godexintl.com) | GZPL |
