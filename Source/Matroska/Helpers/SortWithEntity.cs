﻿#region Copyright (C) 2007-2013 Team MediaPortal

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
  public class SortWithEntity : Simple
  {
    public SortWithEntity(Simple simple)
    {
      this.BinaryValue = simple.BinaryValue;
      this.DefaultLanguage = simple.DefaultLanguage;
      this.DefaultLanguageValue = simple.DefaultLanguageValue;
      this.Name = simple.Name;
      this.Simples = simple.Simples;
      this.StringValue = simple.StringValue;
      this.TagLanguageValue = simple.TagLanguageValue;
    }

    public string SortWith
    {
      get
      {
        try
        {
          Simple sortSimple = GetSimple("SORT_WITH");
          return sortSimple.StringValue;
        }
        catch (Exception)
        {
          return null;
        }
      }
      set
      {
        Simple sortSimple = GetOrAddSimple("SORT_WITH");
        sortSimple.StringValue = value;
      }
    }
  }
}