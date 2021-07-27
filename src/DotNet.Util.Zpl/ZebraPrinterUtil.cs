using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace DotNet.Util
{
    /// <summary>
    /// 斑马打印助手，支持LPT/COM/USB/TCP四种模式，适用于标签、票据、条码打印。
    /// </summary>
    public static class ZebraPrinterUtil
    {
        #region 定义API方法

        #region 写打印口(LPT)方法
        private const short FILE_ATTRIBUTE_NORMAL = 0x80;
        private const short INVALID_HANDLE_VALUE = -1;
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint CREATE_NEW = 1;
        private const uint CREATE_ALWAYS = 2;
        private const uint OPEN_EXISTING = 3;
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(string strFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr intptrSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr intptrTemplateFile);
        #endregion

        /// <summary>
        /// OpenPrinter
        /// </summary>
        /// <param name="printerName"></param>
        /// <param name="intptrPrinter"></param>
        /// <param name="intptrPrintDocument"></param>
        /// <returns></returns>
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]

        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string printerName, out IntPtr intptrPrinter, IntPtr intptrPrintDocument);

        /// <summary>
        /// ClosePrinter
        /// </summary>
        /// <param name="intptrPrinter"></param>
        /// <returns></returns>
        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr intptrPrinter);

        /// <summary>
        /// StartDocPrinter
        /// </summary>
        /// <param name="intptrPrinter"></param>
        /// <param name="level"></param>
        /// <param name="docInfo"></param>
        /// <returns></returns>
        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr intptrPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DocInfo docInfo);

        /// <summary>
        /// EndDocPrinter
        /// </summary>
        /// <param name="intptrPrinter"></param>
        /// <returns></returns>
        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr intptrPrinter);

        /// <summary>
        /// StartPagePrinter
        /// </summary>
        /// <param name="intptrPrinter"></param>
        /// <returns></returns>
        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr intptrPrinter);

        /// <summary>
        /// EndPagePrinter
        /// </summary>
        /// <param name="intptrPrinter"></param>
        /// <returns></returns>
        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr intptrPrinter);

        /// <summary>
        /// WritePrinter
        /// </summary>
        /// <param name="intptrPrinter"></param>
        /// <param name="intptrBytes"></param>
        /// <param name="count"></param>
        /// <param name="written"></param>
        /// <returns></returns>
        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr intptrPrinter, IntPtr intptrBytes, Int32 count, out Int32 written);

        #endregion

        #region 定义私有字段  
        /// <summary>  
        /// 线程锁，防止多线程调用。  
        /// </summary>  
        private static object SyncRoot = new object();
        /// <summary>  
        /// ZPL压缩字典  
        /// </summary>  
        private static List<KeyValue> compressDictionary = new List<KeyValue>();
        #endregion

        #region 定义属性
        /// <summary>
        /// TCP标签最大高度CM
        /// </summary>
        public static float TcpLabelMaxHeightCM { get; set; }
        /// <summary>
        /// TCP打印机的分辨率
        /// </summary>
        public static int TcpPrinterDPI { get; set; }
        /// <summary>
        /// TCP打印机的IP地址
        /// </summary>
        public static string TcpIpAddress { get; set; }
        /// <summary>
        /// TCP端口号（默认9100）
        /// </summary>
        public static int TcpPort { get; set; } = 9100;
        /// <summary>
        /// 分数
        /// </summary>
        public static int Copies { get; set; }
        /// <summary>
        /// 端口号
        /// </summary>
        public static int Port { get; set; }
        /// <summary>
        /// 打印机名称
        /// </summary>
        public static string PrinterName { get; set; }
        /// <summary>
        /// 是否写日志
        /// </summary>
        public static bool IsWriteLog { get; set; }
        /// <summary>
        /// 打印机类型
        /// </summary>
        public static DeviceType PrinterType { get; set; }
        /// <summary>
        /// 打印机编程语言
        /// </summary>
        public static ProgrammingLanguage PrinterProgrammingLanguage { get; set; }
        /// <summary>  
        /// 日志保存目录，WEB应用注意不能放在BIN目录下。  
        /// </summary>  
        public static string LogsDirectory { get; set; }
        /// <summary>
        /// 图形缓存
        /// </summary>
        public static byte[] GraphBuffer { get; set; }
        /// <summary>
        /// 图形宽度
        /// </summary>
        private static int GraphWidth { get; set; }
        /// <summary>
        /// 图形高度
        /// </summary>
        private static int GraphHeight { get; set; }
        /// <summary>
        /// 行数
        /// </summary>
        private static int RowSize
        {
            get
            {
                return (((GraphWidth) + 31) >> 5) << 2;
            }
        }
        private static int RowRealBytesCount
        {
            get
            {
                if ((GraphWidth % 8) > 0)
                {
                    return GraphWidth / 8 + 1;
                }
                else
                {
                    return GraphWidth / 8;
                }
            }
        }
        #endregion

        #region 定义发送原始数据到打印机的方法
        private static bool SendBytesToPrinter(string printerName, IntPtr intptrBytes, Int32 count)
        {
            Int32 error = 0, written = 0;
            IntPtr intptrPrinter = new IntPtr(0);
            DocInfo docInfo = new DocInfo();
            bool bSuccess = false;

            docInfo.DocName = ".NET RAW Document";
            docInfo.DataType = "RAW";

            // Open the printer.  
            if (OpenPrinter(printerName.Normalize(), out intptrPrinter, IntPtr.Zero))
            {
                // Start a document.  
                if (StartDocPrinter(intptrPrinter, 1, docInfo))
                {
                    // Start a page.  
                    if (StartPagePrinter(intptrPrinter))
                    {
                        // Write your bytes.  
                        bSuccess = WritePrinter(intptrPrinter, intptrBytes, count, out written);
                        EndPagePrinter(intptrPrinter);
                    }
                    EndDocPrinter(intptrPrinter);
                }
                ClosePrinter(intptrPrinter);
            }
            // If you did not succeed, GetLastError may give more information  
            // about why not.  
            if (bSuccess == false)
            {
                error = Marshal.GetLastWin32Error();
            }
            return bSuccess;
        }
        /// <summary>
        /// SendFileToPrinter
        /// </summary>
        /// <param name="printerName">Printer Name</param>
        /// <param name="fileName">File Name</param>
        /// <returns></returns>
        public static bool SendFileToPrinter(string printerName, string fileName)
        {
            // Open the file.  
            FileStream fs = new FileStream(fileName, FileMode.Open);
            // Create a BinaryReader on the file.  
            BinaryReader br = new BinaryReader(fs);
            // Dim an array of bytes big enough to hold the file's contents.  
            Byte[] bytes = new Byte[fs.Length];
            bool bSuccess = false;
            // Your unmanaged pointer.  
            IntPtr pUnmanagedBytes = new IntPtr(0);
            int nLength;

            nLength = Convert.ToInt32(fs.Length);
            // Read the contents of the file into the array.  
            bytes = br.ReadBytes(nLength);
            // Allocate some unmanaged memory for those bytes.  
            pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
            // Copy the managed byte array into the unmanaged array.  
            Marshal.Copy(bytes, 0, pUnmanagedBytes, nLength);
            // Send the unmanaged bytes to the printer.  
            bSuccess = SendBytesToPrinter(printerName, pUnmanagedBytes, nLength);
            // Free the unmanaged memory that you allocated earlier.  
            Marshal.FreeCoTaskMem(pUnmanagedBytes);
            return bSuccess;
        }
        /// <summary>
        /// SendBytesToPrinter
        /// </summary>
        /// <param name="printerName">Printer Name</param>
        /// <param name="bytes">Bytes</param>
        /// <returns></returns>
        public static bool SendBytesToPrinter(string printerName, byte[] bytes)
        {
            bool bSuccess = false;
            IntPtr pUnmanagedBytes = new IntPtr(0);
            int nLength = bytes.Length;
            // Allocate some unmanaged memory for those bytes.  
            pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
            // Copy the managed byte array into the unmanaged array.  
            Marshal.Copy(bytes, 0, pUnmanagedBytes, nLength);
            // Send the unmanaged bytes to the printer.  
            bSuccess = SendBytesToPrinter(printerName, pUnmanagedBytes, nLength);
            // Free the unmanaged memory that you allocated earlier.  
            Marshal.FreeCoTaskMem(pUnmanagedBytes);
            return bSuccess;
        }
        /// <summary>
        /// SendStringToPrinter
        /// </summary>
        /// <param name="printerName">Printer Name</param>
        /// <param name="text">Text</param>
        /// <returns></returns>
        public static bool SendStringToPrinter(string printerName, string text)
        {
            IntPtr pBytes;
            Int32 dwCount;
            // How many characters are in the string?  
            dwCount = (text.Length + 1) * Marshal.SystemMaxDBCSCharSize;
            // Assume that the printer is expecting ANSI text, and then convert  
            // the string to ANSI text.  
            pBytes = Marshal.StringToCoTaskMemAnsi(text);
            // Send the converted ANSI string to the printer.  
            SendBytesToPrinter(printerName, pBytes, dwCount);
            Marshal.FreeCoTaskMem(pBytes);
            return true;
        }
        #endregion

        #region 静态构造方法  
        static ZebraPrinterUtil()
        {
            initCompressCode();
            Port = 1;
            GraphBuffer = new byte[0];
            IsWriteLog = false;
            LogsDirectory = "logs";
            PrinterProgrammingLanguage = ProgrammingLanguage.ZPL;
        }

        private static void initCompressCode()
        {
            //G H I J K L M N O P Q R S T U V W X Y        对应1,2,3,4……18,19。  
            //g h i j k l m n o p q r s t u v w x y z      对应20,40,60,80……340,360,380,400。              
            for (int i = 0; i < 19; i++)
            {
                compressDictionary.Add(new KeyValue() { Key = Convert.ToChar(71 + i), Value = i + 1 });
            }
            for (int i = 0; i < 20; i++)
            {
                compressDictionary.Add(new KeyValue() { Key = Convert.ToChar(103 + i), Value = (i + 1) * 20 });
            }
        }
        #endregion

        #region 日志记录方法  
        private static void WriteLog(string text, LogType logType)
        {
            string endTag = string.Format("\r\n{0}\r\n", new string('=', 80));
            string path = string.Format("{0}\\{1}-{2}.log", LogsDirectory, DateTime.Now.ToString("yyyy-MM-dd"), logType);
            if (!Directory.Exists(LogsDirectory))
            {
                Directory.CreateDirectory(LogsDirectory);
            }
            if (logType == LogType.Error)
            {
                File.AppendAllText(path, string.Format("{0}{1}", text, endTag), Encoding.UTF8);
            }
            if (logType == LogType.Print)
            {
                File.AppendAllText(path, string.Format("{0}{1}", text, endTag), Encoding.UTF8);
            }
        }

        private static void WriteLog(byte[] bytes, LogType logType)
        {
            string endTag = string.Format("\r\n{0}\r\n", new string('=', 80));
            string path = string.Format("{0}\\{1}-{2}.log", LogsDirectory, DateTime.Now.ToString("yyyy-MM-dd"), logType);
            if (!Directory.Exists(LogsDirectory))
            {
                Directory.CreateDirectory(LogsDirectory);
            }
            if (logType == LogType.Error)
            {
                File.AppendAllText(path, string.Format("{0}{1}", Encoding.UTF8.GetString(bytes), endTag), Encoding.UTF8);
            }
            if (logType == LogType.Print)
            {
                File.AppendAllText(path, string.Format("{0}{1}", Encoding.UTF8.GetString(bytes), endTag), Encoding.UTF8);
            }
        }
        #endregion

        #region 封装方法，方便调用。  
        /// <summary>
        /// PrintWithCOM
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="port"></param>
        /// <param name="isWriteLog"></param>
        /// <returns></returns>
        public static bool PrintWithCOM(string cmd, int port, bool isWriteLog)
        {
            PrinterType = DeviceType.COM;
            Port = port;
            IsWriteLog = isWriteLog;
            return PrintCommand(cmd);
        }
        /// <summary>
        /// PrintWithCOM
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="port"></param>
        /// <param name="isWriteLog"></param>
        /// <returns></returns>
        public static bool PrintWithCOM(byte[] bytes, int port, bool isWriteLog)
        {
            PrinterType = DeviceType.COM;
            Port = port;
            IsWriteLog = isWriteLog;
            return PrintGraphics(bytes);
        }
        /// <summary>
        /// PrintWithLPT
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="port"></param>
        /// <param name="isWriteLog"></param>
        /// <returns></returns>
        public static bool PrintWithLPT(string cmd, int port, bool isWriteLog)
        {
            PrinterType = DeviceType.LPT;
            Port = port;
            IsWriteLog = isWriteLog;
            return PrintCommand(cmd);
        }
        /// <summary>
        /// PrintWithLPT
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="port"></param>
        /// <param name="isWriteLog"></param>
        /// <returns></returns>
        public static bool PrintWithLPT(byte[] bytes, int port, bool isWriteLog)
        {
            PrinterType = DeviceType.LPT;
            Port = port;
            IsWriteLog = isWriteLog;
            return PrintGraphics(bytes);
        }
        /// <summary>
        /// PrintWithTCP
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="isWriteLog"></param>
        /// <returns></returns>
        public static bool PrintWithTCP(string cmd, bool isWriteLog)
        {
            PrinterType = DeviceType.TCP;
            IsWriteLog = isWriteLog;
            return PrintCommand(cmd);
        }
        /// <summary>
        /// PrintWithTCP
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="isWriteLog"></param>
        /// <returns></returns>
        public static bool PrintWithTCP(byte[] bytes, bool isWriteLog)
        {
            PrinterType = DeviceType.TCP;
            IsWriteLog = isWriteLog;
            return PrintGraphics(bytes);
        }
        /// <summary>
        /// PrintWithDRV
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="printerName"></param>
        /// <param name="isWriteLog"></param>
        /// <returns></returns>
        public static bool PrintWithDRV(string cmd, string printerName, bool isWriteLog)
        {
            PrinterType = DeviceType.DRV;
            PrinterName = printerName;
            IsWriteLog = isWriteLog;
            return PrintCommand(cmd);
        }
        /// <summary>
        /// PrintWithDRV
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="printerName"></param>
        /// <param name="isWriteLog"></param>
        /// <returns></returns>
        public static bool PrintWithDRV(byte[] bytes, string printerName, bool isWriteLog)
        {
            PrinterType = DeviceType.DRV;
            PrinterName = printerName;
            IsWriteLog = isWriteLog;
            return PrintGraphics(bytes);
        }
        #endregion

        #region 打印ZPL、EPL、CPCL、TCP指令
        /// <summary>
        /// PrintCommand
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static bool PrintCommand(string cmd)
        {
            lock (SyncRoot)
            {
                bool result = false;
                try
                {
                    switch (PrinterType)
                    {
                        case DeviceType.COM:
                            result = comPrint(Encoding.Default.GetBytes(cmd));
                            break;
                        case DeviceType.LPT:
                            result = lptPrint(Encoding.Default.GetBytes(cmd));
                            break;
                        case DeviceType.DRV:
                            result = drvPrint(Encoding.Default.GetBytes(cmd));
                            break;
                        case DeviceType.TCP:
                            result = tcpPrint(Encoding.Default.GetBytes(cmd));
                            break;
                    }
                    if (!string.IsNullOrEmpty(cmd) && IsWriteLog)
                    {
                        WriteLog(cmd, LogType.Print);
                    }
                }
                catch (Exception ex)
                {
                    //记录日志  
                    if (IsWriteLog)
                    {
                        WriteLog(string.Format("{0} => {1}\r\n{2}", DateTime.Now, ex.Message, ex), LogType.Error);
                    }
                    throw ex;
                }
                finally
                {
                    GraphBuffer = new byte[0];
                }
                return result;
            }
        }
        #endregion

        #region 打印图像
        /// <summary>
        /// PrintGraphics
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public static bool PrintGraphics(byte[] graph)
        {
            lock (SyncRoot)
            {
                bool result = false;
                try
                {
                    GraphBuffer = graph;
                    byte[] cmdBytes = new byte[0];
                    switch (PrinterProgrammingLanguage)
                    {
                        case ProgrammingLanguage.ZPL:
                            cmdBytes = getZPLBytes();
                            break;
                        case ProgrammingLanguage.EPL:
                            cmdBytes = getEPLBytes();
                            break;
                        case ProgrammingLanguage.CPCL:
                            cmdBytes = getCPCLBytes();
                            break;
                    }
                    switch (PrinterType)
                    {
                        case DeviceType.COM:
                            result = comPrint(cmdBytes);
                            break;
                        case DeviceType.LPT:
                            result = lptPrint(cmdBytes);
                            break;
                        case DeviceType.DRV:
                            result = drvPrint(cmdBytes);
                            break;
                        case DeviceType.TCP:
                            result = tcpPrint(cmdBytes);
                            break;
                    }
                    if (cmdBytes.Length > 0 && IsWriteLog)
                    {
                        WriteLog(cmdBytes, LogType.Print);
                    }
                }
                catch (Exception ex)
                {
                    //记录日志  
                    if (IsWriteLog)
                    {
                        WriteLog(string.Format("{0} => {1}\r\n{2}", DateTime.Now, ex.Message, ex), LogType.Error);
                    }
                    throw ex;
                }
                finally
                {
                    GraphBuffer = new byte[0];
                }
                return result;
            }
        }
        #endregion

        #region COM/LPT/DRV三种模式打印方法 
        private static bool drvPrint(byte[] cmdBytes)
        {
            bool result = false;
            try
            {
                if (!string.IsNullOrEmpty(PrinterName))
                {
                    result = SendBytesToPrinter(PrinterName, cmdBytes);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        private static bool comPrint(byte[] cmdBytes)
        {
            bool result = false;
            SerialPort com = new SerialPort(string.Format("{0}{1}", PrinterType, Port), 9600, Parity.None, 8, StopBits.One);
            try
            {
                com.Open();
                com.Write(cmdBytes, 0, cmdBytes.Length);
                result = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (com.IsOpen)
                {
                    com.Close();
                }
            }
            return result;
        }

        private static bool lptPrint(byte[] cmdBytes)
        {
            bool result = false;
            FileStream fileStream = null;
            StreamWriter streamWriter = null;
            SafeFileHandle handle = null;
            try
            {
                handle = CreateFile(string.Format("{0}{1}", PrinterType, Port), GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                if (!handle.IsInvalid)
                {
                    fileStream = new FileStream(handle, FileAccess.ReadWrite);
                    streamWriter = new StreamWriter(fileStream, Encoding.Default);
                    streamWriter.Write(cmdBytes);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream = null;
                }
                if (streamWriter != null)
                {
                    streamWriter.Close();
                    streamWriter = null;
                }
                if (handle != null)
                {
                    handle.Close();
                    handle = null;
                }
            }
            return result;

        }

        private static bool tcpPrint(byte[] cmdBytes)
        {
            bool result = false;
            TcpClient tcp = new System.Net.Sockets.TcpClient();
            try
            {
                tcp.Connect(TcpIpAddress, TcpPort);
                tcp.SendTimeout = 1000;
                tcp.ReceiveTimeout = 1000;
                if (tcp.Connected)
                {
                    tcp.Client.Send(cmdBytes);
                    //// Send Zpl data to printer
                    //var writer = new System.IO.StreamWriter(tcp.GetStream());
                    //writer.Write(zplData);
                    //writer.Flush();
                    //// Close Connection
                    //writer.Close();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("打印失败，请检查打印机或网络设置。", ex);
            }
            finally
            {
                if (tcp != null)
                {
                    if (tcp.Client != null)
                    {
                        tcp.Client.Close();
                        tcp.Client = null;
                    }
                    tcp.Close();
                    tcp = null;
                }
            }
            return result;
        }
        #endregion

        #region 生成ZPL图像打印指令  
        private static byte[] getZPLBytes()
        {
            byte[] bmpData = getBitmapData();
            int bmpDataLength = bmpData.Length;
            for (int i = 0; i < bmpDataLength; i++)
            {
                bmpData[i] ^= 0xFF;
            }
            string textBitmap = string.Empty, copiesString = string.Empty;
            string textHex = BitConverter.ToString(bmpData).Replace("-", string.Empty);
            textBitmap = CompressLZ77(textHex);
            for (int i = 0; i < Copies; i++)
            {
                copiesString += "^XA^FO0,0^XGR:IMAGE.GRF,1,1^FS^XZ";
            }
            string text = string.Format("~DGR:IMAGE.GRF,{0},{1},{2}{3}^IDR:IMAGE.GRF",
                GraphHeight * RowRealBytesCount,
                RowRealBytesCount,
                textBitmap,
                copiesString);
            return Encoding.UTF8.GetBytes(text);
        }
        #endregion

        #region 生成EPL图像打印指令  
        private static byte[] getEPLBytes()
        {
            byte[] buffer = getBitmapData();
            string text = string.Format("N\r\nGW{0},{1},{2},{3},{4}\r\nP{5},1\r\n",
                0,
                0,
                RowRealBytesCount,
                GraphHeight,
                Encoding.GetEncoding("iso-8859-1").GetString(buffer),
                Copies);
            return Encoding.GetEncoding("iso-8859-1").GetBytes(text);
        }
        #endregion

        #region 生成CPCL图像打印指令
        /// <summary>
        /// getCPCLBytes
        /// </summary>
        /// <returns></returns>
        public static byte[] getCPCLBytes()
        {
            //GRAPHICS Commands  
            //Bit-mapped graphics can be printed by using graphics commands. ASCII hex (hexadecimal) is  
            //used for expanded graphics data (see example). Data size can be reduced to one-half by utilizing the  
            //COMPRESSED-GRAPHICS commands with the equivalent binary character(s) of the hex data. When  
            //using CG, a single 8 bit character is sent for every 8 bits of graphics data. When using EG two characters  
            //(16 bits) are used to transmit 8 bits of graphics data, making EG only half as efficient. Since this data is  
            //character data, however, it can be easier to handle and transmit than binary data.  
            //Format:  
            //{command} {width} {height} {x} {y} {data}  
            //where:  
            //{command}: Choose from the following:  
            //EXPANDED-GRAPHICS (or EG): Prints expanded graphics horizontally.  
            //VEXPANDED-GRAPHICS (or VEG): Prints expanded graphics vertically.  
            //COMPRESSED-GRAPHICS (or CG): Prints compressed graphics horizontally.  
            //VCOMPRESSED-GRAPHICS (or VCG): Prints compressed graphics vertically.  
            //{width}: Byte-width of image.  
            //{height} Dot-height of image.  
            //{x}: Horizontal starting position.  
            //{y}: Vertical starting position.  
            //{data}: Graphics data.  
            //Graphics command example  
            //Input:  
            //! 0 200 200 210 1  
            //EG 2 16 90 45 F0F0F0F0F0F0F0F00F0F0F0F0F0F0F0F  
            //F0F0F0F0F0F0F0F00F0F0F0F0F0F0F0F  
            //FORM  
            //PRINT  

            byte[] bmpData = getBitmapData();
            int bmpDataLength = bmpData.Length;
            for (int i = 0; i < bmpDataLength; i++)
            {
                bmpData[i] ^= 0xFF;
            }
            string textHex = BitConverter.ToString(bmpData).Replace("-", string.Empty);
            string text = string.Format("! {0} {1} {2} {3} {4}\r\nEG {5} {6} {7} {8} {9}\r\nFORM\r\nPRINT\r\n",
                0, //水平偏移量  
                TcpPrinterDPI, //横向DPI  
                TcpPrinterDPI, //纵向DPI  
                (int)(TcpLabelMaxHeightCM / 2.54f * TcpPrinterDPI), //标签最大像素高度=DPI*标签纸高度(英寸)  
                Copies, //份数  
                RowRealBytesCount, //图像的字节宽度  
                GraphHeight, //图像的像素高度  
                0, //横向的开始位置  
                0, //纵向的开始位置  
                textHex
                );
            return Encoding.UTF8.GetBytes(text);
        }
        #endregion

        #region 获取单色位图数据  
        /// <summary>  
        ///   
        /// </summary>  
        /// <param name="pimage"></param>  
        /// <returns></returns>  
        public static Bitmap ConvertToGrayscale(Bitmap pimage)
        {
            Bitmap source = null;

            // If original bitmap is not already in 32 BPP, ARGB format, then convert  
            if (pimage.PixelFormat != PixelFormat.Format32bppArgb)
            {
                source = new Bitmap(pimage.Width, pimage.Height, PixelFormat.Format32bppArgb);
                source.SetResolution(pimage.HorizontalResolution, pimage.VerticalResolution);
                using (Graphics g = Graphics.FromImage(source))
                {
                    g.DrawImageUnscaled(pimage, 0, 0);
                }
            }
            else
            {
                source = pimage;
            }

            // Lock source bitmap in memory  
            BitmapData sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            // Copy image data to binary array  
            int imageSize = sourceData.Stride * sourceData.Height;
            byte[] sourceBuffer = new byte[imageSize];
            Marshal.Copy(sourceData.Scan0, sourceBuffer, 0, imageSize);

            // Unlock source bitmap  
            source.UnlockBits(sourceData);

            // Create destination bitmap  
            Bitmap destination = new Bitmap(source.Width, source.Height, PixelFormat.Format1bppIndexed);

            // Lock destination bitmap in memory  
            BitmapData destinationData = destination.LockBits(new Rectangle(0, 0, destination.Width, destination.Height), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);

            // Create destination buffer  
            imageSize = destinationData.Stride * destinationData.Height;
            byte[] destinationBuffer = new byte[imageSize];

            int sourceIndex = 0;
            int destinationIndex = 0;
            int pixelTotal = 0;
            byte destinationValue = 0;
            int pixelValue = 128;
            int height = source.Height;
            int width = source.Width;
            int threshold = 500;

            // Iterate lines  
            for (int y = 0; y < height; y++)
            {
                sourceIndex = y * sourceData.Stride;
                destinationIndex = y * destinationData.Stride;
                destinationValue = 0;
                pixelValue = 128;

                // Iterate pixels  
                for (int x = 0; x < width; x++)
                {
                    // Compute pixel brightness (i.e. total of Red, Green, and Blue values)  
                    pixelTotal = sourceBuffer[sourceIndex + 1] + sourceBuffer[sourceIndex + 2] + sourceBuffer[sourceIndex + 3];
                    if (pixelTotal > threshold)
                    {
                        destinationValue += (byte)pixelValue;
                    }
                    if (pixelValue == 1)
                    {
                        destinationBuffer[destinationIndex] = destinationValue;
                        destinationIndex++;
                        destinationValue = 0;
                        pixelValue = 128;
                    }
                    else
                    {
                        pixelValue >>= 1;
                    }
                    sourceIndex += 4;
                }
                if (pixelValue != 128)
                {
                    destinationBuffer[destinationIndex] = destinationValue;
                }
            }

            // Copy binary image data to destination bitmap  
            Marshal.Copy(destinationBuffer, 0, destinationData.Scan0, imageSize);

            // Unlock destination bitmap  
            destination.UnlockBits(destinationData);

            // Dispose of source if not originally supplied bitmap  
            if (source != pimage)
            {
                source.Dispose();
            }

            // Return  
            return destination;
        }
        /// <summary>  
        /// 获取单色位图数据(1bpp)，不含文件头、信息头、调色板三类数据。  
        /// </summary>  
        /// <returns></returns>  
        private static byte[] getBitmapData()
        {
            MemoryStream srcStream = new MemoryStream();
            MemoryStream dstStream = new MemoryStream();
            Bitmap srcBmp = null;
            Bitmap dstBmp = null;
            byte[] srcBuffer = null;
            byte[] dstBuffer = null;
            byte[] result = null;
            try
            {
                srcStream = new MemoryStream(GraphBuffer);
                srcBmp = Bitmap.FromStream(srcStream) as Bitmap;
                srcBuffer = srcStream.ToArray();
                GraphWidth = srcBmp.Width;
                GraphHeight = srcBmp.Height;
                //dstBmp = srcBmp.Clone(new Rectangle(0, 0, srcBmp.Width, srcBmp.Height), PixelFormat.Format1bppIndexed);  
                dstBmp = ConvertToGrayscale(srcBmp);
                dstBmp.Save(dstStream, ImageFormat.Bmp);
                dstBuffer = dstStream.ToArray();

                int bfOffBits = BitConverter.ToInt32(dstBuffer, 10);
                result = new byte[GraphHeight * RowRealBytesCount];

                //读取时需要反向读取每行字节实现上下翻转的效果，打印机打印顺序需要这样读取。  
                for (int i = 0; i < GraphHeight; i++)
                {
                    Array.Copy(dstBuffer, bfOffBits + (GraphHeight - 1 - i) * RowSize, result, i * RowRealBytesCount, RowRealBytesCount);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (srcStream != null)
                {
                    srcStream.Dispose();
                    srcStream = null;
                }
                if (dstStream != null)
                {
                    dstStream.Dispose();
                    dstStream = null;
                }
                if (srcBmp != null)
                {
                    srcBmp.Dispose();
                    srcBmp = null;
                }
                if (dstBmp != null)
                {
                    dstBmp.Dispose();
                    dstBmp = null;
                }
            }
            return result;
        }
        #endregion

        #region LZ77图像字节流压缩方法
        /// <summary>
        /// LZ77压缩
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string CompressLZ77(string text)
        {
            //将转成16进制的文本进行压缩  
            string result = string.Empty;
            char[] arrChar = text.ToCharArray();
            int count = 1;
            for (int i = 1; i < text.Length; i++)
            {
                if (arrChar[i - 1] == arrChar[i])
                {
                    count++;
                }
                else
                {
                    result += convertNumber(count) + arrChar[i - 1];
                    count = 1;
                }
                if (i == text.Length - 1)
                {
                    result += convertNumber(count) + arrChar[i];
                }
            }
            return result;
        }
        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string DecompressLZ77(string text)
        {
            string result = string.Empty;
            char[] arrChar = text.ToCharArray();
            int count = 0;
            for (int i = 0; i < arrChar.Length; i++)
            {
                if (isHexChar(arrChar[i]))
                {
                    //十六进制值  
                    result += new string(arrChar[i], count == 0 ? 1 : count);
                    count = 0;
                }
                else
                {
                    //压缩码  
                    int value = GetCompressValue(arrChar[i]);
                    count += value;
                }
            }
            return result;
        }

        private static int GetCompressValue(char c)
        {
            int result = 0;
            for (int i = 0; i < compressDictionary.Count; i++)
            {
                if (c == compressDictionary[i].Key)
                {
                    result = compressDictionary[i].Value;
                }
            }
            return result;
        }

        private static bool isHexChar(char c)
        {
            return c > 47 && c < 58 || c > 64 && c < 71 || c > 96 && c < 103;
        }

        private static string convertNumber(int count)
        {
            //将连续的数字转换成LZ77压缩代码，如000可用I0表示。  
            string result = string.Empty;
            if (count > 1)
            {
                while (count > 0)
                {
                    for (int i = compressDictionary.Count - 1; i >= 0; i--)
                    {
                        if (count >= compressDictionary[i].Value)
                        {
                            result += compressDictionary[i].Key;
                            count -= compressDictionary[i].Value;
                            break;
                        }
                    }
                }
            }
            return result;
        }
        #endregion
    }

    #region 自定义数据类
    /// <summary>
    /// 自定义数据类
    /// </summary>
    internal class KeyValue
    {
        public char Key { get; set; }
        public int Value { get; set; }
    }
    #endregion

    #region 打印机编程语言
    /// <summary>
    /// 打印机编程语言
    /// </summary>
    public enum ProgrammingLanguage
    {
        /// <summary>
        /// ZPL
        /// </summary>
        ZPL = 0,
        /// <summary>
        /// EPL
        /// </summary>
        EPL = 1,
        /// <summary>
        /// CPCL
        /// </summary>
        CPCL = 2
    }
    #endregion

    #region 设备类型
    /// <summary>
    /// 设备类型
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// COM
        /// </summary>
        COM,
        /// <summary>
        /// LPT
        /// </summary>
        LPT,
        /// <summary>
        /// TCP
        /// </summary>
        TCP,
        /// <summary>
        /// DRV
        /// </summary>
        DRV
    }
    #endregion

    #region 日志类型
    /// <summary>
    /// 日志类型
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// Error
        /// </summary>
        Error,
        /// <summary>
        /// Print
        /// </summary>
        Print
    }
    #endregion

    #region 打印文档信息类
    /// <summary>
    /// DocInfo
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class DocInfo
    {
        /// <summary>
        /// DocName
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string DocName;
        /// <summary>
        /// OutputFile
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string OutputFile;
        /// <summary>
        /// DataType
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string DataType;
    }
    #endregion
}
