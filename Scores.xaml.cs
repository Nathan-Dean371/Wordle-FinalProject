using System.Collections.ObjectModel;

namespace Wordle_FinalProject;


public partial class Scores : ContentPage
{
    private static string mainDir = FileSystem.Current.AppDataDirectory;
    private static string scoresFileName = "scores.txt";
    private static string scoresPath = System.IO.Path.Combine(mainDir, scoresFileName);

    


	public Scores()
	{
		InitializeComponent();

        Dispatcher.DispatchAsync(async () => await ReadFile());

        BindingContext = this;

    }

    public ObservableCollection<ScoreTable> ScoreList { get; set; } = new ObservableCollection<ScoreTable>();
    

    //Load date and time, word and guesses taken data from file as an observable collection
    private async Task<string> ReadFile()
	{
        //If scores.txt file doesn't exist, display "no scores" message
        if (!File.Exists(System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, scoresFileName)))
        {
           await DisplayAlert("Error", "No scores to display", "OK");
           await Navigation.PopAsync();
           return null;
            
        }


        var fileText = await File.ReadAllTextAsync(scoresPath);

        var lines = fileText.Split('\n');

        foreach (var line in lines)
        {
            var parts = line.Split('-');

            
                ScoreList.Add(new ScoreTable
                {
                    DateTime = parts[0],
                    Word = parts[1],
                    NumberOfGuesses = int.Parse(parts[2])
                });
            
        }

        return fileText;
    }

    private void Back_Button_Clicked(object sender, EventArgs e)
    {
        //Pop current page off navigation stack
        Navigation.PopAsync();
    }


}

	