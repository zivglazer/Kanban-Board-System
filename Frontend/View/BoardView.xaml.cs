using Frontend.Controllers;
using Frontend.Model;
using Frontend.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace Frontend.View
{
    public partial class BoardView : Window
    {
        private BoardVM boardVM;
        private UserModel user;
        public BoardView(UserModel user,BoardModel board)
        {
            InitializeComponent();
            this.user = user;
            boardVM = new BoardVM(user,board);
            DataContext = boardVM;
        }

        private void AdvanceTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is TaskModel task)
            {
                try
                {
                    boardVM.SelectedTask = task;
                    boardVM.AdvanceTask();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to advance task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var (title, description, dueDate) = ShowAddTaskDialog();
            try
            {
                boardVM.AddTask();
                MessageBox.Show("Task created successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AssignTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is TaskModel task)
            {
                string assigneeEmail = ShowInputDialog("Assign Task", "Enter assignee email:");
                if (!string.IsNullOrWhiteSpace(assigneeEmail))
                {
                    try
                    {
                        boardVM.SelectedTask = task; // Ensure the correct task is selected
                        boardVM.AssignTask(assigneeEmail);
                        MessageBox.Show("Task assigned successfully.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to assign task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // Simple input dialog (reuse your existing ShowInputDialog or use this)
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
                DataContext = boardVM
            };

            Grid grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            TextBlock prompt = new TextBlock { Text = promptText, Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(prompt, 0);
            grid.Children.Add(prompt);

            TextBox textBox = new TextBox { Margin = new Thickness(0, 0, 0, 15) };
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


        private (string Title, string Description, DateTime? DueDate) ShowAddTaskDialog()
        {
            Window dialog = new Window
            {
                Title = "Add Task",
                Width = 350,
                Height = 260,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                DataContext = boardVM
            };

            Grid grid = new Grid { Margin = new Thickness(10) };
            for (int i = 0; i < 6; i++)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Title
            grid.Children.Add(new TextBlock { Text = "Title:", Margin = new Thickness(0, 0, 0, 5) });
            TextBox titleBox = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
            titleBox.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("NewTaskTitle") { Mode = System.Windows.Data.BindingMode.TwoWay });
            Grid.SetRow(titleBox, 1);
            grid.Children.Add(titleBox);

            // Description
            Grid.SetRow(new TextBlock { Text = "Description:", Margin = new Thickness(0, 0, 0, 5) }, 2);
            TextBlock descLabel = new TextBlock { Text = "Description:", Margin = new Thickness(0, 0, 0, 5) };
            
            Grid.SetRow(descLabel, 2);
            grid.Children.Add(descLabel);
            TextBox descBox = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
            descBox.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("NewTaskDescription") { Mode = System.Windows.Data.BindingMode.TwoWay });
            Grid.SetRow(descBox, 3);
            grid.Children.Add(descBox);

            // Due Date
            TextBlock dueLabel = new TextBlock { Text = "Due Date:", Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(dueLabel, 4);
            grid.Children.Add(dueLabel);
            DatePicker duePicker = new DatePicker { SelectedDate = DateTime.Now.AddDays(1) };
            duePicker.SetBinding(DatePicker.SelectedDateProperty, new System.Windows.Data.Binding("NewTaskDueDate") { Mode = System.Windows.Data.BindingMode.TwoWay });
            Grid.SetRow(duePicker, 5);
            grid.Children.Add(duePicker);

            // Buttons
            StackPanel buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 15, 0, 0) };
            Button okButton = new Button { Content = "OK", Width = 75, Margin = new Thickness(0, 0, 10, 0), IsDefault = true };
            okButton.Click += (s, e) => dialog.DialogResult = true;
            Button cancelButton = new Button { Content = "Cancel", Width = 75, IsCancel = true };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            Grid.SetRow(buttonPanel, 6);
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.Children.Add(buttonPanel);

            dialog.Content = grid;
            titleBox.Focus();

            if (dialog.ShowDialog() == true)
                return (titleBox.Text, descBox.Text, duePicker.SelectedDate);
            else
                return (null, null, null);
        }
    }
}
