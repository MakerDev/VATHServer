using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VathServer.Models;

#if MACCATALYST
using VathServer.Platforms.MacCatalyst;
#endif

namespace VathServer.ViewModels
{
    public partial class SessionViewModel : ObservableObject
    {
        private const double DEFAULT_SIZE_IN_CENTIMETERS = 5;
        private const double DEFAULT_CONTRAST_VALUE = 1.0f;
        private const float DEFAULT_INCH_VALUE = 14;

        private List<int> IMAGE_NUMBERS = new() { 2, 3, 5 };

        private double _currentSizeCm = DEFAULT_SIZE_IN_CENTIMETERS;
        private double _currentOpacity = DEFAULT_CONTRAST_VALUE;
        private int _currentImageIndex = -1;

        [ObservableProperty]
        private double _contrastValue = 1.0;
        [ObservableProperty]
        private double _screenSizeInInch = 14;
        [ObservableProperty]
        private double _imageSizeInCm = 5;
        [ObservableProperty]
        private int _targetNumber = 0;

        [ObservableProperty]
        private ObservableCollection<ImageModel> _numberImages = new();
        [ObservableProperty]
        private ObservableCollection<ImageModel> _indicatorImages = new();

        public SessionViewModel()
        {
            //// Create the Image controls
            ChangeImagesWithSize();

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
                    ChangeImageSet(null);
                });
                break;

            case "ChangeSize":
                _currentSizeCm = int.Parse(param);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ChangeImagesWithSize();
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
            ChangeImagesWithSize(shuffle: false);

            for (int i = 0; i < IMAGE_NUMBERS.Count; i++)
            {
                // *2 is needed as dummy images exist.
                NumberImages[i * 2].Opacity = 1.0;
            }

            NumberImages[_currentImageIndex * 2].Opacity = _currentOpacity;
            OnPropertyChanged(nameof(NumberImages));
        }

        [RelayCommand]
        private void SelectTarget(object obj)
        {
            _currentImageIndex = TargetNumber;

            if (_currentImageIndex < 0 && _currentImageIndex > IMAGE_NUMBERS.Count - 1)
            {
                _currentImageIndex = new Random().Next(IMAGE_NUMBERS.Count);
            }
        }

        [RelayCommand]
        private void ChangeImageSet(object obj)
        {
            SelectTarget(obj);
            ChangeImagesWithSize();
            SetupIndicator();
            OnPropertyChanged(nameof(NumberImages));
            OnPropertyChanged(nameof(IndicatorImages));
        }

        private void SetupIndicator()
        {
            if (IndicatorImages.Count <= 0)
            {
                for (int i = 0; i < IMAGE_NUMBERS.Count * 2 - 1; i++)
                {
                    ImageModel indicator = new()
                    {
                        ImageUrl = "pointing_hand.png",
                        Width = ConvertCentimetersToPixels(_currentSizeCm),
                        Height = ConvertCentimetersToPixels(_currentSizeCm),
                        Opacity = 0.0
                    };

                    IndicatorImages.Add(indicator);
                }
            }

            for (int i = 0; i < IMAGE_NUMBERS.Count; i++)
            {
                IndicatorImages[i * 2].Opacity = 0.0;
            }


            IndicatorImages[_currentImageIndex * 2].Opacity = 1.0;
            return;
        }

        private void ChangeImagesWithSize(bool shuffle = true)
        {
            NumberImages.Clear();

            if (shuffle)
            {
                IMAGE_NUMBERS = IMAGE_NUMBERS.OrderBy(a => Guid.NewGuid()).ToList();
            }

            for (int i = 0; i < IMAGE_NUMBERS.Count * 2 - 1; i++)
            {
                ImageModel image = new()
                {
                    ImageUrl = $"img{IMAGE_NUMBERS[i / 2]}.png",
                    Width = ConvertCentimetersToPixels(_currentSizeCm),
                    Height = ConvertCentimetersToPixels(_currentSizeCm),
                    Opacity = 1.0
                };

                if (i % 2 != 0)
                {
                    image.ImageUrl = "img2.png";
                    image.Opacity = 0.0;
                }

                NumberImages.Add(image);
            }
        }

        [RelayCommand]
        private void AdjustContrastForImages()
        {
            var opacity = ContrastValue;
            SelectTarget(opacity);
            ChangeBrightness(opacity);
        }

        private double CalculateDPI(double screenSizeInInch, double width, double height)
        {
            double dPixels = Math.Sqrt(width * width + height * height);
            return dPixels / screenSizeInInch;
        }

        private double ConvertCentimetersToPixels(double centimeters)
        {
            double width = DeviceDisplay.Current.MainDisplayInfo.Width;
            double height = DeviceDisplay.Current.MainDisplayInfo.Height;
            double dpi = CalculateDPI(ScreenSizeInInch, width, height);

            if (DeviceInfo.Platform == DevicePlatform.MacCatalyst)
            {
                dpi = 163;
            }

            double inches = centimeters / 2.54;
            return inches * dpi;
        }
    }
}
