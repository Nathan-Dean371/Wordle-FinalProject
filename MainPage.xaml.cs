using System.Diagnostics;
using System.IO;
using Wordle_FinalProject.Resources.Themes;



namespace Wordle_FinalProject;

public partial class MainPage : ContentPage
{
    private static string mainDir = FileSystem.Current.AppDataDirectory;
    private static string fileName = "nappistate.txt";
    private static string filePath = System.IO.Path.Combine(mainDir, fileName);

    HttpClient httpClient;

    public MainPage()
	{
        httpClient = new HttpClient();

		InitializeComponent();

        Get_Saved_Theme();
        //Dispatcher.DispatchAsync(async () => await ReadFile());
        
        BindingContext = this;

    }

    //Load the saved theme and set it
    private void Get_Saved_Theme()
    {
        if(Preferences.Default.Get("theme", "light") == "light")
        {
            ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            if (mergedDictionaries != null)
            {
                mergedDictionaries.Clear();
                mergedDictionaries.Add(new LightTheme());
            }
        }
        else
        {
            ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            if (mergedDictionaries != null)
            {
                mergedDictionaries.Clear();
                mergedDictionaries.Add(new DarkTheme());
            }
        }
    }

    //When the start button is clicked, read the file and then navigate to the game screen
    private async void Start_Button_Clicked(object sender, EventArgs e)
    {
        await Task.Run(() => ReadFile());
        //await ReadFile().ConfigureAwait(false); ;
        await Navigation.PushAsync(new Gamescreen());
        
    }

    //When the scores button is clicked, navigate to the scores screen
    private async void Scores_Button_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Scores());
    }

    //When the settings button is clicked, navigate to the settings page
    private void Settings_Button_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new Settings());
    }

    //Read the file from the github repo and save it to the app data directory, if already saved read from the device
    private async Task<string> ReadFile()
    {
        if (!File.Exists(System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, fileName)))
        {
            try
            {
                var response = await httpClient.GetAsync("https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt");

                if (response.IsSuccessStatusCode)
                {
                    String result = await response.Content.ReadAsStringAsync();
                    await WriteTextToFile(result, fileName);
                }

            }
            catch (Exception ex)
            {
                Debug.Print("Cannot download word file.");
            }
        }
        return String.Empty;
    }

    //Write the given text to the given file
    public async Task WriteTextToFile(string text, string targetFileName)
    {
        // Write the file content to the app data directory  
        string targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, targetFileName);
        using FileStream outputStream = System.IO.File.OpenWrite(targetFile);
        using StreamWriter streamWriter = new StreamWriter(outputStream);
        await streamWriter.WriteAsync(text);
    }
}

