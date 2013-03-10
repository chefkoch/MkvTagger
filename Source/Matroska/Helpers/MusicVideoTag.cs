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
  public class MusicVideoTag
  {
    private readonly MatroskaTags _matroskaTags;

    public MusicVideoTag(MatroskaTags tags)
    {
      _matroskaTags = tags;
    }

    #region Album Tags

    public string AlbumTitle
    {
      get
      {
        try
        {
          Tag albumTag = _matroskaTags.GetTag(50);
          Simple titleSimple = albumTag.GetSimple("TITLE");
          return titleSimple.StringValue;
        }
        catch (Exception)
        {
          return null;
        }
      }
      set
      {
        Tag albumTag = _matroskaTags.GetOrAddTag(50);
        Simple titleSimple = albumTag.GetOrAddSimple("TITLE");
        titleSimple.StringValue = value;
      }
    }

    public string AlbumArtist
    {
      get
      {
        try
        {
          Tag albumTag = _matroskaTags.GetTag(50);
          Simple artistSimple = albumTag.GetSimple("ARTIST");
          return artistSimple.StringValue;
        }
        catch (Exception)
        {
          return null;
        }
      }
      set
      {
        Tag albumTag = _matroskaTags.GetOrAddTag(50);
        Simple artistSimple = albumTag.GetOrAddSimple("ARTIST");
        artistSimple.StringValue = value;
      }
    }

    public string AlbumReleaseDate
    {
      get
      {
        try
        {
          Tag albumTag = _matroskaTags.GetTag(50);
          Simple dateSimple = albumTag.GetSimple("DATE_RELEASE");
          return dateSimple.StringValue;
        }
        catch (Exception)
        {
          return null;
        }
      }
      set
      {
        Tag albumTag = _matroskaTags.GetOrAddTag(50);
        Simple dateSimple = albumTag.GetOrAddSimple("DATE_RELEASE");
        dateSimple.StringValue = value;
      }
    }

    //public DateTime? AlbumReleaseDate
    //{
    //  get
    //  {
    //    try
    //    {
    //      Tag albumTag = _matroskaTags.GetTag(50);
    //      Simple dateSimple = albumTag.GetSimple("DATE_RELEASE");
    //      return DateTime.Parse(dateSimple.StringValue);
    //    }
    //    catch (Exception)
    //    {
    //      return null;
    //    }
    //  }
    //  set
    //  {
    //    if (value.HasValue)
    //    {
    //      Tag albumTag = _matroskaTags.GetOrAddTag(50);
    //      Simple dateSimple = albumTag.GetOrAddSimple("DATE_RELEASE");
    //      dateSimple.StringValue = value.Value.ToString("yyyy-mm-dd");
    //    }
    //  }
    //}

    #endregion

    #region Track Tags

    public int? TrackNumber
    {
      get
      {
        try
        {
          Tag trackTag = _matroskaTags.GetTag(30);
          Simple numSimple = trackTag.GetSimple("PART_NUMBER");
          return int.Parse(numSimple.StringValue);
        }
        catch (Exception)
        {
          return null;
        }
      }
      set
      {
        Tag trackTag = _matroskaTags.GetOrAddTag(30);
        Simple numSimple = trackTag.GetOrAddSimple("PART_NUMBER");
        numSimple.StringValue = value.ToString();
      }
    }

    public string TrackTitle
    {
      get
      {
        try
        {
          Tag trackTag = _matroskaTags.GetTag(30);
          Simple titleSimple = trackTag.GetSimple("TITLE");
          return titleSimple.StringValue;
        }
        catch (Exception)
        {
          return null;
        }
      }
      set
      {
        Tag trackTag = _matroskaTags.GetOrAddTag(30);
        Simple titleSimple = trackTag.GetOrAddSimple("TITLE");
        titleSimple.StringValue = value;
      }
    }

    public string TrackArtist
    {
      get
      {
        try
        {
          Tag trackTag = _matroskaTags.GetTag(30);
          Simple artistSimple = trackTag.GetSimple("ARTIST");
          return artistSimple.StringValue;
        }
        catch (Exception)
        {
          return null;
        }
      }
      set
      {
        Tag trackTag = _matroskaTags.GetOrAddTag(30);
        Simple artistSimple = trackTag.GetOrAddSimple("ARTIST");
        artistSimple.StringValue = value;
      }
    }

    public string TrackReleaseDate
    {
      get
      {
        try
        {
          Tag trackTag = _matroskaTags.GetTag(30);
          Simple dateSimple = trackTag.GetSimple("DATE_RELEASE");
          return dateSimple.StringValue;
        }
        catch (Exception)
        {
          return null;
        }
      }
      set
      {
        Tag trackTag = _matroskaTags.GetOrAddTag(30);
        Simple dateSimple = trackTag.GetOrAddSimple("DATE_RELEASE");
        dateSimple.StringValue = value;
      }
    }

    public ReadOnlyCollection<string> GenreList
    {
      get
      {
        List<string> result = new List<string>();

        Tag trackTag = _matroskaTags.GetTag(30);
        if (ReferenceEquals(trackTag, null)) return result.AsReadOnly();

        foreach (Simple indexSimple in trackTag.Simples.Where(s => s.Name == "GENRE"))
          result.Add(indexSimple.StringValue);

        return result.AsReadOnly();
      }
      set
      {
        Tag trackTag = _matroskaTags.GetOrAddTag(30);
        trackTag.Simples.RemoveAll(s => s.Name == "GENRE");
        foreach (string i in value)
        {
          Simple indexSimple = new Simple("GENRE", i);
          trackTag.Simples.Add(indexSimple);
        }
      }
    }

    #endregion

    public override string ToString()
    {
      return String.Format("{0} - {1}|{2}|{3}|{4}", TrackArtist, TrackTitle, AlbumTitle, TrackNumber, AlbumReleaseDate);
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