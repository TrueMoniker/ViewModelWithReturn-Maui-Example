using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EntryTest;

public partial class NewPage1 : ContentPage
{
	public NewPage1(NewPage1ViewModel newPage1ViewModel)
	{
		InitializeComponent();

        this.BindingContext = newPage1ViewModel;
    }
}

public partial class NewPage1ViewModel : ObservableObject
{
    [ObservableProperty]
    MyModel _myResponse;

    public NewPage1ViewModel()
    {
        MyResponse = new();
    }

    [RelayCommand]
    private async Task MyGo()
    {
        await NewPage2ViewModel.GoToAndReturn(this,
            (response) =>
            {
                MyResponse = response;
            },
        nameof(NewPage2));
    }
}
