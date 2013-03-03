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
  public class MovieTag
  {
    private readonly MatroskaTags _matroskaTags;

    public MovieTag(MatroskaTags tags)
    {
      _matroskaTags = tags;
    }

    public bool HasMovieTitle
    {
      get { return !ReferenceEquals(MovieTitle, null); }
    }

    public SortWithEntity MovieTitle
    {
      get
      {
        try
        {
          Tag movieTag = _matroskaTags.GetTag(50);
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
      Tag movieTag = _matroskaTags.GetOrAddTag(50);
      Simple titleSimple = movieTag.GetOrAddSimple("TITLE");
      titleSimple.StringValue = titleValue;
    }

    //public string MovieTitleSort
    //{
    //  get
    //  {
    //    try
    //    {
    //      Tag movieTag = _matroskaTags.GetTag(50);
    //      Simple titleSimple = movieTag.GetSimple("TITLE");
    //      Simple sortSimple = titleSimple.GetSimple("SORT_WITH");
    //      return sortSimple.StringValue;
    //    }
    //    catch (Exception)
    //    {
    //      return null;
    //    }
    //  }
    //  set
    //  {
    //    Tag movieTag = _matroskaTags.GetOrAddTag(50);
    //    Simple titleSimple = _matroskaTags.GetOrAddSimple(movieTag, "TITLE");
    //    Simple sortSimple = _matroskaTags.GetOrAddSimple(titleSimple, "SORT_WITH");
    //    sortSimple.StringValue = value;
    //  }
    //}

    public string IMDB_ID
    {
      get
      {
        try
        {
          Tag movieTag = _matroskaTags.GetTag(50);
          Simple titleSimple = movieTag.GetSimple("IMDB");
          return titleSimple.StringValue;
        }
        catch (Exception)
        {
          return null;
        }
      }
      set
      {
        Tag movieTag = _matroskaTags.GetOrAddTag(50);
        Simple titleSimple = movieTag.GetOrAddSimple("IMDB");
        titleSimple.StringValue = value;
      }
    }

    public ReadOnlyCollection<SortWithEntity> Directors
    {
      get
      {
        List<SortWithEntity> result = new List<SortWithEntity>();

        Tag movieTag = _matroskaTags.GetTag(50);
        if (ReferenceEquals(movieTag, null)) return result.AsReadOnly();

        foreach (Simple simple in movieTag.Simples.Where(s => s.Name == "DIRECTOR"))
        {
          result.Add(simple as SortWithEntity);
        }

        return result.AsReadOnly();
      }
      set
      {
        Tag movieTag = _matroskaTags.GetOrAddTag(50);
        movieTag.Simples.RemoveAll(s => s.Name == "DIRECTOR");
        foreach (SortWithEntity simple in value)
        {
          movieTag.Simples.Add(simple);
        }
      }
    }

    public ReadOnlyCollection<Actor> Actors
    {
      get
      {
        List<Actor> result = new List<Actor>();

        Tag movieTag = _matroskaTags.GetTag(50);
        if (ReferenceEquals(movieTag, null)) return result.AsReadOnly();

        foreach (Simple simple in movieTag.Simples.Where(s => s.Name == "ACTOR"))
        {
          result.Add(simple as Actor);
        }

        return result.AsReadOnly();
      }
      set
      {
        Tag movieTag = _matroskaTags.GetOrAddTag(50);
        movieTag.Simples.RemoveAll(s => s.Name == "ACTOR");
        foreach (Actor simple in value)
        {
          movieTag.Simples.Add(simple);
        }
      }
    }
  }
}