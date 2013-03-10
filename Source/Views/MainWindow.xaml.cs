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

namespace video2brain.DasDataGridSteuerelement
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Person> Persons { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Persons = new List<Person>();

            Person personA = new Person();
            personA.Company = "impuls Informationsmanagement GmbH";
            personA.Name = "Ralf Bußmann";

            Person personB = new Person();
            personB.Company = "impuls Informationsmanagement GmbH";
            personB.Name = "Timo Winter";

            Person personC = new Person();
            personC.Company = "impuls Informationsmanagement GmbH";
            personC.Name = "Gregor Biswanger";

            Persons.Add(personA);
            Persons.Add(personB);
            Persons.Add(personC);

            DataContext = this;
        }
    }
}
