using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Matroska;
using MediaPortal.OnlineLibraries.TheTvDb;
using MediaPortal.OnlineLibraries.TheTvDb.Data;

namespace MkvTagger.DataSources
{
  public class TheTvDbImporter
  {
    private const string API_KEY = "91A89A984264307A";
    private TvdbHandler TVDB;

    public SeriesTag UpdateTags(SeriesTag seriesTag)
    {
      if (TVDB == null)
        TVDB = new TvdbHandler(API_KEY);

      string imdb = seriesTag.IMDB_ID;
      string name = seriesTag.SeriesName;
      if (string.IsNullOrEmpty(imdb) && string.IsNullOrEmpty(name))
      {
        MessageBox.Show("TvDb lookup needs atleast IMDB id or series name.");
        return seriesTag;
      }

      int iSeason;
      int iEpisode;
      if (!int.TryParse(seriesTag.SeasonIndex, out iSeason) ||
          !int.TryParse(seriesTag.EpisodeIndexList.FirstOrDefault(), out iEpisode))
      {
        MessageBox.Show("TvDb lookup needs season & episode index.");
        return seriesTag;
      }

      TvdbSearchResult searchResult;

      if (!string.IsNullOrEmpty(imdb))
        searchResult = SearchSeries(imdb, true);
      else
        searchResult = SearchSeries(name);

      if (searchResult == null)
        return seriesTag;

      TvdbSeries series = TVDB.GetFullSeries(searchResult.Id, App.Config.SelectedTvDbLanguage, false);
      if (series == null)
        return seriesTag;

      return CopySeriesInfos(seriesTag, series, iSeason, iEpisode);
    }

    private TvdbSearchResult SearchSeries(string idOrName, bool stringIsIMDB = false)
    {
      TvdbSearchResult result;
      if (stringIsIMDB)
      {
        try
        {
          result = TVDB.GetSeriesByRemoteId(ExternalId.ImdbId, idOrName);
          if (result == null)
            return null;
        }
        catch (Exception ex)
        {
          MessageBox.Show("An error occured: " + ex.Message);
          return null;
        }
      }
      else
      {
        try
        {
          var list = TVDB.SearchSeries(idOrName);
          if (list.Count == 0)
          {
            MessageBox.Show("nothing found");
            return null;
          }

          int iResult;
          if (list.Count > 1)
          {
            SearchResult w = new SearchResult();
            w.SetItemSource(list);
            w.ShowDialog();
            iResult = w.SelectedIndex;
          }
          else
            iResult = 0;

          if (iResult < 0)
            return null;

          result = list[iResult];
        }
        catch (Exception ex)
        {
          MessageBox.Show("An error occured: " + ex.Message);
          return null;
        }
      }

      return result;
    }

    private SeriesTag CopySeriesInfos(SeriesTag seriesTag, TvdbSeries series, int seasonIndex, int episodeIndex)
    {
      seriesTag.SeriesName = series.SeriesName;
      seriesTag.IMDB_ID = series.ImdbId;
      seriesTag.TVDB_ID = series.Id.ToString();
      // Do not overwrite the index
      //seriesTag.SeasonIndex
      //seriesTag.EpisodeIndexList

      seriesTag.Network = series.Network;
      seriesTag.Certification = series.ContentRating;
      seriesTag.SeriesFirstAired = series.FirstAired.ToString("yyyy-MM-dd");
      seriesTag.SeriesOverview = series.Overview;
      seriesTag.SeriesGenreList = series.Genre.AsReadOnly();
      seriesTag.SeriesActors = series.Actors.AsReadOnly();

      TvdbEpisode ep =
        series.Episodes.FirstOrDefault(e => e.SeasonNumber == seasonIndex && e.EpisodeNumber == episodeIndex);
      if (ep == null)
        return seriesTag;

      seriesTag.EpisodeFirstAired = ep.FirstAired.ToString("yyyy-MM-dd");
      seriesTag.EpisodeTitle = ep.EpisodeName;
      seriesTag.EpisodeIMDB_ID = ep.ImdbId;
      seriesTag.EpisodeOverview = ep.Overview;
      seriesTag.GuestStars = ep.GuestStars.AsReadOnly();
      seriesTag.Directors = ep.Directors.AsReadOnly();
      seriesTag.Writers = ep.Writer.AsReadOnly();

      return seriesTag;
    }

    public List<TvdbLanguage> GetAvailableLanguages()
    {
      if (TVDB == null)
        TVDB = new TvdbHandler(API_KEY);

      return TVDB.Languages;
    }
  }
}