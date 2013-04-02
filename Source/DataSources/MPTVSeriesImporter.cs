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

namespace MatroskaTagger.DataSources
{
  public class MPTVSeriesImporter
  {
    public const string DatabaseFilename = "TVSeriesDatabase4.db3";
    private const string KEY_SERIES_SORTNAME = "SeriesSortName";

    private SQLiteConnection connection;

    public MPTVSeriesImporter()
    {
    }

    public static string GetDefaultDatabasePath()
    {
      string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
      return Path.Combine(path, "Team MediaPortal", "MediaPortal", "database", DatabaseFilename);
    }

    //public Dictionary<string,Episode> GetAllEpisodes()
    //{
    //  string db = DatabasePath;
    //  OpenConnection(db);

    //  Dictionary<string,Episode> episodes = new Dictionary<string, Episode>();
    //  using (var command = new SQLiteCommand(connection))
    //  {
    //    command.CommandText = "SELECT * FROM local_episodes";

    //    using (SQLiteDataReader reader = command.ExecuteReader())
    //    {
    //      while (reader.Read())
    //      {
    //        Episode ep = FillLocalEpisodeInfo(new Episode(), reader);
    //        episodes.Add(ep.Filename,ep);
    //      }
    //      reader.Close();
    //    }
    //  }

    //  foreach (Episode episode in episodes.Values)
    //  {
        
    //  }

    //  CloseConnection();
    //  return episodes;
    //}

    //public Dictionary<int, Series> GetAllSeries()
    //{
    //  string db = DatabasePath;
    //  OpenConnection(db);

    //  Dictionary<int, Series> series = new Dictionary<int, Series>();
    //  using (var command = new SQLiteCommand(connection))
    //  {
    //    command.CommandText = "SELECT * FROM online_series";

    //    using (SQLiteDataReader reader = command.ExecuteReader())
    //    {
    //      while (reader.Read())
    //      {
    //        Series s = FillSeriesInfo(new Series(), reader);
    //        series.Add(s.ID, s);
    //      }
    //      reader.Close();
    //    }
    //  }

    //  CloseConnection();
    //  return series;
    //}

    public SeriesTag UpdateTags(SeriesTag seriesTag, string filename)
    {
      Episode ep = null;
      try
      {
        MPTVSeriesImporter i = new MPTVSeriesImporter();
        ep = i.GetEpisodeInfo(filename);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return seriesTag;
      }

      if (ep == null)
        return seriesTag;

      return CopyEpisodeToTag(seriesTag, ep);
    }


    private Episode GetEpisodeInfo(string filename)
    {
      if (connection == null)
        OpenConnection();

      Episode ep = new Episode();
      using (var command = new SQLiteCommand(connection))
      {
        command.CommandText = String.Format("SELECT * FROM local_episodes WHERE EpisodeFilename='{0}'", filename);

        using (SQLiteDataReader reader = command.ExecuteReader())
        {
          while (reader.Read())
            ep = FillLocalEpisodeInfo(ep, reader);

          reader.Close();
        }
      }
      if (ep.EpisodeIndexList.Count == 0 && ep.FirstAired == null) return null;

      using (var command = new SQLiteCommand(connection))
      {
        command.CommandText = String.Format("SELECT * FROM online_episodes WHERE CompositeID='{0}'", ep.CompositeID);

        using (SQLiteDataReader reader = command.ExecuteReader())
        {
          while (reader.Read())
            ep = FillOnlineEpisodeInfo(ep, reader);

          reader.Close();
        }
      }

      Series s = new Series();
      using (var command = new SQLiteCommand(connection))
      {
        command.CommandText = String.Format("SELECT * FROM online_series WHERE ID={0}", ep.SeriesID);

        using (SQLiteDataReader reader = command.ExecuteReader())
        {
          while (reader.Read())
            s = FillSeriesInfo(s, reader);

          reader.Close();
        }
      }

      if (s == null) return null;

      ep.SeriesInfo = s;
      return ep;
    }

