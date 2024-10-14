using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; // Added for FontFamily

public class CustomMessageBox : Window
{
    public CustomMessageBox(string message, Point location)
    {
        Title = "Message";
        Width = 300;
        WindowStartupLocation = WindowStartupLocation.Manual;
        Left = location.X;
        Top = location.Y;
        ResizeMode = ResizeMode.NoResize;
        WindowStyle = WindowStyle.ToolWindow;

        var stackPanel = new StackPanel
        {
            Margin = new Thickness(10)
        };

        var textBlock = new TextBlock
        {
            Text = message,
            Margin = new Thickness(0, 0, 0, 10),
            FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./UI/#Pretendard Semi Bold"),
            FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 280, // Ensure the text wraps within the window width
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var button = new Button
        {
            Content = "OK",
            Width = 75,
            Height = 30,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./UI/#Pretendard Semi Bold"),
            FontWeight = FontWeights.SemiBold
        };
        button.Click += (s, e) => this.Close();

        stackPanel.Children.Add(textBlock);
        stackPanel.Children.Add(button);

        Content = stackPanel;

        // Calculate the required height based on the text content
        Size textSize = MeasureTextSize(textBlock);
        Height = textSize.Height + 100; // Add extra space for margins and button
    }

    private Size MeasureTextSize(TextBlock textBlock)
    {
        var formattedText = new FormattedText(
            textBlock.Text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
            textBlock.FontSize,
            Brushes.Black,
            new NumberSubstitution(),
            1);

        formattedText.MaxTextWidth = textBlock.MaxWidth;
        return new Size(formattedText.Width, formattedText.Height);
    }
}
