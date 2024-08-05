using System.Reflection;

namespace EntryTest;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(NewPage2), typeof(NewPage2));
    }
}

