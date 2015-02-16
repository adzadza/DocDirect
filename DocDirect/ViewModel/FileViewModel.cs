using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace DocDirect.ViewModel
{
    class FileViewModel : ViewModelBase
    {
        private FileModel m_File;

        public FileViewModel(FileModel file)
        {
            this.m_File = file;
        }

        public String Path
        {
            get { return m_File.Path; }
            set {
                m_File.Path = value;
                OnPropertyChanged("Path");
            }
        }
        public String Name
        {
            get { return m_File.Name; }
            set { 
                m_File.Name = value;
                OnPropertyChanged("Name");
            }
        }
        public ulong Size
        {
            get { return m_File.Size; }
            set { 
                m_File.Size = value;
                OnPropertyChanged("Size");
            }
        }
    }
}
