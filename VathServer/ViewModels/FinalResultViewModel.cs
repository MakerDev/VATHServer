using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VathServer.Interfaces;

namespace VathServer.ViewModels
{
    public partial class FinalResultViewModel : ObservableObject, IQueryAttributable
    {
        [ObservableProperty]
        private string _resultPrompt = "";
        private readonly IMultipeerManager _multipeerManager;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            var result = (double)query["result"];

            ResultPrompt = $"시력은 {result:F2}입니다";
        }

        public FinalResultViewModel(IMultipeerManager multipeerManager)
        {
            _multipeerManager = multipeerManager;
            _multipeerManager.OnDataReceived += OnDataReceived;
        }

        private void OnDataReceived(string message)
        {
            Console.WriteLine(message);
            var commandParam = message.Split(' ');
            var command = commandParam[0];

            switch (command)
            {
                case "redo":
                    MainThread.BeginInvokeOnMainThread(async () => await TestAgain());

                    break;
            }
        }

        private async Task TestAgain()
        {
            _multipeerManager.OnDataReceived -= OnDataReceived;
            var param = SettingPageViewModel.GetSesseionNavigationParam();
            await Shell.Current.GoToAsync(nameof(SessionView), param);
        }
    }
}
