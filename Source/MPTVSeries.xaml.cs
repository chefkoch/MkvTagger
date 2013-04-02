using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Xml;
using System.Xml.Linq;
using Matroska;
using MatroskaTagger.DataSources;
using MkvTagger.Helper;

namespace MatroskaTagger
{
  public partial class MPTVSeries
  {
    public MPTVSeries()
    {
      InitializeComponent();
    }

    public void SetMediaPath(string folder)
    {
      mediaPath.Text = folder;
    }

    private void BrowseMediaPath(object sender, RoutedEventArgs e)
    {
      System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
      fbd.ShowNewFolderButton = false;

      if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        mediaPath.Text = fbd.SelectedPath;
      }
    }

    #region Progress / BackgroundWorker

    private BackgroundWorker bw = new BackgroundWorker();

    private void DoStart(object sender, RoutedEventArgs e)
    {
      if (!ReferenceEquals(bw, null) && bw.IsBusy)
        return;

      bw = new BackgroundWorker();
      bw.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
      bw.WorkerReportsProgress = true;
      bw.WorkerSupportsCancellation = true;

      // prepare args
      WorkerArgs args = new WorkerArgs();
      args.DatabasePath = App.Config.MPTVSeriesDatabasePath;
      args.Directory = mediaPath.Text;
      args.DeleteXmlAfterMkvUpdate = (bool) deleteXmlAfterUpdate.IsChecked;

      if (Equals(sender, writeXmlTagButton))
      {
        if (!WriteXmlTagsInit()) return;
        bw.DoWork += WriteXmlTags;
        bw.ProgressChanged += WriteXmlTagsProgressChanged;
        bw.RunWorkerCompleted += (o, eventArgs) =>
          {
            writeMkvButton.IsEnabled = true;
          };
      }
      else if (Equals(sender, writeMkvButton))
      {
        if (!WriteMkvTagsInit()) return;
        bw.DoWork += WriteMkvTags;
        bw.ProgressChanged += WriteMkvTagsProgressChanged;
      }

      contentDock.IsEnabled = false;
      progressText.Text = string.Empty;
      progressBar.Style = FindResource("greenProgress") as Style;
      progressDock.IsEnabled = true;
      progressDock.Visibility = Visibility.Visible;
      bw.RunWorkerAsync(args);
    }

    private class WorkerArgs
    {
      public string DatabasePath { get; set; }
      public string Directory { get; set; }

      public bool DeleteXmlAfterMkvUpdate { get; set; }
    }

    private void cancelAsyncButton_Click(object sender, RoutedEventArgs e)
    {
      if (!ReferenceEquals(bw, null) && bw.WorkerSupportsCancellation)
      {
        // Cancel the asynchronous operation.
        bw.CancelAsync();
      }
    }

    private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      progressBar.Value = 100;
      if (e.Cancelled)
      {
        progressText.Text = "Canceled!";
        progressBar.Style = FindResource("yellowProgress") as Style;
      }
      else if (e.Error != null)
      {
        progressText.Text = "Error: " + e.Error.Message;
        progressBar.Style = FindResource("redProgress") as Style;
      }
      else
      {
        progressText.Text = "Done!";
      }

