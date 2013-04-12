using System.Collections.Generic;

namespace MkvTagger
{
  internal class Consts
  {
    public const string KeySeriesTitle = "SeriesTitle";
    public const string KeySeriesImdb = "SeriesIMDB";
    public const string KeySeriesSeasonIndex = "SeriesSeasonIndex";
    public const string KeySeriesEpisodeIndizes = "SeriesEpisodeIndizes";
    public const string KeySeriesEpisodeFirstAired = "SeriesEpisodeFirstAired";

    public const string KeySeriesTitleSort = "SeriesTitleSort";
    public const string KeySeriesFirstAired = "SeriesFirstAired";
    public const string KeySeriesGenre = "SeriesGenre";

    public const string KeySeriesEpisodeTitle = "SeriesEpisodeTitle";
    public const string KeySeriesEpisodeSummary = "SeriesEpisodeSummary";
    public const string KeySeriesEpisodeGuestStars = "SeriesEpisodeGuestStars";
    public const string KeySeriesEpisodeDirectors = "SeriesEpisodeDirectors";
    public const string KeySeriesEpisodeWriters = "SeriesEpisodeWriters";

    public static List<TagSetting> SeriesTags = new List<TagSetting>
      {
        new TagSetting(KeySeriesTitle),
        new TagSetting(KeySeriesImdb),
        new TagSetting(KeySeriesSeasonIndex),
        new TagSetting(KeySeriesEpisodeIndizes),
        new TagSetting(KeySeriesEpisodeFirstAired),
        new TagSetting(KeySeriesTitleSort),
        new TagSetting(KeySeriesFirstAired),
        new TagSetting(KeySeriesGenre),
        new TagSetting(KeySeriesEpisodeTitle)
      };
  }
}