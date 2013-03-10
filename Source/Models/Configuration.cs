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

namespace MatroskaTagger
{
  [XmlRoot]
  public class Configuration
  {
    #region Properties

    [XmlElement]
    public string MPTVSeriesDatabasePath { get; set; }

    [XmlElement]
    public bool BasedOnExistingTags { get; set; }

    [XmlArray]
    public Dictionary<string, TagSetting> OptionalSeriesTags { get; set; }

    #endregion Properties

    public Configuration()
    {
      MPTVSeriesDatabasePath = MPTVSeriesImporter.GetDefaultDatabasePath();

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