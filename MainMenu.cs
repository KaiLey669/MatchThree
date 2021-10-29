using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MatchThree
{
    public partial class MainMenu : Form
    {
        public MainMenu()
        {
            InitializeComponent();
            this.BackgroundImage = new Bitmap(Properties.Resources.Back, this.Width, this.Height);
        }


        private void playButton_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            GameForm GameForm = new GameForm();
            GameForm.FormClosed += delegate { this.Show(); };
            GameForm.Show();
            Hide();
        }
    }


}
