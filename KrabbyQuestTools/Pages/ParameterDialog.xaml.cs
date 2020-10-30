using KrabbyQuestTools.Controls;
using StinkyFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KrabbyQuestTools.Pages
{
    /// <summary>
    /// Interaction logic for ParameterDialog.xaml
    /// </summary>
    public partial class ParameterDialog : Page
    {
        public List<BlockParameter> Source { get; private set; }
        public LevelDataBlock Subject;
        bool askSave = false, isWindow = false;

        public ParameterDialog()
        {
            InitializeComponent();
            if (!(Parent is Window))
            {
                CancelButton.Visibility = Visibility.Collapsed;
                isWindow = false;
            }
            else isWindow = true;
        }

        public ParameterDialog(LevelDataBlock subject) : this()
        {           
            Load(subject);
        }

        public void Load(LevelDataBlock subject)
        {
            if (Subject != subject)
            {
                Source = subject.Parameters.Values.ToList();
                Subject = subject;
            }
            ParameterStack.Children.Clear();
            foreach (var param in Source) {
                var dock = new Grid() { Margin = new Thickness(0, 0, 0, 5) };
                dock.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(1, GridUnitType.Star),
                });
                dock.ColumnDefinitions.Add(new ColumnDefinition()                
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });
                var deleteButton = new Button() { Content = "x", Background = Brushes.DarkRed, BorderBrush = Brushes.Red, Margin = new Thickness(0,0,10,0) };
                deleteButton.Click += DeleteButton_Click;
                deleteButton.Tag = param;
                var stack = new DockPanel();
                stack.Children.Add(deleteButton);
                Grid.SetColumn(stack, 0);
                dock.Children.Add(stack);
                var textbox = new TextBox() { Text = param.Value };
                dock.Children.Add(textbox);
                Grid.SetColumn(textbox, 1);
                textbox.KeyDown += Textbox_KeyDown;
                textbox.Tag = param;
                var nameBox = new TextBlock() { Margin = new Thickness(0, 0, 10, 0), Text = param.Name + ":" };
                DockPanel.SetDock(nameBox, Dock.Right);
                stack.Children.Add(nameBox);
                ParameterStack.Children.Add(dock);
            }
            AddNewButton.IsEnabled = true;
            BlockParameter.LoadParameterDB();
            namebox.AutoSuggestionList.Clear();
            foreach (var name in BlockParameter.ParameterDBDescriptions.Keys)
                namebox.AutoSuggestionList.Add(name);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Source.Remove((BlockParameter)(sender as Button).Tag);
            Load(Subject);
        }

        private void AddNewButton_Click(object sender, RoutedEventArgs e)
        {
            ParameterStack.Children.Add(AddNewDock);
            ScrollView.ScrollToBottom();
            askSave = true;
            AddNewButton.IsEnabled = false;
        }

        private void Save(string name, string value)
        {
            var duplicates = Source.FindAll(x => x.Name == name);
            if (duplicates.Any()) 
                foreach (var dup in duplicates) 
                    Source.Remove(dup);
            Source.Add(new BlockParameter() { Name = namebox.autoTextBox.Text, Value = value });            
            askSave = false;
        }

        TextBox editing = null;

        private void Textbox_KeyDown(object sender, KeyEventArgs e)
        {
            editing = (sender as SuggestionTextbox)?.autoTextBox;
            askSave = true;
            if (sender is TextBox)
            {
                askSave = false;
                editing = sender as TextBox;
            }
            if (e.Key == Key.Enter)
            {
                if (editing == namebox.autoTextBox)
                {
                    MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
                    return;
                }
                string value = editing.Text;
                if (editing.Tag == null)
                {
                    Save(namebox.autoTextBox.Text, value);
                    Load(Subject);
                    return;
                }
                BlockParameter block = editing.Tag as BlockParameter;
                block.Value = value;                
                Load(Subject);
            }                
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (askSave)
                Save(namebox.autoTextBox.Text, editing.Text);
            Subject.Parameters = new Dictionary<string, BlockParameter>();
            foreach (var param in Source)
                Subject.Parameters.Add(param.Name, param);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void Close()
        {
            if (isWindow)
                (Parent as Window).Close();
        }
    }
}
