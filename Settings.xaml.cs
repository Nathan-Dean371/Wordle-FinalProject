
using Wordle_FinalProject.Resources.Themes;

namespace Wordle_FinalProject;

public partial class Settings : ContentPage
{
	public Settings()
	{
		InitializeComponent();
        if (Preferences.Default.Get("theme", "light") == "light")
        {
            themePicker.SelectedIndex = 0;
        }
        else
        {
            themePicker.SelectedIndex = 1;
        }
    }

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
}