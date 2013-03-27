using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
