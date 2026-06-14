using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Input;

namespace AutomatonEditor;


public partial class MainWindow : Window
{
<<<<<<< HEAD
    Automaton automation = new();
    int counter = 0;
    State? selectedState;
    bool isDragging = false;
=======
    Automaton automation = new();
    int counter = 0;
    State? selectedState;
    bool isDragging = false;
>>>>>>> 4608bdaf0314368fd9aa635c4b0457c9661b8291
    Point offset;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = automation;
    }
<<<<<<< HEAD
    public void CanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            Point pos = e.GetPosition(StatesControl);

            State state = new()
            {
                Name = $"q{counter}",
                X = pos.X - 25,
                Y = pos.Y - 25,
            };
            automation.States.Add(state);
            counter++;

        }
        else {
            foreach (var s in automation.States)
                s.IsSelected = false;

            selectedState = null;
        
        }
    }
    public void State_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if (sender is FrameworkElement element && element.DataContext is State state) {
            foreach (var s in automation.States)
                s.IsSelected = false;
            state.IsSelected = true;
            selectedState = state;
            isDragging = true;

            Point mousePos = e.GetPosition(StatesControl);

            offset = new Point(
                mousePos.X - state.X,
                mousePos.Y - state.Y);

            element.CaptureMouse();
            e.Handled = true;

        }
    }
    private void State_MouseMove(object sender, MouseEventArgs e) {
=======
    public void CanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            Point pos = e.GetPosition(StatesControl);

            State state = new()
            {
                Name = $"q{counter}",
                X = pos.X - 25,
                Y = pos.Y - 25,
            };
            automation.States.Add(state);
            counter++;

        }
        else {
            foreach (var s in automation.States)
                s.IsSelected = false;

            selectedState = null;
        
        }
    }
    public void State_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if (sender is FrameworkElement element && element.DataContext is State state) {
            foreach (var s in automation.States)
                s.IsSelected = false;
            state.IsSelected = true;
            selectedState = state;
            isDragging = true;

            Point mousePos = e.GetPosition(StatesControl);

            offset = new Point(
                mousePos.X - state.X,
                mousePos.Y - state.Y);

            element.CaptureMouse();
            e.Handled = true;

        }
    }
    private void State_MouseMove(object sender, MouseEventArgs e) {
>>>>>>> 4608bdaf0314368fd9aa635c4b0457c9661b8291
        if (isDragging && selectedState != null) { 
            Point pos = e.GetPosition (StatesControl);

            selectedState.X = pos.X - offset.X;
            selectedState.Y = pos.Y - offset.Y;
        
        }
    
    
    }
<<<<<<< HEAD
    private void State_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
=======
    private void State_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
>>>>>>> 4608bdaf0314368fd9aa635c4b0457c9661b8291
        isDragging = false;
        selectedState = null;
        if (sender is FrameworkElement element)
            element.ReleaseMouseCapture();
    
    }
}

