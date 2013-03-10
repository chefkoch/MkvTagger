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

namespace MatroskaTagger
{
  /// <summary>
  /// Interaktionslogik für EditSimple.xaml
  /// </summary>
  public partial class EditSortWith : UserControl
  {
    public EditSortWith()
    {
      InitializeComponent();
    }

    #region Description

    public string Description
    {
      get { return group.Header as string; }
      set { group.Header = value; }
    }

    //public string Description
    //{
    //  get { return (string)GetValue(DescriptionProperty); }
    //  set { SetValue(DescriptionProperty, value); }
    //}

    //// Using a DependencyProperty as the backing store for Description.  This enables animation, styling, binding, etc...
    //public static readonly DependencyProperty DescriptionProperty =
    //    DependencyProperty.Register("Description", typeof(string), typeof(EditSortWith), new PropertyMetadata("Description: ", DescriptionChanged));

    //private static void DescriptionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    //{
    //  EditSimple editSimple = (EditSortWith)dependencyObject;
    //  editSimple.label.Content = dependencyPropertyChangedEventArgs.NewValue as string;
    //}

    #endregion Description

    #region Value

    public string Value
    {
      get { return textBox.Text; }
      set { textBox.Text = value; }
    }

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

    #region ValueSort

    public string ValueSort
    {
      get { return textBoxSort.Text; }
      set { textBoxSort.Text = value; }
    }

    #endregion ValueSort

    public void Clear()
    {
      Value = string.Empty;
      ValueSort = string.Empty;
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
      if (TextChanged != null)
        TextChanged(this, e);
    }

    public event TextChangedEventHandler TextChanged;
  }
}
