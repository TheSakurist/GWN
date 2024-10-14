using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls; // Added for Button

namespace GWN
{
    public partial class GridOptionsWindow : Window
    {
        private const string CodeFilePath = "gridCodes.txt";

        public GridOptionsWindow()
        {
            InitializeComponent();
        }

        private void GenerateGridsButton_Click(object sender, RoutedEventArgs e)
        {
            // Generate grid codes in a background thread
            System.Threading.Tasks.Task.Run(() =>
            {
                PreGenerateAllGridCodes();
            }).ContinueWith(t =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Copy the file to the clipboard
                    var fileDropList = new System.Collections.Specialized.StringCollection();
                    fileDropList.Add(Path.GetFullPath(CodeFilePath));
                    Clipboard.SetFileDropList(fileDropList);

                    var button = sender as Button;
                    if (button != null)
                    {
                        var point = button.PointToScreen(new Point(0, 0));
                        var messageBox = new CustomMessageBox("Grid codes generated. The gridCodes.txt file has been copied to the clipboard. Send it to your friend.", point);
                        messageBox.ShowDialog();
                    }
                    this.DialogResult = true; // Close the window and return to MainWindow
                });
            });
        }

        private void SkipGridsButton_Click(object sender, RoutedEventArgs e)
        {
            string destinationPath = "OLDgridCodes.txt";

            if (string.IsNullOrEmpty(destinationPath))
            {
                var button = sender as Button;
                if (button != null)
                {
                    var point = button.PointToScreen(new Point(0, 0));
                    var messageBox = new CustomMessageBox("Destination path is not set.", point);
                    messageBox.ShowDialog();
                }
                return;
            }

            if (!string.IsNullOrEmpty(CodeFilePath) && File.Exists(CodeFilePath))
            {
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }
                File.Move(CodeFilePath, destinationPath);
            }
            else
            {
                var button = sender as Button;
                if (button != null)
                {
                    var point = button.PointToScreen(new Point(0, 0));
                    var messageBox = new CustomMessageBox("CodeFilePath is not set or the file does not exist.", point);
                    messageBox.ShowDialog();
                }
                return;
            }

            var skipButton = sender as Button;
            if (skipButton != null)
            {
                var point = skipButton.PointToScreen(new Point(0, 0));
                var messageBox = new CustomMessageBox("Please paste your friend's gridCodes.txt in the same folder as GWN.exe.", point);
                messageBox.ShowDialog();
            }
            this.DialogResult = true; // Close the window and return to MainWindow
        }

        private static void PreGenerateAllGridCodes()
        {
            var allCombinations = new HashSet<string>(); // Use HashSet to avoid duplicates
            var images = LoadImagePaths(); // Load image paths from the Images folder

            for (int i = 0; i < 1000; i++)
            {
                // Shuffle the images and take the first 36 for the grid
                var shuffledImages = images.OrderBy(x => Guid.NewGuid()).Take(36).ToList();

                // Generate a unique grid code based on the selected images
                string gridCode = GenerateGridCode(shuffledImages);

                // Ensure this grid code is unique before adding
                if (!allCombinations.Contains(gridCode))
                {
                    allCombinations.Add(gridCode);
                    File.AppendAllText(CodeFilePath, $"{gridCode}:{string.Join(",", shuffledImages)}\n");
                }
                else
                {
                    i--; // Decrement the counter to ensure we generate 1000 unique combinations
                }
            }
        }

        private static string GenerateGridCode(List<string> selectedImages)
        {
            // Generate a unique 8-character alphanumeric code based on the selected images
            string combined = string.Join("", selectedImages.Select(img => Path.GetFileNameWithoutExtension(img)));
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
            return Convert.ToBase64String(hash)[..8]; // Simplified Substring
        }

        private static List<string> LoadImagePaths()
        {
            string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            return Directory.GetFiles(imagesFolder, "*.png")
                            .Select(Path.GetFileName)
                            .Where(fileName => !string.IsNullOrEmpty(fileName))
                            .Select(fileName => fileName!) // Use the null-forgiving operator
                            .ToList(); // Ensure this returns List<string>
        }
    }
}
