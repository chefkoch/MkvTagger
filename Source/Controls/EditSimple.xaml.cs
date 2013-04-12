using System.Windows.Controls;

namespace MkvTagger.Controls
{
  /// <summary>
  /// Interaktionslogik für EditSimple.xaml
  /// </summary>
  public partial class EditSimple : UserControl
  {
    public EditSimple()
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

    public string Value
    {
      get { return textBox.Text; }
      set
      {
        if (string.IsNullOrWhiteSpace(value))
          textBox.Text = string.Empty;
        else
          textBox.Text = value;
      }
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

    public void Clear()
    {
      Value = string.Empty;
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
      if (TextChanged != null)
        TextChanged(this, e);
    }

    public event TextChangedEventHandler TextChanged;
  }
}