using System;
using System.Windows;
using TP.ConcurrentProgramming.Presentation.ViewModel;

namespace TP.ConcurrentProgramming.PresentationView
{
  /// <summary>
  /// View implementation
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      //Random random = new Random();
      InitializeComponent();
      //MainWindowViewModel viewModel = (MainWindowViewModel)DataContext;
      //double screenWidth = SystemParameters.PrimaryScreenWidth;
      //double screenHeight = SystemParameters.PrimaryScreenHeight;
      //viewModel.Start(random.Next(5, 10));
    }

    /// <summary>
    /// Raises the <seealso cref="System.Windows.Window.Closed"/> event.
    /// </summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnClosed(EventArgs e)
    {
      if (DataContext is MainWindowViewModel viewModel)
        viewModel.Dispose();
      base.OnClosed(e);
    }
  }
}