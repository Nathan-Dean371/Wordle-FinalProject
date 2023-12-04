using System.Diagnostics;


namespace Wordle_FinalProject;

public partial class MainPage : ContentPage
{
    private static string mainDir = FileSystem.Current.AppDataDirectory;
    private static string fileName = "nappistate.txt";
    private static string filePath = System.IO.Path.Combine(mainDir, fileName);

    //public Command GetWordList { get; }

    HttpClient httpClient;

    public MainPage()
	{
        httpClient = new HttpClient();

		InitializeComponent();
        
        //GetWordList = new Command(async () => await ReadFile());

        BindingContext = this;
        
        

    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
		await Navigation.PushAsync(new Gamescreen());
        await ReadFile();
    }

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
                    WriteTextToFile(result, fileName);
                }

            }
            catch (Exception ex)
            {
                Debug.Print("Cannot download word file.");
            }
        }
        return String.Empty;
    }

    public async void WriteTextToFile(string text, string targetFileName)
    {
        // Write the file content to the app data directory  
        string targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, targetFileName);
        using FileStream outputStream = System.IO.File.OpenWrite(targetFile);
        using StreamWriter streamWriter = new StreamWriter(outputStream);
        await streamWriter.WriteAsync(text);
    }
}

