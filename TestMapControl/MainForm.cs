using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using sControl.Map;

namespace TestMapControl
{
    public partial class MainForm : Form
    {
        sMapControl mapControl = new sMapControl("..\\MapData\\") { Dock = DockStyle.Fill };
        public MainForm()
        {
            InitializeComponent();

            this.Controls.Add(mapControl);
        }
    }
}
