using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Compression;
using System.IO;
using DialogMaker.operable;
using System.Drawing;

namespace DialogMaker.jzip
{
    public class Container : IDisposable, operable.IImageContainer
    {
        System.IO.Compression.ZipArchive linkedArchive;
        Stream baseStream;
        const string JsonRecordName = "graphdata_v1.json";
        public string FileName { get; private set; }

        public Container(string path = null)
        {
            if (path == null)
                LinkFile(Path.GetTempFileName());
            else
                LinkFile(path);
        }
        [System.Serializable]
        public class EncodedException : Exception
        {
            public EncodedException() { }
            public EncodedException(string message) : base(message) { }
            public EncodedException(string message, Exception inner) : base(message, inner) { }
            protected EncodedException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
        private void LinkFile(string fileLocation)
        {
            FileName = fileLocation;
            baseStream = new FileStream(fileLocation, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            int a = baseStream.ReadByte(); int b = baseStream.ReadByte();
            if ((a == 0x9e && b == 0x3f))
            {
                throw new EncodedException();
            }
            baseStream.Seek(0, SeekOrigin.Begin);
            try
            {
                linkedArchive = new System.IO.Compression.ZipArchive(baseStream, ZipArchiveMode.Update, false);
            }
            catch
            {
                baseStream.Close();
                baseStream = null;
                throw;
            }
        }
        public void FlushReopen()
        {
            linkedArchive.Dispose();
            linkedArchive = null;
            baseStream.Close();

            LinkFile(FileName);
        }

        public string reWriteRecord(string entryName, byte[] data)
        {
            if (string.IsNullOrWhiteSpace(entryName))
                entryName = Guid.NewGuid().ToString();
            ZipArchiveEntry newEntry;

            if (null != (newEntry = linkedArchive.GetEntry(entryName)))
            {
                newEntry.Delete();
                //throw new Exception(string.Format("entry {0} already exists in archive", entryName));
            }

            newEntry = linkedArchive.CreateEntry(entryName, CompressionLevel.Fastest);
            Stream es = newEntry.Open();
            es.Write(data, 0, data.Length);
            es.Close();

            return entryName;
        }
        public byte[] TakeRecord(string entryName)
        {
            ZipArchiveEntry newEntry;

            if (null != (newEntry = linkedArchive.GetEntry(entryName)))
            {
                Stream es = newEntry.Open();

                int dataSize = (int)es.Length;
                byte[] rec = new byte[dataSize];
                int rok = es.Read(rec, 0, dataSize);
                if (rok != dataSize)
                    throw new Exception();
                es.Close();

                return rec;
            }
            return null;
        }
        public Dialog ExtractLinkedData(Dialog dlg)
        {
            byte[] jsonrec = this.TakeRecord(JsonRecordName);
            if (jsonrec == null)
                return null;
            string json = Encoding.Unicode.GetString(jsonrec);

            Newtonsoft.Json.JsonConvert.PopulateObject(json, dlg);
            return dlg;
        }
        public delegate void FileCopy(string srcLoc, string dstLoc);
        public void SaveDialogData(string fileLocation, IDialog graphInstance, FileCopy customCopy = null)
        {
            // copy current zip and then write json data in
            reWriteRecord(JsonRecordName, Encoding.Unicode.GetBytes(graphInstance.Json));
            FlushReopen();

            linkedArchive.Dispose();
            linkedArchive = null;
            baseStream.Close();
            try
            {
                //File.Move(FileName, fileLocation);
                //LinkFile(fileLocation);
                if (customCopy == null)
                    File.Copy(FileName, fileLocation);
                else
                    customCopy(FileName, fileLocation);// optional encoding
                LinkFile(FileName);
            }
            catch(Exception exc)
            {
                System.Windows.Forms.MessageBox.Show("Error while savings: " + exc.Message);
                LinkFile(FileName);
            }
        }
        #region IDisposable Support
        private bool disposedValue = false; // Для определения избыточных вызовов

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты).
                    linkedArchive.Dispose();
                    baseStream.Close();
                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить ниже метод завершения.
                // TODO: задать большим полям значение NULL.
                linkedArchive = null;
                baseStream = null;
                disposedValue = true;
            }
        }

        // TODO: переопределить метод завершения, только если Dispose(bool disposing) выше включает код для освобождения неуправляемых ресурсов.
        // ~Container()
        // {
        //   // Не изменяйте этот код. Разместите код очистки выше, в методе Dispose(bool disposing).
        //   Dispose(false);
        // }

        // Этот код добавлен для правильной реализации шаблона высвобождаемого класса.
        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки выше, в методе Dispose(bool disposing).
            Dispose(true);
            // TODO: раскомментировать следующую строку, если метод завершения переопределен выше.
            // GC.SuppressFinalize(this);
        }


        #endregion

        Image IImageContainer.GetImageData(string name)
        {
            byte[] recd = this.TakeRecord(name);
            if (recd == null)
                return null;

            return Phrase.byteArrayToImage(recd);
        }

        void IImageContainer.RemoveImageData(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;
            ZipArchiveEntry newEntry;

            if (null != (newEntry = linkedArchive.GetEntry(name)))
            {
                newEntry.Delete();
            }
            FlushReopen();
        }

        string IImageContainer.UpdateImageData(string oldName, Image data)
        {
            if (string.IsNullOrWhiteSpace(oldName))
                oldName = Guid.NewGuid().ToString();
            ZipArchiveEntry newEntry;

            if (null != (newEntry = linkedArchive.GetEntry(oldName)))
            {
                newEntry.Delete();
            }
            newEntry = linkedArchive.CreateEntry(oldName, CompressionLevel.NoCompression);
            Stream es = newEntry.Open();
            data.Save(es, System.Drawing.Imaging.ImageFormat.Png);
            es.Close();

            FlushReopen();

            return oldName;
        }
    }
}
