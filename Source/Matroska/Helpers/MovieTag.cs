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

using System.Collections.ObjectModel;

namespace Matroska
{
  public class MovieTag
  {
    private readonly MatroskaTags _matroskaTags;

    public MovieTag(MatroskaTags tags)
    {
      _matroskaTags = tags;
    }

    #region Recommended for Identification

    public string Title
    {
      get { return _matroskaTags.GetValue(50, "TITLE"); }
      set { _matroskaTags.SetValue(50, "TITLE", value); }
    }

    public string IMDB_ID
    {
      get { return _matroskaTags.GetValue(50, "IMDB"); }
      set { _matroskaTags.SetValue(50, "IMDB", value); }
    }

    public string TMDB_ID
    {
      get { return _matroskaTags.GetValue(50, "TMDB"); }
      set { _matroskaTags.SetValue(50, "TMDB", value); }
    }

    #endregion

    #region Additional tags for collection support

    public string CollectionTitle
    {
      get { return _matroskaTags.GetValue(70, "TITLE"); }
      set { _matroskaTags.SetValue(70, "TITLE", value); }
    }

    public string CollectionIndex
    {
      get { return _matroskaTags.GetValue(50, "PART_NUMBER"); }
      set { _matroskaTags.SetValue(50, "PART_NUMBER", value); }
    }

    public string CollectionKeywords
    {
      get { return _matroskaTags.GetValue(70, "KEYWORDS"); }
      set { _matroskaTags.SetValue(70, "KEYWORDS", value); }
    }

    #endregion

    #region Additional Movie Tags

    public string ReleaseDate
    {
      get { return _matroskaTags.GetValue(50, "DATE_RELEASED"); }
      set { _matroskaTags.SetValue(50, "DATE_RELEASED", value); }
    }

    public string Overview
    {
      get { return _matroskaTags.GetValue(50, "SUMMARY"); }
      set { _matroskaTags.SetValue(50, "SUMMARY", value); }
    }

    public string Tagline
    {
      get { return _matroskaTags.GetValue(50, "SUBTITLE"); }
      set { _matroskaTags.SetValue(50, "SUBTITLE", value); }
    }

    public string Certification
    {
      get { return _matroskaTags.GetValue(50, "LAW_RATING"); }
      set { _matroskaTags.SetValue(50, "LAW_RATING", value); }
    }

    public ReadOnlyCollection<string> Genres
    {
      get { return _matroskaTags.GetValueCollection(50, "GENRE"); }
      set { _matroskaTags.SetValueCollection(50, "GENRE", value); }
    }

    public ReadOnlyCollection<string> Actors
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

    public string MovieKeywords
    {
      get { return _matroskaTags.GetValue(50, "KEYWORDS"); }
      set { _matroskaTags.SetValue(50, "KEYWORDS", value); }
    }

    #endregion
  }
}