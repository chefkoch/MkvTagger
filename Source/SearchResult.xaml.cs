using System.Collections;
using System.Windows;

namespace MkvTagger
{
  /// <summary>
  /// Interaktionslogik für SearchResult.xaml
  /// </summary>
  public partial class SearchResult : Window
  {
    public SearchResult()
    {
      InitializeComponent();
    }

    public int SelectedIndex { get; set; }

    public void SetItemSource<TE>(TE searchResults)
    {
      _grid.ItemsSource = searchResults as IEnumerable;
      SelectedIndex = -1;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      SelectedIndex = _grid.SelectedIndex;
      this.Close();
    }
  }
}