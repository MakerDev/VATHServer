using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VathServer.Interfaces;
using VathServer.Models;
using VathServer.Views;

#if MACCATALYST
using VathServer.Platforms.MacCatalyst;
#endif

namespace VathServer.ViewModels
{
    [QueryProperty(nameof(ContrastValue), "ContrastValue")]
    [QueryProperty(nameof(ScreenSizeInInch), "ScreenSizeInInch")]
    public partial class SessionViewModel : ObservableObject, IQueryAttributable
    {
        private const int MAX_MISS_COUNT = 2;
        private readonly List<double> IMAGE_SIZES = new() { 6, 5.2, 4.2, 3.3, 2.6, 2.1, 1.8, 1.3, 1, 0.8, 0.65, 0.5, 0.4 }; // 각 레벨 별 이미지 크기
        private readonly List<double> EYESIGHT_RESULTS = new() { 0.1, 0.12, 0.16, 0.2, 0.25, 0.32, 0.4, 0.5, 0.63, 0.8, 1, 1.25, 1.6 };
        private readonly IMultipeerManager _multipeerManager;
        private List<int> IMAGE_NUMBERS = new() { 2, 3, 5, 6, 9 };

        private int _currentLevel = 0;
        private int _currentImageIndex = -1;
        private int _levelMissCount = 0; // The number of wrong answers for a certain level.

        public double ContrastValue { get; set; } = 1;
        public double ScreenSizeInInch { get; set; } = 14;
        public int NumImagesToDisplay { get; set; } = 3;

        [ObservableProperty]
        private bool _isDummyHeaderVisible = true;
        [ObservableProperty]
        private ObservableCollection<ImageModel> _numberImages = new();
        [ObservableProperty]
        private ObservableCollection<ImageModel> _indicatorImages = new();

        public SessionViewModel(IMultipeerManager multipeerManager)
        {
            _multipeerManager = multipeerManager;
            _multipeerManager.OnDataReceived += OnDataReceived;
        }

        private void OnDataReceived(string message)
        {
            Console.WriteLine(message);
            var commandParam = message.Split(' ');
            var command = commandParam[0];
            var param = commandParam[1];

            switch (command)
            {
                case "Answer":
                    var answer = int.Parse(param);
                    var isCorrect = IMAGE_NUMBERS[_currentImageIndex] == answer;

                    if (isCorrect)
                    {
                        MainThread.BeginInvokeOnMainThread(async () => await MoveToNextLevel());
                    }
                    else
                    {
                        _levelMissCount++;

                        if (_levelMissCount >= MAX_MISS_COUNT)
                        {
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                await MoveToNextLevel(endTest: true);
                            });

                            return;
                        }
                        else
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                SelectTarget();
                                ChangeImagesWithSize(shuffle: false);
                                SetupIndicator();
                                OnPropertyChanged(nameof(IndicatorImages));
                            });
                        }

                    }

                    _multipeerManager.SendData(isCorrect.ToString());

                    break;
            }
        }

        private async Task MoveToNextLevel(bool endTest = false)
        {
            //TODO: Display effect
            _currentLevel++;
            _levelMissCount = 0;

            if (endTest || _currentLevel >= IMAGE_SIZES.Count)
            {
                //TODO: Move to end page and nofity done
                var result = EYESIGHT_RESULTS[_currentLevel - 1];
                _multipeerManager.SendData($"end {result}");

                var navigationParameter = new Dictionary<string, object>()
                {
                    { nameof(result), result }
                };

                await Shell.Current.GoToAsync(nameof(FinalResultView), navigationParameter);
                return;
            }

            ChangeImageSet();
        }

        private void SelectTarget()
        {
            var newTargetIndex = new Random().Next(NumImagesToDisplay);
            while (newTargetIndex == _currentImageIndex)
            {
                newTargetIndex = new Random().Next(NumImagesToDisplay);
            }

            _currentImageIndex = newTargetIndex;
        }

        private void ChangeImageSet()
        {
            var density = DeviceDisplay.Current.MainDisplayInfo.Density;
            var width = DeviceDisplay.Current.MainDisplayInfo.Width / density;
            var height = DeviceDisplay.Current.MainDisplayInfo.Height / density;

            var pixels = ConvertCentimetersToPixels(IMAGE_SIZES[_currentLevel]);

            NumImagesToDisplay = (int)(width / pixels / 2);


            IsDummyHeaderVisible = height / pixels > 3;


            SelectTarget();
            ChangeImagesWithSize();
            SetupIndicator();
            OnPropertyChanged(nameof(NumberImages));
            OnPropertyChanged(nameof(IndicatorImages));
        }

        private void SetupIndicator()
        {
            IndicatorImages.Clear();

            for (int i = 0; i < NumImagesToDisplay * 2 - 1; i++)
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

            for (int i = 0; i < NumImagesToDisplay * 2 - 1; i++)
            {
                ImageModel image = new()
                {
                    ImageUrl = $"img{IMAGE_NUMBERS[i / 2]}.png",
                    Width = ConvertCentimetersToPixels(IMAGE_SIZES[_currentLevel]),
                    Height = ConvertCentimetersToPixels(IMAGE_SIZES[_currentLevel]),
                    Opacity = ContrastValue
                };

                if (i / 2 == _currentImageIndex)
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
            double scaleFactor = DeviceDisplay.Current.MainDisplayInfo.Density;

            double inches = centimeters / 2.54;
            var pixels = inches * dpi / scaleFactor;

            return pixels;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            ContrastValue = (double)query[nameof(ContrastValue)];
            ScreenSizeInInch = (double)query[nameof(ScreenSizeInInch)];

            // Generate first image set
            ChangeImageSet();
        }
    }
}
