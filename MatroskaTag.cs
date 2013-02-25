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

namespace MPTvServies2MKV
{
  public class MatroskaTag
  {
    private XDocument document;

    public SeriesTag Series { get; set; }
    public MovieTag Movie { get; set; }

    public MatroskaTag()
    {
      document = new XDocument(new XDeclaration("1.0", "UTF-8", null),
                               new XElement("Tags"));
      Series = new SeriesTag(this);
      Movie = new MovieTag(this);
    }

    public void Load(string tagFile)
    {
      document = XDocument.Load(tagFile);
      Series = new SeriesTag(this);
      Movie = new MovieTag(this);
    }

    public void Save(string tagFile)
    {
      document.Save(tagFile);
    }

    public XElement GetTag(string targetTypeValue)
    {
      return document.Root.Descendants("TargetTypeValue").FirstOrDefault(type => (string)type == targetTypeValue).Parent.Parent;
    }

    public XElement GetOrAddTag(string targetTypeValue)
    {
      XElement tag = document.Root.Descendants("TargetTypeValue").FirstOrDefault(type => (string)type == targetTypeValue);
      if (!ReferenceEquals(tag, null))
        return tag.Parent.Parent;

      XElement targets = new XElement("Targets");
      targets.SetElementValue("TargetTypeValue", targetTypeValue);

      tag = new XElement("Tag");
      tag.Add(targets);

      document.Root.Add(tag);
      return tag;
    }

    public XElement GetSimple(XElement tagOrSimple, string name)
    {
      return tagOrSimple.Elements("Simple").FirstOrDefault(simple =>
        {
          XElement element = simple.Element("Name");
          return element != null && element.Value == name;
        });
    }

    public XElement GetOrAddSimple(XElement tagOrSimple, string name)
    {
      XElement simpleElement = tagOrSimple.Elements("Simple").FirstOrDefault(simple =>
          {
            XElement element = simple.Element("Name");
            return element != null && element.Value == name;
          });
      if (!ReferenceEquals(simpleElement, null))
        return simpleElement;

      simpleElement = new XElement("Simple");
      simpleElement.SetElementValue("Name", name);

      tagOrSimple.Add(simpleElement);
      return simpleElement;
    }

    public IEnumerable<XElement> GetSimpleCollection(XElement tagOrSimple, string name)
    {
      return tagOrSimple.Elements("Simple").Where(simple =>
        {
          XElement element = simple.Element("Name");
          return element != null && element.Value == name;
        });
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

    public override string ToString()
    {
      StringBuilder builder = new StringBuilder();

      using (TextWriter writer = new StringWriter(builder))
      {
        document.Save(writer);
      }

      return builder.ToString();
    }

    public class SeriesTag
    {
      public MatroskaTag Parent { get; set; }

      internal SeriesTag(MatroskaTag tag)
      {
        Parent = tag;
      }

      public string SeriesName
      {
        get
        {
          try
          {
            XElement seriesTag = Parent.GetTag("70");
            XElement titleSimple = Parent.GetSimple(seriesTag, "TITLE");
            return titleSimple.Element("String").Value;
          }
          catch (Exception)
          {
            return null;
          }
        }
        set
        {
          XElement seriesTag = Parent.GetOrAddTag("70");
          XElement titleSimple = Parent.GetOrAddSimple(seriesTag, "TITLE");
          titleSimple.SetElementValue("String", value);
        }
      }

      public string SeriesNameSort
      {
        get
        {
          try
          {
            XElement seriesTag = Parent.GetTag("70");
            XElement titleSimple = Parent.GetSimple(seriesTag, "TITLE");
            XElement sortSimple = Parent.GetSimple(titleSimple, "SORT_WITH");
            return sortSimple.Element("String").Value;
          }
          catch (Exception)
          {
            return null;
          }
        }
        set
        {
          XElement seriesTag = Parent.GetOrAddTag("70");
          XElement titleSimple = Parent.GetOrAddSimple(seriesTag, "TITLE");
          XElement sortSimple = Parent.GetOrAddSimple(titleSimple, "SORT_WITH");
          sortSimple.SetElementValue("String", value);
        }
      }

      public int? SeasonIndex
      {
        get
        {
          try
          {
            XElement seasontag = Parent.GetTag("60");
            XElement indexSimple = Parent.GetSimple(seasontag, "PART_NUMBER");
            return int.Parse(indexSimple.Element("String").Value);
          }
          catch (Exception)
          {
            return null;
          }
        }
        set
        {
          XElement seasontag = Parent.GetOrAddTag("60");
          XElement indexSimple = Parent.GetOrAddSimple(seasontag, "PART_NUMBER");
          indexSimple.SetElementValue("String", value);
        }
      }

      public ReadOnlyCollection<int> EpisodeIndexList
      {
        get
        {
          List<int> result = new List<int>();

          XElement episodetag = Parent.GetTag("50");
          if (ReferenceEquals(episodetag, null)) return result.AsReadOnly();

          foreach (XElement indexSimple in Parent.GetSimpleCollection(episodetag, "PART_NUMBER"))
          {
            try
            {
              result.Add(int.Parse(indexSimple.Element("String").Value));
            }
            catch
            {
            }
          }

          return result.AsReadOnly();
        }
        set
        {
          XElement episodetag = Parent.GetOrAddTag("50");
          Parent.RemoveSpecificSimpleChild(episodetag, "PART_NUMBER");
          Parent.AddSimpleCollection(episodetag, "PART_NUMBER", value);
        }
      }

      public override string ToString()
      {
        return String.Format("{0}|{1}|{2}|{3}", SeriesName, SeriesNameSort, SeasonIndex, string.Join("|", EpisodeIndexList));
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

    public class MovieTag
    {
      public MatroskaTag Parent { get; set; }

      internal MovieTag(MatroskaTag tag)
      {
        Parent = tag;
      }

      public string MovieName
      {
        get
        {
          try
          {
            XElement movieTag = Parent.GetTag("50");
            XElement titleSimple = Parent.GetSimple(movieTag, "TITLE");
            return titleSimple.Element("String").Value;
          }
          catch (Exception)
          {
            return null;
          }
        }
        set
        {
          XElement movieTag = Parent.GetOrAddTag("50");
          XElement titleSimple = Parent.GetOrAddSimple(movieTag, "TITLE");
          titleSimple.SetElementValue("String", value);
        }
      }

      public string IMDB_ID
      {
        get
        {
          try
          {
            XElement movieTag = Parent.GetTag("50");
            XElement titleSimple = Parent.GetSimple(movieTag, "IMDB");
            return titleSimple.Element("String").Value;
          }
          catch (Exception)
          {
            return null;
          }
        }
        set
        {
          XElement movieTag = Parent.GetOrAddTag("50");
          XElement titleSimple = Parent.GetOrAddSimple(movieTag, "IMDB");
          titleSimple.SetElementValue("String", value);
        }
      }
    }
  }
}