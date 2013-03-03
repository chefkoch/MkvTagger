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
  /// Interaktionslogik für CustomSeriesTag.xaml
  /// </summary>
  public partial class CustomSeriesTag : UserControl
  {
    public CustomSeriesTag()
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
        seriesName.Value = tags.Series.SeriesName.StringValue;

      if (tags.Movie.HasMovieTitle && !string.IsNullOrEmpty(tags.Movie.MovieTitle.SortWith))
        seriesName.ValueSort = tags.Series.SeriesName.SortWith;

      if (!ReferenceEquals(tags.Series.SeasonIndex, null))
        seasonIndex.Value = tags.Series.SeasonIndex.ToString();
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

      if (!string.IsNullOrEmpty(seriesName.Value))
      {
        tag.Series.SetTitle(seriesName.Value);

        if (!string.IsNullOrEmpty(seriesName.ValueSort))
          tag.Series.SeriesName.SortWith = seriesName.ValueSort;
      }

      if (!string.IsNullOrEmpty(seasonIndex.Value))
      {
        int index;
        if (int.TryParse(seasonIndex.Value, out index))
          tag.Series.SeasonIndex = index;
      }

      if (!string.IsNullOrEmpty(episodeIndexList.Text))
      {
        List<int> indexList = new List<int>();
        foreach (string s in episodeIndexList.Text.Split(','))
        {
          int index;
          if (int.TryParse(s, out index))
            indexList.Add(index);
        }

        if (indexList.Count > 0)
          tag.Series.EpisodeIndexList = indexList.AsReadOnly();
      }

      textEditorOriginal.Text = MatroskaLoader.GetXML(originalTag);
      textEditorNew.Text = MatroskaLoader.GetXML(tag);
    }
  }
}
