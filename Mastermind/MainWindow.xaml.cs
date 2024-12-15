using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Printing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace C_mastermindSprint1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        List<string> colors = new List<string> { "Red", "Yellow", "Orange", "White", "Green", "Blue" };
        List<Brush> ellipseColor = new List<Brush> { Brushes.Red, Brushes.Yellow, Brushes.DarkOrange, Brushes.White, Brushes.Green, Brushes.Blue };
        private List<string> secretCode = new List<string>();
        int guessAttempts = 0;
        DateTime startedGuessTime;
        int score = 100;
        private string[] highscores = new string[15];
        string playerName;
        int maxAttempts = 0;
        private List<string> multiplePlayers = new List<string>();
        private TimeSpan remainingTime;
        public MainWindow()
        {
            InitializeComponent();
            this.Height = 700;
            this.Width = 550;
            this.Title = $"Speler: {playerName} - Poging: {guessAttempts}";
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var randomCode = new Random();
            for (int i = 0; i < 4; i++)
            {
                secretCode.Add(colors[randomCode.Next(colors.Count)]);
            }

            generatedCodeTextBox.Text = $"{secretCode}".ToString();
            generatedCodeTextBox.Visibility = Visibility.Collapsed;
            StartGame();
        }
        private void StartCountDown()
        {
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            startedGuessTime = DateTime.Now;
            timer.Start();
        }
        /// <summary>
        /// Start de interval van de timer en zorgt ervoor dat de timer elke 100 milliseconden telt.
        /// </summary>
        private void Timer_Tick(object? sender, EventArgs e)
        {
            // Deel 1. Bereken tijdsverschil
            TimeSpan timeUsedToGuess = DateTime.Now - startedGuessTime;
            // Deel 2. controle, if time > 10 seconds
            if (timeUsedToGuess.TotalSeconds > 10)
            {
                StopCountDown();
                startedGuessTime = DateTime.Now;
            }
            timerTextBox.Text = timeUsedToGuess.TotalSeconds.ToString("N0");
        }
        private void StartGame()
        {
            if (multiplePlayers.Count == 0)
            {
                bool addMorePlayers = true;
                while (addMorePlayers)
                {
                    string newPlayerName = Interaction.InputBox("Voer je naam in:", "Naam Speler", "");

                    if (string.IsNullOrWhiteSpace(newPlayerName))
                    {
                        MessageBox.Show("Je moet een naam invoeren.", "Ongeldige invoer", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        multiplePlayers.Add(newPlayerName);
                        MessageBoxResult result = MessageBox.Show("Wil je nog een speler toevoegen?",
                            "Nog een speler?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.No)
                        {
                            addMorePlayers = false;
                        }
                    }
                }
                playerName = multiplePlayers[0];
                SetMaxAttempts();
            }
            StartCountDown();
            labelScore.Content = $"Speler: {playerName} - Score: {score} - Pogingen: {guessAttempts}";
        }
        /// <summary>
        /// Deze methode zorgt eerst en vooral ervoor dat de timer stopt en vervolgens de poging verhoogt. 
        /// Nadien wordt er een messagebox getoond.
        /// de titel van het venster wordt aangepast en de timer wordt opnieuw gestart.
        /// </summary>
        private void StopCountDown()
        {
            timer.Stop();
            guessAttempts++;

            List<string> emptyGuess = new List<string> { "", "", "", "" };
            int penaltyPoints = CalculatePenaltyPoints(emptyGuess);
            score -= penaltyPoints;

            if (guessAttempts >= maxAttempts || score <= 0)
            {
                score = 0;
                MessageBox.Show($"Je hebt geen pogingen meer over. De correcte code was " +
                    $"{string.Join(", ", secretCode)}.\n Nu is speler {GetNextPlayer()} aan de beurt", $"{playerName}", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetGame();
            }
            else
            {
                MessageBox.Show("Je beurt is over.\nProbeer opnieuw", "Je tijd is verstreken.", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Title = $"Poging: {guessAttempts}";
                StartCountDown();
            }
            labelScore.Content = $"Speler: {playerName} - Score: {score} - Pogingen: {guessAttempts}";
        }
        private void SetMaxAttempts()
        {
            if (maxAttempts == 0)
            {
                string input = string.Empty;
                int attempts = 0;

                while (true)
                {
                    input = Interaction.InputBox("Voer het maximaal aantal pogingen in (tussen 3 en 20):", "Maximaal Aantal Pogingen", maxAttempts.ToString());

                    if (int.TryParse(input, out attempts) && attempts >= 3 && attempts <= 20)
                    {
                        maxAttempts = attempts;
                        break;
                    }
                    else
                    {
                        MessageBox.Show("Ongeldige invoer. Voer een getal in tussen 3 en 20.", "Ongeldige Invoer", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
            }   }
        }
        private void checkButton_Click(object sender, RoutedEventArgs e)
        {
            guessAttempts++;
            this.Title = $"Speler: {playerName} - Poging: {guessAttempts}";
            UpdateTitle();

            string checkColor1 = colors[ellipseColor.IndexOf(ellipseOne.Fill)];
            string checkColor2 = colors[ellipseColor.IndexOf(ellipseTwo.Fill)];
            string checkColor3 = colors[ellipseColor.IndexOf(ellipseThree.Fill)];
            string checkColor4 = colors[ellipseColor.IndexOf(ellipseFour.Fill)];

            List<string> inputColor = new List<string> { checkColor1, checkColor2, checkColor3, checkColor4 };

            StartCountDown();

            StackPanel colorPanel = new StackPanel { Orientation = Orientation.Horizontal };
            for (int i = 0; i < inputColor.Count; i++)
            {
                Ellipse rect = new Ellipse
                {
                    Width = 42.5,
                    Height = 42.5,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(inputColor[i]))
                };

                if (secretCode[i] == inputColor[i])
                {
                    rect.StrokeThickness = 5;
                    rect.Stroke = Brushes.DarkRed;
                    rect.ToolTip = "Juiste kleur\nJuiste positie";
                }
                else if (secretCode.Contains(inputColor[i]))
                {
                    rect.StrokeThickness = 5;
                    rect.Stroke = Brushes.Wheat;
                    rect.ToolTip = "Juiste kleur\nFoute positie";
                }
                else
                {
                    rect.StrokeThickness = 5;
                    rect.Stroke = Brushes.Transparent;
                    rect.ToolTip = "Foute kleur";
                }
                ToolTipService.SetInitialShowDelay(rect, 0);
                colorPanel.Children.Add(rect);
            }
            colorHistoryListBox.Items.Add(new ListBoxItem { Content = colorPanel });

            int penaltyPoints = CalculatePenaltyPoints(inputColor); // Berekening van strafpunten
            score -= penaltyPoints; // Aftrekken van strafpunten van de score
            scoreTextBox.Text = $"Score: {score}"; // Bijwerken van de score

            labelScore.Content = $"Speler: {playerName} - Score: {score} - Pogingen: {guessAttempts}";
            if (score <= 0)
            {
                MessageBox.Show($"Je score is 0. De correcte code was {string.Join(", ", secretCode)}.\n Nu is speler {GetNextPlayer()} aan de beurt.", $"{playerName}", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetGame();
                return;
            }

            if (inputColor.SequenceEqual(secretCode))
            {
                timer.Stop();
                MessageBoxResult result = MessageBox.Show
                    ($"Proficiat, je hebt de code gekraakt in {guessAttempts} pogingen.\n Nu is speler {GetNextPlayer()} aan de beurt.",
                    $"{playerName}", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateHighScores();
                ResetGame();
            }
            else
            {
                if (guessAttempts >= maxAttempts)
                {
                    score = 0;
                    timer.Stop();
                    MessageBoxResult result = MessageBox.Show
                        ($"Je hebt geen pogingen meer over. De correcte code was {string.Join(", ", secretCode)}.\n" +
                        $"De volgende speler is {GetNextPlayer()}.", $"{playerName}", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ResetGame();
                }
            }
            this.Title = $"Poging: {guessAttempts}";
        }
        private string GetNextPlayer()
        {
            if (multiplePlayers.Count == 0)
            {
                return string.Empty;
            }
            else
            {
                int currentIndex = multiplePlayers.IndexOf(playerName);
                int nextIndex = (currentIndex + 1) % multiplePlayers.Count;
                return multiplePlayers[nextIndex];
            }
        }
        private void ResetGame()
        {
            UpdateHighScores();
            secretCode.Clear();
            var randomCode = new Random();
            for (int i = 0; i < 4; i++)
            {
                secretCode.Add(colors[randomCode.Next(colors.Count)]);
            }
            timerTextBox.Text = $"Score: {score}";
            colorHistoryListBox.Items.Clear();
            this.Title = $"Poging: {guessAttempts}";
            scoreTextBox.Clear();
            ellipseOne.Fill = ellipseColor[0];
            ellipseTwo.Fill = ellipseColor[0];
            ellipseThree.Fill = ellipseColor[0];
            ellipseFour.Fill = ellipseColor[0];
            score = 100;
            guessAttempts = 0;

            if (multiplePlayers.IndexOf(playerName) == multiplePlayers.Count - 1)
            {
                playerName = multiplePlayers[0];
            }
            else
            {
                playerName = GetNextPlayer();
            }
            scoreTextBox.Text = $"Score: {score}";
            labelScore.Content = $"Speler: {playerName} - Score: {score} - Pogingen: {guessAttempts}";
            StartGame();
        }
        private void HighScore()
        {
            StringBuilder opslaanHighScore = new StringBuilder();
            foreach (string s in highscores)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    opslaanHighScore.AppendLine(s);
                }
            }
            MessageBox.Show(opslaanHighScore.ToString(), "Highscores", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void UpdateHighScores()
        {
            string newScore = $"{playerName} - {guessAttempts} pogingen - {score}/100";
            for (int i = 0; i < highscores.Length; i++)
            {
                if (highscores[i] != null && highscores[i].StartsWith(playerName))
                {
                    highscores[i] = newScore;
                    return;
                }
            }
            for (int i = 0; i < highscores.Length; i++)
            {
                if (string.IsNullOrEmpty(highscores[i]))
                {
                    highscores[i] = newScore;
                    break;
                }
            }
        }
        private int CalculatePenaltyPoints(List<string> userGuess)
        {
            int penaltyPoints = 0;

            for (int i = 0; i < userGuess.Count; i++)
            {
                if (userGuess[i] == secretCode[i])
                {
                    // Correct color and position
                    penaltyPoints += 0;
                }
                else if (secretCode.Contains(userGuess[i]))
                {
                    // Correct color but wrong position
                    penaltyPoints += 1;
                }
                else
                {
                    // Color not in the code
                    penaltyPoints += 2;
                }
            }
            return penaltyPoints;
        }
        private void UpdateTitle()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(guessAttempts);
            this.Title = sb.ToString();
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F12)
            {
                ToggleDebug();
            }
        }
        /// <summary>
        /// Hier kijkt de methode of de textbox zichtbaar is of niet.
        /// Vervolgens kan ik de textbox zichtbaar maken of verbergen met de toetsencombinatie CTRL + F12.
        /// </summary>
        private void ToggleDebug()
        {
            if (generatedCodeTextBox.Visibility == Visibility.Visible)
            {
                generatedCodeTextBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (secretCode is List<string> secretCodeList)
                {
                    generatedCodeTextBox.Text = string.Join(", ", secretCodeList);
                    generatedCodeTextBox.Visibility = Visibility.Visible;
                }
            }
        }
        private void ellipseOne_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse ellipse)
            {
                if (ellipse.Fill == null)
                {
                    ellipse.Fill = ellipseColor[0];
                }
                else
                {
                    // Get the current brush of the ellipse
                    Brush currentBrush = ellipse.Fill;

                    // Find the current brush in the list
                    int currentIndex = ellipseColor.IndexOf(currentBrush);

                    // Calculate the index of the next brush
                    int nextIndex = (currentIndex - 1 + ellipseColor.Count) % ellipseColor.Count;

                    // Get the next brush
                    Brush nextBrush = ellipseColor[nextIndex];

                    // Set the ellipse's fill to the next brush
                    ellipse.Fill = nextBrush;

                }
            }
        }
        private void ellipseOne_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse ellipse)
            {
                if (ellipse.Fill == null)
                {
                    ellipse.Fill = ellipseColor[0];
                }
                else
                {
                    // Get the current brush of the ellipse
                    Brush currentBrush = ellipse.Fill;

                    // Find the current brush in the list
                    int currentIndex = ellipseColor.IndexOf(currentBrush);

                    // Calculate the index of the next brush
                    int nextIndex = (currentIndex + 1) % ellipseColor.Count;

                    // Get the next brush
                    Brush nextBrush = ellipseColor[nextIndex];

                    // Set the ellipse's fill to the next brush
                    ellipse.Fill = nextBrush;
                }
            }
        }
        private void Menu_Nieuw_Spel_Click(object sender, RoutedEventArgs e)
        {
            multiplePlayers.Clear();
            maxAttempts = 0;
            StartGame();
        }
        private void Menu_HighScores_Click(object sender, RoutedEventArgs e)
        {
            HighScore();
        }
        private void Menu_Afsluiten_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Menu_Aantal_Pogingen_Click(object sender, RoutedEventArgs e)
        {
            SetMaxAttempts();
            MessageBox.Show($"Het maximaal aantal pogingen is nu ingesteld op {maxAttempts}.", "Aantal Pogingen", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void buyHintButton_Click(object sender, RoutedEventArgs e)
        {
            remainingTime = TimeSpan.FromMilliseconds(100) - (DateTime.Now - startedGuessTime);
            timer.Stop();
            MessageBoxResult result = MessageBox.Show("Wil je een hint kopen, het kost je wel strafpunten?", "Hint", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                MessageBoxResult hintResult = 
                    MessageBox.Show($"Ja: een juiste kleur (kost 15 strafpunten).\n"
                    + "Nee: een juiste kleur en positie (kost 30 strafpunten).\n"
                    , "Hint", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (hintResult == MessageBoxResult.Yes)
                {
                    CorrectColor();
                    score -= 15;
                    scoreTextBox.Text = $"Score: {score}";
                }
                else if (hintResult == MessageBoxResult.No)
                {
                    CorrectColorAndPosition();
                    score -= 30;
                    scoreTextBox.Text = $"Score: {score}";
                }
                scoreTextBox.Text = $"Score: {score}";
                labelScore.Content = $"Speler: {playerName} - Score: {score} - Pogingen: {guessAttempts}";
            }
            startedGuessTime = DateTime.Now - (TimeSpan.FromMilliseconds(100) - remainingTime);
            timer.Start();
        }
        private void CorrectColor()
        {
            Random hintCorrectColor = new Random();
            string correctColor = secretCode[hintCorrectColor.Next(secretCode.Count)];
            MessageBox.Show($"Kleur:\n {correctColor}", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void CorrectColorAndPosition()
        {
            Random hintCorrectColorAndPosition = new Random();
            int correctColorAndPosition = hintCorrectColorAndPosition.Next(secretCode.Count);
            string correctColor = secretCode[correctColorAndPosition];
            MessageBox.Show($"Kleur + positie {correctColorAndPosition + 1} is: {correctColor}", "Hint", MessageBoxButton.OK, MessageBoxImage.Information); 
        }
    }
}


     
    