using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VathServer.ViewModels
{
    public partial class SettingPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private double _screenSizeInInch = 14;
        [ObservableProperty]
        private double _imageSizeInCm = 5;
        [ObservableProperty]
        private double _contrastValue = 1.0;

        [RelayCommand]
        private async Task StartSessionAsync()
        {
            var navigationParameter = new Dictionary<string, object>()
            {
                { nameof(ScreenSizeInInch), ScreenSizeInInch },
                { nameof(ContrastValue), ContrastValue }
            };
            await Shell.Current.GoToAsync(nameof(SessionView), navigationParameter);
        }
    }
}
