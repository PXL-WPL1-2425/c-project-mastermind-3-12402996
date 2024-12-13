using Microsoft.VisualBasic;
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
        public MainWindow()
        {
            InitializeComponent();
            this.Height = 700;
            this.Width = 550;
        }
        private void SetMaxAttempts()
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
            }
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
            }
            SetMaxAttempts();
            StartCountDown();
            labelScore.Content = $"Speler: {playerName} - Score: {score} - Pogingen: {guessAttempts}";
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var randomCode = new Random();
            for (int i = 0; i < 4; i++)
            {
                secretCode.Add(colors[randomCode.Next(colors.Count)]);
            }

            generatedCodeTextBox.Text = $"{secretCode}".ToString();
            this.Title = $"Poging: {guessAttempts}";
            generatedCodeTextBox.Visibility = Visibility.Collapsed;
            StartGame();
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
        /// <summary>
        /// Deze methode zorgt eerst en vooral ervoor dat de timer stopt en vervolgens de poging verhoogt. 
        /// Nadien wordt er een messagebox getoond.
        /// de titel van het venster wordt aangepast en de timer wordt opnieuw gestart.
        /// </summary>
        private void StopCountDown()
        {
            timer.Stop();
            guessAttempts++;
            if (guessAttempts >= maxAttempts)
            {
                MessageBox.Show($"Je hebt geen pogingen meer over. De correcte code was " +
                    $"{string.Join(", ", secretCode)}.\n Nu is speler {GetNextPlayer()} aan de beurt", $"{playerName}", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetGame();
            }
            else
            {
                MessageBox.Show("Je beurt is over, probeer opnieuw", "Je tijd is verstreken, To Slow.", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Title = $"Poging: {guessAttempts}";
                StartCountDown();
            }

        }
        /// <summary>
        /// Start de interval van de timer en zorgt ervoor dat de timer elke 100 milliseconden telt.
        /// </summary>
        private void StartCountDown()
        {
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            startedGuessTime = DateTime.Now;
            timer.Start();
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
                }
                else if (secretCode.Contains(inputColor[i]))
                {
                    rect.StrokeThickness = 5;
                    rect.Stroke = Brushes.Wheat;
                }
                else
                {
                    rect.StrokeThickness = 5;
                    rect.Stroke = Brushes.Transparent;
                }

                colorPanel.Children.Add(rect);
            }
            colorHistoryListBox.Items.Add(new ListBoxItem { Content = colorPanel });

            int penaltyPoints = CalculatePenaltyPoints(inputColor); // Berekening van strafpunten
            score -= penaltyPoints; // Aftrekken van strafpunten van de score
            scoreTextBox.Text = $"Score: {score}"; // Bijwerken van de score

            labelScore.Content = $"Speler: {playerName} - Score: {score} - Pogingen: {guessAttempts}";
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
                    timer.Stop();
                    MessageBoxResult result = MessageBox.Show
                        ($"Je hebt geen pogingen meer over. De correcte code was {string.Join(", ", secretCode)}.\n" +
                        $"De volgende speler is {GetNextPlayer()}.", $"{playerName}", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ResetGame();
                }
            }
            this.Title = $"Speler: {playerName} - Poging: {guessAttempts}";
        }
        private void UpdateHighScores()
        {
            string newScore = $"{playerName} - {guessAttempts} pogingen - {score}/100";
            for (int i = 0; i < highscores.Length; i++)
            {
                if (string.IsNullOrEmpty(highscores[i]))
                {
                    highscores[i] = newScore;
                    break;
                }
            }
        }

        private void ResetGame()
        {
            secretCode.Clear();
            var randomCode = new Random();
            for (int i = 0; i < 4; i++)
            {
                secretCode.Add(colors[randomCode.Next(colors.Count)]);
            }

            guessAttempts = 0;
            score = 100;
            timerTextBox.Text = $"Score: {score}";
            colorHistoryListBox.Items.Clear();
            this.Title = $"Poging: {guessAttempts}";
            scoreTextBox.Clear();
            ellipseOne.Fill = ellipseColor[0];
            ellipseTwo.Fill = ellipseColor[0];
            ellipseThree.Fill = ellipseColor[0];
            ellipseFour.Fill = ellipseColor[0];
            
            if (multiplePlayers.IndexOf(playerName) == multiplePlayers.Count - 1)
            {
                playerName = multiplePlayers[0];
            }
            else
            {
                playerName = GetNextPlayer();
            }
            StartGame();
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

        private void Menu_Nieuw_Spel_Click(object sender, RoutedEventArgs e)
        {
            ResetGame();
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
    }
}


     
    