using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

namespace DocDirect
{
    public partial class FilesList : Page
    {
        public FilesList()
        {
            InitializeComponent();
        }

        private void btnThumbnailViewFile_Click(object sender, RoutedEventArgs e)
        {
            listViewFiles.View = this.FindResource("ImageView") as ViewBase;
        }

        private void btnTileViewFile_Click(object sender, RoutedEventArgs e)
        {
            listViewFiles.View = this.FindResource("GridView") as ViewBase;
        }        
    }
}
