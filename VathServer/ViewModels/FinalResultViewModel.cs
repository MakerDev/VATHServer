using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VathServer.ViewModels
{
    public partial class FinalResultViewModel : ObservableObject, IQueryAttributable
    {
        [ObservableProperty]
        private string _resultPrompt = "";

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            var result = (double)query["result"];
            var targetEye = (string)query["target"];

            ResultPrompt = $"{targetEye}눈의 시력은 {result:.1f}입니다";
        }
    }
}
