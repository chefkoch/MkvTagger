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

namespace MPTvServies2MKV
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

    private void UpdatePreview(object sender, TextChangedEventArgs e)
    {
      MatroskaTag tag = new MatroskaTag();

      if (!string.IsNullOrEmpty(seriesNameTextBox.Text))
        tag.Series.SeriesName = seriesNameTextBox.Text;

      if (!string.IsNullOrEmpty(seriesSortNameTextBox.Text))
        tag.Series.SeriesNameSort = seriesSortNameTextBox.Text;

      if (!string.IsNullOrEmpty(seasonIndexTextBox.Text))
      {
        int seasonIndex;
        if (int.TryParse(seasonIndexTextBox.Text, out seasonIndex))
          tag.Series.SeasonIndex = seasonIndex;
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

      previewTextBox.Text = tag.ToString();
    }
  }
}
