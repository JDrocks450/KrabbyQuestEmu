using StinkyFile.Blitz3D;
using StinkyFile.Blitz3D.Prim;
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

namespace KrabbyQuestTools.Controls
{
    /// <summary>
    /// Interaction logic for AnimKeysEditor.xaml
    /// </summary>
    public partial class AnimKeysEditor : Window
    {
        private readonly Animator blitz3DAnimation;
        private readonly Animation preview;
        List<Seq> _sequences = new List<Seq>();

        public AnimKeysEditor(Animator Blitz3DAnimation, Animation preview)
        {
            InitializeComponent();
            blitz3DAnimation = Blitz3DAnimation;
            _sequences.AddRange(blitz3DAnimation.Sequences);
            this.preview = preview;            
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            foreach(var seq in _sequences)
            {
                if (seq.IsRootSeq) continue;
                var nSeq = blitz3DAnimation.extractSeq(seq.First, seq.Last, 0);
                nSeq.Name = seq.Name;
            }
            Close();
        }

        enum KeySelectorMode
        {
            Start,
            End,
            Done
        }
        int selectedStartFrame = 0, selectedEndFrame = 0, currentFrame = 0;
        KeySelectorMode SelectorMode;
        bool cursorShown = false;
        Line cursor, startCursor, endCursor;
        Rectangle space;

        void Redisplay()
        {
            //reset
            SelectorMode = KeySelectorMode.Start;
            selectedStartFrame = selectedEndFrame = 0;
            cursorShown = false;
            endCursor = startCursor = cursor = null;
            space = null;
            KeySelector.Children.Clear();

            //display sequences
            foreach (var seq in _sequences)
            {
                if (seq.IsRootSeq) continue;
                SolidColorBrush randomColor = new SolidColorBrush(AppResources.BatchRandomColor(1)[0]);
                Brush semiTransparent = new SolidColorBrush(randomColor.Color);
                semiTransparent.Opacity = .5;
                Border border = null;
                KeySelector.Children.Add(
                    border = new Border
                    {
                        Child = new TextBlock()
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Text = (string.IsNullOrWhiteSpace(seq.Name) ? seq.ID.ToString() : seq.Name + $" ({seq.ID})") + $": {seq.FrameLength} frames",
                            Padding = new Thickness(5,3,5,3),
                            TextWrapping = TextWrapping.Wrap,
                            Background = randomColor,
                            TextAlignment = TextAlignment.Center
                        },
                        Padding = new Thickness(10),
                        BorderBrush = randomColor,
                        BorderThickness = new Thickness(2, 0, 2, 0),
                        Background = semiTransparent,
                        Width = (seq.FrameLength / (double)blitz3DAnimation.Frames) * KeySelector.ActualWidth,
                        Height = KeySelector.ActualHeight
                    });

                if (border.Width < 60)
                {
                    ((TextBlock)border.Child).Text = seq.ID.ToString();
                    border.Padding = new Thickness(0);
                }
                Canvas.SetLeft(border, (seq.First / (double)blitz3DAnimation.Frames) * KeySelector.ActualWidth);
                
            }
            DescBlock.Text = $"Sequences: {_sequences.Count()}, Frames: {preview.numPositionKeys()}";
            DataChart.DisplayAnimation(preview);
        }

        void createCursor(ref Line refCursor)
        {
            refCursor = new Line()
            {
                Stroke = SelectorMode == KeySelectorMode.Start ? Brushes.Yellow : Brushes.Orange,
                StrokeThickness = 3
            };
            if (SelectorMode == KeySelectorMode.Start)
                startCursor = refCursor;
            else if (SelectorMode == KeySelectorMode.End) endCursor = refCursor;
        }

        private void KeySelector_MouseEnter(object sender, MouseEventArgs e)
        {
            if (SelectorMode == KeySelectorMode.Done) return;
            if (!cursorShown)
            {
                createCursor(ref cursor);                
                KeySelector.Children.Add(cursor);
                cursorShown = true;
            }            
        }

        private void KeySelector_MouseLeave(object sender, MouseEventArgs e)
        {
            //StartBox.Text = selectedStartFrame.ToString();
            //EndBox.Text = selectedEndFrame.ToString();
        }

        private void KeySelector_Loaded(object sender, RoutedEventArgs e)
        {
            Redisplay();
        }

        private void PosButton_Click(object sender, RoutedEventArgs e)
        {
            DataChart.DisplayData(AnimKeyFrameGraph.GraphingData.Position);
            SelectedGraphLabel.Text = "Position";
        }

