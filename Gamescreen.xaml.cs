namespace Wordle_FinalProject;


public partial class Gamescreen : ContentPage
{

    private static string mainDir = FileSystem.Current.AppDataDirectory;
    private static string fileName = "nappistate.txt";
    private static string filePath = System.IO.Path.Combine(mainDir, fileName);
    public string chosenWord;
    private bool wordChosen;
    private string guess;

    public string ChosenWord
    {
        get => chosenWord;
        set
        {
            if (chosenWord == value)
                return; 

            chosenWord = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(chosenWord));  
        }
    }

    public string Guess
    {
        get => guess;
        set
        {
            guess = guessEntry.Text;
            OnPropertyChanged();
            OnPropertyChanged(nameof(guess));
        }
    }

    public Gamescreen()
	{
		InitializeComponent();
		BindingContext = this;
        wordChosen = false;
        chooseWord();
	}

    void chooseWord()
    {
        if (wordChosen)
            return;

        string targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, fileName);
        
        var lines = File.ReadAllLines(targetFile);
        Random random = new Random();
        var index = random.Next(0, lines.Length - 1);
        ChosenWord = lines[index];

        wordChosen = true;



        
    }

    void OnGuessChange(object sender, TextChangedEventArgs e)
    {
        Guess = guessEntry.Text;
    }
    


}