using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LanguageTrainer
{
    public partial class MainWindow : Window
    {
        private readonly SavingLanguageGroup groupSaver = new SavingLanguageGroup();

        public MainWindow()
        {
            InitializeComponent();
            foreach (string groupName in groupSaver.Groups)
                CreateGroupUI(groupName);
        }

        private void CreateGroupButton_Click(object sender, RoutedEventArgs e)
        {
            CreateGroupUI();
        }

        private void CreateGroupUI(string groupName = null)
        {
            Grid container = new Grid();
            container.Style = (Style)FindResource("ButtonContainerStyle");

            Button newButton = new Button();
            newButton.Style = (Style)FindResource("GroupButtonStyle");

            SolidColorBrush purpleBrush = new SolidColorBrush(Color.FromRgb(88, 86, 125));
            newButton.Background = purpleBrush;
            newButton.Foreground = Brushes.White;
            newButton.FontSize = 16;
            newButton.FontFamily = new FontFamily("Comic Sans MS");

            if (groupName != null)
            {
                string flagUrl = FlagManager.GetFlagUrl(groupName);
                if (!string.IsNullOrEmpty(flagUrl))
                {
                    try
                    {
                        ImageBrush flagBrush = new ImageBrush();
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(flagUrl);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        flagBrush.ImageSource = bitmap;
                        flagBrush.Stretch = Stretch.UniformToFill;
                        newButton.Background = flagBrush;
                    }
                    catch
                    {
                    }
                }

                newButton.Tag = groupName;
                newButton.Content = groupName;
            }
            else
            {
                newButton.Content = "Новая группа";
            }

            newButton.Click += NewButton_ClickHandler;

            ContextMenu contextMenu = new ContextMenu();
            contextMenu.Style = (Style)FindResource("GroupContextMenuStyle");

            MenuItem deleteItem = new MenuItem();
            deleteItem.Header = "Удалить";
            deleteItem.Style = (Style)FindResource("GroupMenuItemStyle");

            MenuItem imageItem = new MenuItem();
            imageItem.Header = "Загрузить картинку";
            imageItem.Style = (Style)FindResource("GroupMenuItemStyle");

            contextMenu.Items.Add(deleteItem);
            contextMenu.Items.Add(imageItem);

            newButton.ContextMenu = contextMenu;

            deleteItem.Click += (s, e) => DeleteGroup(container, newButton.Tag as string);
            imageItem.Click += (s, e) => { };

            container.Children.Add(newButton);

            if (groupName != null)
            {
                CategoriesContainer.Children.Add(container);
                return;
            }

            TextBox textBox = new TextBox();
            textBox.Style = (Style)FindResource("BottomTextBoxStyle");

            textBox.KeyDown += (s, e) =>
            {
                if (e.Key != Key.Enter) return;
                string name = textBox.Text.Trim();
                if (name.Length == 0) return;

                if (groupSaver.AddGroup(name))
                {
                    string flagUrl = FlagManager.GetFlagUrl(name);
                    if (!string.IsNullOrEmpty(flagUrl))
                    {
                        try
                        {
                            ImageBrush flagBrush = new ImageBrush();
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(flagUrl);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            flagBrush.ImageSource = bitmap;
                            flagBrush.Stretch = Stretch.UniformToFill;
                            newButton.Background = flagBrush;
                        }
                        catch
                        {
                        }
                    }

                    newButton.Content = name;
                    newButton.Tag = name;
                    container.Children.Remove(textBox);
                }
            };

            CategoriesContainer.Children.Add(container);
            container.Children.Add(textBox);
            textBox.Focus();
        }

        private void NewButton_ClickHandler(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            string selectedGroupName = clickedButton.Tag as string;

            if (string.IsNullOrEmpty(selectedGroupName))
            {
                return;
            }

            GroupPage groupPage = new GroupPage(selectedGroupName);
            MainFrame.Navigate(groupPage);
            MainFrame.Visibility = Visibility.Visible;
            MainContent.Visibility = Visibility.Collapsed;
        }

        private void DeleteGroup(Grid container, string groupName)
        {
            if (string.IsNullOrEmpty(groupName)) return;

            if (MessageBox.Show("Удалить группу '" + groupName + "' и все её словари?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            CategoriesContainer.Children.Remove(container);

            groupSaver.DeleteGroup(groupName);

            DictionaryManager dictionaryManager = new DictionaryManager();
            dictionaryManager.DeleteAllDictionariesForGroup(groupName);
        }
    }
}