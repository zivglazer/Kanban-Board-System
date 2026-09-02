using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Frontend.Model;
using Frontend.ViewModel;

namespace Frontend.View
{
    public partial class DashboardView : Window
    {
        private DashboardVM dashVM;
        private UserModel userModel;
        public DashboardView(UserModel user) 
        {
            InitializeComponent();
            userModel = user;
            dashVM = new DashboardVM(user);
            DataContext = dashVM;
            dashVM.LoadBoards(); 
        }

        private void Board_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item && item.Content is BoardModel board)
            {
                OpenBoard(board);
            }
        }

        private void OpenBoard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is BoardModel board)
            {
                OpenBoard(board);
            }
        }

        private void DeleteBoard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is BoardModel board)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete the board '{board.Name}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        dashVM.SelectedBoard = board;
                        dashVM.DeleteBoard();
                        dashVM.LoadBoards();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting board: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CreateBoard_Click(object sender, RoutedEventArgs e)
        {
            ShowInputDialog("Create New Board", "Enter board name:");
            try
            {
                dashVM.CreateBoard();
                dashVM.LoadBoards();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating board: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dashVM.Logout();
                var loginView = new LoginView();
                loginView.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during logout: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenBoard(BoardModel board)
        {
            try
            {
                var boardView = new BoardView(userModel,board);
                boardView.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening board: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ShowInputDialog(string title, string promptText)
        {
            Window inputDialog = new Window
            {
                Title = title,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                DataContext = dashVM
            };

            Grid grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            TextBlock prompt = new TextBlock { Text = promptText, Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(prompt, 0);
            grid.Children.Add(prompt);

            TextBox textBox = new TextBox { Margin = new Thickness(0, 0, 0, 15) };
            textBox.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("NewBoardName") { Mode = System.Windows.Data.BindingMode.TwoWay });
            Grid.SetRow(textBox, 1);
            grid.Children.Add(textBox);

            StackPanel buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };

            Button okButton = new Button { Content = "OK", Width = 75, Margin = new Thickness(0, 0, 10, 0), IsDefault = true };
            okButton.Click += (s, e) => inputDialog.DialogResult = true;

            Button cancelButton = new Button { Content = "Cancel", Width = 75, IsCancel = true };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 2);
            grid.Children.Add(buttonPanel);

            inputDialog.Content = grid;
            textBox.Focus();

            return inputDialog.ShowDialog() == true ? textBox.Text : null;
        }
    }
}
