using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Matroska;
using MkvTagger.DataSources;

namespace MkvTagger
{
  /// <summary>
  /// Interaktionslogik für CustomSeriesTag.xaml
  /// </summary>
  public partial class CustomSeriesTag : UserControl
  {
    private MatroskaTags originalTag;

    public CustomSeriesTag()
    {
      InitializeComponent();
    }

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
      // Recommended for identification
      seriesTitle.Clear();
      seriesIMDB.Clear();
      seriesTVDB.Clear();
      seasonIndex.Clear();
      episodeIndexList.Clear();
      episodeAired.Clear();

      // additional series infos
      seriesFirstAired.Clear();
      seriesSummary.Clear();
      seriesGenre.Clear();
      seriesActors.Clear();
      Certification.Clear();
      Network.Clear();
      seriesKeywords.Clear();
      // additional episode infos
      episodeIMDB.Clear();
      episodeTitle.Clear();
      episodeSummary.Clear();
      GuestStars.Clear();
      Directors.Clear();
      Writers.Clear();
      episodeKeywords.Clear();
    }

    private void UpdateGUI(SeriesTag tag = null)
    {
      ClearGUI();
      if (ReferenceEquals(tag, null)) return;

      // Recommended for identification
      seriesTitle.Value = tag.SeriesName;
      seriesIMDB.Value = tag.IMDB_ID;
      seriesTVDB.Value = tag.TVDB_ID;
      seasonIndex.Value = tag.SeasonIndex;
      episodeIndexList.Value = tag.EpisodeIndexList;
      episodeAired.Value = tag.EpisodeFirstAired;

      // additional series infos
      seriesFirstAired.Value = tag.SeriesFirstAired;
      seriesSummary.Value = tag.SeriesOverview;
      seriesGenre.Value = tag.SeriesGenreList;
      seriesActors.Value = tag.SeriesActors;
      Certification.Value = tag.Certification;
      Network.Value = tag.Network;
      seriesKeywords.Value = tag.SeriesKeywords;
      // additional episode infos
      episodeIMDB.Value = tag.EpisodeIMDB_ID;
      episodeTitle.Value = tag.EpisodeTitle;
      episodeSummary.Value = tag.EpisodeOverview;
      GuestStars.Value = tag.GuestStars;
      Directors.Value = tag.Directors;
      Writers.Value = tag.Writers;
      episodeKeywords.Value = tag.EpisodeKeywords;
    }

    private void UpdatePreview(object sender, TextChangedEventArgs e)
    {
      MatroskaTags tag = MatroskaLoader.Clone(originalTag);

      tag.Series = UpdateTagFromGUI(tag.Series);
      tag.Cleanup();

      textEditorNew.Text = MatroskaLoader.GetXML(tag);
      saveButton.IsEnabled = true;
    }

    private SeriesTag UpdateTagFromGUI(SeriesTag tag)
    {
      // Recommended for identification
      tag.SeriesName = seriesTitle.Value;
      tag.IMDB_ID = seriesIMDB.Value;
      tag.TVDB_ID = seriesTVDB.Value;
      tag.SeasonIndex = seasonIndex.Value;
      tag.EpisodeIndexList = episodeIndexList.Value;
      tag.EpisodeFirstAired = episodeAired.Value;

      // additional series infos
      tag.SeriesFirstAired = seriesFirstAired.Value;
      tag.SeriesOverview = seriesSummary.Value;
      tag.SeriesGenreList = seriesGenre.Value;
      tag.SeriesActors = seriesActors.Value;
      tag.Certification = Certification.Value;
      tag.Network = Network.Value;
      tag.SeriesKeywords = seriesKeywords.Value;
      // additional episode infos
      tag.EpisodeIMDB_ID = episodeIMDB.Value;
      tag.EpisodeTitle = episodeTitle.Value;
      tag.EpisodeOverview = episodeSummary.Value;
      tag.GuestStars = GuestStars.Value;
      tag.Directors = Directors.Value;
      tag.Writers = Writers.Value;
      tag.EpisodeKeywords = episodeKeywords.Value;

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

      // Use original tags as base with latest changes from GUI
      MatroskaTags tag = MatroskaLoader.Clone(originalTag);
      tag.Series = UpdateTagFromGUI(tag.Series);

      // Update from TVSeries
      try
      {
        MPTVSeriesImporter importer = new MPTVSeriesImporter();
        tag.Series = importer.UpdateTags(tag.Series, txtFilename.Text);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return;
      }

      UpdateGUI(tag.Series);
    }

    private void tvdb_OnClick(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrWhiteSpace(txtFilename.Text)) return;
      if (!File.Exists(txtFilename.Text)) return;

      // Use original tags as base with latest changes from GUI
      MatroskaTags tag = MatroskaLoader.Clone(originalTag);
      tag.Series = UpdateTagFromGUI(tag.Series);

      // Update from TVSeries
      try
      {
        TheTvDbImporter importer = new TheTvDbImporter();
        tag.Series = importer.UpdateTags(tag.Series);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return;
      }

      UpdateGUI(tag.Series);
    }
  }
}