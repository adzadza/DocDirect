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
using Windows.Storage;
using Windows.Networking.Sockets;
using Windows.Networking;
using Windows.Storage.Streams;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using DocDirect.Commands.AsyncCommand;
using DocDirect.Commands;
using DocDirect.Crypto;

namespace DocDirect.ViewModel
{
    class FilesListViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<FileViewModel> _filesList = new ObservableCollection<FileViewModel>();
        private CryptoUtils     _cryptoUtils;
        private string          _sizeSelectedFile;
        private ulong           _countItem = 0;
            
        private String           _workFolderName = @"C:\Doc";
        private StorageFolder    _workFolder;
        private StorageFolder    __workFolder;
        private FileViewModel    _selectedFile;
        private uint             _sizeBufer = 1024;
        private String           _port = "22112";
        private HostName         _LOCAL = new HostName("192.168.56.1"); // DEBUG
        #endregion

        #region Constructor
        public FilesListViewModel()
        {
            InitialisCommands();

            _filesList = GetFiles();
            _cryptoUtils = new CryptoUtils();
        }

        private void InitialisCommands()
        {
            this.SelectedFileCommand = new RelayCommand(obj => this.SetCommandSelected(obj));

            this.OpenFileCommand = new RelayCommand(param => this.OpenFile(param));
            this.RemoveFileCommand = new RelayCommand(param => this.RemoveFile(param));
            this.CopyFileCommand = new RelayCommand(param => this.CopyFile(param));
            this.PastFileCommand = new RelayCommand(param => this.PastFile());

            //this.SendFileCommand = new RelayCommand(param => this.SendFileToClient(param));

            this.SendFileCommandAsync = AsyncCommand.Create(token => this.SendFileToClient(CurrentSelectedFile, token));
            this.InitialisStreamSocketAsync = AsyncCommand.Create(tocken => this.InitialisStreamSocket(tocken));

            InitialisStreamSocketAsync.Execute(null);

            //ConnectCommand = AsyncCommand.Create(param => this.ConnectCom(param));
            //ListenerCommand = AsyncCommand.Create(param => this.ListenerCom(param));
            //SendCommand = AsyncCommand.Create(param => this.SendCom(param));
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
        public string SelectedFileSize
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
        public FileViewModel CurrentSelectedFile
        {
            get { return _selectedFile; }
            set {
                _selectedFile = value;
                OnPropertyChanged("CurrentSelectedFile");
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
        public ICommand SelectedFileCommand { get; set;}

        public ICommand OpenFileCommand { get; private set; }
        public ICommand RemoveFileCommand { get; private set; }
        public ICommand CopyFileCommand { get; private set; }
        public ICommand PastFileCommand { get; private set; }
        public ICommand SendFileCommand { get; private set; }

        public IAsyncCommand SendFileCommandAsync { get; private set; }
        public IAsyncCommand InitialisStreamSocketAsync { get; private set; }

        public IAsyncCommand ConnectCommand { get; private set; }
        public IAsyncCommand ListenerCommand { get; private set; }
        public IAsyncCommand SendCommand { get; private set; }
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
            string pattern = "(.doc)|(.docx)|(.pdf)|(.xls)|(.xlsx)|(.txt)";
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
            DirectoryInfo directory = new DirectoryInfo(_workFolderName);
            ObservableCollection<FileViewModel> filesList = new ObservableCollection<FileViewModel>();

            if (directory.Exists)
            {
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
                catch (Exception) {}
            }
            else
            {
                Directory.CreateDirectory(_workFolderName);
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
                        catch (IOException)
                        {
                            DialogBoxInfo dlgError = new DialogBoxInfo("This file can not be deleted!", "Error");
                            dlgError.ShowDialog();
                        }
                        catch (Exception) { }
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
                    local = _workFolderName+ "\\" +GetNameFile(path);
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

                //сохраним размер, для того что бы вывести его на экран
                SelectedFileSize = "  1 item selected " + ConverterSize(file.Size);

                // и сохраним модель файла для того что бы передать в async command'у
                CurrentSelectedFile = file;
            }
        }

        private async Task InitialisStreamSocket(CancellationToken token = new CancellationToken())
        {
            try
            {
                StreamSocketListener Listener = new StreamSocketListener();
                Listener.ConnectionReceived += OnClientConnectionReceived;

                CoreApplication.Properties.Add("Listener", Listener);

                await Listener.BindEndpointAsync(_LOCAL, _port);

                _workFolder = await StorageFolder.GetFolderFromPathAsync(_workFolderName);
                __workFolder = await StorageFolder.GetFolderFromPathAsync(@"D:\Doc");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async Task SendFileToClient(object param, CancellationToken token = new CancellationToken())
        {
            if (param is FileViewModel)
            {
                FileViewModel file = param as FileViewModel;

                if (System.IO.File.Exists(file.Path))
                {
                    StorageFile fileStorage = await StorageFile.GetFileFromPathAsync(file.Path);
                    if (!CoreApplication.Properties.ContainsKey("FileSend"))
                    {
                        CoreApplication.Properties.Add("FileSend", fileStorage);
                    }

                    await ExchangeKeySend();
                }
            }
        }
        #endregion End Handler Command

        #region FileTransfer
        private async void OnClientConnectionReceived(StreamSocketListener listener, 
                                                      StreamSocketListenerConnectionReceivedEventArgs args)
        {
            using (StreamSocket socket = args.Socket)
            using (var dr = new DataReader(socket.InputStream))
            {
                await dr.LoadAsync(sizeof(Byte));
                byte step = dr.ReadByte();

                switch(step)
                {
                    case (Byte)Step.ExchangeKey:
                        await ExchangeKeyDownload(dr);
                        break;
                    case (Byte)Step.DownloadFileWhithHash:
                        await DownloadFileAsync(dr);
                        break;
                    case (Byte)Step.SuccessfulTransmission:
                        SuccessfulTransmission();
                        break;
                    case (Byte)Step.TransmissionError:
                        TransmissionError();
                        break;
                }
                dr.DetachStream();
            }
        }

        public async Task SendFileAsync()
        {            
            Debug.WriteLine("---> SendFileAsync <----\n");
            object outValue;
            if (CoreApplication.Properties.TryGetValue("FileSend", out outValue))
            {
                StorageFile file = outValue as StorageFile;

                byte[] output = new byte[_sizeBufer];
                var prop = await file.GetBasicPropertiesAsync();

                using (StreamSocket socket = new StreamSocket())
                using (var dw = new DataWriter(socket.OutputStream))
                {
                    await socket.ConnectAsync(_LOCAL, _port);
                    // Write byte current step
                    dw.WriteByte((byte)Step.DownloadFileWhithHash);
                    // Send lenght hash
                    dw.WriteUInt32((UInt32)_cryptoUtils.GetHashFile.Length);
                    // Send hash
                    dw.WriteBytes(_cryptoUtils.GetHashFile);
                    // Send the filename length
                    dw.WriteInt32(file.Name.Length); // filename length is fixed
                    // Send the filename
                    dw.WriteString(file.Name);
                    // Send the file length
                    dw.WriteUInt64(prop.Size);
                    // Send the file
                    var fileStream = await file.OpenStreamForReadAsync();
                    while (fileStream.Position < (long)prop.Size)
                    {
                        var rlen = await fileStream.ReadAsync(output, 0, output.Length);
                        dw.WriteBytes(output);
                    }

                    await dw.FlushAsync();
                    await dw.StoreAsync();

                    await socket.OutputStream.FlushAsync();
                }
            }
            else
            {
                DialogBoxInfo dlg = new DialogBoxInfo("File not found!", "Error");
                dlg.ShowDialog();
            }
        }
        private async Task DownloadFileAsync(DataReader rw)
        {
            Debug.WriteLine("--->> DownloadFileAsync");

            await rw.LoadAsync(sizeof(Int32));
            UInt32 lenght = rw.ReadUInt32();
            // get lenght hash
            byte[] outValue = new byte[lenght]; 
            // download hash
            await rw.LoadAsync(lenght);
            rw.ReadBytes(outValue);
            _cryptoUtils.GetHashFile = outValue;

            StorageFile file;
            //1. Read the filename lenght
            await rw.LoadAsync(sizeof(Int32));
            var fileNameLenght = (uint)rw.ReadInt32();
            // 2. Read the filename
            await rw.LoadAsync(fileNameLenght);
            var originalFileName = rw.ReadString(fileNameLenght);
            // 3. Read the file length
            await rw.LoadAsync(sizeof(UInt64));
            var fileLenght = rw.ReadUInt64();

            // 4. Read file
            using (var memStream = await DownloadFile(rw, fileLenght))
            {
                file = await _workFolder.CreateFileAsync(originalFileName, CreationCollisionOption.ReplaceExisting);
                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await RandomAccessStream.CopyAndCloseAsync(memStream.GetInputStreamAt(0), fileStream.GetOutputStreamAt(0));
                }
            }

            // compare the hash value
            _cryptoUtils.CompareHash(file);
        }
        private async Task<InMemoryRandomAccessStream> DownloadFile(DataReader rw, ulong fileLength)
        {
            var memStream = new InMemoryRandomAccessStream();

            // Download the file
            while (memStream.Position < fileLength)
            {
                var lenToRead = Math.Min(1024, fileLength - memStream.Position);
                await rw.LoadAsync((uint)lenToRead);
                var tempBuff = rw.ReadBuffer((uint)lenToRead);
                await memStream.WriteAsync(tempBuff);
            }

            return memStream;
        }
        private async Task ExchangeKeySend()
        {
            Debug.WriteLine("--->> ExchangeKeySend");

            using (StreamSocket socket = new StreamSocket())
            using (var dw = new DataWriter(socket.OutputStream))
            {
                await socket.ConnectAsync(_LOCAL, _port);

                // Write byte current step
                dw.WriteByte((byte)Step.ExchangeKey);

                dw.WriteUInt64(_cryptoUtils.distributedKey);
                // Write open key
                dw.WriteUInt64(_cryptoUtils.PublicKey.E);
                dw.WriteUInt64(_cryptoUtils.PublicKey.N);

                await dw.FlushAsync();
                await dw.StoreAsync();

                await socket.OutputStream.FlushAsync();
            }
        }
        private async Task ExchangeKeyDownload(DataReader dr) 
        {
            Debug.WriteLine("--->> ExchangeKeyDownload");

            await dr.LoadAsync(sizeof(UInt64));
            _cryptoUtils.distributedKey = dr.ReadUInt64();

            await dr.LoadAsync(sizeof(UInt64));
            _cryptoUtils.PublicKey.E = dr.ReadUInt64();

            await dr.LoadAsync(sizeof(UInt64));
            _cryptoUtils.PublicKey.N = dr.ReadUInt64();

            dr.DetachStream();

            _cryptoUtils.GenerateGeneralKey();
            _cryptoUtils.GenerateHash();

            await SendFileAsync();
        }
        private void SuccessfulTransmission()
        {
            DialogBoxInfo dlg = new DialogBoxInfo("File has been successfully sent!", "Info");
            dlg.ShowDialog();
        }
        private void TransmissionError()
        {
            DialogBoxInfo dlg = new DialogBoxInfo("Send a file failed!", "Info");
            dlg.ShowDialog();
        }
        #endregion FileTransfer

        #region Socket
        private async Task ConnectCom(CancellationToken token = new CancellationToken())
        {
            StreamSocket Socket = new StreamSocket();

            CoreApplication.Properties.Add("Socket", Socket);

            try
            {
                await Socket.ConnectAsync(new HostName("192.168.56.1"), "22112");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async Task ListenerCom(CancellationToken token = new CancellationToken())
        {
            try
            {
                StreamSocketListener Listener = new StreamSocketListener();
                Listener.ConnectionReceived += OnClientConnection;

                CoreApplication.Properties.Add("Listener", Listener);

                await Listener.BindEndpointAsync(new HostName("192.168.56.1"), "22112");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async Task SendCom(object param)
        {
            object outValue ;
            StreamSocket Socket;

            if (!CoreApplication.Properties.TryGetValue("Socket", out outValue))
            {
                MessageBox.Show("Not Socket");
                return;
            }

            Socket = (StreamSocket)outValue; 

            try
            {
                DataWriter writer;
                if (!CoreApplication.Properties.TryGetValue("DataWriter", out outValue))
                {
                    writer = new DataWriter(Socket.OutputStream);
                    CoreApplication.Properties.Add("DataWriter", writer);
                }
                else
                {
                    writer = (DataWriter)outValue;
                }

                string stringToSend = "Hello World!";
                writer.WriteUInt32(writer.MeasureString(stringToSend));
                writer.WriteString(stringToSend);
            
                await writer.StoreAsync();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
        private async void OnClientConnection(StreamSocketListener listener,
                                              StreamSocketListenerConnectionReceivedEventArgs args)
        {
            DataReader reader = new DataReader(args.Socket.InputStream);
            try
            {
                while (true)
                {
                    uint sizeFieldCount = await reader.LoadAsync(sizeof(uint));
                    if (sizeFieldCount != sizeof(uint))
                    {
                        return;
                    }

                    uint stringLength = reader.ReadUInt32();
                    uint actualStringLength = await reader.LoadAsync(stringLength);
                    if (stringLength != actualStringLength)
                    {
                        return;
                    }
                    
                    MessageBox.Show("Received data: " + reader.ReadString(actualStringLength));
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
        #endregion
    }
}