        private void RotButton_Click(object sender, RoutedEventArgs e)
        {
            DataChart.DisplayData(AnimKeyFrameGraph.GraphingData.Rotation);
            SelectedGraphLabel.Text = "Rotation";
        }

        private void SclButton_Click(object sender, RoutedEventArgs e)
        {
            DataChart.DisplayData(AnimKeyFrameGraph.GraphingData.Scale);
            SelectedGraphLabel.Text = "Scale";
        }

        private void StartBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!int.TryParse(StartBox.Text, out selectedStartFrame))
                return;
            bool numChanged = false;
            if (selectedStartFrame < 0)
            {
                selectedStartFrame = 0;
                numChanged = true;
            }
            if (selectedStartFrame > blitz3DAnimation.Frames)
            {
                selectedStartFrame = blitz3DAnimation.Frames;
                numChanged = true;
            }
            if (numChanged)
            {
                StartBox.Text = selectedStartFrame.ToString();
                return;
            }
            if (startCursor == null)
            {
                createCursor(ref cursor);                
                KeySelector.Children.Add(cursor);
                cursorShown = true;
            }
            MoveSelectionCursor(startCursor, selectedStartFrame);  
        }

        private void EndBox_TextChanged(object sender, TextChangedEventArgs e)
        {            
            if (!int.TryParse(EndBox.Text, out selectedEndFrame))
                return;
            bool numChanged = false;
            if (selectedEndFrame > blitz3DAnimation.Frames)
            {
                selectedEndFrame = blitz3DAnimation.Frames;
                numChanged = true;
            }
            if (selectedEndFrame < selectedStartFrame)
            {
                selectedEndFrame = selectedStartFrame;
                numChanged = true;
            }
            if (numChanged)
            {
                //EndBox.Text = selectedEndFrame.ToString();
                return;
            }
            if (endCursor == null)
            {
                SelectorMode = KeySelectorMode.End;
                createCursor(ref cursor);                
                KeySelector.Children.Add(cursor);    
            }
            MoveSelectionCursor(endCursor, selectedEndFrame);  
        }

        private void MoveSelectionCursor(Line cursor, int time)
        {
            cursor.X1 = (time / (double)blitz3DAnimation.Frames) * KeySelector.ActualWidth;
            if (cursor.X1 < 0 )
                cursor.X1 = 0;
            if (cursor.X1 > KeySelector.ActualWidth)
                cursor.X1 = KeySelector.ActualWidth;
            if (SelectorMode == KeySelectorMode.End)
            {
                if (cursor.X1 <= startCursor.X1)
                    cursor.X1 = startCursor.X1;
            }
            cursor.X2 = cursor.X1;
            cursor.Y1 = 0;
            cursor.Y2 = KeySelector.ActualHeight;

            if (startCursor != null && endCursor != null)
            {
                if (space == null)
                {
                    space = new Rectangle()
                    {
                        Fill = Brushes.Pink,
                        Opacity = .5
                    };
                    KeySelector.Children.Add(space);
                }
                Canvas.SetLeft(space, startCursor.X1);
                space.Width = Math.Abs(endCursor.X1 - startCursor.X1);
                space.Height = KeySelector.ActualHeight;
            }
        }

        private void KeySelector_MouseMove(object sender, MouseEventArgs e)
        {
            if (SelectorMode == KeySelectorMode.Done) return;
            if (cursor == null) return;
            currentFrame = (int)Math.Round((e.GetPosition(KeySelector).X / KeySelector.ActualWidth) * blitz3DAnimation.Frames);            

            if (SelectorMode == KeySelectorMode.Start)
                StartBox.Text = currentFrame.ToString();
            else EndBox.Text = currentFrame.ToString();            
        }

        private void KeySelector_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectorMode == KeySelectorMode.Done) return;
            if (SelectorMode == KeySelectorMode.Start)
                selectedStartFrame = currentFrame;
            else selectedEndFrame = currentFrame;
            if (SelectorMode == KeySelectorMode.End)
                SelectorMode = KeySelectorMode.Done;
            else
            {
                SelectorMode = KeySelectorMode.End;
                createCursor(ref cursor);                
                KeySelector.Children.Add(cursor);                                 
            }
        }

        private void FinishSeq_Click(object sender, RoutedEventArgs e)
        {
            Seq createdSequence = new Seq(selectedStartFrame, selectedEndFrame, _sequences.Count);
            createdSequence.Name = NameBox.Text;
            _sequences.Add(createdSequence);
            Redisplay();
        }

        private void CancelSeq_Click(object sender, RoutedEventArgs e)
        {
            Redisplay();
        }
    }
}
