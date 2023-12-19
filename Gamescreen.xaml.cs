using System.Diagnostics;

namespace Wordle_FinalProject;





public partial class Gamescreen : ContentPage
{

    private static string mainDir = FileSystem.Current.AppDataDirectory;
    private static string fileName = "nappistate.txt";
    private static string filePath = System.IO.Path.Combine(mainDir, fileName);

    private static string playerDataFileName = "playerData.txt";
    private static string playerDataPath = System.IO.Path.Combine(mainDir, playerDataFileName);

    public string chosenWord;
    private bool wordChosen;
    private string guess;
    private bool wordGuessed = false;

    private int gamesPlayed;
    public int GamesPlayed
    {
        get => gamesPlayed;
        set
        {
            gamesPlayed = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(gamesPlayed));
        }
    }
    int gamesWon;
    string winPercentage;
    public string WinPercentage
    {
        get => winPercentage; 
        set 
        {
            winPercentage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(winPercentage));
        }
    }
    
    int currentStreak;
    public int CurrentStreak
    {
        get => currentStreak;
        set
        {
            currentStreak = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(currentStreak));
        }
    }
    int longestStreak;
    public int LongestStreak
    {
        get => longestStreak;
        set
        {
            longestStreak = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(longestStreak));
        }
    }

    private string guessFeedbackString = "";

    private Timer focusTimer;

    private int guessCount = 0;

    public string GuessFeedbackString
    {
        get => guessFeedbackString;
        set
        {
            guessFeedbackString = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(guessFeedbackString));
        }
    }

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
        ReadPlayerData();
		BindingContext = this;
        wordChosen = false;
        chooseWord();

        focusTimer = new Timer(async _ => await EntryFocus(), null, 0, 500);

        AddBindingToRow(0);

	}

    void chooseWord()
    {
        if (wordChosen)
            return;

        if (File.Exists(filePath))
        {

            string targetFile = filePath;

            var lines = File.ReadAllLines(targetFile);
            Random random = new Random();
            var index = random.Next(0, lines.Length - 1);
            ChosenWord = lines[index];

            wordChosen = true;

        }
        else
        {
            chooseWord();
        }
    }

    void OnGuessChange(object sender, TextChangedEventArgs e)
    {
        Entry entry = (Entry)sender;
        string newText = entry.Text;

        string filteredText = new string(newText.Where(char.IsLetter).ToArray());  

        guessEntry.Text = filteredText;
        Guess = guessEntry.Text;
        GuessFeedbackString = "";
    }

    private async Task EntryFocus()
    {
        if(wordGuessed == false)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                guessEntry.Focus();
            });
        }
    }
    
    private void AddBindingToRow(int row)
    {
        foreach (Frame frame in gameboard.Children.Where(child => gameboard.GetRow(child) == row - 1))
        {
            frame.Content.RemoveBinding(Label.TextProperty);
        }

        foreach (Frame frame in gameboard.Children.Where(child => gameboard.GetRow(child) == row))
        {
            int i = gameboard.GetColumn(frame);
            Binding binding = new Binding($"Guess[{i}]");
            frame.Content.SetBinding(Label.TextProperty, binding);
        }
    }

    private void Guess_Button_Clicked(object sender, EventArgs e)
    {
        guessCount++;
        AddBindingToRow(guessCount);
    }

    private void GuessEntry_Completed(object sender, EventArgs e)
    {
        if(guessCount == 5)
        {   
            GuessFeedbackString = "No more guesses";

            GamesPlayed++;
            currentStreak = 0;
            UpdateWinPercentage();
            WritePlayerData();
            return;
        }
        if(Guess.Length < 5)
        {
            GuessFeedbackString = "Word too short";
            GuessFeedbackLabel.TextColor = Colors.Red;
            return;
        }

        if(!CheckValidWord(Guess))
        {
            GuessFeedbackString = "Invalid word";
            GuessFeedbackLabel.TextColor = Colors.Red;
            return;
        }
        if (Guess == ChosenWord)
        {
            CheckGuess(Guess);
            
            wordGuessed = true;
            WinOverlay.IsVisible = true;

            gamesWon++;
            GamesPlayed++;
            CurrentStreak++;
            if(currentStreak > longestStreak)
            {
                LongestStreak = currentStreak;
            }
            UpdateWinPercentage();
            WritePlayerData();
            return;
        }
        CheckGuess(Guess);
        guessCount++;
        AddBindingToRow(guessCount);
        guessEntry.Text = "";
        
    }

    private async void Label_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        
        //change parent frames background color to white and add a thin black border
        if (e.PropertyName == nameof(Label.Text))
        {
            Label label = (Label)sender;
            Frame frame = (Frame)label.Parent;  

            

            if(frame != null && (label.Text != "" && label.Text != null))
            {
                frame.BackgroundColor = Colors.White;
                frame.BorderColor = Colors.Black;
                await frame.ScaleTo(1.2, 50, Easing.Linear);
                await frame.ScaleTo(1, 50, Easing.Linear);
            }

            if(frame != null && label.Text == "" || label.Text == null)
            {
                frame.BackgroundColor = Colors.White;
                frame.BorderColor = Colors.Grey;
            }
        }


    }

    //Check if guessed word is a valid guess against the words contained in the wordlist file
    private bool CheckValidWord(string guess)
    {
        string targetFile = filePath;

        var lines = File.ReadAllLines(targetFile);

        foreach (string line in lines)
        {
            if (line == guess.ToLower())
            {
                return true;
            }
        }

        return false;
    }

    //Loop over guessed word, then if letter is contained in the word and is in the correct position turn the frame green, if letter is contained in the word but is in the wrong position turn the frame yellow, if letter is not contained in the word turn the frame grey
    private void CheckGuess(string guess)
    {
        for (int i = 0; i < guess.Length; i++)
        {
            if (guess[i] == ChosenWord[i])
            {
                Frame frame = (Frame)gameboard.Children.Where(child => gameboard.GetRow(child) == guessCount && gameboard.GetColumn(child) == i).FirstOrDefault();
                frame.BackgroundColor = Colors.Green;
            }
            else if (ChosenWord.Contains(guess[i]))
            {
                Frame frame = (Frame)gameboard.Children.Where(child => gameboard.GetRow(child) == guessCount && gameboard.GetColumn(child) == i).FirstOrDefault();
                frame.BackgroundColor = Colors.Yellow;
            }
            else
            {
                Frame frame = (Frame)gameboard.Children.Where(child => gameboard.GetRow(child) == guessCount && gameboard.GetColumn(child) == i).FirstOrDefault();
                frame.BackgroundColor = Colors.Grey;
            }
        }
    }

    private void Close_Overlay_ButtonClicked(object sender, EventArgs e)
    {
        WinOverlay.IsVisible = false;
    }

    private void Settings_Menu_Clicked(object sender, EventArgs e)
    {
        WinOverlay.IsVisible = true;
    }

    private void ReadPlayerData()
    {
        String line;

        if (PlayerFileExists())
        {
            StreamReader playerFile = new StreamReader(playerDataPath);

            line = playerFile.ReadLine();

            //Set games played
            GamesPlayed = Int32.Parse(line);
            line = playerFile.ReadLine();
            gamesWon = Int32.Parse(line);
            line = playerFile.ReadLine();
            currentStreak = Int32.Parse(line);
            line = playerFile.ReadLine();
            longestStreak = Int32.Parse(line);
            playerFile.Close();

        }
        else
        {
            //Create player data file
            StreamWriter playerFile = new StreamWriter(playerDataPath);

            //Write default values to file
            playerFile.WriteLine("0");
            playerFile.WriteLine("0");
            playerFile.WriteLine("0");
            playerFile.WriteLine("0");

            //Close file
            playerFile.Close();
        }

        UpdateWinPercentage();
    }

    bool PlayerFileExists()
    {
        if (!File.Exists(playerDataPath))
        {
            return false;
        }
        return true;
    }

    private void WritePlayerData()
    {
        if(PlayerFileExists())
        {
            StreamWriter playerFile = new StreamWriter(playerDataPath);

            playerFile.WriteLine(GamesPlayed);
            playerFile.WriteLine(gamesWon);
            playerFile.WriteLine(currentStreak);
            playerFile.WriteLine(longestStreak);

            playerFile.Close();
        }
    }

    void UpdateWinPercentage()
    {
        if(gamesPlayed != 0)
            WinPercentage = ((gamesWon / GamesPlayed) * 100) + "%";
        else
            WinPercentage = "0%";

    }
}