    private static Episode FillLocalEpisodeInfo(Episode ep, SQLiteDataReader reader)
    {
      ep.Filename = reader["EpisodeFilename"].ToString();
      ep.CompositeID = reader["CompositeID"].ToString();

      ep.SeriesID = Int32.Parse(reader["SeriesID"].ToString());
      ep.SeasonIndex = Int32.Parse(reader["SeasonIndex"].ToString());
      
      ep.EpisodeIndexList.Clear();
      int index = Int32.Parse(reader["EpisodeIndex"].ToString());
      if (index != 0)
        ep.EpisodeIndexList.Add(index.ToString());

      index = Int32.Parse(reader["EpisodeIndex2"].ToString());
      if (index != 0)
        ep.EpisodeIndexList.Add(index.ToString());

      return ep;
    }

    private static Episode FillOnlineEpisodeInfo(Episode ep, SQLiteDataReader reader)
    {
      ep.FirstAired = reader["FirstAired"].ToString();

      ep.EpisodeName = reader["EpisodeName"].ToString();
      ep.Summary = reader["Summary"].ToString();
      ep.IMDB_ID = reader["IMDB_ID"].ToString();

      ep.GuestStars = SplitText(reader["GuestStars"].ToString());
      ep.Directors = SplitText(reader["Director"].ToString());
      ep.Writers = SplitText(reader["Writer"].ToString());

      return ep;
    }

    private static Series FillSeriesInfo(Series s, SQLiteDataReader reader)
    {
      s.ID = Int32.Parse(reader["ID"].ToString());
      s.Title = reader["Pretty_Name"].ToString();
      s.IMDB_ID = reader["IMDB_ID"].ToString();

      s.TitleSort = reader["SortName"].ToString();
      s.Network = reader["Network"].ToString();
      s.Summary = reader["Summary"].ToString();
      s.FirstAired = reader["FirstAired"].ToString();

      s.Genre = SplitText(reader["Genre"].ToString());
      s.Actors = SplitText(reader["Actors"].ToString());

      return s;
    }

    private static List<string> SplitText(string input)
    {
      List<string> result = new List<string>();

      if (String.IsNullOrEmpty(input)) return result;

      string[] array = input.Split(',', '|');
      foreach (string item in array)
      {
        string s = item.Trim();
        if (String.IsNullOrEmpty(s)) continue;

        if (!result.Contains(s))
          result.Add(s);
      }

      return result;
    }

    public void OpenConnection(string filepath = null)
    {
      if (connection != null)
        CloseConnection();

      if (filepath == null)
        filepath = App.Config.MPTVSeriesDatabasePath;

      connection = new SQLiteConnection();
      connection.ConnectionString = @"Data Source=" + filepath + ";Pooling=true;FailIfMissing=false;Version=3";
      connection.Open();
    }

    public void CloseConnection()
    {
      if (connection == null)
        return;

      connection.Close();
      connection.Dispose();
      connection = null;
    }

    private static SeriesTag CopyEpisodeToTag(SeriesTag tag, Episode ep)
    {
      tag.SeriesName = ep.SeriesInfo.Title;
      tag.IMDB_ID = ep.SeriesInfo.IMDB_ID;
      tag.TVDB_ID = ep.SeriesInfo.ID.ToString();
      tag.SeasonIndex = ep.SeasonIndex.ToString();
      tag.EpisodeIndexList = ep.EpisodeIndexList.AsReadOnly();
      tag.EpisodeFirstAired = ep.FirstAired;

      // additional series tags
      tag.SeriesFirstAired = ep.SeriesInfo.FirstAired;
      tag.Network = ep.SeriesInfo.Network;
      tag.SeriesOverview = ep.SeriesInfo.Summary;

      tag.SeriesGenreList = ep.SeriesInfo.Genre.AsReadOnly();
      tag.SeriesActors = ep.SeriesInfo.Actors.AsReadOnly();

      // additional episode tags
      tag.EpisodeTitle = ep.EpisodeName;
      tag.EpisodeIMDB_ID = ep.IMDB_ID;
      tag.EpisodeOverview = ep.Summary;

      tag.GuestStars = ep.GuestStars.AsReadOnly();
      tag.Directors = ep.Directors.AsReadOnly();
      tag.Writers = ep.Writers.AsReadOnly();

      return tag;
    }

