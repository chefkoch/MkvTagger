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
      seriesName.Clear();
      seasonIndex.Clear();
#warning clear ep index
    }

    private void UpdateGUI(MatroskaTags tags)
    {
      if (tags.Series.HasSeriesName)
        seriesName.Value = tags.Series.SeriesName.StringValue;

      if (tags.Series.HasSeriesName && !string.IsNullOrEmpty(tags.Series.SeriesName.SortWith))
        seriesName.ValueSort = tags.Series.SeriesName.SortWith;

      if (!ReferenceEquals(tags.Series.SeasonIndex, null))
        seasonIndex.Value = tags.Series.SeasonIndex.ToString();

      if (tags.Series.EpisodeIndexList.Count > 0)
      {
        episodeIndexList.Text = String.Join(",", tags.Series.EpisodeIndexList);
      }
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

      textEditorNew.Text = MatroskaLoader.GetXML(tag);
      saveButton.IsEnabled = true;
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
  }
}
