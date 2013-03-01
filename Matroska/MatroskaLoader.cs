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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Matroska
{
  public static class MatroskaLoader
  {
    public static string[] MatroskaExtensions = new string[] { ".mkv", ".mk3d", ".mka" };

    internal static bool TryExtractTagFromMatroska(string matroskaFile, out MatroskaTags tags)
    {
      tags = ReadTagFromMatroska(matroskaFile);

      return !ReferenceEquals(tags, null);
    }

    public static MatroskaTags ReadTag(string fileName)
    {
      if (string.IsNullOrEmpty(fileName))
        throw new ArgumentNullException("fileName");
      if (!File.Exists(fileName))
        throw new FileNotFoundException("No file found for reading.", fileName);

      //if (tagCache.ContainsKey(fileName))
      //  return tagCache[fileName];

      string extension = Path.GetExtension(fileName);
      if (extension == null)
        throw new FileFormatException("File was not identified as XML or Matroska-file.");

      // XML file
      if (extension.ToLower().Equals(".xml"))
        return ReadTagFromXML(fileName);

      // Matroska file
      if (MatroskaExtensions.Contains(extension.ToLower()))
        return ReadTagFromMatroska(fileName);

      // Invalid file
      throw new FileFormatException("File was not identified as XML or Matroska-file.");
    }

    public static MatroskaTags ReadTagFromXML(string xmlFile)
    {
      MatroskaTags tags;

      try
      {
        XmlSerializer deserializer = new XmlSerializer(typeof(MatroskaTags));
        TextReader textReader = new StreamReader(xmlFile);
        tags = (MatroskaTags)deserializer.Deserialize(textReader);
        textReader.Close();
      }
      catch (Exception)
      {
        return null;
      }

      return tags;
    }

    public static MatroskaTags ReadTagFromMatroska(string matroskaFile)
    {
      string tempFile = Path.GetTempFileName();

      ProcessStartInfo info = new ProcessStartInfo();
      info.FileName = "mkvextract.exe";
      info.Arguments = String.Format(" tags \"{0}\" --redirect-output \"{1}\"", matroskaFile, tempFile);
      info.UseShellExecute = false;
      info.RedirectStandardInput = true;
      info.RedirectStandardOutput = true;
      info.RedirectStandardError = true;
      info.CreateNoWindow = true;

      Process proc = Process.Start(info);
      proc.WaitForExit();

      MatroskaTags result = ReadTagFromXML(tempFile);
      File.Delete(tempFile);
      return result;
    }

    public static string GetXML(MatroskaTags tags)
    {
      using (StringWriter textWriter = new StringWriter())
      {
        XmlSerializerNamespaces noNamespaces = new XmlSerializerNamespaces();
        noNamespaces.Add(string.Empty, string.Empty);

        XmlSerializer serializer = new XmlSerializer(typeof(MatroskaTags));
        serializer.Serialize(textWriter, tags, noNamespaces);
        textWriter.Close();
        return textWriter.ToString();
      }
    }

    public static void WriteTagToXML(MatroskaTags tags, string xmlFile)
    {
      XmlSerializerNamespaces noNamespaces = new XmlSerializerNamespaces();
      noNamespaces.Add(string.Empty, string.Empty);

      XmlSerializer serializer = new XmlSerializer(typeof(MatroskaTags));
      TextWriter textWriter = new StreamWriter(xmlFile);
      serializer.Serialize(textWriter, tags, noNamespaces);
      textWriter.Close();
    }

    public static int WriteTagToMatroska(string matroskaFile, string tagFile)
    {
      ProcessStartInfo info = new ProcessStartInfo();
      info.FileName = "mkvpropedit.exe";
      info.Arguments = String.Format(" \"{0}\" --tags global:\"{1}\"", matroskaFile, tagFile);
      info.UseShellExecute = false;
      info.RedirectStandardInput = true;
      info.RedirectStandardOutput = true;
      info.RedirectStandardError = true;
      info.CreateNoWindow = true;

      Process proc = Process.Start(info);
      proc.WaitForExit();

      return proc.ExitCode;
    }

    public static int WriteTagToMatroska(string matroskaFile, MatroskaTags tags)
    {
      int exitCode;
      string tempFile = Path.GetTempFileName();

      WriteTagToXML(tags, tempFile);
      exitCode = WriteTagToMatroska(tempFile, matroskaFile);

      File.Delete(tempFile);

      return exitCode;
    }
  }
}