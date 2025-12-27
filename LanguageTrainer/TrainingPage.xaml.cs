using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LanguageTrainer
{
    public partial class TrainingPage : Page
    {
        private string groupName;
        private string dictionaryName;
        private WordDictionary currentDictionary;
        private List<WordPair> allWords;
        private List<WordPair> wordsToRepeat;
        private List<WordPair> remainingWords;
        private Dictionary<WordPair, int> wordRepeatCount = new Dictionary<WordPair, int>();
        private int correctAnswers = 0;
        private int totalWords = 0;
        private bool isNativeToForeign = true;
        private int trainingMode = 0;
        private int questionType = 0;
        private string correctAnswer = "";
        private string questionWithBlanks = "";
        private bool wordWasWrong = false;
        private List<TrainingResult> trainingResults = new List<TrainingResult>();
        private Random random = new Random();

        public TrainingPage(string groupName, string dictionaryName, bool nativeToForeign, int mode = 0, int qType = 0)
        {
            InitializeComponent();

            this.groupName = groupName;
            this.dictionaryName = dictionaryName;
            this.isNativeToForeign = nativeToForeign;
            this.trainingMode = mode;
            this.questionType = qType;

            InitializeTraining();
            LoadNextWord();
            UpdateStats();
        }

        private void InitializeTraining()
        {
            DictionaryManager dictionaryManager = new DictionaryManager();
            currentDictionary = dictionaryManager.GetDictionary(dictionaryName, groupName);

            if (currentDictionary == null || currentDictionary.Words == null || currentDictionary.Words.Count == 0)
            {
                MessageBox.Show("Словарь пуст!");
                return;
            }

            allWords = new List<WordPair>(currentDictionary.Words);
            wordsToRepeat = new List<WordPair>(allWords);
            wordRepeatCount.Clear();

            foreach (WordPair word in allWords)
            {
                wordRepeatCount[word] = 0;
            }

            totalWords = allWords.Count;
            correctAnswers = 0;
            trainingResults.Clear();

            if (trainingMode == 2)
            {
                remainingWords = new List<WordPair>(allWords);
                ShuffleList(remainingWords);
            }
            else if (trainingMode == 1)
            {
                remainingWords = new List<WordPair>(allWords);
                ShuffleList(remainingWords);
            }
            else
            {
                remainingWords = new List<WordPair>(allWords);
            }

            TrainingTitle.Text = "Тренировка: " + dictionaryName;
            if (trainingMode == 1)
            {
                TrainingTitle.Text += " (Случайный)";
            }
            else if (trainingMode == 2)
            {
                TrainingTitle.Text += " (Бесконечный)";
                ResultsScrollViewer.Visibility = Visibility.Collapsed;
            }

            if (questionType == 1)
            {
                TrainingTitle.Text += " [_B_]";
            }
        }

        private string CreateQuestionWithBlanks(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;

            if (word.Length <= 3)
            {
                char[] chars = word.ToCharArray();
                for (int i = 1; i < chars.Length; i++)
                {
                    chars[i] = '_';
                }
                return new string(chars);
            }
            else
            {
                char[] chars = word.ToCharArray();

                List<int> availablePositions = new List<int>();
                for (int i = 1; i < word.Length; i++)
                {
                    availablePositions.Add(i);
                }

                int blanksToCreate = Math.Min(3, availablePositions.Count);

                for (int i = 0; i < blanksToCreate; i++)
                {
                    int randomIndex = random.Next(0, availablePositions.Count);
                    int pos = availablePositions[randomIndex];

                    chars[pos] = '_';
                    availablePositions.RemoveAt(randomIndex);
                }

                return new string(chars);
            }
        }

        private void ShuffleList(List<WordPair> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                WordPair value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private void LoadNextWord()
        {
            if (remainingWords.Count == 0 && trainingMode != 2)
            {
                ShowTrainingComplete();
                return;
            }
            else if (remainingWords.Count == 0 && trainingMode == 2)
            {
                RefillRemainingWords();
            }

            WordPair currentWord = remainingWords[0];
            correctAnswer = isNativeToForeign ? currentWord.ForeignWord : currentWord.NativeWord;

            if (isNativeToForeign)
            {
                if (questionType == 0)
                {
                    WordToTranslate.Text = "Переведите: " + currentWord.NativeWord;
                }
                else
                {
                    string wordToShow = currentWord.ForeignWord;
                    questionWithBlanks = CreateQuestionWithBlanks(wordToShow);
                    WordToTranslate.Text = "Вставьте буквы: " + questionWithBlanks;
                }
            }
            else
            {
                if (questionType == 0)
                {
                    WordToTranslate.Text = "Переведите: " + currentWord.ForeignWord;
                }
                else
                {
                    string wordToShow = currentWord.NativeWord;
                    questionWithBlanks = CreateQuestionWithBlanks(wordToShow);
                    WordToTranslate.Text = "Вставьте буквы: " + questionWithBlanks;
                }
            }

            AnswerTextBox.Text = "";
            AnswerTextBox.Background = new SolidColorBrush(Color.FromRgb(88, 86, 125));
            CorrectAnswerText.Text = "";
            wordWasWrong = false;
            AnswerTextBox.Focus();
        }

        private void RefillRemainingWords()
        {
            if (trainingMode == 2)
            {
                wordsToRepeat.Clear();

                foreach (WordPair word in allWords)
                {
                    if (wordRepeatCount[word] < 2)
                    {
                        wordsToRepeat.Add(word);
                    }
                }

                if (wordsToRepeat.Count > 0)
                {
                    remainingWords = new List<WordPair>(wordsToRepeat);
                    ShuffleList(remainingWords);
                }
                else
                {
                    remainingWords = new List<WordPair>(allWords);
                    ShuffleList(remainingWords);
                }
            }
        }

        private void AnswerTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CheckAnswer();
            }
        }

        private void CheckAnswer()
        {
            if (remainingWords.Count == 0 && trainingMode != 2)
                return;

            WordPair currentWord = remainingWords[0];
            string userAnswer = AnswerTextBox.Text.Trim();

            if (questionType == 1)
            {
                userAnswer = ReconstructWordFromBlanks(userAnswer);
            }

            string normalizedUserAnswer = userAnswer.ToLower();
            string normalizedCorrect = correctAnswer.Trim().ToLower();

            bool isCorrect = normalizedUserAnswer == normalizedCorrect;

            if (!wordWasWrong)
            {
                string questionText = "";
                if (isNativeToForeign)
                {
                    questionText = questionType == 0 ?
                        "Переведите: " + currentWord.NativeWord :
                        "Вставьте буквы: " + questionWithBlanks;
                }
                else
                {
                    questionText = questionType == 0 ?
                        "Переведите: " + currentWord.ForeignWord :
                        "Вставьте буквы: " + questionWithBlanks;
                }

                trainingResults.Add(new TrainingResult
                {
                    Question = questionText,
                    UserAnswer = AnswerTextBox.Text.Trim(),
                    CorrectAnswer = correctAnswer,
                    IsCorrect = isCorrect
                });
            }

            if (isCorrect)
            {
                if (!wordWasWrong)
                {
                    correctAnswers++;
                }
                currentWord.IsLearned = true;

                wordRepeatCount[currentWord] = wordRepeatCount[currentWord] + 1;

                remainingWords.RemoveAt(0);

                if (trainingMode == 2)
                {
                    if (remainingWords.Count == 0)
                    {
                        RefillRemainingWords();
                    }
                    else if (remainingWords.Count > 0)
                    {
                        ShuffleList(remainingWords);
                    }
                }
                else if (trainingMode == 1 && remainingWords.Count > 0)
                {
                    ShuffleList(remainingWords);
                }

                if (remainingWords.Count > 0 || trainingMode == 2)
                {
                    LoadNextWord();
                    UpdateStats();
                }
                else
                {
                    ShowTrainingComplete();
                }
            }
            else
            {
                AnswerTextBox.Background = Brushes.Red;
                CorrectAnswerText.Text = "Правильный ответ: " + correctAnswer;
                wordWasWrong = true;
            }
        }

        private string ReconstructWordFromBlanks(string userInput)
        {
            StringBuilder result = new StringBuilder();
            int inputIndex = 0;

            for (int i = 0; i < questionWithBlanks.Length; i++)
            {
                if (questionWithBlanks[i] == '_')
                {
                    if (inputIndex < userInput.Length)
                    {
                        result.Append(userInput[inputIndex]);
                        inputIndex++;
                    }
                    else
                    {
                        result.Append('_');
                    }
                }
                else
                {
                    result.Append(questionWithBlanks[i]);
                }
            }

            return result.ToString();
        }

        private void UpdateStats()
        {
            if (trainingMode == 2)
            {
                StatsText.Text = "";
            }
            else
            {
                int wordsDone = allWords.Count - remainingWords.Count;

                if (remainingWords.Count > 0)
                {
                    StatsText.Text = $"Слово: {wordsDone + 1} из {totalWords} | Правильно: {correctAnswers}";
                }
            }
        }

        private void ShowTrainingComplete()
        {
            WordToTranslate.Text = "Тренировка завершена!";
            AnswerTextBox.Visibility = Visibility.Collapsed;
            CorrectAnswerText.Visibility = Visibility.Collapsed;

            if (trainingMode != 2)
            {
                double percentage = totalWords > 0 ? (double)correctAnswers / totalWords * 100 : 0;
                StatsText.Text = $"Результат: {correctAnswers} из {totalWords} ({percentage:F0}%)";
            }
            else
            {
                StatsText.Text = "";
            }

            ShowResultsList();
        }

        private void ShowResultsList()
        {
            if (trainingMode == 2)
            {
                ResultsScrollViewer.Visibility = Visibility.Collapsed;
                return;
            }

            ResultsStackPanel.Children.Clear();
            ResultsScrollViewer.Visibility = Visibility.Visible;

            foreach (TrainingResult result in trainingResults)
            {
                TextBlock resultText = new TextBlock();
                resultText.FontSize = 14;
                resultText.FontFamily = new FontFamily("Comic Sans MS");
                resultText.Margin = new Thickness(0, 5, 0, 5);

                if (result.IsCorrect)
                {
                    resultText.Foreground = Brushes.LightGreen;
                    resultText.Text = $"{result.Question} - {result.UserAnswer} ({result.CorrectAnswer})";
                }
                else
                {
                    resultText.Foreground = Brushes.LightCoral;
                    resultText.Text = $"{result.Question} - {result.UserAnswer} ({result.CorrectAnswer})";
                }

                ResultsStackPanel.Children.Add(resultText);
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToHome();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.MainFrame.Navigate(new GroupPage(groupName));
            }
        }

        private void NavigateToHome()
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.MainFrame.Visibility = Visibility.Collapsed;
                mainWindow.MainFrame.Navigate(null);
                mainWindow.MainContent.Visibility = Visibility.Visible;
            }
        }
    }

    public class TrainingResult
    {
        public string Question { get; set; }
        public string UserAnswer { get; set; }
        public string CorrectAnswer { get; set; }
        public bool IsCorrect { get; set; }
    }
}