using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Matroska;

namespace MatroskaTagger
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

      // additional collection infos
      CollectionTitle.Clear();
      CollectionIndex.Clear();
      // additional movie infos
      ReleaseDate.Clear();
      Overview.Clear();
      Genres.Clear();
      Actors.Clear();
      Directors.Clear();
      Writers.Clear();
    }

    private void UpdateGUI(MovieTag tag = null)
    {
      ClearGUI();
      if (ReferenceEquals(tag, null)) return;

      // Recommended for identification
      Title.Value = tag.Title;
      IMDB_ID.Value = tag.IMDB_ID;

      // additional collection infos
      CollectionTitle.Value = tag.CollectionTitle;
      CollectionIndex.Value = tag.CollectionIndex;
      // additional episode infos
      ReleaseDate.Value = tag.ReleaseDate;
      Overview.Value = tag.Overview;
      Genres.Value = tag.Genres;
      Actors.Value = tag.Actors;
      Directors.Value = tag.Directors;
      Writers.Value = tag.Writers;
    }

    private void UpdatePreview(object sender, TextChangedEventArgs e)
    {
      MatroskaTags tag = MatroskaLoader.Clone(originalTag);

      tag.Movie = UpdateTagFromGUI(tag.Movie);

      textEditorNew.Text = MatroskaLoader.GetXML(tag);
      saveButton.IsEnabled = true;
    }

    private MovieTag UpdateTagFromGUI(MovieTag tag)
    {
      // Recommended for identification
      tag.Title = Title.Value;
      tag.IMDB_ID = IMDB_ID.Value;

      // additional collection infos
      tag.CollectionTitle = CollectionTitle.Value;
      tag.CollectionIndex = CollectionIndex.Value;
      // additional episode infos
      tag.ReleaseDate = ReleaseDate.Value;
      tag.Overview = Overview.Value;
      tag.Genres = Genres.Value;
      tag.Actors = Actors.Value;
      tag.Directors = Directors.Value;
      tag.Writers = Writers.Value;

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
  }
}
