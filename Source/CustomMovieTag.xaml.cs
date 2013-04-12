using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Matroska;
using MkvTagger.DataSources;
using MkvTagger.WpfExtensions;

namespace MkvTagger
{
  /// <summary>
  /// Interaktionslogik für CustomMoviesTag.xaml
  /// </summary>
  public partial class CustomMovieTag : UserControl
  {
    public CustomMovieTag()
    {
      InitializeComponent();
    }

    private MatroskaTags originalTag;

    #region Command

    private DelegateCommand openWebsiteCommand;

    public ICommand OpenWebsiteCommand
    {
      get
      {
        if (openWebsiteCommand == null)
          openWebsiteCommand = new DelegateCommand(OpenWebsite, OpenWebsiteCanExecute);
        return openWebsiteCommand;
      }
    }

    #endregion

    #region Implementations

    private bool OpenWebsiteCanExecute(object o)
    {
      return true;
      string website = o as string;
      switch (website)
      {
        case "imdb":
          return !string.IsNullOrEmpty(IMDB_ID.Value);

        case "tmdb":
          return !string.IsNullOrEmpty(TMDB_ID.Value);

        default:
          return false;
      }
    }

    private void OpenWebsite(object obj)
    {
      string website = obj as string;
      if (string.IsNullOrEmpty(website)) return;
      switch (website)
      {
        case "imdb":
          if (string.IsNullOrEmpty(IMDB_ID.Value))
          {
            MessageBox.Show("IMDB id is empty!");
            return;
          }
          website = String.Format("http://www.imdb.com/title/{0}", IMDB_ID.Value);
          break;

        case "tmdb":
          if (string.IsNullOrEmpty(TMDB_ID.Value))
          {
            MessageBox.Show("TMDB id is empty!");
            return;
          }
          website = String.Format("http://www.themoviedb.org/movie/{0}", TMDB_ID.Value);
          break;

        default:
          return;
      }

      try
      {
        Process.Start(website);
      }
      catch (Exception) {}
    }

    #endregion

    public void SetFile(string filepath)
    {
      originalTag = MatroskaLoader.ReadTag(filepath);
      if (ReferenceEquals(originalTag, null))
      {
        //clear textboxes
        txtXmlFilename.Text = string.Empty;
        dockXml.Visibility = Visibility.Collapsed;
        txtFilename.Text = filepath;

        textEditorOriginal.Text = string.Empty;
        UpdateGUI();
      }
      else
      {
        txtXmlFilename.Text = string.Empty;
        dockXml.Visibility = Visibility.Collapsed;
        txtFilename.Text = filepath;

        textEditorOriginal.Text = MatroskaLoader.GetXML(originalTag);
        UpdateGUI(originalTag.Movie);
      }
      saveButton.IsEnabled = false;
    }

    private void ClearGUI()
    {
      // Recommended for identification
      Title.Clear();
      IMDB_ID.Clear();
      TMDB_ID.Clear();

      // additional collection infos
      CollectionTitle.Clear();
      CollectionIndex.Clear();
      CollectionKeywords.Clear();
      // additional movie infos
      ReleaseDate.Clear();
      Overview.Clear();
      Tagline.Clear();
      Certification.Clear();
      Genres.Clear();
      Actors.Clear();
      Directors.Clear();
      Writers.Clear();
      MovieKeywords.Clear();
    }

    private void UpdateGUI(MovieTag tag = null)
    {
      ClearGUI();
      if (ReferenceEquals(tag, null)) return;

      // Recommended for identification
      Title.Value = tag.Title;
      IMDB_ID.Value = tag.IMDB_ID;
      TMDB_ID.Value = tag.TMDB_ID;

      // additional collection infos
      CollectionTitle.Value = tag.CollectionTitle;
      CollectionIndex.Value = tag.CollectionIndex;
      CollectionKeywords.Value = tag.CollectionKeywords;
      // additional episode infos
      ReleaseDate.Value = tag.ReleaseDate;
      Overview.Value = tag.Overview;
      Tagline.Value = tag.Tagline;
      Certification.Value = tag.Certification;
      Genres.Value = tag.Genres;
      Actors.Value = tag.Actors;
      Directors.Value = tag.Directors;
      Writers.Value = tag.Writers;
      MovieKeywords.Value = tag.MovieKeywords;
    }

    private void UpdatePreview(object sender, TextChangedEventArgs e)
    {
      MatroskaTags tag = MatroskaLoader.Clone(originalTag);

      tag.Movie = UpdateTagFromGUI(tag.Movie);
      tag.Cleanup();

      textEditorNew.Text = MatroskaLoader.GetXML(tag);
      saveButton.IsEnabled = true;
    }

    private MovieTag UpdateTagFromGUI(MovieTag tag)
    {
      // Recommended for identification
      tag.Title = Title.Value;
      tag.IMDB_ID = IMDB_ID.Value;
      tag.TMDB_ID = TMDB_ID.Value;

      // additional collection infos
      tag.CollectionTitle = CollectionTitle.Value;
      tag.CollectionIndex = CollectionIndex.Value;
      tag.CollectionKeywords = CollectionKeywords.Value;
      // additional episode infos
      tag.ReleaseDate = ReleaseDate.Value;
      tag.Overview = Overview.Value;
      tag.Tagline = Tagline.Value;
      tag.Certification = Certification.Value;
      tag.Genres = Genres.Value;
      tag.Actors = Actors.Value;
      tag.Directors = Directors.Value;
      tag.Writers = Writers.Value;
      tag.MovieKeywords = MovieKeywords.Value;

      return tag;
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
      MatroskaTags tags;

      try
      {
        tags = MatroskaLoader.ReadTagFromXML(textEditorNew.Text);
        if (tags == null)
        {
          MessageBox.Show("invalid tag file.");
          return;
        }
      }
      catch (Exception)
      {
        MessageBox.Show("invalid tag file.");
        return;
      }

      MessageBoxResult result = MessageBox.Show("Do you really want to overwrite the old tags with the new ones?",
                                                "Overwrite tags",
                                                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

      if (result == MessageBoxResult.Yes)
      {
        MatroskaLoader.WriteTags(tags, txtFilename.Text);
        SetFile(txtFilename.Text);
      }
    }

    private void refreshButton_OnClick(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrWhiteSpace(txtFilename.Text)) return;
      if (!File.Exists(txtFilename.Text)) return;

      SetFile(txtFilename.Text);
    }

    private void tmdb_OnClick(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrWhiteSpace(txtFilename.Text)) return;
      if (!File.Exists(txtFilename.Text)) return;

      // Use original tags as base with latest changes from GUI
      MatroskaTags tag = MatroskaLoader.Clone(originalTag);
      tag.Movie = UpdateTagFromGUI(tag.Movie);

      // Update from TVSeries
      try
      {
        TheMovieDbImporter importer = new TheMovieDbImporter();
        tag.Movie = importer.UpdateTags(tag.Movie);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return;
      }

      UpdateGUI(tag.Movie);
    }
  }
}