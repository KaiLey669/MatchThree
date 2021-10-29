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
    public partial class GameForm : Form
    {
        const int FieldSize = 8;

        public Form mainForm;
        Bitmap[] bitmap;
        VisualElem[,] elements;
        GameFunc game;
        Timer timer;
        int timerCount;
        int elemSize;
        bool gameForm_active = false;


        public GameForm()
        {
            InitializeComponent();
            
            //gamePictureBox.Width = 800;
            //gamePictureBox.Height = 800;
        }

        
        private void GameForm_Load(object sender, EventArgs e)
        {
            Paint += delegate {
                gamePictureBox.Refresh();
            };

            //gamePictureBox.BackgroundImage = new Bitmap(Properties.Resources.BackPicBox2, gamePictureBox.Width, gamePictureBox.Height);
            timer = new Timer();
            timer.Interval = 1000;
            timerCount = 60;
            timerLabel.Text = "TIMER - " + (timerCount - 60 * (timerCount / 60)).ToString("00");

            timer.Tick += delegate
            {
                if (timerCount > 0)
                {
                    timerCount--;
                    timerLabel.Text = "TIMER - " + (timerCount - 60 * (timerCount / 60)).ToString("00");
                }
                else if (gameForm_active)
                {
                    Finish();
                }

            };

            game = new GameFunc(FieldSize, FieldSize, Enum.GetValues(typeof(Elements)).Length);
            game.ElementRemoved += GameFunc_ElementRemoved;
            game.MatchesRemoved += GameFunc_MatchesRemoved;
            game.ElementsFalled += GameFunc_ElementsFalled;

            bitmap = new Bitmap[20];
            elements = new VisualElem[FieldSize, FieldSize];
            elemSize = Math.Min(gamePictureBox.Width, gamePictureBox.Height) / FieldSize;

            game.FillMatrix();
            gamePictureBox.Paint += gamePictureBox_Paint;

            InitBitmaps();

            for (int i = 0; i < FieldSize; i++)
            {
                for (int j = 0; j < FieldSize; j++)
                {
                    elements[i, j] = new VisualElem(this);
                    elements[i, j].Index = new Index(j, i);
                }
            }

            UpdateElem();

            scoreLabel.Text = "SCORE - " + game.GetScore().ToString();

            Active = true;

            timer.Start();
        }

        public enum Elements
        {
            first = 0,
            second = 1,
            third = 2,
            forth = 3,
            fifth = 4
        }

        public void InitBitmaps()
        {
            //Elements
            bitmap[0] = new Bitmap(Properties.Resources.first, elemSize, elemSize);
            bitmap[1] = new Bitmap(Properties.Resources.second, elemSize, elemSize);
            bitmap[2] = new Bitmap(Properties.Resources.third, elemSize, elemSize);
            bitmap[3] = new Bitmap(Properties.Resources.forth, elemSize, elemSize);
            bitmap[4] = new Bitmap(Properties.Resources.fifth, elemSize, elemSize);
            //Bombs
            bitmap[5] = new Bitmap(Properties.Resources.bomb_red, elemSize, elemSize);
            bitmap[6] = new Bitmap(Properties.Resources.bomb_brown, elemSize, elemSize);
            bitmap[7] = new Bitmap(Properties.Resources.bomb_violet, elemSize, elemSize);
            bitmap[8] = new Bitmap(Properties.Resources.bomb_black, elemSize, elemSize);
            bitmap[9] = new Bitmap(Properties.Resources.bomb_yellow, elemSize, elemSize);
            //Line horizontal
            bitmap[10] = new Bitmap(Properties.Resources.Line_red_hor, elemSize, elemSize);
            bitmap[11] = new Bitmap(Properties.Resources.Line_brown_hor, elemSize, elemSize);
            bitmap[12] = new Bitmap(Properties.Resources.Line_violet_hor, elemSize, elemSize);
            bitmap[13] = new Bitmap(Properties.Resources.Line_black_hor, elemSize, elemSize);
            bitmap[14] = new Bitmap(Properties.Resources.Line_yellow_hor, elemSize, elemSize);
            //Line vertical
            bitmap[15] = new Bitmap(Properties.Resources.Line_red_vert, elemSize, elemSize);
            bitmap[16] = new Bitmap(Properties.Resources.Line_brown_vert, elemSize, elemSize);
            bitmap[17] = new Bitmap(Properties.Resources.Line_violet_vert, elemSize, elemSize);
            bitmap[18] = new Bitmap(Properties.Resources.Line_black_vert, elemSize, elemSize);
            bitmap[19] = new Bitmap(Properties.Resources.Line_yellow_vert, elemSize, elemSize);
        }

        private void gamePictureBox_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < FieldSize; i++)
            {
                for (int j = 0; j < FieldSize; j++)
                {
                    VisualElem el = elements[i, j];
                    if (el.BackColor != Color.Transparent)
                    {
                        SolidBrush b = new SolidBrush(el.BackColor);
                        e.Graphics.FillRectangle(b, el.Rectangle);
                    }
                    if (el.Image != null)
                        e.Graphics.DrawImage(el.Image, el.Rectangle);
                }
            }
        }

        private void GameFunc_ElementRemoved(int j, int i)
        {
            elements[i, j].Image = null;
        }

        private void GameFunc_MatchesRemoved()
        {
            scoreLabel.Text = "SCORE - " + game.GetScore().ToString();
            game.Fall();
        }

        private void GameFunc_ElementsFalled(List<Index> indexes)
        {
            FallAnimation anim = new FallAnimation(this);
            List<VisualElem> elementsList = new List<VisualElem>();

            foreach (Index index in indexes)
                elementsList.Add(elements[index.I, index.J]);

            anim.AnimationEnd += delegate
            {
                if (game.Fall())
                {
                }
                else if (game.RemoveLine() == false)
                    Active = true;
            };

            UpdateElem();
            anim.Start(elementsList);
        }

        public void UpdateElem()
        {
            Point begin = new Point((gamePictureBox.Width - elemSize * FieldSize) / 2,
                                    (gamePictureBox.Height - elemSize * FieldSize) / 2);
            for (int i = 0; i < FieldSize; i++)
            {
                for (int j = 0; j < FieldSize; j++)
                {
                    elements[i, j].Location = new Point(begin.X + j * elemSize, begin.Y + i * elemSize);
                    elements[i, j].Size = new Size(elemSize, elemSize);
                    int value = game.GetValue(j, i);
                    if (value >= 0)
                        elements[i, j].Image = bitmap[game.GetValue(j, i)];
                    else
                        elements[i, j].Image = null;
                }
            }
        }

        public void MoveElements(VisualElem a, VisualElem b)
        {
            Index aInd = new Index(a.Index);
            Index bInd = new Index(b.Index);
            game.Swap(aInd, bInd);
            swapElements(aInd, bInd);

            bool result = game.RemoveLine();
            if (result == false)
            {
                SwapAnimation swap = new SwapAnimation(this);
                swap.AnimationEnd += delegate
                {
                    aInd = new Index(a.Index);
                    bInd = new Index(b.Index);
                    game.Swap(aInd, bInd);
                    swapElements(aInd, bInd);
                    Active = true;
                };
                swap.Start(b, a);
            }
        }

        private void swapElements(Index a, Index b)
        {
            VisualElem tempElem = elements[a.I, a.J];
            elements[a.I, a.J] = elements[b.I, b.J];
            elements[b.I, b.J] = tempElem;

            elements[a.I, a.J].Index = new Index(a.J, a.I);
            elements[b.I, b.J].Index = new Index(b.J, b.I);
        }

        public bool Active
        {
            get { return gameForm_active; }

            set
            {
                if (gameForm_active == false && value == true && timerCount <= 0)
                    Finish();
                else
                    gameForm_active = value;
            }
        }

        public void Finish()
        {
            gameForm_active = false;
            MessageBox.Show("Game Over! Your score is " + game.GetScore().ToString(), "The End", MessageBoxButtons.OK);
            Close();
        }

        private void gamePictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            Point begin = new Point((gamePictureBox.Width - elemSize * FieldSize) / 2,
                                    (gamePictureBox.Height - elemSize * FieldSize) / 2);
            Point pos = new Point(e.Location.X - begin.X, e.Location.Y - begin.Y);
            int col = pos.X / elemSize;
            int row = pos.Y / elemSize;
            if (col < FieldSize && row < FieldSize)
                elements[row, col].Click();
        }

        private void Game_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && gameForm_active)
            {
                timer.Dispose();
                Close();
            }
        }
    }

    public class VisualElem
    {
        Color color;
        Rectangle rectangle;
        Image image;
        GameForm game;
        Index index;

        static VisualElem checkElem = null;

        public Index Index
        {
            get { return index; }

            set { index = new Index(value); }
        }
        public Color BackColor 
        { 
            get { return color; } 
            
            set { color = value; } 
        }
        public Point Location 
        { 
            get { return rectangle.Location; } 
            
            set { rectangle = new Rectangle(value, rectangle.Size); } 
        }
        public Size Size 
        { 
            get { return rectangle.Size; } 
            
            set { rectangle = new Rectangle(rectangle.Location, value); } 
        }
        public Rectangle Rectangle 
        { 
            get { return rectangle; } 
            
            set { rectangle = value; } 
        }
        public Image Image 
        { 
            get { return image; } 
            
            set { image = value; } 
        }

        public VisualElem(GameForm f_game) : base()
        {
            game = f_game;
        }

        public void Click()
        {
            if (game.Active == false)
                return;

            if (checkElem != null)
            {
                if (checkElem == this)
                {
                    color = Color.Transparent;
                    checkElem = null;
                }
                else 
                {
                    bool near = false;

                    if ((checkElem.index.J == index.J - 1 && checkElem.index.I == index.I) ||
                        (checkElem.index.J == index.J + 1 && checkElem.index.I == index.I) ||
                        (checkElem.index.J == index.J && checkElem.index.I == index.I - 1) ||
                        (checkElem.index.J == index.J && checkElem.index.I == index.I + 1))
                    {
                        near = true;
                    }

                    if (!near)
                    {
                        checkElem.color = Color.Transparent;
                        color = Color.DarkSlateBlue; 
                        checkElem = this;
                    }
                    else
                    {
                        checkElem.color = Color.Transparent;
                        game.Active = false;
                        VisualElem el = checkElem;
                        checkElem = null;
                        SwapAnimation swap = new SwapAnimation(game);
                        swap.AnimationEnd += delegate
                        {
                            game.MoveElements(el, this);
                        };
                        swap.Start(el, this);
                    }
                }
            }
            else
            {
                color = Color.DarkSlateBlue; 
                checkElem = this;
            }
            game.Refresh();

        }        
    }
}
