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
            Source = subject.Parameters.ToList();
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
                var nameBox = new TextBlock() { Margin = new Thickness(0, 0, 10, 0), Text = param.Name + ":" };
                dock.Children.Add(nameBox);
                DockPanel.SetDock(nameBox, Dock.Left);
                DockPanel.SetDock(textbox, Dock.Right);
                ParameterStack.Children.Add(dock);
            }
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
        }

        private void Save(string value)
        {
            var duplicates = Source.FindAll(x => x.Name == namebox.Text);
            if (duplicates.Any()) foreach (var dup in duplicates) Source.Remove(dup);
            Source.Add(new BlockParameter() { Name = namebox.Text, Value = value });            
            askSave = false;
        }

        TextBox editing = null;

        private void Textbox_KeyDown(object sender, KeyEventArgs e)
        {
            editing = sender as TextBox;
            if (editing == namebox)
                editing = textbox;
            if (e.Key == Key.Enter)
            {
                string value = (sender as TextBox).Text;
                if (sender == namebox)
                    value = textbox.Text;
                Save(value);
                Load();
            }
            else
                askSave = true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (askSave)
                Save(editing.Text);
            Subject.Parameters = Source;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
