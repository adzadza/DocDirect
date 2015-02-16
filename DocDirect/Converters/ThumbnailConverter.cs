using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

namespace DocDirect.Converters
{
    public class ThumbnailConverter : IValueConverter
    {
        private static ImageSource IconForFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                return null;

            using (System.Drawing.Icon sysicon = System.Drawing.Icon.ExtractAssociatedIcon(filePath))
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                          sysicon.Handle,
                          System.Windows.Int32Rect.Empty,
                          System.Windows.Media.Imaging.BitmapSizeOptions.FromWidthAndHeight(40, 40));
            }
        }
        //private async static Task<Object> GetBitmap(StorageItemThumbnail thumbnail)
        //{
            //var buffer = new Windows.Storage.Streams.Buffer(Convert.ToUInt32(thumbnail.Size));
            //IBuffer thumbBuffer = await thumbnail.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.None);
            //using (var str = await thumbImageFile.OpenAsync(FileAccessMode.ReadWrite))
            //{
             //   await str.WriteAsync(thumbBuffer);
            //}
        //}
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //var bitmap = new BitmapImage();
            return IconForFile((string)value);
            
            //return bitmap;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }    
}
