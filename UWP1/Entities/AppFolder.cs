using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace UWP1.Entities
{
    class AppFolder
    {
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

    }
}
