using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Data.Sqlite;

namespace JogodaForca
{
    public partial class MainPage : ContentPage
    {
        private DatabaseHelper dbHelper;
        private string wordToGuess;
        private char[] guessedWord;
        private int attemptsLeft;
        private HashSet<char> wrongGuesses;

        private readonly List<string> hangmanStages = new List<string>
        {
            "   +---+\n   |   |\n       |\n       |\n       |\n      ===",
            "   +---+\n   |   |\n   O   |\n       |\n       |\n      ===",
            "   +---+\n   |   |\n   O   |\n   |   |\n       |\n      ===",
            "   +---+\n   |   |\n   O   |\n  /|   |\n       |\n      ===",
            "   +---+\n   |   |\n   O   |\n  /|\\  |\n       |\n      ===",
            "   +---+\n   |   |\n   O   |\n  /|\\  |\n  /    |\n      ===",
            "   +---+\n   |   |\n   O   |\n  /|\\  |\n  / \\  |\n      ==="
        };

        private readonly string trophyArt = @"
         ___________
          '._==_==_=_.'
            .-\:      /-.
           | (|:.     |) |
            '-|:.     |-'
              \::.    /
               '::. .'
                 ) (
               _.' '._
              `""""""""""` 

       PARABÉNS!
     VOCÊ ACERTOU.
        ";

        private readonly string gameOverArt = @"
          G A M E   O V E R
        ";

        private int player1Score;
        private int player1Errors;
        private int player2Score;
        private int player2Errors;
        private int currentPlayer; // Jogador que insere a palavra
        private int guessingPlayer; // Jogador que adivinha

        public MainPage()
        {
            InitializeComponent();

            dbHelper = new DatabaseHelper();

            // Popula o Picker com as categorias
            foreach (var category in dbHelper.GetCategories())
            {
                CategoryPicker.Items.Add(category);
            }

            // Inicializa o jogador atual
            currentPlayer = 1;

            // Adiciona o evento SelectedIndexChanged
            GameModePicker.SelectedIndexChanged += OnGameModeChanged;

            // Define o modo de jogo padrão como "Um Jogador"
            GameModePicker.SelectedIndex = 0;

            // Chama o método para atualizar a interface de acordo com o modo de jogo
            OnGameModeChanged(null, null);

            // Inicializa os elementos da interface
            ResetInterface();
        }


        private void ResetInterface()
        {
            HangmanDrawing.Text = "";
            WordLabel.Text = "";
            WrongGuessesLabel.Text = "";
            MessageLabel.Text = "";
            ScoreLabel.Text = "";
            LettersLayout.IsVisible = false;
            WordEntry.IsVisible = false;
            StartGameButton.IsVisible = true;
            CategoryPicker.IsVisible = true;
            CategoryPicker.IsEnabled = true;
            GameModePicker.IsEnabled = true;
            GameModePicker.IsVisible = true;
            DifficultyLayout.IsVisible = false;
            DifficultyPicker.SelectedIndex = -1;
            CategoryPicker.SelectedIndex = -1;
            NextRoundButton.IsVisible = false;
            RestartGameButton.IsVisible = false;
            UseHintButton.IsVisible = false;

            // Chama o método para atualizar a interface de acordo com o modo de jogo
            OnGameModeChanged(null, null);
        }


        private void OnGameModeChanged(object sender, EventArgs e)
        {
            if (GameModePicker.SelectedIndex == 1) // Dois Jogadores
            {
                WordEntry.IsVisible = true;
                CategoryPicker.IsEnabled = true;
                DifficultyLayout.IsVisible = false;
            }
            else if (GameModePicker.SelectedIndex == 0) // Um Jogador
            {
                WordEntry.IsVisible = false;
                CategoryPicker.IsEnabled = true;
                DifficultyLayout.IsVisible = true;
            }
            else
            {
                // Se nenhum modo de jogo estiver selecionado
                WordEntry.IsVisible = false;
                DifficultyLayout.IsVisible = false;
            }
        }


        private void OnStartGameClicked(object sender, EventArgs e)
        {
            if (GameModePicker.SelectedIndex == 1) // Dois Jogadores
            {
                if (CategoryPicker.SelectedIndex == -1)
                {
                    MessageLabel.Text = "Por favor, escolha uma categoria para iniciar o jogo.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(WordEntry.Text))
                {
                    MessageLabel.Text = $"Jogador {currentPlayer}: Por favor, digite uma palavra.";
                    return;
                }

                wordToGuess = WordEntry.Text.Trim().ToUpper();

                StartGameButton.IsVisible = false;
                WordEntry.Text = "";
                WordEntry.IsVisible = false;
                CategoryPicker.IsVisible = false;
                GameModePicker.IsEnabled = false; // Desabilita apenas durante o jogo

                // Define o jogador que irá adivinhar
                guessingPlayer = currentPlayer == 1 ? 2 : 1;

                InitializeGame();
            }
            else // Um Jogador
            {
                if (CategoryPicker.SelectedIndex == -1)
                {
                    MessageLabel.Text = "Por favor, escolha uma categoria para iniciar o jogo.";
                    return;
                }

                if (DifficultyPicker.SelectedIndex == -1)
                {
                    MessageLabel.Text = "Por favor, selecione um nível de dificuldade.";
                    return;
                }

                StartGameButton.IsVisible = false;
                CategoryPicker.IsVisible = false;
                DifficultyLayout.IsVisible = false;
                GameModePicker.IsEnabled = false; // Desabilita apenas durante o jogo

                InitializeGame();
            }
        }

        private void InitializeGame()
        {
            try
            {
                if (CategoryPicker.SelectedItem == null)
                {
                    MessageLabel.Text = "Por favor, escolha uma categoria.";
                    return;
                }
                string selectedCategory = CategoryPicker.SelectedItem.ToString();

                if (GameModePicker.SelectedIndex == 0) // Um Jogador
                {
                    if (DifficultyPicker.SelectedItem == null)
                    {
                        MessageLabel.Text = "Por favor, selecione um nível de dificuldade.";
                        return;
                    }
                    string selectedDifficulty = DifficultyPicker.SelectedItem.ToString();

                    var wordsList = dbHelper.GetWordsByCategoryAndDifficulty(selectedCategory, selectedDifficulty);

                    if (wordsList.Count == 0)
                    {
                        MessageLabel.Text = "Não há palavras disponíveis para a categoria e dificuldade selecionadas.";
                        ResetInterface();
                        return;
                    }

                    Random random = new Random();
                    wordToGuess = wordsList[random.Next(wordsList.Count)].ToUpper();
                }
                else // Dois Jogadores
                {
                    if (string.IsNullOrWhiteSpace(wordToGuess))
                    {
                        MessageLabel.Text = $"Jogador {currentPlayer}: Por favor, digite uma palavra.";
                        return;
                    }

                    // Adiciona a palavra à categoria selecionada
                    dbHelper.AddWord(selectedCategory, wordToGuess);

                    MessageLabel.Text = $"Jogador {guessingPlayer}: Adivinhe a palavra.";
                }

                // Inicializa as variáveis do jogo
                guessedWord = new string('_', wordToGuess.Length).ToCharArray();
                attemptsLeft = 6;
                wrongGuesses = new HashSet<char>();

                // Atualiza a interface
                UpdateWordDisplay();
                UpdateWrongGuesses();
                UpdateHangmanDrawing();
                CreateLetterButtons();

                // Torna o layout das letras visível
                LettersLayout.IsVisible = true;

                // Torna o botão de dica visível (apenas no modo um jogador)
                UseHintButton.IsVisible = GameModePicker.SelectedIndex == 0;

                // Atualiza a pontuação
                ScoreLabel.Text = GetScoreText();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao inicializar o jogo: {ex.Message}");
                MessageLabel.Text = $"Erro ao iniciar o jogo: {ex.Message}";
            }
        }

        private void UpdateHangmanDrawing()
        {
            int stageIndex = 6 - attemptsLeft;
            if (stageIndex >= 0 && stageIndex < hangmanStages.Count)
            {
                HangmanDrawing.Text = hangmanStages[stageIndex];
            }
        }

        private void OnLetterButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                char guess = button.Text[0];
                button.IsEnabled = false;

                if (wordToGuess.Contains(guess))
                {
                    for (int i = 0; i < wordToGuess.Length; i++)
                    {
                        if (wordToGuess[i] == guess)
                        {
                            guessedWord[i] = guess;
                        }
                    }
                    UpdateWordDisplay();

                    if (!guessedWord.Contains('_'))
                    {
                        MessageLabel.Text = $"Parabéns, você acertou a palavra!";
                        HangmanDrawing.Text = trophyArt;
                        DisableAllLetterButtons();
                        UpdateScore(guessingPlayer, true);
                        ShowNextRoundButton();
                    }
                }
                else
                {
                    if (!wrongGuesses.Contains(guess))
                    {
                        wrongGuesses.Add(guess);
                        attemptsLeft--;
                        UpdateWrongGuesses();
                        UpdateHangmanDrawing();

                        if (attemptsLeft <= 0)
                        {
                            MessageLabel.Text = $"Você errou! A palavra era {wordToGuess}.";
                            HangmanDrawing.Text = gameOverArt;
                            DisableAllLetterButtons();
                            UpdateScore(guessingPlayer, false);
                            ShowNextRoundButton();
                        }
                    }
                }
            }
        }

