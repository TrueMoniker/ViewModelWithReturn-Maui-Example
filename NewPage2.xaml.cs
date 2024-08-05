using CommunityToolkit.Mvvm.Input;

namespace EntryTest;

public partial class NewPage2 : ContentPage
{
	public NewPage2(NewPage2ViewModel newPage2ViewModel)
	{
		InitializeComponent();

        this.BindingContext = newPage2ViewModel;
	}
}

public partial class NewPage2ViewModel : ViewModelWithReturn<MyModel>
{
    [RelayCommand]
    private async Task MyGoBack()
    {
        await GoBackAsync(new MyModel() { MyProperty = 10 });
    }
}