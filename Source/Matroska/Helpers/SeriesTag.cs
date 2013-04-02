#region Copyright (C) 2007-2013 Team MediaPortal

/*
    Copyright (C) 2007-2013 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Matroska
{
  public class SeriesTag
  {
    private readonly MatroskaTags _matroskaTags;

    public SeriesTag(MatroskaTags tags)
    {
      _matroskaTags = tags;
    }

    #region Recommended for Identification

    public string SeriesName
    {
      get { return _matroskaTags.GetValue(70, "TITLE"); }
      set { _matroskaTags.SetValue(70, "TITLE", value); }
    }

    public string IMDB_ID
    {
      get { return _matroskaTags.GetValue(70, "IMDB"); }
      set { _matroskaTags.SetValue(70, "IMDB", value); }
    }

    public string TVDB_ID
    {
      get { return _matroskaTags.GetValue(70, "TVDB"); }
      set { _matroskaTags.SetValue(70, "TVDB", value); }
    }

    public string SeasonIndex
    {
      get { return _matroskaTags.GetValue(60, "PART_NUMBER"); }
      set { _matroskaTags.SetValue(60, "PART_NUMBER", value); }
    }

    public ReadOnlyCollection<string> EpisodeIndexList
    {
      get { return _matroskaTags.GetValueCollection(50, "PART_NUMBER"); }
      set { _matroskaTags.SetValueCollection(50, "PART_NUMBER", value); }
    }

    public string EpisodeFirstAired
    {
      get { return _matroskaTags.GetValue(50, "DATE_RELEASED"); }
      set { _matroskaTags.SetValue(50, "DATE_RELEASED", value); }
    }

    #endregion

    #region Additional Series Tags

    public string SeriesFirstAired
    {
      get { return _matroskaTags.GetValue(70, "DATE_RELEASED"); }
      set { _matroskaTags.SetValue(70, "DATE_RELEASED", value); }
    }

    public string Network
    {
      get { return _matroskaTags.GetValue(70, "NETWORK"); }
      set { _matroskaTags.SetValue(70, "NETWORK", value); }
    }

    public string SeriesOverview
    {
      get { return _matroskaTags.GetValue(70, "SUMMARY"); }
      set { _matroskaTags.SetValue(70, "SUMMARY", value); }
    }

    public ReadOnlyCollection<string> SeriesGenreList
    {
      get { return _matroskaTags.GetValueCollection(70, "GENRE"); }
      set { _matroskaTags.SetValueCollection(70, "GENRE", value); }
    }

    public ReadOnlyCollection<string> SeriesActors
    {
      get { return _matroskaTags.GetValueCollection(70, "ACTOR"); }
      set { _matroskaTags.SetValueCollection(70, "ACTOR", value); }
    }

    #endregion

    #region Additional Episode Tags

    public string EpisodeTitle
    {
      get { return _matroskaTags.GetValue(50, "TITLE"); }
      set { _matroskaTags.SetValue(50, "TITLE", value); }
    }

    public string EpisodeIMDB_ID
    {
      get { return _matroskaTags.GetValue(50, "IMDB"); }
      set { _matroskaTags.SetValue(50, "IMDB", value); }
    }

    public string EpisodeOverview
    {
      get { return _matroskaTags.GetValue(50, "SUMMARY"); }
      set { _matroskaTags.SetValue(50, "SUMMARY", value); }
    }

    public ReadOnlyCollection<string> GuestStars
    {
      get { return _matroskaTags.GetValueCollection(50, "ACTOR"); }
      set { _matroskaTags.SetValueCollection(50, "ACTOR", value); }
    }

    public ReadOnlyCollection<string> Directors
    {
      get { return _matroskaTags.GetValueCollection(50, "DIRECTOR"); }
      set { _matroskaTags.SetValueCollection(50, "DIRECTOR", value); }
    }

    public ReadOnlyCollection<string> Writers
    {
      get { return _matroskaTags.GetValueCollection(50, "WRITTEN_BY"); }
      set { _matroskaTags.SetValueCollection(50, "WRITTEN_BY", value); }
    }

    #endregion

    public override string ToString()
    {
      return String.Format("{0}|{1}|{2}|{3}|{4}", SeriesName, IMDB_ID, SeasonIndex, string.Join("|", EpisodeIndexList),
                           EpisodeFirstAired);
    }

    //public void Test()
    //{
    //  foreach (XElement element in Parent.document.Root.Descendants("TargetTypeValue"))
    //  {
    //    Console.WriteLine("Element: TargetTypeValue=" + element.Value);
    //  }
    //  Console.WriteLine("-----------------");

    //  //var mydata = from tag in Parent.document.Root.Elements("TargetTypeValue")
    //  //             where 
    //  //             select new
    //  //             {
    //  //               Label = (string)item.Element("label"),
    //  //               Description = (string)item.Element("description"),
    //  //               Id = (int)FindParameter(item, "id"),
    //  //               Title = (string)FindParameter(item, "name"),
    //  //               Zip = (string)FindParameter(item, "zip")
    //  //             };
    //}

    //public string Title
    //{
    //  get
    //  {
    //    //var mydata = from tag in Parent.document.Root.Elements("TargetTypeValue").First()
    //    //   select new {
    //    //       Label = (string) item.Element("label"),
    //    //       Description = (string) item.Element("description"),
    //    //       Id = (int) FindParameter(item, "id"),
    //    //       Title = (string) FindParameter(item, "name"),
    //    //       Zip = (string) FindParameter(item, "zip")
    //    //   };

    //  }
    //}
  }
}