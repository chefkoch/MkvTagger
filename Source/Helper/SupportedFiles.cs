using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MkvTagger.Helper
{
  internal class SupportedFiles
  {
    public static readonly List<string> DEFAULT_VIDEO_FILE_EXTENSIONS = new List<string>
      {
        ".mkv",
        ".mk3d",
        ".ogm",
        ".avi",
        ".wmv",
        ".mpg",
        ".mp4",
        ".ts",
        ".flv",
        ".m2ts",
        ".mts",
        ".mov",
        ".wtv",
        ".dvr-ms",
      };

    public static bool IsFileSupportedVideo(string filename)
    {
      string ext = Path.GetExtension(filename);
      return DEFAULT_VIDEO_FILE_EXTENSIONS.Contains(ext);
    }
  }
}
