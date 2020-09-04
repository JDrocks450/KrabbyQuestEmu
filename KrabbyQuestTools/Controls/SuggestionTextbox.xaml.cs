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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KrabbyQuestTools.Controls
{
    /// <summary>
    /// Interaction logic for SuggestionTextbox.xaml
    /// </summary>
    public partial class SuggestionTextbox : UserControl
    {
        public SuggestionTextbox()
        {
            InitializeComponent();
            PreviewKeyDown += SuggestionTextbox_PreviewKeyDown;
        }

        private void SuggestionTextbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down && autoList.SelectedIndex < autoList.Items.Count)
                autoList.SelectedIndex++;
            if (e.Key == Key.Up && autoList.SelectedIndex > 0)
                autoList.SelectedIndex--;
            if (e.Key == Key.Enter)
            {                
                // Settings.  
                if (autoList.SelectedItem != null && autoList.Visibility == Visibility.Visible)
                    this.autoTextBox.Text = this.autoList.SelectedItem.ToString();  
                // Disable.  
                this.CloseAutoSuggestionBox();   
            }
        }

        #region Private properties.  

        /// <summary>  
        /// Auto suggestion list property.  
        /// </summary>  
        private List<string> autoSuggestionList = new List<string>();

        #endregion
 
        #region Protected / Public properties.  
  
        /// <summary>  
        /// Gets or sets Auto suggestion list property.  
        /// </summary>  
        public List<string> AutoSuggestionList  
        {  
            get { return this.autoSuggestionList; }  
            set { this.autoSuggestionList = value; }  
        }  
 
        #endregion  
 
        #region Open Auto Suggestion box method  
  
        /// <summary>  
        ///  Open Auto Suggestion box method  
        /// </summary>  
        private void OpenAutoSuggestionBox()  
        {  
            try  
            {  
                // Enable.  
                this.autoListPopup.Visibility = Visibility.Visible;  
                this.autoListPopup.IsOpen = true;  
                this.autoList.Visibility = Visibility.Visible;  
            }  
            catch (Exception ex)  
            {  
                // Info.  
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);  
                Console.Write(ex);  
            }  
        }  
 
        #endregion  
 
        #region Close Auto Suggestion box method  
  
        /// <summary>  
        ///  Close Auto Suggestion box method  
        /// </summary>  
        private void CloseAutoSuggestionBox()  
        {  
            try  
            {  
                // Enable.  
                this.autoListPopup.Visibility = Visibility.Collapsed;  
                this.autoListPopup.IsOpen = false;  
                this.autoList.Visibility = Visibility.Collapsed;  
            }  
            catch (Exception ex)  
            {  
                // Info.  
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);  
                Console.Write(ex);  
            }  
        }  
 
        #endregion  
 
        #region Auto Text Box text changed the method  
  
        /// <summary>  
        ///  Auto Text Box text changed method.  
        /// </summary>  
        /// <param name="sender">Sender parameter</param>  
        /// <param name="e">Event parameter</param>  
        private void AutoTextBox_TextChanged(object sender, TextChangedEventArgs e)  
        {  
            try  
            {  
                // Verification.  
                if (string.IsNullOrEmpty(this.autoTextBox.Text))  
                {  
                    // Disable.  
                    this.CloseAutoSuggestionBox();  
  
                    // Info.  
                    return;  
                }                   

                var source = this.AutoSuggestionList.Where(p => p.ToLower().Contains(this.autoTextBox.Text.ToLower())).ToList();
                if (source.Count == 1 && source[0] == autoTextBox.Text) {
                    CloseAutoSuggestionBox();
                    return;
                }
                // Enable.  
                this.OpenAutoSuggestionBox();
                // Settings.  
                this.autoList.ItemsSource = source;  
                
            }  
            catch (Exception ex)  
            {  
                // Info.  
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);  
                Console.Write(ex);  
            }  
        }  
 
        #endregion  
 
        #region Auto list selection changed method  
  
        /// <summary>  
        ///  Auto list selection changed method.  
        /// </summary>  
        /// <param name="sender">Sender parameter</param>  
        /// <param name="e">Event parameter</param>  
        private void AutoList_SelectionChanged(object sender, SelectionChangedEventArgs e)  
        {
            return;
            try  
            {  
                // Verification.  
                if (this.autoList.SelectedIndex <= -1)  
                {  
                    // Disable.  
                    this.CloseAutoSuggestionBox();  
  
                    // Info.  
                    return;  
                }  
  
                // Disable.  
                this.CloseAutoSuggestionBox();  
  
                // Settings.  
                this.autoTextBox.Text = this.autoList.SelectedItem.ToString();  
                this.autoList.SelectedIndex = -1;  
            }  
            catch (Exception ex)  
            {  
                // Info.  
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);  
                Console.Write(ex);  
            }  
        }  
 
        #endregion        
    }
}
