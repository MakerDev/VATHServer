using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VathServer.ViewModels
{
    public partial class SettingPageViewModel: ObservableObject
    {
        [RelayCommand]
        private async Task StartSessionAsync()
        {
            await Shell.Current.GoToAsync(nameof(SessionView), true);
        }
    }
}
