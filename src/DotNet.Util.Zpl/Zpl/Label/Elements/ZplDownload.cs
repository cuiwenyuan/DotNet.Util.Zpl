namespace DotNet.Util.Zpl.Label.Elements
{
    /// <summary>
    /// ZplDownload
    /// </summary>
    public abstract class ZplDownload : ZplElementBase
    {
        /// <summary>
        /// DRAM, Memory Card, EPROM, Flash
        /// R, E, B, and A
        /// </summary>
        public char StorageDevice { get; private set; }
        /// <summary>
        /// ZplDownload
        /// </summary>
        /// <param name="storageDevice"></param>
        public ZplDownload(char storageDevice)
        {
            StorageDevice = storageDevice;
        }
    }
}
