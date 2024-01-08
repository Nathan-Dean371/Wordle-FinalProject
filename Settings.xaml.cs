
using Wordle_FinalProject.Resources.Themes;

namespace Wordle_FinalProject;

public partial class Settings : ContentPage
{
	public Settings()
	{
		InitializeComponent();

        InitializeTheme();

        InitializeHints();
    }


    //Initialize theme picker to current theme
    void InitializeTheme()
    {
        if (Preferences.Default.Get("theme", "light") == "light")
        {
            themePicker.SelectedIndex = 0;
        }
        else
        {
            themePicker.SelectedIndex = 1;
        }
    }

    //Initialize hints switch to current setting
    void InitializeHints()
    {
        if (Preferences.Default.Get("hints", "on") == "on")
        {
            hintsSwitch.IsToggled = true;
        }
        else
        {
            hintsSwitch.IsToggled = false;
        }
    }

    //Change theme based on picker selection
    void OnPickerSelectionChanged(object sender, EventArgs e)
    {
        Picker picker = sender as Picker;
        ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;
        if (mergedDictionaries != null && picker.SelectedIndex == 0)
        {
            mergedDictionaries.Clear();
            mergedDictionaries.Add(new LightTheme());
            Preferences.Default.Set("theme", "light");
        }
        else
        {
            mergedDictionaries.Clear();
            mergedDictionaries.Add(new DarkTheme());
            Preferences.Default.Set("theme", "dark");
        }
    }

    
    private void Back_Button_Clicked(object sender, EventArgs e)
    {
        //Pop current page off navigation stack
        Navigation.PopAsync();
    }

    //Change hints setting based on switch selection
    private void Switch_Toggled(object sender, ToggledEventArgs e)
    {
        if(e.Value)
        {
            Preferences.Default.Set("hints", "on");
        }
        else
        {
            Preferences.Default.Set("hints", "off");
        }
    }
}