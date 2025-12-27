using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Input;

namespace LanguageTrainer
{
    public partial class GroupPage : Page
    {
        private string groupName;
        private DictionaryManager dictionaryManager;
        private DictionaryViewControl currentDictionaryView;
        private string selectedDictionaryName;

        private int currentLanguageDirection = 0;
        private int currentTrainingMode = 0;
        private int currentQuestionType = 0;

        private string[] languageDirections = new string[]
        {
            "Иностр. → Русский",
            "Русский → Иностр"
        };

        private string[] trainingModes = new string[]
        {
            "По очереди",
            "Случайное",
            "Бесконечное"
        };

        private string[] questionTypes = new string[]
        {
            "ABC",
            "_B_"
        };

        public GroupPage(string inputGroupName)
        {
            InitializeComponent();

            groupName = inputGroupName;
            dictionaryManager = new DictionaryManager();
            selectedDictionaryName = null;
            currentDictionaryView = null;

            InitializeContent();
            LoadDictionaries();
            ShowDictionaryContent();
            LoadTrainingDictionaries();

            UpdateSettings();
        }

        private void InitializeContent()
        {
            DictionaryTitle.Text = "Группа: " + groupName;
        }

        private void LoadTrainingDictionaries()
        {
            TrainingDictionariesContainer.Children.Clear();
            selectedDictionaryName = null;

            List<WordDictionary> dictionaries = dictionaryManager.GetDictionariesForGroup(groupName);

            foreach (WordDictionary dictionary in dictionaries)
            {
                CreateTrainingDictionaryButton(dictionary.Name);
            }
        }

        private void CreateTrainingDictionaryButton(string dictionaryName)
        {
            Button dictionaryButton = new Button();
            dictionaryButton.Style = (Style)FindResource("DictionaryButtonStyle");
            dictionaryButton.Content = dictionaryName;
            dictionaryButton.Tag = dictionaryName;
            dictionaryButton.Margin = new Thickness(10, 5, 10, 5);

            dictionaryButton.Click += (object sender, RoutedEventArgs e) =>
            {
                SelectDictionaryForTraining(dictionaryButton, dictionaryName);
            };

            TrainingDictionariesContainer.Children.Add(dictionaryButton);
        }

        private void SelectDictionaryForTraining(Button clickedButton, string dictionaryName)
        {
            foreach (object childObject in TrainingDictionariesContainer.Children)
            {
                if (childObject is Button button)
                {
                    if (button.Tag.ToString() == dictionaryName)
                    {
                        button.Background = new SolidColorBrush(Color.FromRgb(100, 150, 200));
                        selectedDictionaryName = dictionaryName;
                    }
                    else
                    {
                        button.Background = new SolidColorBrush(Color.FromRgb(88, 86, 125));
                    }
                }
            }
        }

        private void LanguageDirection_Click(object sender, MouseButtonEventArgs e)
        {
            currentLanguageDirection = (currentLanguageDirection + 1) % languageDirections.Length;
            UpdateSettings();
        }

        private void TrainingMode_Click(object sender, MouseButtonEventArgs e)
        {
            currentTrainingMode = (currentTrainingMode + 1) % trainingModes.Length;
            UpdateSettings();
        }

        private void QuestionType_Click(object sender, MouseButtonEventArgs e)
        {
            currentQuestionType = (currentQuestionType + 1) % questionTypes.Length;
            UpdateSettings();
        }

        private void UpdateSettings()
        {
            LanguageDirectionText.Text = languageDirections[currentLanguageDirection];
            TrainingModeText.Text = trainingModes[currentTrainingMode];
            QuestionTypeText.Text = questionTypes[currentQuestionType];
        }

        private void TrainingButton_Click(object sender, RoutedEventArgs e)
        {
            ShowTrainingContent();
        }

        private void ShowTrainingContent()
        {
            TrainingContent.Visibility = Visibility.Visible;
            DictionaryContent.Visibility = Visibility.Collapsed;
            TrainingTitle.Text = "Тренировать " + groupName;
            LoadTrainingDictionaries();

            selectedDictionaryName = null;
            DeselectAllTrainingDictionaries();
        }

        private void DeselectAllTrainingDictionaries()
        {
            foreach (object childObject in TrainingDictionariesContainer.Children)
            {
                if (childObject is Button button)
                {
                    button.Background = new SolidColorBrush(Color.FromRgb(88, 86, 125));
                }
            }
        }

        private void StartTraining_Click(object sender, RoutedEventArgs e)
        {
            if (selectedDictionaryName == null || selectedDictionaryName == "")
            {
                MessageBox.Show("Выберите словарь для тренировки!");
                return;
            }

            StartTrainingWithSettings(selectedDictionaryName);
        }

