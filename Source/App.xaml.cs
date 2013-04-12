using System.IO;
using System.Reflection;
using System.Windows;
using MediaPortal.OnlineLibraries.TheTvDb;
using MkvTagger.Models;

namespace MkvTagger
{
  /// <summary>
  /// Interaktionslogik für "App.xaml"
  /// </summary>
  public partial class App : Application
  {
    public static Configuration Config { get; set; }

    private TvdbHandler handler;

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