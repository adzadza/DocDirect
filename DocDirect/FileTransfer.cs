using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;
using System.IO;
using Windows.Networking;

namespace DocDirect
{
    public sealed class FileTransfer
    {
        private String _port = "6505";
        private String _workFolderName = @"C:\Doc";
        private StreamSocketListener _tcpListener;
        private StorageFolder _workFolder;
        private uint _sizeBufer = 1024;

        public FileTransfer()
        {
            //Start();
        }
 
        private async void Start() 
        {
            _tcpListener = new StreamSocketListener();

            _tcpListener.ConnectionReceived += OnClientConnectionReceived;
            await _tcpListener.BindServiceNameAsync(_port);
            await _tcpListener.BindEndpointAsync(new HostName("192.168.10.66"), "adzadza");
        
            
            _workFolder = await StorageFolder.GetFolderFromPathAsync(_workFolderName);
        }

        private async void OnClientConnectionReceived(StreamSocketListener listener, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            StorageFile file;
            using(StreamSocket socket = args.Socket)
            using (var rw = new DataReader(socket.InputStream))
            {
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
                using(var memStream = await DownloadFile(rw, fileLenght))
                {
                    file = await _workFolder.CreateFileAsync(originalFileName, CreationCollisionOption.ReplaceExisting);
                    using(var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await RandomAccessStream.CopyAndCloseAsync(memStream.GetInputStreamAt(0), fileStream.GetOutputStreamAt(0));
                    }
                    rw.DetachStream();
                }
            }
        }

        public async void SendFileAsync(StorageFile selectedFile)
        {
            Start();

            byte[] output = new byte[_sizeBufer];
            var prop = await selectedFile.GetBasicPropertiesAsync();

            using(StreamSocket socket = new StreamSocket())
            using(var dw = new DataWriter(socket.OutputStream))
            {
                await socket.ConnectAsync(new HostName("192.167.10.33"), _port);

                // 1. Send the filename length
                dw.WriteInt32(selectedFile.Name.Length); // filename length is fixed
                // 2. Send the filename
                dw.WriteString(selectedFile.Name);
                // 3. Send the file length
                dw.WriteUInt64(prop.Size);
                // 4. Send the file
                var fileStream = await selectedFile.OpenStreamForReadAsync();
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
        private async Task<InMemoryRandomAccessStream> DownloadFile(DataReader rw, ulong fileLength)
        {
            var memStream = new InMemoryRandomAccessStream();

            // Download the file
            while (memStream.Position < fileLength)
            {
                //Verbose(string.Format("Receiving file...{0}/{1} bytes", memStream.Position, fileLength));
                var lenToRead = Math.Min(1024, fileLength - memStream.Position);
                await rw.LoadAsync((uint)lenToRead);
                var tempBuff = rw.ReadBuffer((uint)lenToRead);
                await memStream.WriteAsync(tempBuff);
            }

            return memStream;
        }
    }
}
