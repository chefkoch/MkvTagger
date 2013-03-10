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
  /// Interaktionslogik für CustomSeriesTag.xaml
  /// </summary>
  public partial class CustomMusicVideoTag : UserControl
  {
    public CustomMusicVideoTag()
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
      UpdatePreview(null, null);
    }

    private void ClearGUI()
    {
      albumArtist.Clear();
      albumTitle.Clear();
      albumReleaseDate.Clear();

      trackArtist.Clear();
      trackTitle.Clear();
      trackNumber.Clear();
      trackReleaseDate.Clear();
      trackGenre.Clear();
    }

    private void UpdateGUI(MatroskaTags tags)
    {
      #region Album

      if (!ReferenceEquals(tags.MusicVideo.AlbumArtist, null))
        albumArtist.Value = tags.MusicVideo.AlbumArtist;

      if (!ReferenceEquals(tags.MusicVideo.AlbumTitle, null))
        albumTitle.Value = tags.MusicVideo.AlbumTitle;

      //if (!ReferenceEquals(tags.MusicVideo.AlbumReleaseDate, null))
      //  albumReleaseDate.Value = tags.MusicVideo.AlbumReleaseDate.Value;

      if (!ReferenceEquals(tags.MusicVideo.AlbumReleaseDate, null))
        albumReleaseDate.Value = tags.MusicVideo.AlbumReleaseDate;

      #endregion Album

      #region Track

      if (!ReferenceEquals(tags.MusicVideo.TrackArtist, null))
        trackArtist.Value = tags.MusicVideo.TrackArtist;

      if (!ReferenceEquals(tags.MusicVideo.TrackTitle, null))
        trackTitle.Value = tags.MusicVideo.TrackTitle;

      if (!ReferenceEquals(tags.MusicVideo.TrackReleaseDate, null))
        trackReleaseDate.Value = tags.MusicVideo.TrackReleaseDate;
      
      trackGenre.Value = tags.MusicVideo.GenreList;

      #endregion Track
    }

    private void UpdatePreview(object sender, TextChangedEventArgs e)
    {
      textEditorNew.Text = string.Empty;

      MatroskaTags tag;
      if (!ReferenceEquals(originalTag, null))
      {
        string xmlString = MatroskaLoader.GetXML(originalTag);
        tag = MatroskaLoader.ReadTagFromXML(xmlString);
      }
      else
        tag = new MatroskaTags();

      #region Album

      if (!string.IsNullOrEmpty(albumArtist.Value))
        tag.MusicVideo.AlbumArtist = albumArtist.Value;

      if (!string.IsNullOrEmpty(albumTitle.Value))
        tag.MusicVideo.AlbumTitle = albumTitle.Value;

      if (!string.IsNullOrEmpty(albumReleaseDate.Value))
        tag.MusicVideo.AlbumReleaseDate = albumReleaseDate.Value;

      //if (albumReleaseDate.Value.HasValue)
      //{
      //  tag.MusicVideo.AlbumReleaseDate = albumReleaseDate.Value.Value;
      //}

      #endregion Album

      #region Track

      if (!string.IsNullOrEmpty(trackArtist.Value))
        tag.MusicVideo.TrackArtist = trackArtist.Value;

      if (!string.IsNullOrEmpty(trackTitle.Value))
        tag.MusicVideo.TrackTitle = trackTitle.Value;

      if (!string.IsNullOrEmpty(trackNumber.Value))
      {
        int index;
        if (int.TryParse(trackNumber.Value, out index))
          tag.MusicVideo.TrackNumber = index;
      }

      if (!string.IsNullOrEmpty(trackReleaseDate.Value))
        tag.MusicVideo.TrackReleaseDate = trackReleaseDate.Value;

      if (trackGenre.Value.Count > 0)
        tag.MusicVideo.GenreList = trackGenre.Value;

      #endregion Track

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

    private void RenameButton_OnClick(object sender, RoutedEventArgs e)
    {
      string file = txtFilename.Text;
      if (string.IsNullOrWhiteSpace(file)) return;

      if (!File.Exists(file)) return;

      string folder = System.IO.Path.GetDirectoryName(file);
      string ext = System.IO.Path.GetExtension(file);
      if (string.IsNullOrWhiteSpace(ext)) return;

      if (ReferenceEquals(originalTag, null)) return;

      if (ReferenceEquals(originalTag.MusicVideo.TrackArtist, null))
      {
        MessageBox.Show("Track Artist is empty.");
        return;
      }

      if (ReferenceEquals(originalTag.MusicVideo.TrackTitle, null))
      {
        MessageBox.Show("Track Title is empty.");
        return;
      }

      string newFile;

      if (ReferenceEquals(originalTag.MusicVideo.TrackReleaseDate, null))
      {
        newFile = String.Format("{0} - {1}{2}",
                                     originalTag.MusicVideo.TrackArtist,
                                     originalTag.MusicVideo.TrackTitle,
                                     ext);
      }
      else
      {
        newFile = String.Format("{0} - {1} ({3}){2}",
                                     originalTag.MusicVideo.TrackArtist,
                                     originalTag.MusicVideo.TrackTitle,
                                     ext,
                                     originalTag.MusicVideo.TrackReleaseDate);
      }


      string newPath = System.IO.Path.Combine(folder, newFile);

      try
      {
        File.Move(file, newPath);

        string oldXml = System.IO.Path.ChangeExtension(file, ".xml");
        string newXml = System.IO.Path.ChangeExtension(newPath, ".xml");
        if (File.Exists(oldXml))
          File.Move(oldXml, newXml);
      }
      catch (Exception)
      {
        return;
      }

      SetFile(newPath);
    }

    private void refreshButton_OnClick(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrWhiteSpace(txtFilename.Text)) return;
      if (!File.Exists(txtFilename.Text)) return;

      SetFile(txtFilename.Text);
    }
  }
}
