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
  [Serializable]
  public class Simple
  {
    public Simple()
    {
      Simples = new List<Simple>();
      TagLanguageValue = "und";
      DefaultLanguageValue = 1;
    }

    public Simple(string name) : this()
    {
      Name = name;
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

    public Simple GetSimple(string name)
    {
      return Simples.FirstOrDefault(s => s.Name == name);
    }

    public Simple GetOrAddSimple(string name)
    {
      Simple newSimple = Simples.FirstOrDefault(s => s.Name == name);
      if (ReferenceEquals(newSimple, null))
      {
        newSimple = new Simple(name);
        Simples.Add(newSimple);
      }
      return newSimple;
    }
  }
}