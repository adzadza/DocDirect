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
        private string m_Name;
        private string m_Path; // full path
        private ulong  m_Size;

        public FileModel(string Name, string Path, ulong Size )
        {
            m_Name = Name;
            m_Path = Path;
            m_Size = Size;
        }
        public String Path
        {
            get { return m_Path; }
            set { m_Path = value; }
        }
        public String Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }
        public ulong Size
        {
            get { return m_Size; }
            set { m_Size = value; }
        }
    }

}
