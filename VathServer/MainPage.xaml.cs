using Microsoft.Maui.Controls;
using System;
using Microsoft.Maui.Graphics;
using System;
using System.Linq;
using Microsoft.VisualBasic;
#if MACCATALYST
using VathServer.Platforms.MacCatalyst;
#endif

namespace VathServer;

public class AdjustedContrastImageEffect : RoutingEffect
{
    public float ContrastValue { get; set; }

    public AdjustedContrastImageEffect() : base($"VathServer.{nameof(AdjustedContrastImageEffect)}")
    {
    }
}


public partial class MainPage : ContentPage
{
    private const double IMAGE_WIDTH_IN_CENTIMETERS = 5;
    private const double IMAGE_HEIGHT_IN_CENTIMETERS = 5;
    private const double DEFAULT_CONTRAST_VALUE = 1.0f;
    private const float DEFAULT_INCH_VALUE = 14;

    private readonly StackLayout _imagesLayout;
    private readonly Entry _contrastEntry;
    private readonly Entry _screenSizeEntry;
    private readonly Label _debugLabel;

    private double _currentSizeCm = IMAGE_HEIGHT_IN_CENTIMETERS;
    private double _currentOpacity = DEFAULT_CONTRAST_VALUE;
    private int _currentImageNumber = -1;

    public MainPage()
    {
        _imagesLayout = new StackLayout
        {
            Orientation = StackOrientation.Horizontal
        };

        _contrastEntry = new Entry
        {
            Placeholder = "Enter contrast value",
            Keyboard = Keyboard.Numeric,
            Text = DEFAULT_CONTRAST_VALUE.ToString(),
            TextColor = Colors.Black
        };

        _screenSizeEntry = new Entry
        {
            Placeholder = "Enter your screen size in inch",
            Keyboard = Keyboard.Numeric,
            Text = DEFAULT_INCH_VALUE.ToString(),
            TextColor = Colors.Black
        };

        _debugLabel = new Label
        {
            Text = "Debug",
            HorizontalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        Button applyButton = new()
        {
            Text = "Apply",
            Command = new Command(AdjustContrastForImages)
        };

        Button changeImageButton = new()
        {
            Text = "New Image",
            Command = new Command(ChangeImage)
        };

        // Create the Image controls
        AddOrChangeImagesWithSize(IMAGE_HEIGHT_IN_CENTIMETERS);


        Content = new Grid
        {
            Children =
            {
                new StackLayout
                {
                    Children =
                    {
                        _imagesLayout,
                        _contrastEntry,
                        _screenSizeEntry,
                        applyButton,
                        changeImageButton,
                        _debugLabel,
                    },
                    BackgroundColor = Colors.White,
                    HorizontalOptions = LayoutOptions.Center,
                }
            },

            BackgroundColor = Colors.White,
        };

#if MACCATALYST
     MultipeerManager.OnDataReceived += OnMCDataReceived;
#endif
    }

#if MACCATALYST
    private void OnMCDataReceived(string message)
    {
        Console.WriteLine(message);
        var commandParam = message.Split(' ');
        var command = commandParam[0];
        var param = commandParam[1];

        switch(command)
        {
            case "ChangeNumber":
                ChangeImage(null);
                break;

            case "ChangeSize":
                AddOrChangeImagesWithSize(double.Parse(param));
                break;

            case "ChangeBrightness":
                ChangeBrightness(double.Parse(param));
                break;
        }
    }

    private void SendMessage(string message)
    {
        MultipeerManager.SendData(message);
    }
#endif

    private void ChangeBrightness(double scale)
    {
        _currentOpacity = scale;
        AddOrChangeImagesWithSize(_currentSizeCm, scale);
    }

    private void ChangeImage(object obj)
    {
        if (_currentImageNumber == -1)
        {
            _currentImageNumber = new Random().Next(5);
            AddOrChangeImagesWithSize(_currentSizeCm, _currentOpacity);
            return;
        }
        
        _currentImageNumber = new Random().Next(5);

        foreach (Image image in _imagesLayout.Children)
        {
            image.IsVisible = false;
        }

        ((Image)_imagesLayout.Children[_currentImageNumber]).IsVisible = true;
    }

    private void AddOrChangeImagesWithSize(double sizeCm, double withOpacity = 1.0)
    {
        _currentSizeCm = sizeCm;
        _imagesLayout.Children.Clear();

        foreach (var imageNumber in new Collection { 2, 3, 5, 6, 9 })
        {
            Image image = new()
            {
                Source = $"img{imageNumber}.png",
                WidthRequest = ConvertCentimetersToPixels(sizeCm),
                HeightRequest = ConvertCentimetersToPixels(sizeCm),
                IsVisible = false,
                Opacity = withOpacity
            };

            _imagesLayout.Children.Add(image);
        }

        if (_currentImageNumber >= 0)
        {
            ((Image)_imagesLayout.Children[_currentImageNumber]).IsVisible = true;
        }
    }

    private void AdjustContrastForImages()
    {
        if (double.TryParse(_contrastEntry.Text, out double opacity))
        {
            ChangeBrightness(opacity);
        }
        else
        {
            DisplayAlert("Invalid Input", "Please enter a valid contrast value.", "OK");
        }
    }

    private double CalculateDPI(double width, double height)
    {
        if (double.TryParse(_screenSizeEntry.Text, out double screenInches))
        {
            double dPixels = Math.Sqrt(width * width + height * height);
            return dPixels / screenInches;
        }

        return 0;
    }

    private double ConvertCentimetersToPixels(double centimeters)
    {
        double width = DeviceDisplay.Current.MainDisplayInfo.Width;
        double height = DeviceDisplay.Current.MainDisplayInfo.Height;
        double dpi = CalculateDPI(width, height);

        if (DeviceInfo.Platform == DevicePlatform.MacCatalyst)
        {
            dpi = 163;
        }

        double inches = centimeters / 2.54;
        return inches * dpi;
    }
}

