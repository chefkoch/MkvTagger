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

namespace MatroskaTagger
{
  public partial class MPTVSeries
  {
    private const string TVSERIES_DB = "TVSeriesDatabase4.db3";
    private const string KEY_SERIES_SORTNAME = "SeriesSortName";

    public MPTVSeries()
    {
      InitializeComponent();

      string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
      path = Path.Combine(path, "Team MediaPortal", "MediaPortal", "database", "TVSeriesDatabase4.db3");
      dbPath.Text = path;
    }

    public void SetMediaPath(string folder)
    {
      mediaPath.Text = folder;
    }

    private void BrowseDbPath(object sender, RoutedEventArgs e)
    {
      System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
      ofd.Multiselect = false;
      ofd.Filter = TVSERIES_DB + "|" + TVSERIES_DB;

      if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        dbPath.Text = ofd.FileName;
      }
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
      args.DatabasePath = dbPath.Text;
      args.Directory = mediaPath.Text;
      args.Episodes = _episodes;
      args.Series = _series;
      args.DeleteXmlAfterMkvUpdate = (bool) deleteXmlAfterUpdate.IsChecked;
      if ((bool) seriesSortName.IsChecked)
        args.OptionalTagsToWrite.Add(KEY_SERIES_SORTNAME);

      // init and setup worker methods
      if (Equals(sender, dbButton))
      {
        if (!ReadDatabaseInit()) return;
        bw.DoWork += ReadDatabase;
        bw.ProgressChanged += ReadDatabaseProgressChanged;
      }
      else if (Equals(sender, readButton))
      {
        if (!ReadMkvTagsInit()) return;
        bw.DoWork += ReadMkvTags;
        bw.ProgressChanged += ReadMkvTagsProgressChanged;
        bw.RunWorkerCompleted += (o, eventArgs) =>
          {
            writeXmlTagButton.IsEnabled = true;
          };
      }
      else if (Equals(sender, writeXmlTagButton))
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
      public Dictionary<int, Series> Series { get; set; }
      public Dictionary<string, Episode> Episodes { get; set; }
      public List<string> OptionalTagsToWrite { get; private set; }

      public bool DeleteXmlAfterMkvUpdate { get; set; }

      public WorkerArgs()
      {
        OptionalTagsToWrite = new List<string>();
      }
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

    private SQLiteConnection connection;
    private Dictionary<int, Series> _series;
    private Dictionary<string, Episode> _episodes;

    private void OpenConnection(string filepath)
    {
      connection = new SQLiteConnection();
      connection.ConnectionString = @"Data Source=" + filepath + ";Pooling=true;FailIfMissing=false;Version=3";
      connection.Open();
    }

    private void CloseConnection()
    {
      connection.Close();
      connection.Dispose();
    }

    private static string GetXmlFilename(string mediaFile)
    {
      string directory = Path.GetDirectoryName(mediaFile);
      string file = Path.GetFileNameWithoutExtension(mediaFile) + ".xml";

      return Path.Combine(directory, file);
    }

    private bool ReadDatabaseInit()
    {
      dbListBox.Items.Clear();

      if (!File.Exists(dbPath.Text))
      {
        System.Windows.Forms.MessageBox.Show("Select existing database");
        return false;
      }

      return true;
    }

    private void ReadDatabaseProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      progressBar.Value = e.ProgressPercentage;

      if (!ReferenceEquals(e.UserState, null))
        dbListBox.Items.Add(e.UserState);
    }

    private void ReadDatabase(object sender, DoWorkEventArgs e)
    {
      BackgroundWorker worker = sender as BackgroundWorker;
      WorkerArgs args = e.Argument as WorkerArgs;
      OpenConnection(args.DatabasePath);

      #region Episodes

      _episodes = new Dictionary<string, Episode>();
      using (var command = new SQLiteCommand(connection))
      {
        command.CommandText = "SELECT * FROM local_episodes";

        using (SQLiteDataReader reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            if (worker.CancellationPending)
            {
              e.Cancel = true;
              break;
            }

            Episode ep = new Episode
              {
                Filename = reader["EpisodeFilename"].ToString(),
                SeriesID = int.Parse(reader["SeriesID"].ToString()),
                SeasonIndex = int.Parse(reader["SeasonIndex"].ToString())
              };

            int index = int.Parse(reader["EpisodeIndex"].ToString());
            if (index != 0)
              ep.EpisodeIndexList.Add(index);

            index = int.Parse(reader["EpisodeIndex2"].ToString());
            if (index != 0)
              ep.EpisodeIndexList.Add(index);

            _episodes.Add(ep.Filename, ep);
            worker.ReportProgress(100, new FileBasedLogEntry(ep.Filename, ep.ToStringNoFilename()));
          }
          reader.Close();
        }
      }

      #endregion

      #region ReadSeries

