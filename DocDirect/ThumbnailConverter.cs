using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using Windows.Storage.Streams;
using System.Windows.Media.Imaging;
using Windows.Storage.FileProperties;
using System.Windows.Media;

namespace DocDirect
{
    public class ThumbnailConverter : IValueConverter
    {
        //Собственно извлечение иконки для файла
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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return IconForFile((string)value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
