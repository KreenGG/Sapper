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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            MapController.GetMapSize();
            MapController.Init(this); 
            MapController.buttonSettings.Click += new EventHandler(настройкиToolStripMenuItem_Click); // Обработчкик нажатия на "Настройки"
        }

        public static void настройкиToolStripMenuItem_Click(object sender, EventArgs e)     // Нажатие на настройки вызывает форму с настройками
        {
            FormSettings formSettings = new FormSettings();
            formSettings.Show();
        }
    }
}
