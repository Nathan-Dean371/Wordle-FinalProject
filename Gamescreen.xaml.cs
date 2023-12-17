namespace Wordle_FinalProject;


public partial class Gamescreen : ContentPage
{

    private static string mainDir = FileSystem.Current.AppDataDirectory;
    private static string fileName = "nappistate.txt";
    private static string filePath = System.IO.Path.Combine(mainDir, fileName);
    public string chosenWord;
    private bool wordChosen;
    private string guess;
    private Timer focusTimer;

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

        focusTimer = new Timer(async _ => await EntryFocus(), null, 0, 500);

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
        Entry entry = (Entry)sender;
        string newText = entry.Text;

        string filteredText = new string(newText.Where(char.IsLetter).ToArray());  

        guessEntry.Text = filteredText;
        Guess = guessEntry.Text;
    }

    private async Task EntryFocus()
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        { 
            guessEntry.Focus();
        });
        
    }
    


}