using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Serialization;
using MatroskaTagger.DataSources;
using System.ComponentModel;
using MatroskaTagger.WpfExtensions;
using MediaPortal.OnlineLibraries.TheTvDb.Data;

namespace MatroskaTagger
{
  [XmlRoot]
  public class Configuration
  {
    private List<TvdbLanguage> availableLanguages;

      #region Properties

    [XmlElement]
    public string MPTVSeriesDatabasePath { get; set; }

    [XmlElement("tvdb_language")]
    public string SelectedTvDbLanguageValue { get; set; }

    [XmlIgnore]
    public TvdbLanguage SelectedTvDbLanguage
    {
      get
      {
        if (string.IsNullOrEmpty(SelectedTvDbLanguageValue))
          return TvdbLanguage.DefaultLanguage;

        try
        {
          if (availableLanguages == null)
            availableLanguages = new TheTvDbImporter().GetAvailableLanguages();

          return availableLanguages.FirstOrDefault(l => l.Abbriviation == SelectedTvDbLanguageValue);
        }
        catch (Exception)
        {
          return TvdbLanguage.DefaultLanguage;
        }
      }
      set
      {
        if (value == null)
          SelectedTvDbLanguageValue = null;
        else
          SelectedTvDbLanguageValue = value.Abbriviation;
      }
    }

    [XmlElement]
    public bool BasedOnExistingTags { get; set; }

    [XmlIgnore]
    public Dictionary<string, TagSetting> OptionalSeriesTags { get; set; }

    #endregion Properties

    public Configuration()
    {
      MPTVSeriesDatabasePath = MPTVSeriesImporter.GetDefaultDatabasePath();
      SelectedTvDbLanguage = TvdbLanguage.DefaultLanguage;

      OptionalSeriesTags = new Dictionary<string, TagSetting>();
      
      foreach (TagSetting tag in Consts.SeriesTags)
        OptionalSeriesTags.Add(tag.ID,tag);
    }

    #region Static Methods

    /// <summary>
    /// Save the supplied configuration to file.
    /// </summary>
    /// <param name="config">Configuration to save.</param>
    /// <param name="fileName">File to save to.</param>
    /// <returns><c>true</c> if successful, otherwise <c>false</c>.</returns>
    public static bool Save(Configuration config, string fileName)
    {
      try
      {
        XmlSerializer writer = new XmlSerializer(typeof (Configuration));
        using (StreamWriter file = new StreamWriter(fileName))
          writer.Serialize(file, config);

        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    /// <summary>
    /// Load a configuration file.
    /// </summary>
    /// <param name="fileName">File to load from.</param>
    /// <returns>Loaded Configuration.</returns>
    public static Configuration Load(string fileName)
    {
      try
      {
        XmlSerializer reader = new XmlSerializer(typeof (Configuration));
        using (StreamReader file = new StreamReader(fileName))
          return (Configuration) reader.Deserialize(file);
      }
      catch (FileNotFoundException)
      {
        //IrssLog.Warn("No configuration file found ({0}), creating new configuration", fileName);
      }
      catch (Exception ex)
      {
        //IrssLog.Error(ex);
      }

      return null;
    }

    public static TagSetting GetTagSetting(string id)
    {
      if (App.Config.OptionalSeriesTags.ContainsKey(id))
        return App.Config.OptionalSeriesTags[id];

      TagSetting setting = new TagSetting(id);
      App.Config.OptionalSeriesTags.Add(id, setting);

      return setting;
    }

    #endregion Static Methods
  }
}