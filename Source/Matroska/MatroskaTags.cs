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
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Matroska
{
  [XmlRoot("Tags")]
  public class MatroskaTags
  {
    [XmlElement("Tag")]
    public List<Tag> TagList { get; set; }

    public MatroskaTags()
    {
      TagList = new List<Tag>();
      Series = new SeriesTag(this);
      Movie = new MovieTag(this);
      MusicVideo = new MusicVideoTag(this);
    }

    #region tag interpreters

    [XmlIgnore]
    public SeriesTag Series { get; set; }

    [XmlIgnore]
    public MovieTag Movie { get; set; }

    [XmlIgnore]
    public MusicVideoTag MusicVideo { get; set; }

    #endregion tag interpreters

    #region helper methods

    public Tag GetTag(int targetTypeValue)
    {
      return TagList.FirstOrDefault(t => t.Targets.TargetTypeValue == targetTypeValue);
    }

    public Tag GetOrAddTag(int targetTypeValue)
    {
      Tag tag = TagList.FirstOrDefault(t => t.Targets.TargetTypeValue == targetTypeValue);
      if (!ReferenceEquals(tag, null))
        return tag;

      tag = new Tag(targetTypeValue);
      TagList.Add(tag);

      return tag;
    }

    public void RemoveSpecificSimpleChild(XElement tagOrSimple, string name)
    {
      while (true)
      {
        IEnumerable<XElement> simpleCollection = tagOrSimple.Elements("Simple").Where(simple =>
          {
            XElement element = simple.Element("Name");
            return element != null && element.Value == name;
          });

        if (!simpleCollection.Any())
          break;

        foreach (XElement xElement in simpleCollection)
          xElement.Remove();
      }
    }

    public void AddSimpleCollection(XElement tagOrSimple, string name, IEnumerable values)
    {
      foreach (object value in values)
      {
        XElement simpleElement = new XElement("Simple");
        simpleElement.SetElementValue("Name", name);
        simpleElement.SetElementValue("String", value);

        tagOrSimple.Add(simpleElement);
      }
    }

    public void ReadTags(string filename)
    {
      MatroskaTags tags = MatroskaLoader.ReadTag(filename);
      if (tags != null)
        TagList = tags.TagList;
    }

    #endregion helper methods

    public void SetValue(int targetTypeValue, string tagName, string tagString)
    {
      Tag targetTag = GetOrAddTag(targetTypeValue);

      if (string.IsNullOrWhiteSpace(tagString))
      {
        targetTag.RemoveSimple(tagName);
        return;
      }

      Simple nameSimple = targetTag.GetOrAddSimple(tagName);
      nameSimple.StringValue = tagString;
    }

    public string GetValue(int targetTypeValue, string tagName)
    {
      try
      {
        Tag targetTag = GetTag(targetTypeValue);
        Simple nameSimple = targetTag.GetSimple(tagName);
        return nameSimple.StringValue;
      }
      catch (Exception)
      {
        return null;
      }
    }

    public ReadOnlyCollection<string> GetValueCollection(int targetTypeValue, string tagName)
    {
      List<string> result = new List<string>();

      Tag targetTag = GetTag(targetTypeValue);
      if (ReferenceEquals(targetTag, null)) return result.AsReadOnly();

      foreach (Simple nameSimple in targetTag.Simples.Where(s => s.Name == tagName))
      {
        try
        {
          result.Add(nameSimple.StringValue);
        }
        catch {}
      }

      return result.AsReadOnly();
    }

    public void SetValueCollection(int targetTypeValue, string tagName, IEnumerable<string> enumerable)
    {
      Tag targetTag = GetOrAddTag(targetTypeValue);
      targetTag.Simples.RemoveAll(s => s.Name == tagName);

      if (enumerable == null)
        return;

      foreach (string s in enumerable)
      {
        Simple nameSimple = new Simple(tagName, s);
        targetTag.Simples.Add(nameSimple);
      }
    }

    /// <summary>
    /// Cleans up empty tags, which only contain a target type value, but no simples
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    internal void Cleanup()
    {
      foreach (Tag tag in TagList)
      {
        if (tag.Simples.Count > 0) continue;

        // Remove current item, start cleanup from beginning (recoursive) and stop current iteration
        TagList.Remove(tag);
        Cleanup();
        break;
      }
    }
  }
}