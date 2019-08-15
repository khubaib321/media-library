using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace UWP1.Entities
{
    class AppFolder
    {
        public static readonly List<String> allowedFileTypes = new List<string>
        {
            ".wmi",
            ".avi",
            ".mp4",
            ".mkv",
        };
        private DirectoryInfo directoryInfo;

        public AppFolder(String location)
        {
            if (String.IsNullOrEmpty(location) || String.IsNullOrWhiteSpace(location))
                throw new ArgumentException("Invalid file location: " + location);

            this.directoryInfo = new DirectoryInfo(location);
        }

        public String getName()
        {
            return this.directoryInfo.Name;
        }

        public String getLocation()
        {
            return this.directoryInfo.FullName;
        }

        public static String getFolderLocations(List<AppFolder> appFolders)
        {
            // returns folder locations as | separated string to easily save in local storage
            String folderLocations = "";
            foreach(AppFolder appFolder in appFolders)
            {
                if (String.IsNullOrEmpty(folderLocations))
                    folderLocations = appFolder.getLocation();
                else
                    folderLocations += "|" + appFolder.getLocation();
            }
            return folderLocations;
        }

        public static List<AppFolder> findAndRemoveFolders(List<AppFolder> folderList, List<String> locations)
        {
            if (locations.Count == 0)
                return folderList;

            foreach(String location in locations)
                folderList.RemoveAll((appFolder) => appFolder.getLocation() == location);

            return folderList;
        }

        public async Task<List<AppFile>> GetFiles()
        {
            // use storage folder to get all files path (full name basically)
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(this.directoryInfo.FullName);
            IReadOnlyList<StorageFile> fileList = await folder.GetFilesAsync();
            List<AppFile> allFiles = new List<AppFile>();
            foreach (StorageFile file in fileList)
            {
                if (AppFolder.allowedFileTypes.Contains(new FileInfo(file.Path).Extension))
                    allFiles.Add(new AppFile(file.Path));
            }
            return allFiles;
        }

    }
}
