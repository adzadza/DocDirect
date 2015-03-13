using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DocDirect.Converters
{
    class GridIconConverter : IValueConverter
    {
        private BitmapImage SearchIcon(string nameFile)
        {
            BitmapImage icon;
            try
            {
                icon = new BitmapImage(new Uri(@"/Resourses/Icon/IconGrid/" + nameFile, UriKind.RelativeOrAbsolute));
            }
            catch(Exception)
            {
                icon = new BitmapImage(new Uri(@"/Resourses/Icon/IconGrid/doc.png", UriKind.RelativeOrAbsolute));
            }

            return icon;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string extencion = value.ToString();
            string fileName = extencion.Split('.')[1]+".png";

            BitmapImage icon = SearchIcon(fileName);

            return icon;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
