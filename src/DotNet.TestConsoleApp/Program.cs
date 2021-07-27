using DotNet.Util;
using System;

namespace DotNet.TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            var zplData = @"^XA^MMP^PW300^LS0^LT0^FT10,60^APN,30,30^FH\^FDSAMPLE TEXT^FS^XZ";
            ZebraPrinterUtil.PrinterProgrammingLanguage = ProgrammingLanguage.ZPL;
            ZebraPrinterUtil.PrinterType = DeviceType.TCP;
            ZebraPrinterUtil.TcpIpAddress = "10.10.5.85";
            ZebraPrinterUtil.TcpPort = 9100;
            ZebraPrinterUtil.PrintCommand(zplData);
            Console.WriteLine("End");
        }
    }
}
