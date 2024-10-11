using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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
            RandomizeImages();
        }

        private static List<string> LoadImagePaths()
        {
            string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            return Directory.GetFiles(imagesFolder, "*.png")
                            .Select(Path.GetFileName)
                            .Where(fileName => !string.IsNullOrEmpty(fileName))
                            .ToList()!;
        }

        private void RandomizeImages()
        {
            imageGrid.Children.Clear();
            var selectedImages = imagePaths.OrderBy(x => random.Next()).Take(36).ToList();

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

            string gridCode = GenerateGridCode(selectedImages);
            gridCodeDisplay.Text = gridCode; // Display the code
            SaveGridCode(gridCode, selectedImages); // Save the code with the images

            ClearNikkeImageBox(); // Clear the image in "Your Nikke" when grid is randomized
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
                MessageBox.Show("Invalid code. Please enter an 8-character code.");
                return;
            }

            var gridData = File.ReadLines(CodeFilePath)
                               .FirstOrDefault(line => line.StartsWith(code));
            if (gridData == null)
            {
                MessageBox.Show("Invalid code. Please ensure the code exists.");
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
                clickedImage.Opacity = clickedImage.Opacity == 1.0 ? 0.4 : 1.0;
            }
        }

        private void ClearNikkeImageBox()
        {
            rightClickedImage = null;
            nikkeImageBox.Source = null; // Clear the image box
        }
    }
}
