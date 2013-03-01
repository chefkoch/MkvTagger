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

namespace MPTvServies2MKV
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

    public void SetFile(string filepath)
    {

    }

    private void UpdatePreview(object sender, TextChangedEventArgs e)
    {
      MatroskaTags tag = new MatroskaTags();

      if (!string.IsNullOrEmpty(movieNameTextBox.Text))
        tag.Movie.MovieName = movieNameTextBox.Text;

      if (!string.IsNullOrEmpty(imdbIdTextBox.Text))
        tag.Movie.IMDB_ID = imdbIdTextBox.Text;

      previewTextBox.Text = tag.ToString();
    }
  }
}
