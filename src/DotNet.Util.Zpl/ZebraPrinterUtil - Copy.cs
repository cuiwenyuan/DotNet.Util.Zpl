using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace DotNet.Util
{
    /// <summary>
    /// 斑马打印助手，支持LPT/COM/USB/TCP四种模式，适用于标签、票据、条码打印。
    /// </summary>
    public static class ZebraPrintHelper
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

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string printerName, out IntPtr intptrPrinter, IntPtr intptrPrintDocument);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr intptrPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr intptrPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DocInfo docInfo);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr intptrPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr intptrPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr intptrPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr intptrPrinter, IntPtr intptrBytes, Int32 count, out Int32 written);
        #endregion

        #region 定义私有字段

        /// <summary>  
        /// 线程锁，防止多线程调用。  
        /// </summary>  
        private static object SyncRoot = new object();

        /// <summary>  
        /// 字节流传递时采用的字符编码  
        /// </summary>  
        private static readonly Encoding TransferFormat = Encoding.GetEncoding("iso-8859-1");

        #endregion

        #region 定义属性
        public static int Port { get; set; }
        public static string PrinterName { get; set; }
        public static bool IsWriteLog { get; set; }
        public static DeviceType PrinterType { get; set; }
        public static ProgrammingLanguage PrinterProgrammingLanguage { get; set; }

        public static float TcpLabelMaxHeightCM { get; set; }
        public static int TcpPrinterDPI { get; set; }
        public static string TcpIpAddress { get; set; }
        public static int TcpPort { get; set; }
        public static int Copies { get; set; }

        /// <summary>  
        /// 日志保存目录，WEB应用注意不能放在BIN目录下。  
        /// </summary>  
        public static string LogsDirectory { get; set; }

        private static byte[] GraphBuffer { get; set; }
        private static int GraphWidth { get; set; }
        private static int GraphHeight { get; set; }

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

        #region 静态构造方法
        static ZebraPrintHelper()
        {
            GraphBuffer = new byte[0];
            IsWriteLog = false;
            LogsDirectory = "logs";
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
                File.AppendAllText(path, string.Format("{0}{1}", text, endTag), Encoding.Default);
            }
            if (logType == LogType.Print)
            {
                if (text.StartsWith("N\r\nGW"))
                {
                    using (FileStream fs = new FileStream(path, FileMode.Append))
                    {
                        byte[] bytes = TransferFormat.GetBytes(text);
                        byte[] tag = TransferFormat.GetBytes(endTag);
                        fs.Write(bytes, 0, bytes.Length);
                        fs.Write(tag, 0, tag.Length);
                        fs.Close();
                    }
                }
                else
                {
                    File.AppendAllText(path, string.Format("{0}{1}", text, endTag), Encoding.Default);
                }
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
                File.AppendAllText(path, string.Format("{0}{1}", Encoding.Default.GetString(bytes), endTag), Encoding.Default);
            }
            if (logType == LogType.Print)
            {
                string transferFormat = TransferFormat.GetString(bytes);
                if (transferFormat.StartsWith("N\r\nGW"))
                {
                    using (FileStream fs = new FileStream(path, FileMode.Append))
                    {
                        byte[] tag = TransferFormat.GetBytes(endTag);
                        fs.Write(bytes, 0, bytes.Length);
                        fs.Write(tag, 0, tag.Length);
                        fs.Close();
                    }
                }
                else
                {
                    File.AppendAllText(path, string.Format("{0}{1}", Encoding.Default.GetString(bytes), endTag), Encoding.Default);
                }
            }
        }
        #endregion  

        #region 封装方法，方便调用。
        public static bool PrintWithCOM(string cmd, int port, bool isWriteLog)
        {
            PrinterType = DeviceType.COM;
            Port = port;
            IsWriteLog = isWriteLog;
            return PrintCommand(cmd);
        }

        public static bool PrintWithCOM(byte[] bytes, int port, bool isWriteLog, ProgrammingLanguage progLanguage)
        {
            PrinterType = DeviceType.COM;
            Port = port;
            IsWriteLog = isWriteLog;
            PrinterProgrammingLanguage = progLanguage;
            return PrintGraphics(bytes);
        }

        public static bool PrintWithLPT(string cmd, int port, bool isWriteLog)
        {
            PrinterType = DeviceType.LPT;
            Port = port;
            IsWriteLog = isWriteLog;
            return PrintCommand(cmd);
        }

        public static bool PrintWithLPT(byte[] bytes, int port, bool isWriteLog, ProgrammingLanguage progLanguage)
        {
            PrinterType = DeviceType.LPT;
            Port = port;
            IsWriteLog = isWriteLog;
            PrinterProgrammingLanguage = progLanguage;
            return PrintGraphics(bytes);
        }

        public static bool PrintWithDRV(string cmd, string printerName, bool isWriteLog)
        {
            PrinterType = DeviceType.DRV;
            PrinterName = printerName;
            IsWriteLog = isWriteLog;
            return PrintCommand(cmd);
        }

        public static bool PrintWithDRV(byte[] bytes, string printerName, bool isWriteLog, ProgrammingLanguage progLanguage)
        {
            PrinterType = DeviceType.DRV;
            PrinterName = printerName;
            IsWriteLog = isWriteLog;
            PrinterProgrammingLanguage = progLanguage;
            return PrintGraphics(bytes);
        }
        #endregion  

        #region 打印ZPL、EPL指令
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
                }
                finally
                {
                    GraphBuffer = new byte[0];
                }
                return result;
            }
        }
        #endregion  

        #region 打印图像字节流
        public static bool PrintGraphics(byte[] graph)
        {
            lock (SyncRoot)
            {
                bool result = false;
                try
                {
                    GraphBuffer = graph;
                    byte[] cmdBytes = new byte[0];
                    if (PrinterProgrammingLanguage == ProgrammingLanguage.ZPL)
                    {
                        cmdBytes = getZPLBytes();
                    }
                    if (PrinterProgrammingLanguage == ProgrammingLanguage.EPL)
                    {
                        cmdBytes = getEPLBytes();
                    }
                    if (PrinterProgrammingLanguage == ProgrammingLanguage.CPCL)
                    {
                        cmdBytes = getCPCLBytes();
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
            TcpClient tcp = null;
            try
            {
                IPAddress ip = IPAddress.Parse(TcpIpAddress);
                IPEndPoint iport = new IPEndPoint(ip, TcpPort);//9100为小票打印机指定端口
                tcp = new TcpClient(iport);
                tcp.SendTimeout = 1000;
                tcp.ReceiveTimeout = 1000;
                if (tcp.Connected)
                {
                    tcp.Client.Send(cmdBytes);
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
            byte[] result = new byte[0];
            byte[] bmpData = getBitmapData();
            string textBitmap = string.Empty;
            string textHex = BitConverter.ToString(bmpData).Replace("-", string.Empty);
            for (int i = 0; i < GraphHeight; i++)
            {
                textBitmap += textHex.Substring(i * RowRealBytesCount * 2, RowRealBytesCount * 2) + "\r\n";
            }
            string text = string.Format("~DGR:IMAGE.GRF,{0},{1},\r\n{2}^XGR:IMAGE.GRF,1,1^FS\r\n^IDR:IMAGE.GRF\r\n",
                GraphHeight * RowRealBytesCount,
                RowRealBytesCount,
                textBitmap);
            result = Encoding.Default.GetBytes(text);
            return result;
        }
        #endregion

        #region 生成EPL图像打印指令
        private static byte[] getEPLBytes()
        {
            byte[] result = new byte[0];
            byte[] buffer = getBitmapData();
            string text = string.Format("N\r\nGW{0},{1},{2},{3},{4}\r\nP\r\n",
                0,
                0,
                RowRealBytesCount,
                GraphHeight,
                TransferFormat.GetString(buffer));
            result = TransferFormat.GetBytes(text);
            return result;
        }
        #endregion  

        #region 生成CPCL图像打印指令
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
                dstBmp = srcBmp.Clone(new Rectangle(0, 0, srcBmp.Width, srcBmp.Height), PixelFormat.Format1bppIndexed);
                dstBmp.Save(dstStream, ImageFormat.Bmp);
                dstBuffer = dstStream.ToArray();

                int bfSize = BitConverter.ToInt32(dstBuffer, 2);
                int bfOffBits = BitConverter.ToInt32(dstBuffer, 10);
                int bitmapDataLength = bfSize - bfOffBits;
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

        #region 扩展

        #region CPCL命令打印24位bmp
        private static string get24BitBmpData(string filePath)
        {
            Bitmap bmp = new Bitmap(filePath);
            byte[] bitArray = { 128, 64, 32, 16, 8, 4, 2, 1 };
            string imgTxt = "";
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            IntPtr firstPix = data.Scan0;

            int rowByteCount = bmp.Width * 3;
            int filledCount = data.Stride - rowByteCount;
            int bytes = data.Stride * data.Height;//Math.Ceiling((double)bmp.Width / 8)
            byte[] rgbValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(firstPix, rgbValues, 0, bytes);

            int printRowByteCount = Convert.ToInt32(Math.Ceiling((double)(bmp.Width) / 8));
            int printRowByteFillCount = 4 - (printRowByteCount % 4);
            //int bitFillCount = 8 - (bmp.Width % 8);
            byte[] printData = new byte[(printRowByteCount + printRowByteFillCount) * bmp.Height];

            int byteCount = 0;
            int bitCount = 0;
            int rowPoint = 0;
            for (int i = 0; i < rgbValues.Length; i += 3)
            {
                int rgbValue = rgbValues[i] + rgbValues[i + 1] + rgbValues[i + 2];
                if (rgbValue != (255 * 3))
                {
                    printData[byteCount] = Convert.ToByte(printData[byteCount] | bitArray[bitCount]);
                }
                if (bitCount == 7)
                {
                    bitCount = 0;
                    byteCount++;
                }
                else
                {
                    bitCount++;
                }
                if ((rowPoint + 3) == rowByteCount)
                {
                    rowPoint = 0;
                    if (bitCount > 0)
                    {
                        byteCount++;
                    }
                    bitCount = 0;
                    byteCount += printRowByteFillCount;
                    i = i + filledCount;
                }
                else
                {
                    rowPoint += 3;
                }
            }

            foreach (byte byteData in printData)
            {
                string hexStr = Convert.ToString(byteData, 16);
                if (hexStr.Length == 1)
                {
                    hexStr = '0' + hexStr;
                }
                imgTxt += hexStr;
            }
            bmp.UnlockBits(data);
            return imgTxt.ToUpper();
        }

        public static void Printer(string bitbmpPath)
        {
            string CRNL = "\r\n";
            string imgTxt = get24BitBmpData(bitbmpPath);
            string cmddata = "! 0 200 200 300 1" + CRNL +
                            "EG " + 24 + " " + 50 + " 10 10 " + imgTxt + CRNL +
                            "FORM" + CRNL +
                            "PRINT" + CRNL;
            try
            {
                string ipAddress = "192.168.1.212";
                int port = 9100;

                // Open connection
                System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient();
                client.Connect(ipAddress, port);

                // Write CPCL String to connection
                System.IO.StreamWriter writer = new System.IO.StreamWriter(client.GetStream());
                writer.Write(cmddata);
                writer.Flush();

                // Close Connection
                writer.Close();
                client.Close();
            }
            catch (Exception)
            {
                // Catch Exception
            }
        }
        #endregion

        #endregion
    }

    #region 定义设备类型枚举
    public enum DeviceType
    {
        COM = 0,
        LPT = 1,
        DRV = 2,
        TCP = 3
    }
    #endregion

    #region 定义打印机指令类型枚举
    public enum ProgrammingLanguage
    {
        ZPL = 0,
        EPL = 1,
        CPCL = 2
    }
    #endregion

    #region 定义日志类型枚举
    public enum LogType
    {
        Print = 0,
        Error = 1
    }
    #endregion

    #region 定义打印文档信息类
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class DocInfo
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string DocName;
        [MarshalAs(UnmanagedType.LPStr)]
        public string OutputFile;
        [MarshalAs(UnmanagedType.LPStr)]
        public string DataType;
    }
    #endregion

    #region 定义图像设备信息类
    public class DeviceInfo
    {
        #region 属性说明
        /* 
        ColorDepth  
            图像输出支持的颜色范围的像素深度。有效值为 1、4、8、24 和 32。默认值为 24。仅对 TIFF 呈现支持 ColorDepth，对于其他图像输出格式报表服务器将忽略此设置。 
 
        注意：  
        对于此版本的 SQL Server，此设置的值将被忽略，且通常将 TIFF 图像呈现为 24 位。 
  
        Columns  
            要为报表设置的列数。此值将覆盖报表的原始设置。 
  
        ColumnSpacing  
            要为报表设置的列间距。此值将覆盖报表的原始设置。 
  
        DpiX  
            输出设备在 X 方向的分辨率。默认值为 96。 
  
        DpiY  
            输出设备在 Y 方向的分辨率。默认值为 96。 
  
        EndPage  
            要呈现的报表的最后一页。默认值为 StartPage 的值。 
  
        MarginBottom  
            要为报表设置的下边距值，以英寸为单位。您必须包含一个整数或小数值，后跟“in”（例如，1in）。此值将覆盖报表的原始设置。 
  
        MarginLeft  
            要为报表设置的左边距值，以英寸为单位。您必须包含一个整数或小数值，后跟“in”（例如，1in）。此值将覆盖报表的原始设置。 
  
        MarginRight  
            要为报表设置的右边距值，以英寸为单位。您必须包含一个整数或小数值，后跟“in”（例如，1in）。此值将覆盖报表的原始设置。 
  
        MarginTop  
            要为报表设置的上边距值，以英寸为单位。您必须包含一个整数或小数值，后跟“in”（例如，1in）。此值将覆盖报表的原始设置。 
  
        OutputFormat  
            图形设备接口 (GDI) 支持的输出格式之一：BMP、EMF、GIF、JPEG、PNG 或 TIFF。 
  
        PageHeight  
            要为报表设置的页高，以英寸为单位。您必须包含一个整数或小数值，后跟“in”（例如，11in）。此值将覆盖报表的原始设置。 
  
        PageWidth  
            要为报表设置的页宽，以英寸为单位。您必须包含一个整数或小数值，后跟“in”（例如，8.5in）。此值将覆盖报表的原始设置。 
  
        StartPage  
            要呈现的报告的第一页。值为 0 指示将呈现所有页。默认值为 1。  
         */
        #endregion

        public enum GDIOutputFormat { BMP, EMF, GIF, JPEG, PNG, TIFF }

        public int ColorDepth { get; set; }
        public int Columns { get; set; }
        public int ColumnSpacing { get; set; }
        public int DpiX { get; set; }
        public int DpiY { get; set; }
        public int EndPage { get; set; }
        public int MarginBottom { get; set; }
        public int MarginLeft { get; set; }
        public int MarginRight { get; set; }
        public int MarginTop { get; set; }
        public GDIOutputFormat OutputFormat { get; set; }
        public int PageHeight { get; set; }
        public int PageWidth { get; set; }
        public int StartPage { get; set; }

        private const string xmlFormater = @"<DeviceInfo>  
                <ColorDepth>{0}</ColorDepth>  
                <Columns>{1}</Columns>  
                <ColumnSpacing>{2}</ColumnSpacing>  
                <DpiX>{3}</DpiX>  
                <DpiY>{4}</DpiY>  
                <EndPage>{5}</EndPage>  
                <MarginBottom>{6}</MarginBottom>  
                <MarginLeft>{7}</MarginLeft>  
                <MarginRight>{8}</MarginRight>  
                <MarginTop>{9}</MarginTop>  
                <OutputFormat>{10}</OutputFormat>  
                <PageHeight>{11}</PageHeight>  
                <PageWidth>{12}</PageWidth>  
                <StartPage>{13}</StartPage>  
                </DeviceInfo>";

        public DeviceInfo()
        {
            this.ColorDepth = 24;
            this.Columns = 0;
            this.StartPage = 1;
            this.EndPage = 1;
        }

        public string GetDeviceInfo()
        {
            string result = string.Format(xmlFormater,
                this.ColorDepth,
                this.Columns,
                this.ColumnSpacing,
                this.DpiX,
                this.DpiY,
                this.EndPage,
                this.MarginBottom,
                this.MarginLeft,
                this.MarginRight,
                this.MarginTop,
                this.OutputFormat,
                this.PageHeight,
                this.PageWidth,
                this.StartPage);
            return result;
        }

        public string GetDeviceInfoForImage()
        {
            string result = string.Format("<DeviceInfo><StartPage>{0}</StartPage><EndPage>{1}</EndPage><OutputFormat>{2}</OutputFormat><DpiX>{3}</DpiX><DpiY>{4}</DpiY></DeviceInfo>",
                this.StartPage,
                this.EndPage,
                this.OutputFormat,
                this.DpiX,
                this.DpiY);
            return result;
        }
    }
    #endregion
}
