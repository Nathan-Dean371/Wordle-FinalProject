using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using System.Diagnostics;
using System.IO;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using System.Timers; // Specify the namespace for Timer to resolve ambiguity
using System.Linq;
using Microsoft.Maui.Graphics;
using System;

using Application = Microsoft.Maui.Controls.Application;
using Microsoft.Maui.Storage;


namespace Wordle_FinalProject;

public partial class Gamescreen : ContentPage
{

    private static string mainDir = FileSystem.Current.AppDataDirectory;
    private static string fileName = "nappistate.txt";
    private static string filePath = System.IO.Path.Combine(mainDir, fileName);

    private static string playerDataFileName = "playerData.txt";
    private static string playerDataPath = System.IO.Path.Combine(mainDir, playerDataFileName);

    private static string scoresFileName = "scores.txt";
    private static string scoresPath = System.IO.Path.Combine(mainDir, scoresFileName);

    public string chosenWord;
    private bool wordChosen;
    private string guess;
    private bool wordGuessed = false;
    private bool playAgain;
    public bool PlayAgain
    {
        get => playAgain;
        set
        {
            playAgain = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(playAgain));
        }
    }

    private string hintString;

    public string HintString
    {
        get => hintString;
        set
        {

           hintString = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(hintString));
        }
    }

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

    private System.Timers.Timer focusTimer;

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

        #if !ANDROID
        focusTimer = new System.Timers.Timer(500);
        focusTimer.Elapsed += async (sender, e) => await EntryFocus();
        focusTimer.AutoReset = true;
        focusTimer.Start();
        #endif

        AddBindingToRow(0);

        #if ANDROID
        Application.Current.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
        #endif

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
        Microsoft.Maui.Controls.Entry entry = (Microsoft.Maui.Controls.Entry)sender;
        string newText = entry.Text;

        string filteredText = new string(newText.Where(char.IsLetter).ToArray());

        guessEntry.Text = filteredText.ToLower();
        Guess = guessEntry.Text;
        GuessFeedbackString = "";
    }

    private async Task EntryFocus()
    {
        if (wordGuessed == false && WinOverlay.IsVisible == false || guessCount == 5)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                #if !ANDROID
                guessEntry.Focus();
                #endif
            });
        }
    }

    private void AddBindingToRow(int row)
    {
        foreach (Frame frame in gameboard.Children.Where(child => gameboard.GetRow(child) == row - 1))
        {
            frame.Content.RemoveBinding(Label.TextProperty);
            
        }
        guessEntry.Text = "";
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

    private async void GuessEntry_Completed(object sender, EventArgs e)
    {
            if(guessCount >= 3 && Preferences.Default.Get("hints", "on") == "on")
            {
                HintString = "Hint: The word contains the letter's " + ChosenWord.Substring(0, 3);
            }
        
            if (guessCount == 5 && Guess != ChosenWord)
            {
                await CheckGuess(Guess);
                HintString = "";
                GuessFeedbackString = "No more guesses.\nThe word was " + chosenWord;
                UpdatePlayAgain();

                WinOverlay.IsVisible = true;

                GamesPlayed++;
                currentStreak = 0;
                UpdateWinPercentage();
                WritePlayerData();
                return;
            }
            if (Guess.Length < 5)
            {
                GuessFeedbackString = "Word too short";
                GuessFeedbackLabel.TextColor = Colors.Red;
                return;
            }

            if (!CheckValidWord(Guess))
            {
                GuessFeedbackString = "Invalid word";
                GuessFeedbackLabel.TextColor = Colors.Red;
                return;
            }
            if (Guess == ChosenWord)
            {
                await CheckGuess(Guess);
                HintString = "";
                wordGuessed = true;
                UpdatePlayAgain();
                WinOverlay.IsVisible = true;

                gamesWon++;
                GamesPlayed++;
                CurrentStreak++;
                if (currentStreak > longestStreak)
                {
                    LongestStreak = currentStreak;
                }
                UpdateWinPercentage();
                WritePlayerData();
                SaveScore();
                return;
            }
            await CheckGuess(Guess);
            guessCount++;
            AddBindingToRow(guessCount);
        
        
        
        

    }

    private async void Label_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {

        //change parent frames background color to white and add a thin black border
        if (e.PropertyName == nameof(Label.Text))
        {
            Label label = (Label)sender;
            Frame frame = (Frame)label.Parent;



            if (frame != null && (label.Text != "" && label.Text != null))
            {
                frame.BackgroundColor = Colors.White;
                frame.BorderColor = Colors.Black;
                await frame.ScaleTo(1.2, 50, Easing.Linear);
                await frame.ScaleTo(1, 50, Easing.Linear);
            }

            if (frame != null && label.Text == "" || label.Text == null)
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
    private async Task CheckGuess(string guess)
    {
        for (int i = 0; i < guess.Length; i++)
        {
            Frame frame = gameboard.Children.OfType<Frame>().Where(child => gameboard.GetRow(child) == guessCount && gameboard.GetColumn(child) == i).FirstOrDefault();
            // Create a weak reference to the object
            WeakReference weakRef = new WeakReference(frame);

            // Later, when you want to call FlipFrame...
            if (weakRef.IsAlive)
            {
                Frame frame_weak = weakRef.Target as Frame;
                if (frame != null)
                {
                    await FlipFrame(frame_weak);
                }
            }

            string guessChar = guess[i].ToString().ToLower();

            //If the character appears twice in the guess but only once in the word
            if (ContainsDuplicateCharacter(guess, guess[i]) && !ContainsDuplicateCharacter(chosenWord, guess[i]))
            {
                if (guess[i] != ChosenWord[i])
                {
                    if (ChosenWord.Contains(guess[i]) && guess.Substring(0, i).Contains(guess[i]))
                    {
                        frame.BackgroundColor = Colors.Yellow;
                        UpdateKeyboardColor(guessChar, Colors.Yellow);
                    }
                    else
                    {
                        frame.BackgroundColor = Colors.Grey;
                        UpdateKeyboardColor(guessChar, Colors.DarkGray);
                    }
                }
                //If the letter is contained in the word but is in the wrong position
                //Only check if the letter is contained in the word if it is not already green
                //This is to prevent the frame from turning yellow if the letter is in the correct position
                //If the letter is contained in the word but appears twice in the guess, only turn the first instance yellow unless the second instance is in the correct position
                else
                {
                    frame.BackgroundColor = Colors.Green;
                    UpdateKeyboardColor(guessChar, Colors.Green);

                }
            }
            else
            {
                if (guess[i] != ChosenWord[i])
                {
                    if (ChosenWord.Contains(guess[i]))
                    {
                        frame.BackgroundColor = Colors.Yellow;
                        UpdateKeyboardColor(guessChar, Colors.Yellow);
                    }
                    else
                    {
                        frame.BackgroundColor = Colors.Grey;
                        UpdateKeyboardColor(guessChar, Colors.DarkGray);
                    }
                }
                //If the letter is contained in the word but is in the wrong position
                //Only check if the letter is contained in the word if it is not already green
                //This is to prevent the frame from turning yellow if the letter is in the correct position
                //If the letter is contained in the word but appears twice in the guess, only turn the first instance yellow unless the second instance is in the correct position
                else
                {
                    frame.BackgroundColor = Colors.Green;
                    UpdateKeyboardColor(guessChar, Colors.Green);

                }
            }

            
        }
    }

    public bool ContainsDuplicateCharacter(string s, char c)
    {
        bool seenFirst = false;
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] != c)
                continue;
            if (seenFirst)
                return true;
            seenFirst = true;
        }
        return false;
    }

    private void UpdateKeyboardColor(string character, Color color)
    {
        foreach (Microsoft.Maui.Controls.Button button in CustomKeyboard.Children)
        {
            if (button.Text.ToLower() == character)
            {
                button.BackgroundColor = color;
            }
        }
    }

    private void Close_Overlay_ButtonClicked(object sender, EventArgs e)
    {
        WinOverlay.IsVisible = false;
    }

    private void Settings_Menu_Clicked(object sender, EventArgs e)
    {
        UpdatePlayAgain();
       
        WinOverlay.IsVisible = true;

        guessEntry.Unfocus();
    }

    private void ReadPlayerData()
    {
        String line;

        if (PlayerFileExists())
        {
            using (StreamReader playerFile = new StreamReader(playerDataPath)) // Use 'using' to ensure the StreamReader is disposed of correctly
            {
                line = playerFile.ReadLine();

                //Set games played
                GamesPlayed = Int32.Parse(line);
                line = playerFile.ReadLine();
                gamesWon = Int32.Parse(line);
                line = playerFile.ReadLine();
                currentStreak = Int32.Parse(line);
                line = playerFile.ReadLine();
                longestStreak = Int32.Parse(line);
            }

        }
        else
        {
            //Create player data file
            using (StreamWriter playerFile = new StreamWriter(playerDataPath)) // Use 'using' to ensure the StreamWriter is disposed of correctly
            {
                //Write default values to file
                playerFile.WriteLine("0");
                playerFile.WriteLine("0");
                playerFile.WriteLine("0");
                playerFile.WriteLine("0");
            }
            
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
        if (PlayerFileExists())
        {
            using (StreamWriter playerFile = new StreamWriter(playerDataPath)) // Use 'using' to ensure the StreamWriter is disposed of correctly
            {
                playerFile.WriteLine(GamesPlayed);
                playerFile.WriteLine(gamesWon);
                playerFile.WriteLine(currentStreak);
                playerFile.WriteLine(longestStreak);
            }
        }
    }

    void UpdateWinPercentage()
    {
        if (gamesPlayed != 0)
            WinPercentage = ((gamesWon / GamesPlayed) * 100) + "%";
        else
            WinPercentage = "0%";

    }

    private void PlayAgainButton_Clicked(object sender, EventArgs e)
    {

        wordGuessed = false;

        //Clear gameboard and set text to each label to ""
        foreach (Frame frame in gameboard.Children)
        {
            frame.BackgroundColor = Colors.White;
            frame.BorderColor = Colors.Grey;
            frame.Content.RemoveBinding(Label.TextProperty);
            frame.Content.SetValue(Label.TextProperty, "");

        }

        Reset_Keyboard();

        guessEntry.Text = "";
        guessCount = 0;
        AddBindingToRow(guessCount);

        wordChosen = false;
        chooseWord();
        WinOverlay.IsVisible = false;
    }

    void UpdatePlayAgain()
    {
        PlayAgain = (wordGuessed == true || guessCount == 5);
    }

    //Vertically flip frame
    private async Task FlipFrame(Frame frame)
    {

        //On my huwawei phone the flip animation causes an unexplanable memory leak, so I have disabled it for android
        //However, testing on other devices it dosen't seem to cause any issues
        #if !ANDROID
        // Rotate the frame 90 degrees upwards over 250ms
        await frame.RotateXTo(90, 250);

        // Then rotate it back down to its original position over 250ms
        await frame.RotateXTo(0, 250);
        return;
        #endif


        //await 200 milliseconds
        await Task.Delay(200).ConfigureAwait(false);
        
    }

    //Save date and time, chosen word and number of guesses to scores file
    private void SaveScore()
    {
        //If scores file does not exist, create it
        if (File.Exists(scoresPath))
        {
            string targetFile = scoresPath;
            StreamWriter sw = File.AppendText(targetFile);
            sw.WriteLine(DateTime.Now + "-" + chosenWord + "-" + (guessCount + 1));
            sw.Close();
        }
        else
        {
            string targetFile = scoresPath;
            StreamWriter sw = new StreamWriter(targetFile);
            sw.WriteLine(DateTime.Now + "-" + chosenWord + "-" + (guessCount + 1));
            sw.Close();
        }

    }

    private void Back_Button_Clicked(object sender, EventArgs e)
    {
        //Pop current page off navigation stack
        Navigation.PopAsync();
    }

    //When a button in the custom keyboard grid is click add the letter to the guess entry
    private void Custom_Keyboard_Button_Clicked(object sender, EventArgs e)
    {
        if(guessEntry.Text.Length == 5)
            return;

        Microsoft.Maui.Controls.Button button = (Microsoft.Maui.Controls.Button)sender;
        guessEntry.Text += button.Text;
    }

    //Backspace button method
    private void Backspace_Button_Clicked(object sender, EventArgs e)
    {
        if (guessEntry.Text.Length > 0)
        {
            guessEntry.Text = guessEntry.Text.Remove(guessEntry.Text.Length - 1);
        }
    }

    //Set onscreen keyboard colours to default
    private void Reset_Keyboard()
    {
        foreach (Microsoft.Maui.Controls.Button button in CustomKeyboard.Children)
        {
            button.BackgroundColor = Color.FromHex("dee1e1");
        }
    }

  
}