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
        ClearGUI();
      }
      else
      {
        txtXmlFilename.Text = string.Empty;
        dockXml.Visibility = Visibility.Collapsed;
        txtFilename.Text = filepath;

        textEditorOriginal.Text = MatroskaLoader.GetXML(originalTag);
        ClearGUI();
        UpdateGUI(originalTag);
      }
      saveButton.IsEnabled = false;
    }

    private void ClearGUI()
    {
      movieName.Clear();
      imdbId.Clear();
    }

    private void UpdateGUI(MatroskaTags tags)
    {
      if (tags.Movie.HasMovieTitle)
        movieName.Value = tags.Movie.MovieTitle.StringValue;

      if (tags.Movie.HasMovieTitle && !string.IsNullOrEmpty(tags.Movie.MovieTitle.SortWith))
        movieName.ValueSort = tags.Movie.MovieTitle.SortWith;

      if (!string.IsNullOrEmpty(tags.Movie.IMDB_ID))
        imdbId.Value = tags.Movie.IMDB_ID;
    }

    private void UpdatePreview(object sender, TextChangedEventArgs e)
    {
      MatroskaTags tag;
      if (!ReferenceEquals(originalTag, null))
      {
        string xmlString = MatroskaLoader.GetXML(originalTag);
        tag = MatroskaLoader.ReadTagFromXML(xmlString);
      }
      else
        tag = new MatroskaTags();

      tag.Movie = UpdateTagFromGUI(tag.Movie);

      textEditorNew.Text = MatroskaLoader.GetXML(tag);
      saveButton.IsEnabled = true;
    }

    private MovieTag UpdateTagFromGUI(MovieTag tag)
    {
      if (!string.IsNullOrEmpty(movieName.Value))
      {
        tag.SetTitle(movieName.Value);

        if (!string.IsNullOrEmpty(movieName.ValueSort))
          tag.MovieTitle.SortWith = movieName.ValueSort;
      }

      if (!string.IsNullOrEmpty(imdbId.Value))
        tag.IMDB_ID = imdbId.Value;

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
