using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MatroskaTagger
{
  public class TagSetting : INotifyPropertyChanged
  {
    private bool _write;
    private bool _delete;

    public TagSetting()
    {
      Write = true;
      Delete = false;
    }

    public TagSetting(string id)
      : this()
    {
      ID = id;
    }

    [XmlAttribute]
    public string ID { get; set; }

    [XmlIgnore]
    public string Category
    {
      get
      {
        if (ID.StartsWith("Series"))
          return "Series";

        if (ID.StartsWith("Movie"))
          return "Movie";

        return "unknown";
      }
    }

    [XmlIgnore]
    public bool Forced
    {
      get
      {
        switch (ID)
        {
          case Consts.KeySeriesTitle:
          case Consts.KeySeriesImdb:
          case Consts.KeySeriesSeasonIndex:
          case Consts.KeySeriesEpisodeIndizes:
            return true;
        }

        return false;
      }
    }

    [XmlAttribute]
    public bool Write
    {
      get
      {
        if (Forced)
          return true;

        return _write;
      }
      set
      {
        PropertyChanged.ChangeAndNotify(ref _write, value, () => Write);

        if (_write && _delete)
          _delete = false;
      }
    }

    [XmlAttribute]
    public bool Delete
    {
      get
      {
        if (Forced)
          return false;
        
        return _delete;
      }
      set
      {
        PropertyChanged.ChangeAndNotify(ref _delete, value, () => Delete);

        if (_write && _delete)
          _write = false;
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
