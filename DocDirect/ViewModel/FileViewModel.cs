using DocDirect.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace DocDirect.ViewModel
{
    public class FileViewModel : ViewModelBase
    {
        private FileModel _file;

        public FileViewModel(FileModel file)
        {
            this._file = file;
        }

        public String Path
        {
            get { return _file.Path; }
            set {
                _file.Path = value;
                OnPropertyChanged("Path");
            }
        }
        public String Name
        {
            get { return _file.Name; }
            set { 
                _file.Name = value;
                OnPropertyChanged("Name");
            }
        }
        public long Size
        {
            get { return _file.Size; }
            set { 
                _file.Size = value;
                OnPropertyChanged("Size");
            }
        }
        public string FileType
        {
            get { return _file.FileType; }
            set { 
                _file.FileType = value;
                OnPropertyChanged("FileType");
            }
        }
    }
}
