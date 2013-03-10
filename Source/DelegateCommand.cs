using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MatroskaTagger
{
  public class DelegateCommand : ICommand
  {
    public delegate void SimpleEventHandler(object sender, EventArgs e);

    private SimpleEventHandler _eventHandler;

    public DelegateCommand(SimpleEventHandler eventHandler)
    {
      _eventHandler = eventHandler;
    }

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public event EventHandler CanExecuteChanged;

    public void Execute(object parameter)
    {
      _eventHandler(this, new EventArgs());
    }
  }
}