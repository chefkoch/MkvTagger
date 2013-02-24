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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Path = System.IO.Path;

namespace MPTvServies2MKV
{
  /// <summary>
  /// Interaktionslogik für MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private const string TABLE_Series = "online_series";
    private const string TABLE_Episodes = "local_episodes";

    public MainWindow()
    {
      InitializeComponent();
    }

    private void BrowseDbPath(object sender, RoutedEventArgs e)
    {
      OpenFileDialog ofd = new OpenFileDialog();
      ofd.Multiselect = false;
      ofd.Filter = "TVSeriesDatabase4.db3|TVSeriesDatabase4.db3";

      if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        dbPath.Text = ofd.FileName;
      }
    }

    private void BrowseMediaPath(object sender, RoutedEventArgs e)
    {
      FolderBrowserDialog fbd = new FolderBrowserDialog();
      fbd.ShowNewFolderButton = false;

      if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        mediaPath.Text = fbd.SelectedPath;
      }
    }

    SQLiteConnection connection;
    Dictionary<int, Series> _series;
    Dictionary<string, Episode> _episodes;
    List<FileInfo> mkvFiles;

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

    private void ReadDatabase(object sender, RoutedEventArgs e)
    {
      listBox.Items.Clear();

      if (!File.Exists(dbPath.Text))
      {
        System.Windows.Forms.MessageBox.Show("Select existing database");
        return;
      }

      OpenConnection(dbPath.Text);
      #region Episodes

      _episodes = new Dictionary<string, Episode>();
      using (var command = new SQLiteCommand(connection))
      {
        command.CommandText = "SELECT * FROM " + TABLE_Episodes;

        using (SQLiteDataReader reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            Episode ep = new Episode
            {
              Filename = reader[0].ToString(),

              OriginalComposite = reader[1].ToString(),
              CompositeID = reader[2].ToString(),

              SeriesID = int.Parse(reader[3].ToString()),
              SeasonIndex = int.Parse(reader[4].ToString()),
              EpisodeIndex = int.Parse(reader[5].ToString())
            };
            _episodes.Add(ep.Filename, ep);
          }
          reader.Close();
        }
      }

      #endregion
      #region ReadSeries

      _series = new Dictionary<int, Series>();
      using (var command = new SQLiteCommand(connection))
      {
        command.CommandText = "SELECT * FROM " + TABLE_Series;

        using (SQLiteDataReader reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            Series s = new Series
            {
              ID = int.Parse(reader[0].ToString()),
              Name = reader[1].ToString()
            };
            _series.Add(s.ID, s);
          }
          reader.Close();
        }
      }

      #endregion
      CloseConnection();

      foreach (Episode episode in _episodes.Values)
        listBox.Items.Add(episode);
      foreach (Series series in _series.Values)
        listBox.Items.Add(series);

      readMkvButton.IsEnabled = true;
    }

    private void ReadMkvTags(object sender, RoutedEventArgs e)
    {
      listBox.Items.Add("--------------------------------------------------------------------");
      listBox.Items.Add("-----------                Read MKV Tags                ------------");

      if (!Directory.Exists(mediaPath.Text))
      {
        System.Windows.Forms.MessageBox.Show("Select existing media folder");
        return;
      }

      DirectoryInfo di = new DirectoryInfo(mediaPath.Text);
      mkvFiles = new List<FileInfo>();
      mkvFiles.AddRange(di.GetFiles("*.mkv", SearchOption.AllDirectories));
      mkvFiles.AddRange(di.GetFiles("*.mk3d", SearchOption.AllDirectories));

      foreach (FileInfo mkvFile in mkvFiles)
      {
        string xmlFile = GetXmlFilename(mkvFile.FullName);

        try
        {
          Process proc = Process.Start("mkvextract.exe", String.Format(" tags \"{0}\" --redirect-output \"{1}\"", mkvFile.FullName, xmlFile));
          proc.WaitForExit();

          string content = File.ReadAllText(xmlFile);
          if (string.IsNullOrEmpty(content))
            File.Delete(xmlFile);
          else
            listBox.Items.Add("Extracted mkv-Tags from " + mkvFile.FullName);
        }
        catch (Exception ex)
        {
          listBox.Items.Add(ex.Message);
        }
      }

      writeXmlTagButton.IsEnabled = true;
    }

    private void WriteXmlTags(object sender, RoutedEventArgs e)
    {
      listBox.Items.Add("--------------------------------------------------------------------");
      listBox.Items.Add("-----------                Write XML Tags               ------------");

      if (!Directory.Exists(mediaPath.Text))
      {
        System.Windows.Forms.MessageBox.Show("Select existing media folder");
        return;
      }

      var filteredfileNames = _episodes.Keys.Where(p => p.StartsWith(mediaPath.Text));
      foreach (var file in filteredfileNames)
      {
        // build xml file name
        string xmlFile = GetXmlFilename(file);

        // reset variable
        bool seriesDone = false;
        bool seasonDone = false;
        bool episodeDone = false;

        string seriesName = _series[_episodes[file].SeriesID].Name;
        int seasonIndex = _episodes[file].SeasonIndex;
        int episodeIndex = _episodes[file].EpisodeIndex;

        // init document
        XmlDocument doc = new XmlDocument();
        XmlNode root;
        if (File.Exists(xmlFile))
        {
          doc.Load(xmlFile);
        }
        else
        {
          XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
          root = doc.CreateElement("Tags");
          doc.AppendChild(xmlDeclaration);
          doc.AppendChild(root);
        }

        XmlNodeList tagNodes = doc.SelectNodes("/Tags/Tag");
        if (tagNodes != null)
          foreach (XmlNode tagNode in tagNodes)
          {
            XmlNode targetsNode = tagNode.SelectSingleNode("Targets");
            if (targetsNode == null) continue;
            XmlNode targetTypeNode = targetsNode.SelectSingleNode("TargetTypeValue");
            if (targetTypeNode == null) continue;

            switch (targetTypeNode.InnerText)
            {
                // Series
              case "70":
                UpdateNode(tagNode, doc, "TITLE", seriesName);
                seriesDone = true;
                break;

                // Season
              case "60":
                UpdateNode(tagNode, doc, "PART_NUMBER", seasonIndex.ToString());
                seasonDone = true;
                break;

                // Episode
              case "50":
                UpdateNode(tagNode, doc, "PART_NUMBER", episodeIndex.ToString());
                episodeDone = true;
                break;
            }
          }

        if (!seriesDone)
          AddNode(doc, "70", "TITLE", seriesName);

        if (!seasonDone)
          AddNode(doc, "60", "PART_NUMBER", seasonIndex.ToString());

        if (!episodeDone)
          AddNode(doc, "50", "PART_NUMBER", episodeIndex.ToString());

        doc.Save(xmlFile);
      }

      writeMkvButton.IsEnabled = true;
    }

    private static void AddNode(XmlDocument doc, string target, string name, string value)
    {
      XmlNode targetNode = doc.CreateElement("TargetTypeValue");
      targetNode.InnerText = target;

      XmlNode targetsNode = doc.CreateElement("Targets");
      targetsNode.AppendChild(targetNode);


      XmlNode simpleName = doc.CreateElement("Name");
      simpleName.InnerText = name;

      XmlNode simpleString = doc.CreateElement("String");
      simpleString.InnerText = value;

      XmlNode simpleNode = doc.CreateElement("Simple");
      simpleNode.AppendChild(simpleName);
      simpleNode.AppendChild(simpleString);


      XmlNode tagNode = doc.CreateElement("Tag");
      tagNode.AppendChild(targetsNode);
      tagNode.AppendChild(simpleNode);

      doc.SelectSingleNode("Tags").AppendChild(tagNode);
    }

    private static void UpdateNode(XmlNode tagNode, XmlDocument doc, string name, string value)
    {
      XmlNodeList simpleNodes = tagNode.SelectNodes("Simple");
      foreach (XmlNode simpleNode in simpleNodes)
      {
        XmlNode simpleName = simpleNode.SelectSingleNode("Name");
        XmlNode simpleString = simpleNode.SelectSingleNode("String");

        if (simpleName.InnerText == name)
        {
          simpleString.InnerText = value;
          return;
        }
      }

      // simple node with name not found >> add it
      if (true)
      {
        XmlNode simpleName = doc.CreateElement("Name");
        simpleName.InnerText = name;
        XmlNode simpleString = doc.CreateElement("String");
        simpleString.InnerText = value;

        XmlNode simpleNode = doc.CreateElement("Simple");
        simpleNode.AppendChild(simpleName);
        simpleNode.AppendChild(simpleString);

        tagNode.AppendChild(simpleNode);
      }
    }

    private void WriteMkvTags(object sender, RoutedEventArgs e)
    {
      listBox.Items.Add("--------------------------------------------------------------------");
      listBox.Items.Add("-----------                Write MKV Tags               ------------");

      if (!Directory.Exists(mediaPath.Text))
      {
        System.Windows.Forms.MessageBox.Show("Select existing media folder");
        return;
      }

      DirectoryInfo di = new DirectoryInfo(mediaPath.Text);
      mkvFiles = new List<FileInfo>();
      mkvFiles.AddRange(di.GetFiles("*.mkv", SearchOption.AllDirectories));
      mkvFiles.AddRange(di.GetFiles("*.mk3d", SearchOption.AllDirectories));

      foreach (FileInfo mkvFile in mkvFiles)
      {
        if (!_episodes.ContainsKey(mkvFile.FullName)) continue;
        
        string xmlFile = GetXmlFilename(mkvFile.FullName);
        if (!File.Exists(xmlFile)) continue;

        try
        {
          Process proc = Process.Start("mkvpropedit.exe", String.Format(" \"{0}\" --tags global:\"{1}\"", mkvFile.FullName, xmlFile));
          proc.WaitForExit();
        }
        catch (Exception ex)
        {
          listBox.Items.Add(ex.Message);
        }
      }
    }
  }

  public class Episode
  {
    public string Filename { get; set; }

    public string OriginalComposite { get; set; }
    public string CompositeID { get; set; }

    public int SeriesID { get; set; }
    public int SeasonIndex { get; set; }
    public int EpisodeIndex { get; set; }

    public override string ToString()
    {
      return String.Format("Episode: {0} ({2}_S{3}_E{4}) file: {1}", OriginalComposite, Filename, SeriesID, SeasonIndex, EpisodeIndex);
    }
  }

  public class Series
  {
    public int ID { get; set; }
    public string Name { get; set; }

    public override string ToString()
    {
      return String.Format("Series: {0} ID: {1}", Name, ID);
    }
  }
}
