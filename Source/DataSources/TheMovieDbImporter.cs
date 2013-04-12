using System;
using System.Linq;
using System.Windows;
using Matroska;
using MediaPortal.OnlineLibraries.TheMovieDb;
using MediaPortal.OnlineLibraries.TheMovieDb.Data;

namespace MkvTagger.DataSources
{
  public class TheMovieDbImporter
  {
    private const string API_KEY = "67f99724d25f922fbc3a8016b7b063f6";
    private MovieDbApiV3 movieDbApi;

    public MovieTag UpdateTags(MovieTag movieTag)
    {
      if (movieDbApi == null)
        movieDbApi = new MovieDbApiV3(API_KEY, null);

      Movie movie;

      string imdb = movieTag.IMDB_ID;
      if (!string.IsNullOrEmpty(imdb))
      {
        movie = movieDbApi.GetMovie(imdb, App.Config.SelectedTMDBLanguageValue);
        if (movie != null)
          return CopyMovieInfos(movieTag, movie);

        MessageBox.Show("TMDB lookup by IMDB id failed.");
      }

      string name = movieTag.Title;
      if (string.IsNullOrEmpty(imdb) && string.IsNullOrEmpty(name))
      {
        MessageBox.Show("TMDB lookup needs atleast IMDB id or movie title.");
        return movieTag;
      }

      MovieSearchResult searchResult = SearchMovie(name);

      if (searchResult == null)
        return movieTag;

      movie = movieDbApi.GetMovie(searchResult.Id, App.Config.SelectedTMDBLanguageValue);
      if (movie == null)
        return movieTag;

      return CopyMovieInfos(movieTag, movie);
    }

    private MovieSearchResult SearchMovie(string title)
    {
      MovieSearchResult result;
      try
      {
        var list = movieDbApi.SearchMovie(title, App.Config.SelectedTMDBLanguageValue);
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

      return result;
    }

    private MovieTag CopyMovieInfos(MovieTag movieTag, Movie movie)
    {
      movieTag.Title = movie.Title;
      movieTag.IMDB_ID = movie.ImdbId;
      movieTag.TMDB_ID = movie.Id.ToString();

      MovieCollection collection = movie.Collection;
      movieTag.CollectionTitle = collection != null ? collection.Name : null;

      movieTag.ReleaseDate = movie.ReleaseDate.HasValue ? movie.ReleaseDate.Value.ToString("yyyy-MM-dd") : null;
      movieTag.Overview = movie.Overview;
      movieTag.Tagline = movie.Tagline;

      //todo: implement certification
      //movieTag.Certification = movie.

      movieTag.Genres = movie.Genres.Select(g => g.Name).ToList().AsReadOnly();


      MovieCasts casts = movieDbApi.GetCastCrew(movie.Id);
      if (casts == null)
        return movieTag;

      movieTag.Actors = casts.Cast.Select(p => p.Name).ToList().AsReadOnly();
      movieTag.Directors = casts.Crew.Where(p => p.Job == "Director").Select(p => p.Name).ToList().AsReadOnly();
      movieTag.Writers = casts.Crew.Where(p => p.Job == "Author").Select(p => p.Name).ToList().AsReadOnly();

      return movieTag;
    }
  }
}