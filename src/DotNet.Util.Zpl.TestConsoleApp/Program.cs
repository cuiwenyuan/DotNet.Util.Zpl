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
            //ZebraPrinterUtil.PrinterProgrammingLanguage = ProgrammingLanguage.ZPL;
            //Print to server
            ZebraPrinterUtil.PrintWithTCP(zplData, "10.10.5.85", 9100, true);
            //Print to printer name - Zebra Z401
            ZebraPrinterUtil.PrintWithDRV(zplData, "Zebra Z410", true);
            // Print to LTP1
            ZebraPrinterUtil.PrintWithLPT(zplData, 1, true);
            // Print to COM1
            ZebraPrinterUtil.PrintWithCOM(zplData, 1, true);
            Console.WriteLine("End");
        }
    }
}
