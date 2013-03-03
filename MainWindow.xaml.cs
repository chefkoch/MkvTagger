using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using Matroska;
using Path = System.IO.Path;

namespace MatroskaTagger
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
          }
        }
      }

      e.Handled = true;
    }
  }
}
