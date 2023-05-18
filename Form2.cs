using Sapper.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sapper
{
    public partial class FormSettings : Form
    {
        public static int newMapSize = 10;      // По умолчанию размер равен 10
        public FormSettings()
        {
            InitializeComponent();
        }

        private void FormSettings_Load(object sender, EventArgs e)      
        {
            numericUpDownMapSize.Value = MapController.mapSize; // При загрузке в настройках в размере число 10
        }

        // При применении настроек меняем размер поля
        private void buttonOK_Click(object sender, EventArgs e)     
        {
            newMapSize = Convert.ToInt32(numericUpDownMapSize.Value);
            MapController.GetMapSize();
            MapController.form.Controls.Clear();
            MapController.Init(MapController.form);
            this.Close();
        }
    }
}
