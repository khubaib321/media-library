using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using UWP1.Helpers;
using UWP1.Entities;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWP1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private String selectedColor = "defaultBackgroundGV1";
        private List<AppFolder> openedAppFolders = new List<AppFolder>();
        private AppDataStorage appDataStorage = new AppDataStorage();
        private Windows.UI.Color defaultBackgroundGV1 = Windows.UI.Colors.BlueViolet;
        private Dictionary<String, object> saveData = new Dictionary<string, object>();
        private Dictionary<string, Windows.UI.Color> colorMappings = new Dictionary<string, Windows.UI.Color>();
        // properties to save
        List<String> propertyList = new List<String> {
            "GV1_BackgroundColor",
            "GV_SavedFolders_List"
        };

        public MainPage()
        {
            this.InitializeComponent();
            this.appDataStorage.registerForSave(propertyList);
            this.colorMappings.Add("red", Windows.UI.Colors.Red);
            this.colorMappings.Add("green", Windows.UI.Colors.Green);
            this.colorMappings.Add("blue", Windows.UI.Colors.Blue);
            this.colorMappings.Add("purple", Windows.UI.Colors.Purple);
            this.colorMappings.Add("black", Windows.UI.Colors.Black);
            this.colorMappings.Add("defaultBackgroundGV1", Windows.UI.Colors.BlueViolet);
            this.loadDataFromLocalStore();
        }

        private void loadDataFromLocalStore()
        {
            this.saveData = this.appDataStorage.fetchAppData();
            // apply saved background color
            this.changeGV1Background(this.saveData["GV1_BackgroundColor"] as String);
            // load saved folder location and display in list
            this.openedAppFolders = new List<AppFolder>();
            String savedFolderLocationsConcat = this.saveData["GV_SavedFolders_List"] as String;
            if (savedFolderLocationsConcat != null)
            {
                // saved folder locations are stored as | separated string
                String[] savedFolderLocations = savedFolderLocationsConcat.Split(new char[] { '|' });
                if (savedFolderLocations.Length > 0)
                {
                    // create new AppFolder for each location
                    foreach (String location in savedFolderLocations)
                        if (!String.IsNullOrEmpty(location))
                            this.openedAppFolders.Add(new AppFolder(location));
                }
            }
            this.refreshOpenedFolders();
        }

        private void SaveAndClose(object sender, RoutedEventArgs e)
        {
            this.prepareOpenedFoldersForSave();
            this.prepareGV1BackgroundColorForSave();
            // if save fails, then app won't exit
            if (this.appDataStorage.commitAppData())
                CoreApplication.Exit();
            else
                Debug.WriteLine("Failed To Save App Data.");

        }

        private void LV1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String itemType = e.AddedItems.First().GetType().ToString();
            if (itemType != "Windows.UI.Xaml.Controls.ListViewItem")
            {
                // only work on list view items
                Debug.WriteLine("Unsupported Item Type " + itemType);
                return;
            }
            
            ListViewItem lvItem = (ListViewItem)e.AddedItems.First();
            this.changeGV1Background(lvItem.Name);
        }

        private bool changeGV1Background(String colorName)
        {
            Windows.UI.Color colorCode;
            if (this.colorMappings.TryGetValue(colorName, out colorCode))
            {
                // if color found, set as gv1 grid background
                this.GV1.Background = new SolidColorBrush(colorCode);
                this.selectedColor = colorName;
                return true;
            }
            return false;
        }

        private void ResetColor(object sender, RoutedEventArgs e)
        {
            this.selectedColor = "defaultBackgroundGV1";
            this.GV1.Background = new SolidColorBrush(this.defaultBackgroundGV1);
        }

        private async void OpenFolderAsync(object sender, RoutedEventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                Debug.WriteLine("Folder selected: " + folder.Name);
                // create new app folder and add it to opened folders list
                AppFolder newFolder = new AppFolder(folder.Path);
                this.openedAppFolders.Add(newFolder);
                this.refreshOpenedFolders();
            }
        }

        public void refreshOpenedFolders()
        {
            this.GV_SavedFolders_LV1.ItemsSource = null;
            this.GV_SavedFolders_LV1.ItemsSource = this.openedAppFolders;
        }

        private void prepareOpenedFoldersForSave()
        {
            this.saveData["GV_SavedFolders_List"] = null;
            this.saveData["GV_SavedFolders_List"] = AppFolder.getFolderLocations(this.openedAppFolders);
            this.appDataStorage.prepareForSave(this.saveData);
        }

        private void prepareGV1BackgroundColorForSave()
        {
            this.saveData["GV1_BackgroundColor"] = this.selectedColor;
            this.appDataStorage.prepareForSave(this.saveData);
        }

        private void RemoveFolder(object sender, RoutedEventArgs e)
        {
            List<String> foldersToRemove = new List<string>();
            foldersToRemove.Add((sender as Button).CommandParameter as String);
            this.openedAppFolders = AppFolder.findAndRemoveFolders(this.openedAppFolders, foldersToRemove);
            this.refreshOpenedFolders();
        }
    }
}
