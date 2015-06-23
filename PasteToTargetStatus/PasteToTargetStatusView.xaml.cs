using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PasteToTargetStatus
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class PasteToTargetStatusView: UserControl
    {
        public PasteToTargetStatusView()
        {
            InitializeComponent();
        }

        public event EventHandler Cancel;

        public string Status
        {
            get { return txtStatus.Text; }
            set { txtStatus.Text = value;  }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Cancel != null)
                Cancel(this, new EventArgs());
        }
    }
}