        private void UpdateWordDisplay()
        {
            WordLabel.Text = string.Join(" ", guessedWord);
        }

        private void UpdateWrongGuesses()
        {
            WrongGuessesLabel.Text = $"Letras erradas: {string.Join(", ", wrongGuesses)} | Tentativas restantes: {attemptsLeft}";
        }

        private void CreateLetterButtons()
        {
            LettersLayout.Children.Clear();

            for (char letter = 'A'; letter <= 'Z'; letter++)
            {
                Button letterButton = new Button
                {
                    Text = letter.ToString(),
                    WidthRequest = 40,
                    HeightRequest = 40,
                    BackgroundColor = Color.FromArgb("#FFFFFF"),
                    TextColor = Color.FromArgb("#333"),
                    FontAttributes = FontAttributes.Bold,
                    CornerRadius = 20,
                    Margin = new Thickness(5)
                };
                letterButton.Clicked += OnLetterButtonClicked;
                LettersLayout.Children.Add(letterButton);
            }
        }

        private void DisableAllLetterButtons()
        {
            foreach (var child in LettersLayout.Children)
            {
                if (child is Button button)
                {
                    button.IsEnabled = false;
                }
            }
        }

        private void ShowNextRoundButton()
        {
            StartGameButton.IsVisible = false;
            CategoryPicker.IsVisible = false;
            GameModePicker.IsEnabled = false;
            WordEntry.IsVisible = false;
            LettersLayout.IsVisible = false;
            UseHintButton.IsVisible = false;
            NextRoundButton.IsVisible = true;
            RestartGameButton.IsVisible = true; // Exibe o botão "Reiniciar Jogo"
        }

