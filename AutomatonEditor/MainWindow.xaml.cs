using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using DfaSimulator.Core;
using System.IO;
using System.Windows.Media.Imaging;

namespace DfaSimulator
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private bool _isDragging = false;
        private Point _clickPosition;
        private StateModel _draggedState;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            ((System.Collections.Specialized.INotifyCollectionChanged)SimulationHistoryList.Items).CollectionChanged +=
            (s, e) => { if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add) SimulationHistoryList.ScrollIntoView(e.NewItems[0]); };
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as Grid;
            if (grid == null) return;

            var hit = VisualTreeHelper.HitTest(grid, e.GetPosition(grid));

            StateModel clickedState = null;
            DependencyObject obj = hit?.VisualHit;
            while (obj != null && obj != grid)
            {
                if (obj is FrameworkElement fe && fe.DataContext is StateModel state)
                {
                    clickedState = state;
                    break;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }

            if (e.ClickCount == 2 && clickedState == null)
            {
                var pos = e.GetPosition(grid);
                _viewModel.AddNewState(pos.X, pos.Y);
                return;
            }

            if (clickedState != null)
            {
                _viewModel.SelectedState = clickedState;
                _isDragging = true;
                _draggedState = clickedState;
                _clickPosition = e.GetPosition(grid);
                grid.CaptureMouse();
            }
            else
            {
                _viewModel.SelectedState = null;
                _viewModel.SelectedTransition = null;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _draggedState != null)
            {
                var currentPosition = e.GetPosition(sender as Grid);
                _draggedState.X += currentPosition.X - _clickPosition.X;
                _draggedState.Y += currentPosition.Y - _clickPosition.Y;
                _clickPosition = currentPosition;
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _draggedState = null;
            (sender as Grid)?.ReleaseMouseCapture();
        }

        private void Transition_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is TransitionModel transition)
            {
                _viewModel.SelectedTransition = transition;
                e.Handled = true;
            }
        }

        private void Transition_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is TransitionModel transition)
            {
                _viewModel.SelectedTransition = transition;
                e.Handled = true;
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            ExportToPng(VisualEditor);
        }

        public void ExportToPng(FrameworkElement element)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PNG Image (*.png)|*.png",
                FileName = "moj_automat"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                double minX = double.MaxValue, minY = double.MaxValue;
                double maxX = double.MinValue, maxY = double.MinValue;

                foreach (var state in _viewModel.States)
                {
                    minX = Math.Min(minX, state.X - 30);
                    minY = Math.Min(minY, state.Y - 30);
                    maxX = Math.Max(maxX, state.X + state.Diameter + 30);
                    maxY = Math.Max(maxY, state.Y + state.Diameter + 30);
                }

                foreach (var transition in _viewModel.Transitions)
                {
                    minX = Math.Min(minX, Math.Min(transition.StartPoint.X, Math.Min(transition.ControlPoint.X, transition.EndPoint.X)) - 30);
                    minY = Math.Min(minY, Math.Min(transition.StartPoint.Y, Math.Min(transition.ControlPoint.Y, transition.EndPoint.Y)) - 30);
                    maxX = Math.Max(maxX, Math.Max(transition.StartPoint.X, Math.Max(transition.ControlPoint.X, transition.EndPoint.X)) + 30);
                    maxY = Math.Max(maxY, Math.Max(transition.StartPoint.Y, Math.Max(transition.ControlPoint.Y, transition.EndPoint.Y)) + 30);

                    if (transition.IsSelfLoop)
                    {
                        minY = Math.Min(minY, transition.From.Y - 60);
                        maxX = Math.Max(maxX, transition.From.X + 80);
                    }
                }

                if (!_viewModel.States.Any() && !_viewModel.Transitions.Any())
                {
                    minX = 0; minY = 0;
                    maxX = element.ActualWidth;
                    maxY = element.ActualHeight;
                }

                int width = (int)Math.Max(maxX - minX, 1);
                int height = (int)Math.Max(maxY - minY, 1);

                double oldWidth = element.Width;
                double oldHeight = element.Height;

                element.Width = Math.Max(element.ActualWidth, maxX + 50);
                element.Height = Math.Max(element.ActualHeight, maxY + 50);
                element.Measure(new Size(element.Width, element.Height));
                element.Arrange(new Rect(0, 0, element.Width, element.Height));
                element.UpdateLayout();

                RenderTargetBitmap bmp = new RenderTargetBitmap(
                    width, height, 96, 96, PixelFormats.Pbgra32);

                DrawingVisual visual = new DrawingVisual();
                using (DrawingContext dc = visual.RenderOpen())
                {
                    dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));

                    dc.PushTransform(new TranslateTransform(-minX, -minY));

                    VisualBrush vb = new VisualBrush(element)
                    {
                        Stretch = Stretch.None,
                        AlignmentX = AlignmentX.Left,
                        AlignmentY = AlignmentY.Top
                    };

                    dc.DrawRectangle(vb, null, new Rect(0, 0, element.Width, element.Height));
                }

                bmp.Render(visual);

                element.Width = oldWidth;
                element.Height = oldHeight;
                element.Measure(new Size(element.ActualWidth, element.ActualHeight));
                element.Arrange(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
                element.UpdateLayout();

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));

                using (var stream = System.IO.File.Create(saveFileDialog.FileName))
                {
                    encoder.Save(stream);
                }

                MessageBox.Show("Obraz został zapisany pomyślnie bez uciętych krawędzi!");
            }
        }
    }
}