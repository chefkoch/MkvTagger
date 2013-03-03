using System;
using System.Collections.Generic;
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
        textEditorOriginal.Text = string.Empty;
        //clear textboxes
      }
      {
        textEditorOriginal.Text = MatroskaLoader.GetXML(originalTag);
        UpdateGUI(originalTag);
      }
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

      if (!string.IsNullOrEmpty(movieName.Value))
      {
        tag.Movie.SetTitle(movieName.Value);

        if (!string.IsNullOrEmpty(movieName.ValueSort))
          tag.Movie.MovieTitle.SortWith = movieName.ValueSort;
      }

      if (!string.IsNullOrEmpty(imdbId.Value))
        tag.Movie.IMDB_ID = imdbId.Value;

      textEditorOriginal.Text = MatroskaLoader.GetXML(originalTag);
      textEditorNew.Text = MatroskaLoader.GetXML(tag);
    }
  }
}
