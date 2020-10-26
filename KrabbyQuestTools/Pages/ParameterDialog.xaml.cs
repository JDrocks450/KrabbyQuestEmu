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
    public partial class ParameterDialog : Window
    {
        public List<BlockParameter> Source { get; }
        public LevelDataBlock Subject;
        bool askSave = false;

        public ParameterDialog(LevelDataBlock subject)
        {
            InitializeComponent();
            Source = subject.Parameters.Values.ToList();
            Subject = subject;
            Load();
        }

        private void Load()
        {
            ParameterStack.Children.Clear();
            foreach (var param in Source) {
                var dock = new DockPanel() { Margin = new Thickness(0, 0, 0, 5) };
                var deleteButton = new Button() { Content = "x", Margin = new Thickness(0,0,10,0) };
                deleteButton.Click += DeleteButton_Click;
                deleteButton.Tag = param;
                DockPanel.SetDock(deleteButton, Dock.Left);
                dock.Children.Add(deleteButton);
                var textbox = new TextBox() { Width = 200, HorizontalAlignment = HorizontalAlignment.Right, Text = param.Value };
                dock.Children.Add(textbox);
                textbox.KeyDown += Textbox_KeyDown;
                textbox.Tag = param;
                var nameBox = new TextBlock() { Margin = new Thickness(0, 0, 10, 0), Text = param.Name + ":" };
                dock.Children.Add(nameBox);
                DockPanel.SetDock(nameBox, Dock.Left);
                DockPanel.SetDock(textbox, Dock.Right);
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
            Load();
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
            if (duplicates.Any()) foreach (var dup in duplicates) Source.Remove(dup);
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
                    Load();
                    return;
                }
                BlockParameter block = editing.Tag as BlockParameter;
                block.Value = value;                
                Load();
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
    }
}