      _series = new Dictionary<int, Series>();
      using (var command = new SQLiteCommand(connection))
      {
        command.CommandText = "SELECT * FROM online_series";

        using (SQLiteDataReader reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            if (worker.CancellationPending)
            {
              e.Cancel = true;
              break;
            }

            Series s = new Series
              {
                ID = int.Parse(reader["ID"].ToString()),
                Name = reader["Pretty_Name"].ToString(),
                SortName = reader["SortName"].ToString()
              };

            _series.Add(s.ID, s);
            worker.ReportProgress(100, s);
          }
          reader.Close();
        }
      }

      #endregion

      CloseConnection();
    }

    private bool ReadMkvTagsInit()
    {
      readListBox.Items.Clear();

      if (!Directory.Exists(mediaPath.Text))
      {
        System.Windows.Forms.MessageBox.Show("Select existing media folder");
        return false;
      }

      return true;
    }

    private void ReadMkvTagsProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      progressBar.Value = e.ProgressPercentage;

      if (!ReferenceEquals(e.UserState, null))
        readListBox.Items.Add(e.UserState);
    }

    private void ReadMkvTags(object sender, DoWorkEventArgs e)
    {
      BackgroundWorker worker = sender as BackgroundWorker;
      WorkerArgs args = e.Argument as WorkerArgs;
      DirectoryInfo di = new DirectoryInfo(args.Directory);
      List<FileInfo> mkvFiles = new List<FileInfo>();
      mkvFiles.AddRange(di.GetFiles("*.mkv", SearchOption.AllDirectories));
      mkvFiles.AddRange(di.GetFiles("*.mk3d", SearchOption.AllDirectories));

      int current = 0;
      int total = mkvFiles.Count;
      foreach (FileInfo mkvFile in mkvFiles)
      {
        if (worker.CancellationPending)
        {
          e.Cancel = true;
          break;
        }

        worker.ReportProgress(100*current/total);
        string xmlFile = GetXmlFilename(mkvFile.FullName);

        try
        {
          MatroskaTags tags;
          if (MatroskaLoader.TryExtractTagFromMatroska(mkvFile.FullName, out tags))
          {
            worker.ReportProgress(100*current/total,
                                  new FileBasedLogEntry(mkvFile.FullName, "Extracted tags <" + tags.Series.SeriesName + "> from "));
          }
        }
        catch (Exception ex)
        {
          worker.ReportProgress(100*current/total, new FileBasedLogEntry(mkvFile.FullName, ex.Message));
        }

        current++;
      }

      //writeXmlTagButton.IsEnabled = true;
    }

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
      var filteredfileNames = args.Episodes.Keys.Where(p => p.StartsWith(args.Directory));

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

        if (!File.Exists(file)) continue;

        // build xml file name
        string xmlFile = GetXmlFilename(file);

        // init document
        MatroskaTags tag = new MatroskaTags();
        if (File.Exists(xmlFile))
          tag = MatroskaLoader.ReadTagFromXMLFile(xmlFile);

        tag.Series.SeriesName.StringValue = args.Series[_episodes[file].SeriesID].Name;
        tag.Series.SeriesName.SortWith = args.Series[_episodes[file].SeriesID].SortName;
        tag.Series.SeasonIndex = args.Episodes[file].SeasonIndex;

        if (args.OptionalTagsToWrite.Contains(KEY_SERIES_SORTNAME))
          tag.Series.EpisodeIndexList = args.Episodes[file].EpisodeIndexList.AsReadOnly();

        if (File.Exists(xmlFile))
        {
          MatroskaLoader.WriteTagToXML(tag, xmlFile);
          worker.ReportProgress(100*current/total, new FileBasedLogEntry(xmlFile, "XML updated: "));
        }
        else
        {
          MatroskaLoader.WriteTagToXML(tag, xmlFile);
          worker.ReportProgress(100*current/total, new FileBasedLogEntry(xmlFile, "XML created: "));
        }
      }
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

      int current = 0;
      int total = mkvFiles.Count;
      foreach (FileInfo mkvFile in mkvFiles)
      {
        if (worker.CancellationPending)
        {
          e.Cancel = true;
          break;
        }

        current++;
        worker.ReportProgress(100*current/total);

        if (!args.Episodes.ContainsKey(mkvFile.FullName)) continue;

        string xmlFile = GetXmlFilename(mkvFile.FullName);
        if (!File.Exists(xmlFile)) continue;

        try
        {
          int exitCode = MatroskaLoader.WriteTagToMatroska(mkvFile.FullName, xmlFile);

          if (exitCode == 0)
          {
            worker.ReportProgress(100*current/total, new FileBasedLogEntry(mkvFile.FullName, "MKV updated: "));
            if (args.DeleteXmlAfterMkvUpdate)
              File.Delete(xmlFile);
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

  public class Episode
  {
    public string Filename { get; set; }

    public int SeriesID { get; set; }
    public int SeasonIndex { get; set; }
    public List<int> EpisodeIndexList { get; private set; }

    public Episode()
    {
      EpisodeIndexList = new List<int>();
    }

    public override string ToString()
    {
      return String.Format("Episode: ({1}_S{2}_E{3}) file: {0}", Filename, SeriesID, SeasonIndex, String.Join("_E", EpisodeIndexList));
    }

    public string ToStringNoFilename()
    {
      return String.Format("Episode: ({1}_S{2}_E{3}) file: ", Filename, SeriesID, SeasonIndex, String.Join("_E", EpisodeIndexList));
    }
  }

  public class Series
  {
    public int ID { get; set; }
    public string Name { get; set; }
    public string SortName { get; set; }

    public override string ToString()
    {
      return String.Format("Series: {0} ID: {1}", Name, ID);
    }
  }
}
