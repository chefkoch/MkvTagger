using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Matroska;

namespace MatroskaTagger
{
  /// <summary>
  /// Interaktionslogik für "App.xaml"
  /// </summary>
  public partial class App : Application
  {
    public static Configuration Config { get; set; }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
      Config = Configuration.Load(GetConfigFile());
      if (Config == null)
        Config = new Configuration();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
      Configuration.Save(Config, GetConfigFile());
    }

    private string GetConfigFile()
    {
      string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
      string file = Path.Combine(path, "settings.xml");
      return file;
    }
  }
}