    //private static SeriesTag CopyEpisodeToTag(SeriesTag tag, Episode ep)
    //{
    //  if (Configuration.GetTagSetting(Consts.KeySeriesTitle).Write)
    //    tag.Series.SetTitle(ep.SeriesInfo.Title);
    //  if (Configuration.GetTagSetting(Consts.KeySeriesImdb).Write)
    //    tag.Series.IMDB_ID = ep.SeriesInfo.IMDB_ID;
    //  if (Configuration.GetTagSetting(Consts.KeySeriesSeasonIndex).Write)
    //    tag.Series.SeasonIndex = ep.SeasonIndex;
    //  if (Configuration.GetTagSetting(Consts.KeySeriesEpisodeIndizes).Write)
    //    tag.Series.EpisodeIndexList = ep.EpisodeIndexList.AsReadOnly();
    //  if (Configuration.GetTagSetting(Consts.KeySeriesEpisodeFirstAired).Write)
    //    tag.Series.EpisodeFirstAired = ep.FirstAired;

    //  // additional series tags
    //  if (Configuration.GetTagSetting(Consts.KeySeriesTitleSort).Write)
    //    tag.Series.SeriesName.SortWith = ep.SeriesInfo.TitleSort;
    //  if (Configuration.GetTagSetting(Consts.KeySeriesGenre).Write)
    //    tag.Series.SeriesGenreList = ep.SeriesInfo.Genre.AsReadOnly();
    //  if (Configuration.GetTagSetting(Consts.KeySeriesFirstAired).Write)
    //    tag.Series.SeriesFirstAired = ep.SeriesInfo.FirstAired;

    //  // additional episode tags
    //  if (Configuration.GetTagSetting(Consts.KeySeriesEpisodeTitle).Write)
    //    tag.Series.EpisodeTitle = ep.EpisodeName;
    //  //if (Configuration.GetTagSetting(Consts.KeySeriesEpisodeSummary).Write)
    //  //  tag.Series.EpisodeFirstAired = ep.Summary;

    //  //if (Configuration.GetTagSetting(Consts.KeySeriesEpisodeFirstAired).Write)
    //  //  tag.Series.EpisodeFirstAired = ep.GuestStars;
    //  //if (Configuration.GetTagSetting(Consts.KeySeriesEpisodeFirstAired).Write)
    //  //  tag.Series.EpisodeFirstAired = ep.Directors;
    //  //if (Configuration.GetTagSetting(Consts.KeySeriesEpisodeFirstAired).Write)
    //  //  tag.Series.EpisodeFirstAired = ep.Writers;

    //  return tag;
    //}

    private class Episode
    {
      public string Filename { get; set; }
      public string CompositeID { get; set; }

      public int SeriesID { get; set; }
      public int SeasonIndex { get; set; }
      public List<string> EpisodeIndexList { get; set; }
      public string FirstAired { get; set; }

      public string EpisodeName { get; set; }
      public string IMDB_ID { get; set; }
      public string Summary { get; set; }

      public List<string> GuestStars { get; set; }
      public List<string> Directors { get; set; }
      public List<string> Writers { get; set; }

      public Series SeriesInfo { get; set; }

      public Episode()
      {
        EpisodeIndexList = new List<string>();

        GuestStars = new List<string>();
        Directors = new List<string>();
        Writers = new List<string>();
      }

      public override string ToString()
      {
        return String.Format("Episode: ({1}_S{2}_E{3}) file: {0}", Filename, SeriesID, SeasonIndex,
                             String.Join("_E", EpisodeIndexList));
      }

      public string ToStringNoFilename()
      {
        return String.Format("Episode: ({1}_S{2}_E{3}) file: ", Filename, SeriesID, SeasonIndex,
                             String.Join("_E", EpisodeIndexList));
      }
    }

    private class Series
    {
      public int ID { get; set; }
      public string Title { get; set; }
      public string IMDB_ID { get; set; }

      public string TitleSort { get; set; }
      public string Network { get; set; }
      public string Summary { get; set; }
      public string FirstAired { get; set; }

      public List<string> Genre { get; set; }
      public List<string> Actors { get; set; }

      public Series()
      {
        Genre = new List<string>();
        Actors = new List<string>();
      }

      public override string ToString()
      {
        return String.Format("Series: {0} ID: {1}", Title, ID);
      }
    }
  }
}
