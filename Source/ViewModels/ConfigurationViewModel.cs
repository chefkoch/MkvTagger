using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using MatroskaTagger.DataSources;
using System.ComponentModel;
using MatroskaTagger.WpfExtensions;
using MediaPortal.OnlineLibraries.TheTvDb;
using MediaPortal.OnlineLibraries.TheTvDb.Data;

namespace MatroskaTagger
{
  public class ConfigurationViewModel : INotifyPropertyChanged
  {
    private Visibility _settingsVisible;
    private string _mptvSeriesDatabasePath;
    private TvdbLanguage _selectedTvDbLanguage;
    private List<TvdbLanguage> _availableTvDbLanguages;
    private bool _basedOnExistingTags;

    #region Properties

    public Visibility SettingsVisible
    {
      get { return _settingsVisible; }
      set { PropertyChanged.ChangeAndNotify(ref _settingsVisible, value, () => SettingsVisible); }
    }

    public string MPTVSeriesDatabasePath
    {
      get { return _mptvSeriesDatabasePath; }
      set { PropertyChanged.ChangeAndNotify(ref _mptvSeriesDatabasePath, value, () => MPTVSeriesDatabasePath); }
    }

    public TvdbLanguage SelectedTvDbLanguage
    {
      get { return _selectedTvDbLanguage; }
      set { PropertyChanged.ChangeAndNotify(ref _selectedTvDbLanguage, value, () => SelectedTvDbLanguage); }
    }

    public List<TvdbLanguage> AvailableTvDbLanguages
    {
      get { return _availableTvDbLanguages; }
      set { PropertyChanged.ChangeAndNotify(ref _availableTvDbLanguages, value, () => AvailableTvDbLanguages); }
    }

    public bool BasedOnExistingTags
    {
      get { return _basedOnExistingTags; }
      set { PropertyChanged.ChangeAndNotify(ref _basedOnExistingTags, value, () => BasedOnExistingTags); }
    }

    public ObservableCollection<TagSetting> OptionalSeriesTags { get; set; }

    #endregion Properties

    #region Command

    private DelegateCommand browseTvSeriesDbCommand;
    public ICommand BrowseTvSeriesDbCommand
    {
      get
      {
        if (browseTvSeriesDbCommand == null)
          browseTvSeriesDbCommand = new DelegateCommand(new Action<object>(BrowseTvSeriesDb));
        return browseTvSeriesDbCommand;
      }
    }

    private DelegateCommand loadSettingsCommand;
    public ICommand LoadSettingsCommand
    {
      get
      {
        if (loadSettingsCommand == null)
          loadSettingsCommand = new DelegateCommand(new Action<object>(LoadSettings));
        return loadSettingsCommand;
      }
    }

    private DelegateCommand saveSettingsCommand;
    public ICommand SaveSettingsCommand
    {
      get
      {
        if (saveSettingsCommand == null)
          saveSettingsCommand = new DelegateCommand(new Action<object>(SaveSettings));
        return saveSettingsCommand;
      }
    }
    //public ICommand BrowseTvSeriesDbCommand
    //{
    //  get
    //  {
    //    if (browseTvSeriesDbCommand == null)
    //      browseTvSeriesDbCommand = new DelegateCommand(new Action(BrowseTvSeriesDb),
    //          new Func<bool>(BrowseTvSeriesDbCanExecute));
    //    return browseTvSeriesDbCommand;
    //  }
    //}

    #endregion
      //  public Configuration Config { get; set; } 
      //  public PersonViewModel() 
      //  { 
      //      //This data will load as the default person from the model attached to
      //      //the view 
      //      Person = new PersonModel 
      //{ FirstName = "John", LastName = "Doe", Age = 999 }; 
      //  } 

    #region Constructors

    public ConfigurationViewModel()
    {
      SettingsVisible = Visibility.Collapsed;
      MPTVSeriesDatabasePath = MPTVSeriesImporter.GetDefaultDatabasePath();
      SelectedTvDbLanguage = TvdbLanguage.DefaultLanguage;

      if (OptionalSeriesTags == null)
      {
        OptionalSeriesTags = new ObservableCollection<TagSetting>();

        foreach (TagSetting tag in Consts.SeriesTags)
          OptionalSeriesTags.Add(tag);
      }

      LoadSettings(null);
    }

    #endregion Constructors

    public void LoadSettings(object parameter)
    {
      SettingsVisible = Visibility.Collapsed;

      AvailableTvDbLanguages = new TheTvDbImporter().GetAvailableLanguages();

      MPTVSeriesDatabasePath = App.Config.MPTVSeriesDatabasePath;
      SelectedTvDbLanguage = App.Config.SelectedTvDbLanguage;

      BasedOnExistingTags = App.Config.BasedOnExistingTags;
      OptionalSeriesTags = new ObservableCollection<TagSetting>(App.Config.OptionalSeriesTags.Values);
    }

    public void SaveSettings(object parameter)
    {
      SettingsVisible = Visibility.Visible;

      App.Config.MPTVSeriesDatabasePath = MPTVSeriesDatabasePath;
      App.Config.SelectedTvDbLanguage = SelectedTvDbLanguage;

      App.Config.BasedOnExistingTags = BasedOnExistingTags;
      foreach (TagSetting setting in OptionalSeriesTags)
        App.Config.OptionalSeriesTags[setting.ID] = setting;
    }

    public void BrowseTvSeriesDb(object parameter)
    {
      System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();

      ofd.InitialDirectory = Path.GetDirectoryName(MPTVSeriesDatabasePath);
      while (!Directory.Exists(ofd.InitialDirectory))
      {
        ofd.InitialDirectory = Directory.GetParent(ofd.InitialDirectory).FullName;
      }

      ofd.Multiselect = false;
      ofd.Filter = MPTVSeriesImporter.DatabaseFilename + "|" + MPTVSeriesImporter.DatabaseFilename;

      if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        MPTVSeriesDatabasePath = ofd.FileName;
        //dbPath.Text = ofd.FileName;
      }
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

    #endregion Static Methods

    public event PropertyChangedEventHandler PropertyChanged;
  }
}