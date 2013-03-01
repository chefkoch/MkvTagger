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
      Series = new SeriesTag(this);
      Movie = new MovieTag(this);
    }

    #region tag interpreters

    [XmlIgnore]
    public SeriesTag Series { get; set; }
    [XmlIgnore]
    public MovieTag Movie { get; set; }

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

    public Simple GetSimple(Tag tag, string name)
    {
      return tag.Simples.FirstOrDefault(s => s.Name == name);
    }

    public Simple GetSimple(Simple simple, string name)
    {
      return simple.Simples.FirstOrDefault(s => s.Name == name);
    }

    public Simple GetOrAddSimple(Tag tag, string name)
    {
      Simple newSimple = tag.Simples.FirstOrDefault(s => s.Name == name);
      if (ReferenceEquals(newSimple, null))
      {
        newSimple = new Simple(name);
        tag.Simples.Add(newSimple);
      }
      return newSimple;
    }

    public Simple GetOrAddSimple(Simple simple, string name)
    {
      Simple newSimple = simple.Simples.FirstOrDefault(s => s.Name == name);
      if (ReferenceEquals(newSimple, null))
      {
        newSimple = new Simple(name);
        simple.Simples.Add(newSimple);
      }
      return newSimple;
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

    #endregion helper methods
  }

  public class Tag
  {
    public Tag()
    {
    }

    public Tag(int targetTypeValue)
    {
      Targets = new Targets { TargetTypeValue = targetTypeValue };
    }

    [XmlElement("Targets")]
    public Targets Targets { get; set; }

    [XmlElement("Simple")]
    public List<Simple> Simples { get; set; }
  }

  [Serializable]
  public class Targets
  {
    [XmlElement("TargetTypeValue")]
    public int TargetTypeValue { get; set; }

    [XmlElement("TargetType")]
    public string TargetType { get; set; }

    [XmlElement("TrackUID")]
    public List<int> TrackUIDs { get; set; }

    [XmlElement("EditionUID")]
    public List<int> EditionUIDs { get; set; }

    [XmlElement("ChapterUID")]
    public List<int> ChapterUIDs { get; set; }

    [XmlElement("AttachmentUID")]
    public List<int> AttachmentUIDs { get; set; }
  }

  [Serializable]
  public class Simple
  {
    public Simple()
    {
    }

    public Simple(string name)
    {
      Name = name;
      TagLanguageValue = "und";
      DefaultLanguageValue = 1;
    }

    public Simple(string name, string value)
      : this(name)
    {
      StringValue = value;
    }

    public Simple(string name, int value)
      : this(name, value.ToString())
    {
    }

    [XmlElement("Name")]
    public string Name { get; set; }

    [XmlElement("String")]
    public string StringValue { get; set; }

    [XmlElement("TagLanguage")]
    public string TagLanguageValue { get; set; }

    [XmlElement("DefaultLanguage")]
    public int DefaultLanguageValue { get; set; }

    [XmlIgnore]
    public bool DefaultLanguage
    {
      get { return DefaultLanguageValue == 1; }
      set { DefaultLanguageValue = value ? 1 : 0; }
    }

    [XmlElement("Binary")]
    public object BinaryValue { get; set; }

    [XmlElement("Simple")]
    public List<Simple> Simples { get; set; }
  }
}