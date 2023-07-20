using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VathServer.Interfaces;
using VathServer.Models;

#if MACCATALYST
using VathServer.Platforms.MacCatalyst;
#endif

namespace VathServer.ViewModels
{
    [QueryProperty(nameof(ContrastValue), "ContrastValue")]
    [QueryProperty(nameof(ScreenSizeInInch), "ScreenSizeInInch")]
    public partial class SessionViewModel : ObservableObject
    {
        private readonly List<double> IMAGE_SIZES = new() { 6, 5.2, 4.2, 3.3, 2.6, 2.1, 1.8, 1.3, 1, 0.8, 0.65, 0.5, 0.4 }; // 각 레벨 별 이미지 크기
        private readonly List<double> EYESIGHT_RESULTS = new() { 0.1, 0.12, 0.16, 0.2, 0.25, 0.32, 0.4, 0.5, 0.63, 0.8, 1, 1.25, 1.6 };
        private readonly IMultipeerManager _multipeerManager;
        private List<int> IMAGE_NUMBERS = new() { 2, 3, 5 };

        private int _currentLevel = 0;
        private int _currentImageIndex = -1;

        public double ContrastValue { get; set; } = 1;
        public double ScreenSizeInInch { get; set; } = 14;

        [ObservableProperty]
        private ObservableCollection<ImageModel> _numberImages = new();
        [ObservableProperty]
        private ObservableCollection<ImageModel> _indicatorImages = new();

        public SessionViewModel(IMultipeerManager multipeerManager)
        {
            // Create the Image controls
            ChangeImageSet();
            _multipeerManager = multipeerManager;
            _multipeerManager.OnDataReceived += OnDataReceived;
#if MACCATALYST
     MacMultipeerManager.OnDataReceived += OnMCDataReceived;
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
            case "Answer":
                var answer = int.Parse(param);
                SendMessage((IMAGE_NUMBERS[_currentImageIndex] == answer).ToString());
                break;
        }
    }

    private void SendMessage(string message)
    {
        MacMultipeerManager.SendData(message);
    }
#endif
        private void OnDataReceived(string message)
        {
            Console.WriteLine(message);
            var commandParam = message.Split(' ');
            var command = commandParam[0];
            var param = commandParam[1];

            //TODO: 이게 Multipeer에서 온 데이터로 UI를 바꾸려고 하면 thread 문제가 발생한다. 해결하기.
            switch (command)
            {
                case "Answer":
                    var answer = int.Parse(param);
                    var isCorrect = IMAGE_NUMBERS[_currentImageIndex] == answer;
                    _multipeerManager.SendData(isCorrect.ToString());

                    if (isCorrect)
                    {
                        MoveToNextLevel();
                    }
                    break;
            }
        }



        private void MoveToNextLevel()
        {
            //TODO: Display effect
            _currentLevel++;

            if (_currentLevel >= IMAGE_SIZES.Count)
            {
                //TODO: Move to end page
            }

            ChangeImageSet();
        }

        private void SelectTarget()
        {
            _currentImageIndex = new Random().Next(IMAGE_NUMBERS.Count);
        }

        [RelayCommand]
        private void ChangeImageSet()
        {
            SelectTarget();
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
                        Width = ConvertCentimetersToPixels(IMAGE_SIZES[_currentLevel]),
                        Height = ConvertCentimetersToPixels(IMAGE_SIZES[_currentLevel]),
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
                    Width = ConvertCentimetersToPixels(IMAGE_SIZES[_currentLevel]),
                    Height = ConvertCentimetersToPixels(IMAGE_SIZES[_currentLevel]),
                    Opacity = ContrastValue
                };

                if (i == _currentImageIndex)
                {
                    image.Opacity = 1.0;
                }

                if (i % 2 != 0)
                {
                    image.ImageUrl = "img2.png";
                    image.Opacity = 0.0;
                }

                NumberImages.Add(image);
            }
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
