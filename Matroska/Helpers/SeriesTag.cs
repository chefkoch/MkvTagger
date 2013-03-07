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

    public bool HasSeriesName
    {
      get { return !ReferenceEquals(SeriesName, null); }
    }

    public SortWithEntity SeriesName
    {
      get
      {
        try
        {
          Tag movieTag = _matroskaTags.GetTag(70);
          return new SortWithEntity(movieTag.GetSimple("TITLE"));
        }
        catch (Exception)
        {
          return null;
        }
      }
    }

    public void SetTitle(string titleValue)
    {
      Tag movieTag = _matroskaTags.GetOrAddTag(70);
      Simple titleSimple = movieTag.GetOrAddSimple("TITLE");
      titleSimple.StringValue = titleValue;
    }

    public int? SeasonIndex
    {
      get
      {
        try
        {
          Tag seasontag = _matroskaTags.GetTag(60);
          Simple indexSimple = seasontag.GetSimple("PART_NUMBER");
          return int.Parse(indexSimple.StringValue);
        }
        catch (Exception)
        {
          return null;
        }
      }
      set
      {
        Tag seasontag = _matroskaTags.GetOrAddTag(60);
        Simple indexSimple = seasontag.GetOrAddSimple("PART_NUMBER");
        indexSimple.StringValue = value.ToString();
      }
    }

    public ReadOnlyCollection<int> EpisodeIndexList
    {
      get
      {
        List<int> result = new List<int>();

        Tag episodetag = _matroskaTags.GetTag(50);
        if (ReferenceEquals(episodetag, null)) return result.AsReadOnly();

        foreach (Simple indexSimple in episodetag.Simples.Where(s => s.Name =="PART_NUMBER"))
        {
          try
          {
            result.Add(int.Parse(indexSimple.StringValue));
          }
          catch
          {
          }
        }

        return result.AsReadOnly();
      }
      set
      {
        Tag episodetag = _matroskaTags.GetOrAddTag(50);
        episodetag.Simples.RemoveAll(s => s.Name == "PART_NUMBER");
        foreach (int i in value)
        {
          Simple indexSimple = new Simple("PART_NUMBER", i);
          episodetag.Simples.Add(indexSimple);
        }
      }
    }

    public override string ToString()
    {
      return String.Format("{0}|{1}|{2}", SeriesName, SeasonIndex, string.Join("|", EpisodeIndexList));
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
    //  //               Name = (string)FindParameter(item, "name"),
    //  //               Zip = (string)FindParameter(item, "zip")
    //  //             };
    //}

    //public string Name
    //{
    //  get
    //  {
    //    //var mydata = from tag in Parent.document.Root.Elements("TargetTypeValue").First()
    //    //   select new {
    //    //       Label = (string) item.Element("label"),
    //    //       Description = (string) item.Element("description"),
    //    //       Id = (int) FindParameter(item, "id"),
    //    //       Name = (string) FindParameter(item, "name"),
    //    //       Zip = (string) FindParameter(item, "zip")
    //    //   };

    //  }
    //}
  }
}