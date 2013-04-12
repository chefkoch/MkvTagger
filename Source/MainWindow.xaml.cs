using System.IO;
using System.Windows;

namespace MkvTagger
{
  /// <summary>
  /// Interaktionslogik für MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private void FileDropHandler(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop))
      {
        //Do work. Can special-case logic based on Copy, Move, etc. 
        string[] fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
        if (fileNames != null)
        {
          // only process one item
          string fileName = fileNames[0];

          if (Directory.Exists(fileName))
          {
            // handle folder
            mptvseries.SetMediaPath(fileName);
          }
          else
          {
            // handle file
            customSeriesTag.SetFile(fileName);
            customMovieTag.SetFile(fileName);
            customMusicVideoTag.SetFile(fileName);
          }
        }
      }

      e.Handled = true;
    }
  }
}