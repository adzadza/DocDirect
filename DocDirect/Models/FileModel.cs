using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace DocDirect
{
    public class FileModel
    {
        private string _Name;
        private string _Path; // full path
        private long  _Size;
        private string _fileType;

        public FileModel(string Name, string Path, long Size, string FileType)
        {
            _Name = Name;
            _Path = Path;
            _Size = Size;
            _fileType = FileType;
        }
        public String Path
        {
            get { return _Path; }
            set { _Path = value; }
        }
        public String Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        public long Size
        {
            get { return _Size; }
            set { _Size = value; }
        }
        public string FileType
        {
            get { return _fileType; }
            set { _fileType = value; }
        }
    }

}