      contentDock.IsEnabled = true;
      progressDock.IsEnabled = false;
    }

    #endregion

    //private void OpenConnection(string filepath)
    //{
    //  connection = new SQLiteConnection();
    //  connection.ConnectionString = @"Data Source=" + filepath + ";Pooling=true;FailIfMissing=false;Version=3";
    //  connection.Open();
    //}

    //private void CloseConnection()
    //{
    //  connection.Close();
    //  connection.Dispose();
    //}

    private static string GetXmlFilename(string mediaFile)
    {
      string directory = Path.GetDirectoryName(mediaFile);
      string file = Path.GetFileNameWithoutExtension(mediaFile) + ".xml";

      return Path.Combine(directory, file);
    }

    //private bool ReadMkvTagsInit()
    //{
    //  readListBox.Items.Clear();

    //  if (!Directory.Exists(mediaPath.Text))
    //  {
    //    System.Windows.Forms.MessageBox.Show("Select existing media folder");
    //    return false;
    //  }

    //  return true;
    //}

    //private void ReadMkvTagsProgressChanged(object sender, ProgressChangedEventArgs e)
    //{
    //  progressBar.Value = e.ProgressPercentage;

    //  if (!ReferenceEquals(e.UserState, null))
    //    readListBox.Items.Add(e.UserState);
    //}

    //private void ReadMkvTags(object sender, DoWorkEventArgs e)
    //{
    //  BackgroundWorker worker = sender as BackgroundWorker;
    //  WorkerArgs args = e.Argument as WorkerArgs;
    //  DirectoryInfo di = new DirectoryInfo(args.Directory);
    //  List<FileInfo> mkvFiles = new List<FileInfo>();
    //  mkvFiles.AddRange(di.GetFiles("*.mkv", SearchOption.AllDirectories));
    //  mkvFiles.AddRange(di.GetFiles("*.mk3d", SearchOption.AllDirectories));

    //  int current = 0;
    //  int total = mkvFiles.Count;
    //  foreach (FileInfo mkvFile in mkvFiles)
    //  {
    //    if (worker.CancellationPending)
    //    {
    //      e.Cancel = true;
    //      break;
    //    }

    //    worker.ReportProgress(100*current/total);
    //    string xmlFile = GetXmlFilename(mkvFile.FullName);

    //    try
    //    {
    //      MatroskaTags tags;
    //      if (MatroskaLoader.TryExtractTagFromMatroska(mkvFile.FullName, out tags))
    //      {
    //        worker.ReportProgress(100*current/total,
    //                              new FileBasedLogEntry(mkvFile.FullName, "Extracted tags <" + tags.Series.SeriesName + "> from "));
    //      }
    //    }
    //    catch (Exception ex)
    //    {
    //      worker.ReportProgress(100*current/total, new FileBasedLogEntry(mkvFile.FullName, ex.Message));
    //    }

    //    current++;
    //  }

    //  //writeXmlTagButton.IsEnabled = true;
    //}

    private bool WriteXmlTagsInit()
    {
      writeXmlListBox.Items.Clear();

      if (!Directory.Exists(mediaPath.Text))
      {
        System.Windows.Forms.MessageBox.Show("Select existing media folder");
        return false;
      }

      return true;
    }

    private void WriteXmlTagsProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      progressBar.Value = e.ProgressPercentage;

      if (!ReferenceEquals(e.UserState, null))
        writeXmlListBox.Items.Add(e.UserState);
    }

    private void WriteXmlTags(object sender, DoWorkEventArgs e)
    {
      BackgroundWorker worker = sender as BackgroundWorker;
      WorkerArgs args = e.Argument as WorkerArgs;
      
      var filteredfileNames = Directory.GetFiles(args.Directory, "*.*", SearchOption.AllDirectories);
      MPTVSeriesImporter importer = new MPTVSeriesImporter();
      importer.OpenConnection();

      int current = 0;
      int total = filteredfileNames.Count();
      foreach (var file in filteredfileNames)
      {
        if (worker.CancellationPending)
        {
          e.Cancel = true;
          break;
        }
        
        current++;
        worker.ReportProgress(100*current/total);

        // Check only video files
        if (!SupportedFiles.IsFileSupportedVideo(file))
          continue;

        // build xml file name
        string xmlFile = GetXmlFilename(file);

        // init document
        MatroskaTags tag = new MatroskaTags();

        // Read MKV tags, if existing should be reused
        if (App.Config.BasedOnExistingTags)
          tag = MatroskaLoader.ReadTag(file);

        // update tags from MP-TVSeries
        tag.Series = importer.UpdateTags(tag.Series, file);

        string logText = File.Exists(xmlFile) ? "XML updated: " : "XML created: ";
        MatroskaLoader.WriteTagToXML(tag, xmlFile);
        worker.ReportProgress(100 * current / total, new FileBasedLogEntry(xmlFile, logText));
      }

      importer.CloseConnection();
    }

    private bool WriteMkvTagsInit()
    {
      writeMkvListBox.Items.Clear();

      if (!Directory.Exists(mediaPath.Text))
      {
        System.Windows.Forms.MessageBox.Show("Select existing media folder");
        return false;
      }

      return true;
    }

    private void WriteMkvTagsProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      progressBar.Value = e.ProgressPercentage;

      if (!ReferenceEquals(e.UserState, null))
        writeMkvListBox.Items.Add(e.UserState);
    }

    private void WriteMkvTags(object sender, DoWorkEventArgs e)
    {
      BackgroundWorker worker = sender as BackgroundWorker;
      WorkerArgs args = e.Argument as WorkerArgs;
      DirectoryInfo di = new DirectoryInfo(args.Directory);
      List<FileInfo> mkvFiles = new List<FileInfo>();
      mkvFiles.AddRange(di.GetFiles("*.mkv", SearchOption.AllDirectories));
      mkvFiles.AddRange(di.GetFiles("*.mk3d", SearchOption.AllDirectories));

      MPTVSeriesImporter importer = new MPTVSeriesImporter();
      importer.OpenConnection();

      int current = 0;
      int total = mkvFiles.Count;
      foreach (FileInfo mkvFile in mkvFiles)
      {
        string file = mkvFile.FullName;
        if (worker.CancellationPending)
        {
          e.Cancel = true;
          break;
        }

        current++;
        worker.ReportProgress(100 * current / total);

        // init document
        MatroskaTags tag = new MatroskaTags();

        // Read MKV tags, if existing should be reused
        if (App.Config.BasedOnExistingTags)
          tag = MatroskaLoader.ReadTag(file);

        // update tags from MP-TVSeries
        tag.Series = importer.UpdateTags(tag.Series, file);

        try
        {
          int exitCode = MatroskaLoader.WriteTagToMatroska(mkvFile.FullName, tag);

          if (exitCode == 0)
          {
            worker.ReportProgress(100*current/total, new FileBasedLogEntry(mkvFile.FullName, "MKV updated: "));
            if (args.DeleteXmlAfterMkvUpdate)
            {
              // build xml file name
              string xmlFile = GetXmlFilename(file);
              File.Delete(xmlFile);
            }
          }
          else
            worker.ReportProgress(100*current/total,
                                  new FileBasedLogEntry(mkvFile.FullName,
                                                        string.Format(
                                                          "MKV updated with MKVPropEdit exit code = {0} file :",
                                                          exitCode)));
        }
        catch (Exception ex)
        {
          worker.ReportProgress(100*current/total, new FileBasedLogEntry(mkvFile.FullName, ex.Message));
        }
      }

      importer.CloseConnection();
    }

    private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      ListBox lb = sender as ListBox;
      if (ReferenceEquals(lb, null)) return;

      if (lb.SelectedItems.Count != 1) return;

      FileBasedLogEntry item = lb.SelectedItems[0] as FileBasedLogEntry;
      if (ReferenceEquals(item, null)) return;

      item.OpenExplorerFileSelected();
    }

    private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ListBox lb = sender as ListBox;
      if (ReferenceEquals(lb, null)) return;

      if (lb.SelectedItems.Count != 1) return;

      FileBasedLogEntry item = lb.SelectedItems[0] as FileBasedLogEntry;
      if (ReferenceEquals(item, null)) return;

      string extension = Path.GetExtension(item.Filepath);
      if (extension == null) return;

      switch (extension.ToLower())
      {
        case ".xml":
        case ".mkv":
          //todo: make it async
          MatroskaTags tags = MatroskaLoader.ReadTag(item.Filepath);
          textEditor.Text = MatroskaLoader.GetXML(tags);
          break;
      }
    }
  }
}
