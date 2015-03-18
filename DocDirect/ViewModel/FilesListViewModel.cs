using DocDirect.Commands;
using System.Collections.ObjectModel;
using System.IO;
using System;
using System.Windows.Input;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace DocDirect.ViewModel
{
    class FilesListViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<FileViewModel> _filesList = new ObservableCollection<FileViewModel>();
        private string _sizeSelectedFile="";
        private ulong  _countItem = 0;
        private string _pathToWorkDictionary = @"D:\Doc";
        #endregion

        #region Constructor
        public FilesListViewModel()
        {
            _filesList = GetFiles();

            InitialisCommands();
        }

        private void InitialisCommands()
        {
            this.SelectedFileCommand = new DelegateCommand<FileViewModel>(obj => 
            {
                CurrentSelectedFile = "  1 item selected " + ConverterSize(obj.Size);
            });

            this.ContextMenuOpenFile = new RelayCommand(param => this.OpenFileCommand(param));
            this.ContextMenuRemoveFile = new RelayCommand(param => this.RemoveFileCommand(param));
        }
        #endregion

        #region Property
        public ObservableCollection<FileViewModel> FilesList
        { 
            get { return _filesList; }
            set {
                _filesList = value;
                OnPropertyChanged("FilesList");
            }
        }
        public string CurrentSelectedFile
        {
            get { return _sizeSelectedFile; }
            set
            {
                if(_sizeSelectedFile!=value)
                {
                    _sizeSelectedFile = value;
                    OnPropertyChanged("CurrentSelectedFile");
                }
            }
        }
        public ulong CountItem
        {
            get { return _countItem; }
            set { 
                _countItem = value;
                OnPropertyChanged("CountItem");
            }
        }        
        #endregion

        #region Commands
        public ICommand ThumbnailViewCommand
        {
            get;
            private set;
        }
        public ICommand SelectedFileCommand
        {
            get;
            private set;
        }

        public ICommand ContextMenuOpenFile
        { 
            get; 
            private set; 
        }
        public ICommand ContextMenuRemoveFile
        {
            get;
            private set;
        }
        #endregion

        #region Method
        private string GetFileType(string fileType)
        {
            if (isImage(fileType)) return "Image";
            else if (isDocument(fileType)) return "Docement";

            if (isVideo(fileType)) return "Video";
            else if (isMusic(fileType)) return "Music";

            return "Other";
        }

        private bool isImage(string fileType)
        {
            string pattern = "(.jpg)|(.png)";
            return Regex.IsMatch(fileType, pattern, RegexOptions.IgnoreCase);
        }
        private bool isDocument(string fileType)
        {
            string pattern = "(.doc)|(.docx)|(.pdf)";
            return Regex.IsMatch(fileType, pattern, RegexOptions.IgnoreCase);
        }
        private bool isVideo(string fileType)
        {
            string pattern = "(.avi)";
            return Regex.IsMatch(fileType, pattern, RegexOptions.IgnoreCase);
        }
        private bool isMusic(string fileType)
        {
            string pattern = "(.mp3)|(.flac)";
            return Regex.IsMatch(fileType, pattern, RegexOptions.IgnoreCase);
        }

        private ObservableCollection<FileViewModel> GetFiles()
        {
            DirectoryInfo directory = new DirectoryInfo(_pathToWorkDictionary);

            ObservableCollection<FileViewModel> filesList = new ObservableCollection<FileViewModel>();
            try
            {
                foreach (var file in directory.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    var modelFile = new FileModel(
                        file.Name,
                        file.FullName,
                        file.Length,
                        GetFileType(file.Extension),
                        file.LastAccessTime,
                        file.Extension
                    );
                    _countItem++;
                    filesList.Add(new FileViewModel(modelFile));
                }
            }
            catch (Exception ex)
            {
                Debugger.Log(1, "Error", ex.Message);
            }

            return filesList;
        }
        
        private string ConverterSize(long size)
        {
            if (size < 1024) return size.ToString() + " KBit";
            else
                if (size > 1024 && size < 1048576) return (size / 1024).ToString() + " KB";
                else
                    if (size > 1048576 && size < 1073741824) return (size / 1048576).ToString() + " MB";
                    else
                        return (size / 1073741824).ToString() + " GB";
        }       
        #endregion

        #region Handler Command
        private void OpenFileCommand(object param)
        {

        }
        private void RemoveFileCommand(object param)
        {

        }
        #endregion End Handler Command
    }
}
