using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace MatroskaTagger
{
  /// <summary>
  /// Interaktionslogik für EditSimple.xaml
  /// </summary>
  public partial class EditStringList : UserControl
  {
    public EditStringList()
    {
      InitializeComponent();
    }

    #region Description

    public string Description
    {
      get { return label.Content as string; }
      set { label.Content = value; }
    }

    //public string Description
    //{
    //  get { return (string)GetValue(DescriptionProperty); }
    //  set { SetValue(DescriptionProperty, value); }
    //}

    //// Using a DependencyProperty as the backing store for Description.  This enables animation, styling, binding, etc...
    //public static readonly DependencyProperty DescriptionProperty =
    //    DependencyProperty.Register("Description", typeof(string), typeof(EditSimple), new PropertyMetadata("Description: ", DescriptionChanged));

    //private static void DescriptionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    //{
    //  EditSimple editSimple = (EditSimple)dependencyObject;
    //  editSimple.label.Content = dependencyPropertyChangedEventArgs.NewValue as string;
    //}

    #endregion Description

    #region Value

    public ReadOnlyCollection<string> Value
    {
      get
      {
        List<string> result = new List<string>();

        foreach (object item in list.Items)
          result.Add(item as string);

        return result.AsReadOnly();
      }
      set
      {
        list.Items.Clear();

        if (!ReferenceEquals(value, null))
          foreach (string s in value)
            if (!string.IsNullOrWhiteSpace(s))
              list.Items.Add(s);

        // report change back
        if (TextChanged != null)
          TextChanged(this, null);
      }
    }

    //public DateTime Value2
    //{
    //  get { return date.sel; }
    //  set { textBox.Text = value; }
    //}

    //public string Value
    //{
    //  get { return (string)GetValue(ValueProperty); }
    //  set { SetValue(ValueProperty, value); }
    //}

    //// Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
    //public static readonly DependencyProperty ValueProperty =
    //    DependencyProperty.Register("Value", typeof(string), typeof(EditSimple), new PropertyMetadata("Value", ValueChanged));

    //private static void ValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    //{
    //  EditSimple editSimple = (EditSimple)dependencyObject;
    //  editSimple.textBox.Text = dependencyPropertyChangedEventArgs.NewValue as string;
    //}

    #endregion Value

    public void Clear()
    {
      Value = new List<string>().AsReadOnly();
    }

    public event TextChangedEventHandler TextChanged;

    private void add_Click(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrWhiteSpace(edit.Text)) return;

      list.Items.Add(edit.Text);

      // report change back
      if (TextChanged != null)
        TextChanged(this, null);
    }

    private void remove_Click(object sender, RoutedEventArgs e)
    {
      if (ReferenceEquals(list.SelectedItem, null)) return;

      list.Items.Remove(list.SelectedItem);

      // report change back
      if (TextChanged != null)
        TextChanged(this, null);
    }

    private void up_Click(object sender, RoutedEventArgs e)
    {
      if (ReferenceEquals(list.SelectedItem, null)) return;

      int index = list.SelectedIndex;
      if (index == 0) return;

      string item = list.SelectedItem as string;
      list.Items.RemoveAt(index);
      list.Items.Insert(index - 1, item);

      // report change back
      if (TextChanged != null)
        TextChanged(this, null);
    }

    private void down_Click(object sender, RoutedEventArgs e)
    {
      if (ReferenceEquals(list.SelectedItem, null)) return;

      int index = list.SelectedIndex;
      if (index == list.Items.Count - 1) return;

      string item = list.SelectedItem as string;
      list.Items.RemoveAt(index);
      list.Items.Insert(index + 1, item);

      // report change back
      if (TextChanged != null)
        TextChanged(this, null);
    }
  }
}
