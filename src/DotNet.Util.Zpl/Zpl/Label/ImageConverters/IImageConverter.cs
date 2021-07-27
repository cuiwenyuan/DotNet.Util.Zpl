namespace DotNet.Util.Zpl.Label.ImageConverters
{
    /// <summary>
    /// IImageConverter
    /// </summary>
    public interface IImageConverter
    {
        /// <summary>
        /// ConvertImage
        /// </summary>
        /// <param name="imageData"></param>
        /// <returns></returns>
        ImageResult ConvertImage(byte[] imageData);
    }
}
