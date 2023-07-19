using Microsoft.Maui.Controls;
using System;
using Microsoft.Maui.Graphics;
using System;
using System.Linq;
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
#if MACCATALYST
using VathServer.Platforms.MacCatalyst;
#endif

namespace VathServer;


public partial class MainPage : ContentPage
{
    private const double DEFAULT_SIZE_IN_CENTIMETERS = 5;
    private const double DEFAULT_CONTRAST_VALUE = 1.0f;
    private const float DEFAULT_INCH_VALUE = 14;
    private List<int> IMAGE_NUMBERS = new() { 2, 3, 5 };

    private readonly StackLayout _imagesLayout;
    private readonly StackLayout _indicatorLayout;
    private readonly Entry _contrastEntry;
    private readonly Entry _screenSizeEntry;
    private readonly Entry _imageCmEntry;
    private readonly Entry _targetEntry;
    private readonly Label _debugLabel;

    private double _currentSizeCm = DEFAULT_SIZE_IN_CENTIMETERS;
    private double _currentOpacity = DEFAULT_CONTRAST_VALUE;
    private int _currentImageIndex = -1;

    public MainPage()
    {
        _imagesLayout = new StackLayout
        {
            Orientation = StackOrientation.Horizontal
        };

        _indicatorLayout = new StackLayout
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

        _imageCmEntry = new Entry
        {
            Placeholder = "Enter your image size in cm",
            Keyboard = Keyboard.Numeric,
            Text = DEFAULT_SIZE_IN_CENTIMETERS.ToString(),
            TextColor = Colors.Black
        };

        _targetEntry = new Entry
        {
            Placeholder = "Enter the target number",
            Keyboard = Keyboard.Numeric,
            Text = "",
            TextColor = Colors.Black
        };

        _debugLabel = new Label
        {
            Text = "Debug",
            HorizontalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        Button changeBrightnessButton = new()
        {
            Text = "Change Brightness",
            Command = new Command(AdjustContrastForImages)
        };

        Button changeImageSetButton = new()
        {
            Text = "New Image Set",
            Command = new Command(ChangeImage)
        };

        Button applyTargetButton = new()
        {
            Text = "Apply target",
            Command = new Command(ChangeImage)
        };

        // Create the Image controls
        ShuffleOrChangeImagesWithSize();

        //TODO: 각 엔트리 앞에 설명 넣어주기
        Content = new Grid
        {
            Children =
            {
                new StackLayout
                {
                    Children =
                    {
                        _imagesLayout,
                        _indicatorLayout,
                        _contrastEntry,
                        _screenSizeEntry,
                        _imageCmEntry,
                        _targetEntry,
                        changeBrightnessButton,
                        changeImageSetButton,
                        applyTargetButton,
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

        //TODO: 이게 Multipeer에서 온 데이터로 UI를 바꾸려고 하면 thread 문제가 발생한다. 해결하기.
        switch (command)
        {
            case "ChangeNumber":
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ChangeImage(null);
                });
                break;

            case "ChangeSize":
                _currentSizeCm = int.Parse(param);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ShuffleOrChangeImagesWithSize();
                });
                break;

            case "ChangeBrightness":
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ChangeBrightness(double.Parse(param));
                });
                break;

            case "Answer":
                var answer = int.Parse(param);
                SendMessage((IMAGE_NUMBERS[_currentImageIndex] == answer).ToString());
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
        ShuffleOrChangeImagesWithSize(shuffle: false);

        for (int i = 0; i < IMAGE_NUMBERS.Count; i++)
        {
            // *2 is needed as dummy images exist.
            ((Image)_imagesLayout.Children[i * 2]).Opacity = 1.0;
        }

        ((Image)_imagesLayout.Children[_currentImageIndex * 2]).Opacity = _currentOpacity;
    }

    private void ApplyTarget(object obj)
    {
        var isSuccess = int.TryParse(_targetEntry.Text, out _currentImageIndex);

        if (!isSuccess || (_currentImageIndex < 0 && _currentImageIndex > IMAGE_NUMBERS.Count - 1))
        {
            _currentImageIndex = new Random().Next(IMAGE_NUMBERS.Count);
        }

        _targetEntry.Text = _currentImageIndex.ToString();
    }

    private void ChangeImage(object obj)
    {
        _currentSizeCm = int.Parse(_imageCmEntry.Text);
        //if (_currentImageIndex == -1)
        //{
        //    _currentImageIndex = new Random().Next(5);
        //    ShuffleOrChangeImagesWithSize();
        //    return;
        //}
        ApplyTarget(obj);
        ShuffleOrChangeImagesWithSize();
        SetupIndicator();
    }

    private void SetupIndicator()
    {
        if (_indicatorLayout.Children.Count > 0)
        {
            for (int i = 0; i < IMAGE_NUMBERS.Count; i++)
            {
                ((Image)_indicatorLayout.Children[i * 2]).Opacity = 0.0;
            }


            ((Image)_indicatorLayout.Children[_currentImageIndex * 2]).Opacity = 1.0;

            return;
        }

        for (int i = 0; i < IMAGE_NUMBERS.Count * 2 - 1; i++)
        {
            Image indicator = new()
            {
                Source = "pointing_hand.png",
                WidthRequest = ConvertCentimetersToPixels(_currentSizeCm),
                HeightRequest = ConvertCentimetersToPixels(_currentSizeCm),
                Opacity = 0.0
            };

            _indicatorLayout.Children.Add(indicator);
        }
    }

    private void ShuffleOrChangeImagesWithSize(bool shuffle = true)
    {
        _imagesLayout.Children.Clear();

        if (shuffle)
        {
            IMAGE_NUMBERS = IMAGE_NUMBERS.OrderBy(a => Guid.NewGuid()).ToList();
        }

        for (int i = 0; i < IMAGE_NUMBERS.Count * 2 - 1; i++)
        {
            if (i % 2 == 0)
            {
                Image image = new()
                {
                    Source = $"img{IMAGE_NUMBERS[i / 2]}.png",
                    WidthRequest = ConvertCentimetersToPixels(_currentSizeCm),
                    HeightRequest = ConvertCentimetersToPixels(_currentSizeCm),
                    Opacity = 1.0
                };

                _imagesLayout.Children.Add(image);
            }
            else
            {
                //dummy image
                Image image = new()
                {
                    Source = $"img2.png",
                    WidthRequest = ConvertCentimetersToPixels(_currentSizeCm),
                    HeightRequest = ConvertCentimetersToPixels(_currentSizeCm),
                    Opacity = 0
                };

                _imagesLayout.Children.Add(image);
            }
        }
    }

    private void AdjustContrastForImages()
    {
        if (double.TryParse(_contrastEntry.Text, out double opacity))
        {
            _currentSizeCm = int.Parse(_imageCmEntry.Text);
            ApplyTarget(opacity);
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

