using System;
using System.Linq;
using System.IO;

namespace UWP1.Entities
{
    public class AppFile
    {
        private readonly FileInfo fileInfo;

        public AppFile(String location)
        {
            if (String.IsNullOrEmpty(location) || String.IsNullOrWhiteSpace(location))
                throw new ArgumentException("Invalid file location.");

            if (!File.Exists(location))
                throw new FileNotFoundException("File not found: " + location);

            this.fileInfo = new FileInfo(location);
        }

        public FileStream getFileReadPointer()
        {
            FileStream fileStream = File.OpenRead(this.fileInfo.FullName);

            if (!fileStream.CanRead)
                throw new FileLoadException("Cannot read file: " + this.fileInfo.FullName);

            return fileStream;
        }
    }
}
