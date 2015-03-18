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
            string[] arr = fullPath.Split('.');
            string extension = arr[arr.Length-1];

            BitmapImage icon;
            
            icon = new BitmapImage(new Uri(@"/Resourses/Icon/DocIcon/" + extension+".png", UriKind.Relative));
            
            if(icon==null)
                icon = new BitmapImage(new Uri(@"/Resourses/Icon/DocIcon/read.png", UriKind.Relative));            

            return icon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }    
}
