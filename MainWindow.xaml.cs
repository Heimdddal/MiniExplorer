using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace MiniExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private List<FileInfo> recentlyOpenedFiles = new List<FileInfo>();
        public MainWindow()
        {
            InitializeComponent();
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                ChooseDisk.Items.Add(drive.Name + " drive capacity " + BytesToGigabytes(drive.TotalSize) + "Gb\nFree space " +
                                     BytesToGigabytes(drive.TotalFreeSpace) + "Gb");
            }
        }

        List<FileInfo> checkFilesTime(List<FileInfo> recentlyFiles)
        {
            foreach (var file in recentlyFiles.ToArray())
            {
                var currentTime = DateTime.Now;
                
                if ((currentTime - file.LastAccessTime).Seconds > 10 || (currentTime - file.LastAccessTime).Seconds < 0)
                {
                    recentlyFiles.Remove(file);
                }
            }

            return recentlyFiles;
        } 
        double BytesToGigabytes(double size)
        {
            return Math.Round(size / (1024 * 1024 * 1024), 1);
        }
        private void ChooseDisk_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var diskName = $@"{ChooseDisk.SelectedItem.ToString().Split(' ')[0]}";
            Dirs.Items.Clear();
            
            if (Directory.Exists(diskName))
            {
                foreach (var dir in Directory.GetDirectories(diskName))
                {
                    var directoryInfo = new DirectoryInfo(dir);
                    Dirs.Items.Add(directoryInfo.FullName + "\n" + directoryInfo.CreationTime + "\n" + directoryInfo.Root);
                }
            }
            else
            {
                Dirs.Items.Add($"Диск {diskName} не найден");
            }
        }

        private void Dirs_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Dirs.SelectedItem != null)
            {
                var selectedDir = Dirs.SelectedItem.ToString().Split('\n')[0];
                Files.Items.Clear();
            
                try
                {
                    if (Directory.Exists(selectedDir))
                    {
                        foreach (var file in Directory.GetFiles(selectedDir))
                        {
                            var fileInfo = new FileInfo(file);
                            Files.Items.Add(fileInfo.FullName);
                        }
                    }
                    else
                    {
                        Files.Items.Add($"Папка {selectedDir} не найдена");
                    }
                }
                catch (System.UnauthorizedAccessException)
                {
                    Files.Items.Add($"Папке {selectedDir} отказано в доступе");
                }
            }
        }

        private void Files_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Files.SelectedItem != null)
            {
                var selectedFile = $@"{Files.SelectedItem}";
                    
                recentlyOpenedFiles.Add(new FileInfo(selectedFile));
                
                Process.Start(selectedFile);
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            recentlyOpenedFiles =  checkFilesTime(recentlyOpenedFiles);

            using (var sw = new StreamWriter("C:\\Users\\dimas\\OneDrive\\Рабочий стол\\MiniExplorerFiles.txt"))
            {
                foreach (var file in recentlyOpenedFiles)
                {
                    sw.WriteLine(file.FullName + "\t" + file.LastAccessTime);
                }
            }
        }
    }
}