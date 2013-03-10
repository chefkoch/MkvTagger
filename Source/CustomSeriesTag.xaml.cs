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
using MatroskaTagger.DataSources;

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
        UpdateGUI();
      }
      else
      {
        txtXmlFilename.Text = string.Empty;
        dockXml.Visibility = Visibility.Collapsed;
        txtFilename.Text = filepath;

        textEditorOriginal.Text = MatroskaLoader.GetXML(originalTag);
        UpdateGUI(originalTag.Series);
      }
      saveButton.IsEnabled = false;
    }

    private void ClearGUI()
    {
      seriesTitle.Clear();
      seriesIMDB.Clear();
      seasonIndex.Clear();
      episodeIndexList.Clear();
      episodeAired.Clear();

      seriesTitleSort.Clear();
      seriesFirstAired.Clear();
      seriesGenre.Clear();

      episodeTitle.Clear();
    }

    private void UpdateGUI(SeriesTag tag = null)
    {
      ClearGUI();
      if (ReferenceEquals(tag, null)) return;

      if (tag.HasSeriesName)
      {
        seriesTitle.Value = tag.SeriesName.StringValue;

        if (!string.IsNullOrEmpty(tag.SeriesName.SortWith))
          seriesTitleSort.Value = tag.SeriesName.SortWith;
      }

      if (!string.IsNullOrEmpty(tag.IMDB_ID))
        seriesIMDB.Value = tag.IMDB_ID;

      if (!ReferenceEquals(tag.SeasonIndex, null))
        seasonIndex.Value = tag.SeasonIndex.ToString();

      if (tag.EpisodeIndexList.Count > 0)
        episodeIndexList.Value = String.Join(",", tag.EpisodeIndexList);

      if (!string.IsNullOrEmpty(tag.EpisodeFirstAired))
        episodeAired.Value = tag.EpisodeFirstAired;


      if (!string.IsNullOrEmpty(tag.SeriesFirstAired))
        seriesFirstAired.Value = tag.SeriesFirstAired;
      seriesGenre.Value = tag.SeriesGenreList;
      if (!string.IsNullOrEmpty(tag.EpisodeTitle))
        episodeTitle.Value = tag.EpisodeTitle;

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

      tag.Series = UpdateTagFromGUI(tag.Series);

      textEditorNew.Text = MatroskaLoader.GetXML(tag);
      saveButton.IsEnabled = true;
    }

    private SeriesTag UpdateTagFromGUI(SeriesTag tag)
    {
      if (!string.IsNullOrEmpty(seriesTitle.Value))
      {
        tag.SetTitle(seriesTitle.Value);

        if (!string.IsNullOrEmpty(seriesTitleSort.Value))
          tag.SeriesName.SortWith = seriesTitleSort.Value;
      }

      if (!string.IsNullOrEmpty(seriesIMDB.Value))
        tag.IMDB_ID = seriesIMDB.Value;

      if (!string.IsNullOrEmpty(seasonIndex.Value))
      {
        int index;
        if (int.TryParse(seasonIndex.Value, out index))
          tag.SeasonIndex = index;
      }

      if (!string.IsNullOrEmpty(episodeIndexList.Value))
      {
        List<int> indexList = new List<int>();
        foreach (string s in episodeIndexList.Value.Split(','))
        {
          int index;
          if (int.TryParse(s, out index))
            indexList.Add(index);
        }

        if (indexList.Count > 0)
          tag.EpisodeIndexList = indexList.AsReadOnly();
      }

      if (!string.IsNullOrEmpty(episodeAired.Value))
        tag.EpisodeFirstAired = episodeAired.Value;

      tag.SeriesGenreList = seriesGenre.Value;
      tag.SeriesFirstAired = seriesFirstAired.Value;
      tag.EpisodeTitle = episodeTitle.Value;

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

    private void mpTVSeries_OnClick(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrWhiteSpace(txtFilename.Text)) return;
      if (!File.Exists(txtFilename.Text)) return;

      Episode ep = null;
      try
      {
        MPTVSeriesImporter i = new MPTVSeriesImporter();
        ep = i.GetEpisodeInfo(txtFilename.Text);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return;
      }
      
      if (ep == null)
        return;

      // recommend
      seriesTitle.Value = ep.SeriesInfo.Title;
      seriesIMDB.Value = ep.SeriesInfo.IMDB_ID;
      seasonIndex.Value = ep.SeasonIndex.ToString();
      episodeIndexList.Value = String.Join(",", ep.EpisodeIndexList);
      episodeAired.Value = ep.FirstAired;

      // addition series
      seriesTitleSort.Value = ep.SeriesInfo.TitleSort;
      seriesFirstAired.Value = ep.SeriesInfo.FirstAired;
      seriesGenre.Value = ep.SeriesInfo.Genre.AsReadOnly();

      // additional episode
      episodeTitle.Value = ep.EpisodeName;
    }
  }
}
