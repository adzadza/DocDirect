using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace DocDirect
{
    public partial class DialogBoxInfo : Window, INotifyPropertyChanged
    {
        private string _messageText;
        private string _titleText;

        public string MessageText { 
            get { return _messageText;}
            set { 
                _messageText = value;
                OnPropertyChanged("MessageText");
            } 
        }
        public string TitleText 
        { 
            get{ return _titleText;} 
            set{ 
                _titleText = value;
                OnPropertyChanged("TitleText");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DialogBoxInfo(string message, string title)
        {
            InitializeComponent();

            MessageText = message;
            TitleText = title;
        }

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        
    }
}
