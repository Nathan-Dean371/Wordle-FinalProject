using System.Diagnostics;

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

    private int guessCount = 0;

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

        AddBindingToRow(0);

	}

    void chooseWord()
    {
        if (wordChosen)
            return;

        Debug.Print(System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, fileName));

        if (File.Exists(System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, fileName)))
        {

            string targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, fileName);

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
    }

    private async Task EntryFocus()
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        { 
            guessEntry.Focus();
        });
        
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
        if(Guess.Length < 5)
        {
            return;
        }

        if (Guess == ChosenWord)
        {
            DisplayAlert("You won!", "You guessed the word!", "OK");  
        }
        else
        {
            DisplayAlert("Wrong guess!", "You didn't guess the word! Try again!", "OK");
            guessCount++;
            AddBindingToRow(guessCount);
            guessEntry.Text = "";
        }
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
}