        private void OnRestartButtonClicked(object sender, EventArgs e)
        {
            NextRoundButton.IsVisible = false;
            RestartGameButton.IsVisible = false; // Oculta o botão "Reiniciar Jogo"

            if (GameModePicker.SelectedIndex == 1) // Dois Jogadores
            {
                // Alterna os papéis dos jogadores
                int temp = currentPlayer;
                currentPlayer = guessingPlayer;
                guessingPlayer = temp;

                CategoryPicker.IsVisible = true;
                CategoryPicker.IsEnabled = true;
                CategoryPicker.SelectedIndex = -1;

                WordEntry.Placeholder = $"Jogador {currentPlayer}: Digite a palavra";
                WordEntry.IsVisible = true;
                StartGameButton.IsVisible = true;
                MessageLabel.Text = $"Jogador {currentPlayer}: Escolha uma categoria e digite uma palavra para o Jogador {guessingPlayer} adivinhar.";
            }
            else // Um Jogador
            {
                ResetInterface();
                StartGameButton.IsVisible = true;
            }
        }

        private void UpdateScore(int playerNumber, bool correct)
        {
            if (GameModePicker.SelectedIndex == 1) // Dois Jogadores
            {
                if (playerNumber == 1)
                {
                    if (correct)
                        player1Score++;
                    else
                        player1Errors++;
                }
                else
                {
                    if (correct)
                        player2Score++;
                    else
                        player2Errors++;
                }
            }
            else // Um Jogador
            {
                if (correct)
                    player1Score++;
                else
                    player1Errors++;
            }

            ScoreLabel.Text = GetScoreText();
        }

        private string GetScoreText()
        {
            if (GameModePicker.SelectedIndex == 1) // Dois Jogadores
            {
                return $"Jogador 1 - Acertos: {player1Score}, Erros: {player1Errors}\nJogador 2 - Acertos: {player2Score}, Erros: {player2Errors}";
            }
            else
            {
                return $"Acertos: {player1Score}, Erros: {player1Errors}";
            }
        }

        private void OnResetScoresButtonClicked(object sender, EventArgs e)
        {
            player1Score = 0;
            player1Errors = 0;
            player2Score = 0;
            player2Errors = 0;
            ScoreLabel.Text = GetScoreText();
            MessageLabel.Text = "Pontuação resetada.";
        }

        private void OnRestartGameButtonClicked(object sender, EventArgs e)
        {
            // Resetar as pontuações
            player1Score = 0;
            player1Errors = 0;
            player2Score = 0;
            player2Errors = 0;

            // Resetar o jogador atual
            currentPlayer = 1;
            guessingPlayer = 2;

            // Resetar a interface
            ResetInterface();
        }

        private void OnUseHintButtonClicked(object sender, EventArgs e)
        {
            if (attemptsLeft > 0)
            {
                var undiscoveredIndices = new List<int>();
                for (int i = 0; i < guessedWord.Length; i++)
                {
                    if (guessedWord[i] == '_')
                    {
                        undiscoveredIndices.Add(i);
                    }
                }

                if (undiscoveredIndices.Count > 0)
                {
                    Random random = new Random();
                    int index = undiscoveredIndices[random.Next(undiscoveredIndices.Count)];
                    char hintLetter = wordToGuess[index];

                    for (int i = 0; i < wordToGuess.Length; i++)
                    {
                        if (wordToGuess[i] == hintLetter)
                        {
                            guessedWord[i] = hintLetter;
                        }
                    }

                    attemptsLeft--; // Reduz o número de tentativas

                    UpdateWordDisplay();
                    UpdateWrongGuesses();
                    UpdateHangmanDrawing();

                    // Verifica se o jogador ganhou após a dica
                    if (!guessedWord.Contains('_'))
                    {
                        MessageLabel.Text = $"Parabéns, você acertou a palavra!";
                        HangmanDrawing.Text = trophyArt;
                        DisableAllLetterButtons();
                        UpdateScore(guessingPlayer, true);
                        ShowNextRoundButton();
                    }
                }
                else
                {
                    MessageLabel.Text = "Todas as letras já foram descobertas.";
                }
            }
            else
            {
                MessageLabel.Text = "Você não tem mais tentativas para usar dicas.";
            }
        }
    }
}