        private void StartTrainingWithSettings(string dictionaryName)
        {
            bool isNativeToForeign = true;
            if (currentLanguageDirection == 1)
            {
                isNativeToForeign = false;
            }

            int trainingMode = currentTrainingMode;
            int questionType = currentQuestionType;

            TrainingPage trainingPage = new TrainingPage(groupName, dictionaryName, isNativeToForeign, trainingMode, questionType);

            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.MainFrame.Navigate(trainingPage);
            }
        }

        private void ShowDictionaryContent()
        {
            DictionaryContent.Visibility = Visibility.Visible;
            TrainingContent.Visibility = Visibility.Collapsed;
            ShowDictionaryList();
        }

        private void ShowDictionaryList()
        {
            DictionariesListState.Visibility = Visibility.Visible;
            AddDictionaryBtn.Visibility = Visibility.Visible;
            DictionaryTitle.Text = "Словари группы: " + groupName;

            DictionaryViewState.Visibility = Visibility.Collapsed;
            DictionaryViewState.Children.Clear();
            currentDictionaryView = null;
        }

        private void LoadDictionaries()
        {
            DictionariesContainer.Children.Clear();

            List<WordDictionary> dictionaries = dictionaryManager.GetDictionariesForGroup(groupName);

            foreach (WordDictionary dictionary in dictionaries)
            {
                CreateDictionaryButton(dictionary.Name);
            }
        }

        private void CreateDictionaryButton(string dictionaryName)
        {
            Button dictionaryButton = new Button();
            dictionaryButton.Style = (Style)FindResource("DictionaryButtonStyle");
            dictionaryButton.Content = dictionaryName;
            dictionaryButton.Tag = dictionaryName;
            dictionaryButton.Margin = new Thickness(10, 5, 10, 5);

            dictionaryButton.Click += (object sender, RoutedEventArgs e) =>
            {
                OpenDictionary(dictionaryName);
            };

            ContextMenu contextMenu = new ContextMenu();
            contextMenu.Style = (Style)FindResource("GroupContextMenuStyle");

            MenuItem deleteItem = new MenuItem();
            deleteItem.Header = "Удалить";
            deleteItem.Style = (Style)FindResource("GroupMenuItemStyle");
            deleteItem.Click += (object s, RoutedEventArgs e) =>
            {
                DeleteDictionary(dictionaryButton, dictionaryName);
            };

            contextMenu.Items.Add(deleteItem);
            dictionaryButton.ContextMenu = contextMenu;

            DictionariesContainer.Children.Add(dictionaryButton);
        }

        private void OpenDictionary(string dictionaryName)
        {
            DictionariesListState.Visibility = Visibility.Collapsed;
            AddDictionaryBtn.Visibility = Visibility.Collapsed;
            DictionaryTitle.Text = "Словарь: " + dictionaryName;

            DictionaryViewState.Children.Clear();
            currentDictionaryView = new DictionaryViewControl(dictionaryName, groupName);

            DictionaryViewState.Children.Add(currentDictionaryView);
            DictionaryViewState.Visibility = Visibility.Visible;
        }

        private void DeleteDictionary(Button dictionaryButton, string dictionaryName)
        {
            MessageBoxResult result = MessageBox.Show(
                "Вы уверены, что хотите удалить словарь '" + dictionaryName + "'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No
            );

            if (result == MessageBoxResult.Yes)
            {
                dictionaryManager.DeleteDictionary(dictionaryName, groupName);
                DictionariesContainer.Children.Remove(dictionaryButton);
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.MainFrame.Visibility = Visibility.Collapsed;
                mainWindow.MainFrame.Navigate(null);
                mainWindow.MainContent.Visibility = Visibility.Visible;
            }
        }

        private void DictionaryButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDictionaryContent();
        }

        private void Create_List(object sender, RoutedEventArgs e)
        {
            Grid container = new Grid();
            container.Width = 250;
            container.Height = 100;
            container.Margin = new Thickness(10, 5, 10, 5);
            container.HorizontalAlignment = HorizontalAlignment.Left;

            TextBox textBox = new TextBox();
            textBox.Style = (Style)FindResource("BottomTextBoxStyleList");
            textBox.VerticalAlignment = VerticalAlignment.Center;
            textBox.HorizontalAlignment = HorizontalAlignment.Center;
            textBox.Text = "Введите название";
            textBox.Foreground = Brushes.Gray;

            textBox.KeyDown += (object s, KeyEventArgs args) =>
            {
                if (args.Key == Key.Enter)
                {
                    string dictionaryName = textBox.Text.Trim();
                    if (dictionaryName != "Введите название" &&
                        !string.IsNullOrWhiteSpace(dictionaryName) &&
                        dictionaryManager.AddDictionary(dictionaryName, groupName))
                    {
                        CreateDictionaryButton(dictionaryName);
                        DictionariesContainer.Children.Remove(container);
                    }
                }
                else if (args.Key == Key.Escape)
                {
                    DictionariesContainer.Children.Remove(container);
                }
            };

            container.Children.Add(textBox);
            DictionariesContainer.Children.Add(container);
            textBox.Focus();
            textBox.SelectAll();
        }
    }
}