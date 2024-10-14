using System;

public class Class1
{
	public Class1()
	{
	}
}
public class CustomMessageBox : Window
{
    public CustomMessageBox(string message, Point location)
    {
        Title = "Message";
        Content = new TextBlock
        {
            Text = message,
            Margin = new Thickness(10)
        };
        Width = 300;
        Height = 150;
        WindowStartupLocation = WindowStartupLocation.Manual;
        Left = location.X;
        Top = location.Y;
    }
}
