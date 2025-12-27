using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LanguageTrainer
{
    public partial class DictionaryViewControl : UserControl
    {
        private readonly string dictionaryName;
        private readonly string groupName;
        private readonly DictionaryManager dictionaryManager = new DictionaryManager();
        private WordDictionary currentDictionary;

        public DictionaryViewControl(string dictionaryName, string groupName)
        {
            InitializeComponent();
            this.dictionaryName = dictionaryName;
            this.groupName = groupName;

            InitializeDictionary();
            LoadWords();

            DictionaryTitle.Text = "Словарь: " + dictionaryName;
        }

        private void InitializeDictionary()
        {
            List<WordDictionary> dictionaries = dictionaryManager.GetDictionariesForGroup(groupName);
            currentDictionary = dictionaries.FirstOrDefault(d => d.Name == dictionaryName);

            if (currentDictionary == null)
            {
                currentDictionary = new WordDictionary
                {
                    Name = dictionaryName,
                    GroupName = groupName,
                    Words = new List<WordPair>()
                };
                dictionaryManager.AddDictionary(dictionaryName, groupName);
            }
        }

        private void LoadWords()
        {
            WordsContainer.Children.Clear();

            if (currentDictionary != null && currentDictionary.Words != null)
            {
                foreach (WordPair wordPair in currentDictionary.Words)
                {
                    AddWordPairToUI(wordPair.NativeWord, wordPair.ForeignWord, false);
                }
            }
        }

        private void AddWordPairToUI(string nativeWord, string foreignWord, bool isNewWord = false)
        {
            Grid container = new Grid();
            container.Style = (Style)FindResource("WordPairContainerStyle");

            if (isNewWord)
            {
                AddNewWordUI(container, nativeWord, foreignWord);
            }
            else
            {
                AddExistingWordUI(container, nativeWord, foreignWord);
            }

            WordsContainer.Children.Add(container);
        }

        private void AddNewWordUI(Grid container, string nativeWord, string foreignWord)
        {
            container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            TextBox nativeTextBox = CreateNewWordTextBox();
            TextBlock separator = CreateWordSeparator();
            TextBox foreignTextBox = CreateNewWordTextBox();

            Grid.SetColumn(nativeTextBox, 0);
            Grid.SetColumn(separator, 1);
            Grid.SetColumn(foreignTextBox, 2);

            void SaveNewWord()
            {
                if (!string.IsNullOrWhiteSpace(nativeTextBox.Text) &&
                    !string.IsNullOrWhiteSpace(foreignTextBox.Text))
                {
                    AddWordToDictionary(nativeTextBox.Text, foreignTextBox.Text);
                    AddWordPairToUI(nativeTextBox.Text, foreignTextBox.Text, false);
                    WordsContainer.Children.Remove(container);
                }
                else
                {
                    MessageBox.Show("Оба поля должны быть заполнены!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    SetFocusWithDelay(nativeTextBox);
                }
            }

            nativeTextBox.KeyDown += (s, e) => HandleNewWordKeyDown(e.Key, nativeTextBox, foreignTextBox, SaveNewWord);
            foreignTextBox.KeyDown += (s, e) => HandleNewWordKeyDown(e.Key, foreignTextBox, null, SaveNewWord, container);

            nativeTextBox.LostFocus += (s, e) => HandleNewWordLostFocus(nativeTextBox, foreignTextBox, container);
            foreignTextBox.LostFocus += (s, e) => HandleNewWordLostFocus(foreignTextBox, nativeTextBox, container);

            container.Children.Add(nativeTextBox);
            container.Children.Add(separator);
            container.Children.Add(foreignTextBox);

            SetFocusWithDelay(nativeTextBox);
        }

        private void AddExistingWordUI(Grid container, string nativeWord, string foreignWord)
        {
            container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });

            Border nativeBorder = CreateWordBorder(nativeWord);
            TextBlock separator = CreateWordSeparator();
            Border foreignBorder = CreateWordBorder(foreignWord);
            Button deleteButton = CreateDeleteButton();

            Grid.SetColumn(nativeBorder, 0);
            Grid.SetColumn(separator, 1);
            Grid.SetColumn(foreignBorder, 2);
            Grid.SetColumn(deleteButton, 3);

            nativeBorder.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ClickCount == 2)
                {
                    EnableWordEditing(container, nativeBorder, nativeWord, foreignWord, UpdateWordInDictionary);
                    e.Handled = true;
                }
            };

            foreignBorder.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ClickCount == 2)
                {
                    EnableWordEditing(container, foreignBorder, foreignWord, nativeWord,
                        (oldForeign, oldNative, newForeign, newNative) => UpdateWordInDictionary(oldNative, oldForeign, newNative, newForeign));
                    e.Handled = true;
                }
            };

            deleteButton.Click += (s, e) =>
            {
                WordsContainer.Children.Remove(container);
                RemoveWordFromDictionary(nativeWord, foreignWord);
            };

            container.Children.Add(nativeBorder);
            container.Children.Add(separator);
            container.Children.Add(foreignBorder);
            container.Children.Add(deleteButton);
        }

        private Border CreateWordBorder(string text)
        {
            TextBlock textBlock = new TextBlock { Text = text };
            textBlock.Style = (Style)FindResource("WordTextBlockStyle");

            Border border = new Border { Child = textBlock };
            border.Style = (Style)FindResource("WordBorderStyle");

            return border;
        }

        private TextBox CreateNewWordTextBox()
        {
            TextBox textBox = new TextBox();
            textBox.Style = (Style)FindResource("NewWordTextBoxStyle");
            return textBox;
        }

        private TextBox CreateEditWordTextBox()
        {
            TextBox textBox = new TextBox();
            textBox.Style = (Style)FindResource("EditWordTextBoxStyle");
            return textBox;
        }

        private TextBlock CreateWordSeparator()
        {
            TextBlock separator = new TextBlock { Text = "—" };
            separator.Style = (Style)FindResource("WordSeparatorStyle");
            return separator;
        }

        private Button CreateDeleteButton()
        {
            Button button = new Button();
            button.Style = (Style)FindResource("DeleteWordButtonStyle");
            return button;
        }

        private void EnableWordEditing(Grid container, Border wordBorder, string currentText,
            string otherWord, Action<string, string, string, string> updateAction)
        {
            int column = Grid.GetColumn(wordBorder);

            TextBox textBox = CreateEditWordTextBox();
            textBox.Text = currentText;

            container.Children.Remove(wordBorder);
            container.Children.Insert(column, textBox);
            Grid.SetColumn(textBox, column);

            SetFocusWithDelay(textBox);

            void CompleteEdit()
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text))
                {
                    TextBlock textBlock = wordBorder.Child as TextBlock;
                    if (textBlock != null)
                    {
                        textBlock.Text = textBox.Text;
                        updateAction(currentText, otherWord, textBox.Text, otherWord);
                    }
                }

                container.Children.Remove(textBox);
                container.Children.Insert(column, wordBorder);
            }

            textBox.LostFocus += (s, e) => CompleteEdit();

            textBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    CompleteEdit();
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    container.Children.Remove(textBox);
                    container.Children.Insert(column, wordBorder);
                    e.Handled = true;
                }
            };
        }

        private void HandleNewWordKeyDown(Key key, TextBox currentTextBox, TextBox nextTextBox,
            Action saveAction, Grid container = null)
        {
            if (key == Key.Enter)
            {
                if (nextTextBox != null)
                {
                    nextTextBox.Focus();
                    nextTextBox.SelectAll();
                }
                else
                {
                    saveAction();
                }
            }
            else if (key == Key.Escape && container != null)
            {
                WordsContainer.Children.Remove(container);
            }
        }

        private void HandleNewWordLostFocus(TextBox textBox1, TextBox textBox2, Grid container)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) &&
                string.IsNullOrWhiteSpace(textBox2.Text))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (!textBox1.IsFocused && !textBox2.IsFocused)
                        WordsContainer.Children.Remove(container);
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void AddWordToDictionary(string nativeWord, string foreignWord)
        {
            if (currentDictionary != null)
            {
                if (currentDictionary.Words == null)
                    currentDictionary.Words = new List<WordPair>();

                currentDictionary.Words.Add(new WordPair
                {
                    NativeWord = nativeWord,
                    ForeignWord = foreignWord
                });
                SaveDictionary();
            }
        }

        private void SetFocusWithDelay(TextBox textBox)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                textBox.Focus();
                textBox.SelectAll();

                if (!textBox.IsFocused)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        textBox.Focus();
                        textBox.SelectAll();
                    }), System.Windows.Threading.DispatcherPriority.Render);
                }
            }), System.Windows.Threading.DispatcherPriority.Render);
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            AddWordPairToUI("", "", true);

            ScrollViewer scrollViewer = GetScrollViewer(WordsContainer);
            if (scrollViewer != null)
                scrollViewer.ScrollToBottom();
        }

        private ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer scrollViewer)
                return scrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                ScrollViewer result = GetScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void UpdateWordInDictionary(string oldNative, string oldForeign, string newNative, string newForeign)
        {
            if (currentDictionary == null || currentDictionary.Words == null) return;

            WordPair wordPair = currentDictionary.Words.FirstOrDefault(w =>
                w.NativeWord == oldNative && w.ForeignWord == oldForeign);

            if (wordPair != null)
            {
                wordPair.NativeWord = newNative;
                wordPair.ForeignWord = newForeign;
                SaveDictionary();
            }
        }

        private void RemoveWordFromDictionary(string nativeWord, string foreignWord)
        {
            if (currentDictionary == null || currentDictionary.Words == null) return;

            WordPair wordPair = currentDictionary.Words.FirstOrDefault(w =>
                w.NativeWord == nativeWord && w.ForeignWord == foreignWord);

            if (wordPair != null)
            {
                currentDictionary.Words.Remove(wordPair);
                SaveDictionary();
            }
        }

        private void SaveDictionary()
        {
            if (currentDictionary == null) return;
            dictionaryManager.UpdateDictionary(currentDictionary);
        }

        public string GetDictionaryName()
        {
            return currentDictionary != null ? currentDictionary.Name : dictionaryName;
        }

        public void UpdateDictionaryName(string newName)
        {
            if (currentDictionary != null)
            {
                currentDictionary.Name = newName;
                dictionaryManager.UpdateDictionary(currentDictionary);
            }
        }

        public WordDictionary GetCurrentDictionary()
        {
            return currentDictionary;
        }
    }
}