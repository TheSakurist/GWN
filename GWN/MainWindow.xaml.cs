using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GWN
{
    public partial class MainWindow : Window
    {
        private readonly List<string> imagePaths;
        private readonly Random random;
        private const string CodeFilePath = "gridCodes.txt"; // Path to store the codes
        private Image? rightClickedImage; // Track the last right-clicked image

        public MainWindow()
        {
            InitializeComponent();
            random = new Random();
            imagePaths = LoadImagePaths();
            SetPlaceholderNikkeImageBox(); // Set placeholder on initialization
        }

        private static List<string> LoadImagePaths()
        {
            string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            return Directory.GetFiles(imagesFolder, "*.png")
                            .Select(Path.GetFileName)
                            .Where(fileName => !string.IsNullOrEmpty(fileName))
                            .Select(fileName => fileName!)
                            .ToList(); // Ensure this returns List<string>
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Show grid options window on startup
            ShowGridOptionsPopup();
        }

        private void ShowCustomMessageBox(string message)
        {
            Point location = new Point(Left + (Width / 2) - 150, Top + (Height / 2) - 50); // Center the message box
            var customMessageBox = new CustomMessageBox(message, location);
            customMessageBox.ShowDialog();
        }

        private void ShowGridOptionsPopup()
        {
            var gridOptionsWindow = new GridOptionsWindow
            {
                Owner = this, // Set the owner to center the window
                WindowStartupLocation = WindowStartupLocation.CenterOwner // Center it relative to the owner
            };
            gridOptionsWindow.ShowDialog(); // Show the pop-up
        }

        private void RandomizeImages()
        {
            // Check if the gridCodes.txt file exists
            if (!File.Exists(CodeFilePath))
            {
                ShowCustomMessageBox("gridCodes.txt not found. Please generate grid codes first.");
                return;
            }

            // Read all grid codes from the file
            var gridCodes = File.ReadAllLines(CodeFilePath);
            if (gridCodes.Length == 0)
            {
                ShowCustomMessageBox("No grid codes available. Please generate grid codes first.");
                return;
            }

            // Select a random grid code
            var randomGridCode = gridCodes[random.Next(gridCodes.Length)];
            string[] parts = randomGridCode.Split(':');
            string[] selectedImages = parts[1].Split(',');

            // Clear the existing images
            imageGrid.Children.Clear();

            // Load the images associated with the selected grid code
            foreach (var imagePath in selectedImages)
            {
                var img = new Image
                {
                    Source = new BitmapImage(new Uri(Path.Combine("Images", imagePath), UriKind.Relative)),
                    Stretch = System.Windows.Media.Stretch.Uniform
                };

                // Handle left and right-clicks
                img.MouseRightButtonDown += Image_MouseRightButtonDown;
                img.MouseLeftButtonDown += Image_MouseLeftButtonDown;

                imageGrid.Children.Add(img);
            }

            // Display the selected grid code
            gridCodeDisplay.Text = parts[0];

            // Clear the image in "Your Nikke" when grid is randomized
            ClearNikkeImageBox();
        }

        private static string GenerateGridCode(List<string> selectedImages)
        {
            // Generate a unique 8-character alphanumeric code based on the selected images
            string combined = string.Join("", selectedImages.Select(img => Path.GetFileNameWithoutExtension(img)));
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
            return Convert.ToBase64String(hash)[..8]; // Simplified Substring
        }

        private static void SaveGridCode(string gridCode, List<string> images)
        {
            File.AppendAllText(CodeFilePath, $"{gridCode}:{string.Join(",", images)}\n");
        }

        private void LoadGridFromCode_Click(object sender, RoutedEventArgs e)
        {
            string code = gridCodeTextBox.Text.Trim();
            if (code.Length != 8)
            {
                ShowCustomMessageBox("Invalid code. Please enter an 8-character code.");
                return;
            }

            var gridData = File.ReadLines(CodeFilePath)
                               .FirstOrDefault(line => line.StartsWith(code));
            if (gridData == null)
            {
                ShowCustomMessageBox("Invalid code. Please ensure the code exists.");
                return;
            }

            string[] parts = gridData.Split(':');
            string[] selectedImages = parts[1].Split(',');

            imageGrid.Children.Clear();

            foreach (var imagePath in selectedImages)
            {
                var img = new Image
                {
                    Source = new BitmapImage(new Uri(Path.Combine("Images", imagePath), UriKind.Relative)),
                    Stretch = System.Windows.Media.Stretch.Uniform
                };

                // Handle left and right-clicks
                img.MouseRightButtonDown += Image_MouseRightButtonDown;
                img.MouseLeftButtonDown += Image_MouseLeftButtonDown;

                imageGrid.Children.Add(img);
            }

            gridCodeDisplay.Text = code; // Display the loaded code
            ClearNikkeImageBox(); // Clear the image in "Your Nikke" when grid is loaded
        }

        private void CopyCode_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(gridCodeDisplay.Text);
            // Removed the MessageBox popup
        }

        private void GridCodeTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (gridCodeTextBox.Text == "Enter grid code here")
            {
                gridCodeTextBox.Text = "";
            }
        }

        private void GridCodeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(gridCodeTextBox.Text))
            {
                gridCodeTextBox.Text = "Enter grid code here";
            }
        }

        private void RandomizeImages_Click(object sender, RoutedEventArgs e)
        {
            RandomizeImages();
        }

        private void Image_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Image clickedImage)
            {
                // Check if the clicked image is already in the "Your Nikke" box
                if (rightClickedImage == clickedImage)
                {
                    ClearNikkeImageBox(); // Remove image if clicked again
                }
                else
                {
                    rightClickedImage = clickedImage; // Set as the current right-clicked image
                    nikkeImageBox.Source = clickedImage.Source; // Copy the image into the "Your Nikke" box
                }
            }
        }

        private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Image clickedImage)
            {
                // Toggle opacity between 40% and 100%
                if (clickedImage.Opacity == 1.0)
                {
                    clickedImage.Opacity = 0.4; // Reduce opacity
                    clickedImage.RenderTransformOrigin = new Point(0.5, 0.5); // Set origin to center
                    clickedImage.RenderTransform = new ScaleTransform(0.9, 0.9); // Scale down to 90%
                }
                else
                {
                    clickedImage.Opacity = 1.0; // Restore opacity
                    clickedImage.RenderTransform = new ScaleTransform(1, 1); // Restore scale
                }
            }
        }

        private void ClearNikkeImageBox()
        {
            rightClickedImage = null;
            SetPlaceholderNikkeImageBox(); // Set the placeholder when the box is cleared
        }

        private void SetPlaceholderNikkeImageBox()
        {
            nikkeImageBox.Source = new BitmapImage(new Uri("UI/nikke_placeholder.png", UriKind.Relative)); // Set the placeholder image
        }

        private void GridOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            // Create and show the grid options window
            ShowGridOptionsPopup();
        }

        private void gridCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void gridCodeTextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }
    }
}
