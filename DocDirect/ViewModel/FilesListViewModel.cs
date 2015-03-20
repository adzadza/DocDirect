using DocDirect.Commands;
using System.Collections.ObjectModel;
using System.IO;
using System;
using System.Windows.Input;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows;
using System.Collections.Specialized;

namespace DocDirect.ViewModel
{
    class FilesListViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<FileViewModel> _filesList = new ObservableCollection<FileViewModel>();
        private string _sizeSelectedFile;
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
            this.SelectedFileCommand = new DelegateCommand<FileViewModel>(obj => this.SetCommandSelected(obj));

            this.OpenFileCommand = new RelayCommand(param => this.OpenFile(param));
            this.RemoveFileCommand = new RelayCommand(param => this.RemoveFile(param));
            this.CopyFileCommand = new RelayCommand(param => this.CopyFile(param));
            this.PastFileCommand = new RelayCommand(param => this.PastFile());
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

        public ICommand OpenFileCommand
        { 
            get; 
            private set; 
        }
        public ICommand RemoveFileCommand
        {
            get;
            private set;
        }
        public ICommand CopyFileCommand
        {
            get;
            private set;
        }
        public ICommand PastFileCommand
        {
            get;
            private set;
        }
        public ICommand SendFileCommand
        {
            get;
            private set;
        }
        #endregion

        #region Method
        private string GetFileType(string fileType)
        {
            if (isImage(fileType)) return "Image";
            else if (isDocument(fileType)) return "Document";

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
        
        // Add one file to collection
        private FileViewModel SetFileToCollection(string path)
        {
            FileInfo file = new FileInfo(path);
            var modelFile = new FileModel(
                        file.Name,
                        file.FullName,
                        file.Length,
                        GetFileType(file.Extension),
                        file.LastAccessTime,
                        file.Extension
                    );
            return new FileViewModel(modelFile);
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
        private string GetNameFile(string path)
        {
            string[] item = path.Split('\\');
            return item[item.Length - 1];
        }

        private void DeleteItemObservableCollection(string nameFile)
        {
            foreach (var item in _filesList)
            {
                if (item.Name == nameFile)
                {
                    _filesList.Remove(item);

                    --CountItem;
                    break;
                }                
            }
        }
        #endregion

        #region Handler Command
        private void OpenFile(object param)
        {
            if (param is FileViewModel) 
            { 
                FileViewModel file = param as FileViewModel;
            
                if (System.IO.File.Exists(file.Path))            
                    Process.Start(file.Path);
                else
                {
                    DialogBoxInfo dlg = new DialogBoxInfo("This file does not exist!", "Inforamation");
                    dlg.ShowDialog();
                }
            }
        }
        private void RemoveFile(object param)
        {
            if (param is FileViewModel)
            {
                FileViewModel file = param as FileViewModel;

                if (System.IO.File.Exists(file.Path))
                {
                    DialogBoxInfo dlg = new DialogBoxInfo("Really want to delete the file?", "Question");
                    dlg.ShowDialog();

                    if (dlg.DialogResult == true)
                    {
                        try
                        {
                            System.IO.File.Delete(file.Path);
                            DeleteItemObservableCollection(file.Name);
                        }
                        catch (IOException ex)
                        {
                            DialogBoxInfo dlgError = new DialogBoxInfo("This file can not be deleted!", "Error");
                            dlg.ShowDialog();
                        }
                    }
                }
            }
        }
        private void CopyFile(object param)
        {
            FileViewModel file = param as FileViewModel;

            if (file != null) 
            { 
                StringCollection FileCollection = new StringCollection();
                FileCollection.Add(file.Path);
                Clipboard.SetFileDropList(FileCollection);
            }
        }
        private void PastFile()
        {
            StringCollection fileCollection = new StringCollection();
            fileCollection = Clipboard.GetFileDropList();

            if(fileCollection.Count > 0)
            {
                string local;
                foreach(var path in fileCollection)
                {
                    local = _pathToWorkDictionary+"\\"+GetNameFile(path);
                    try { 
                        File.Copy(path, local);
                        FilesList.Add(SetFileToCollection(local));
                    }
                    catch(IOException)
                    {
                        if (!local.Equals(path))
                        {
                            DialogBoxInfo dlg = new DialogBoxInfo("This file exists, replace?", "Information");
                            dlg.ShowDialog();

                            if (dlg.DialogResult == true)
                            {
                                File.Delete(local);

                                File.Copy(path, local);
                                FilesList.Add(SetFileToCollection(local));
                            }
                        }
                    }
                    catch (Exception) { }
                }
               
            }
        }
        private void SetCommandSelected(object param)
        {
            if (param != null) 
            { 
                FileViewModel file = param as FileViewModel;
                CurrentSelectedFile = "  1 item selected " + ConverterSize(file.Size);
            }
        }
        #endregion End Handler Command
    }
}
