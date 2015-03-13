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
            if (!System.IO.File.Exists(filePath)) return null;

            using (System.Drawing.Icon sysicon = System.Drawing.Icon.ExtractAssociatedIcon(filePath))
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                          sysicon.Handle,
                          System.Windows.Int32Rect.Empty,
                          System.Windows.Media.Imaging.BitmapSizeOptions.FromWidthAndHeight(100, 100));
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string fullPath = value.ToString();
            string extension = fullPath.Split('.')[1];



            return IconForFile(fullPath);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }    
}
