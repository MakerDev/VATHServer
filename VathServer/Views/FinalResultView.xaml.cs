using VathServer.ViewModels;

namespace VathServer.Views;

public partial class FinalResultView : ContentPage
{
	public FinalResultView(FinalResultViewModel finalResultViewModel)
	{
		InitializeComponent();

		BindingContext = finalResultViewModel;
	}
}