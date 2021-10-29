using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MatchThree
{
    public abstract class Animation
    {
        protected Timer timer;
        protected GameForm game;
        protected int time;
        protected int steps;
        protected int count;

        public delegate void AnimationEndHandler();

        public Animation(GameForm game)
        {
            this.game = game;
            time = 250;
            steps = 12;
            count = 0;
        }
    }

    public class SwapAnimation : Animation
    {
        public SwapAnimation(GameForm game) : base(game)
        {
            steps = 10;
        }

        public void Start(VisualElem a, VisualElem b)
        {
            Point distance = new Point(b.Location.X - a.Location.X, b.Location.Y - a.Location.Y);
            Point speed = new Point(distance.X / steps, distance.Y / steps);
            int last = steps - 1;
            count = 0;

            timer = new Timer();
            timer.Interval = time / steps;
            timer.Tick += delegate
            {
                if (count > last)
                {
                    timer.Dispose();
                    return;
                }
                if (count < last)
                {
                    a.Location = new Point(a.Location.X + speed.X, a.Location.Y + speed.Y);
                    b.Location = new Point(b.Location.X - speed.X, b.Location.Y - speed.Y);
                }
                else
                {
                    a.Location = new Point(a.Location.X + (distance.X - speed.X * last), a.Location.Y + (distance.Y - speed.Y * last));
                    b.Location = new Point(b.Location.X - (distance.X - speed.X * last), b.Location.Y - (distance.Y - speed.Y * last));
                }
                count++;
                game.Refresh();
            };

            timer.Disposed += delegate
            {
                if (AnimationEnd != null)
                    AnimationEnd();
            };

            timer.Start();
        }

        public event AnimationEndHandler AnimationEnd = null;
    }

    public class FallAnimation : Animation
    {
        public FallAnimation(GameForm game) : base(game)
        {
            time = 100;
        }

        public void Start(List<VisualElem> elements)
        {
            int distance = elements.First().Size.Height;
            int speed = distance / steps;

            for (int j = 0; j < elements.Count; ++j)
            {
                VisualElem element = elements[j];
                element.Location = new Point(element.Location.X, element.Location.Y - distance);
            }
            int last = steps - 1;
            count = 0;
            timer = new Timer();
            timer.Interval = time / steps;
            timer.Tick += delegate
            {
                if (count > last)
                {
                    timer.Dispose();
                    return;
                }
                if (count < last)
                {
                    for (int j = 0; j < elements.Count; ++j)
                    {
                        VisualElem element = elements[j];
                        element.Location = new Point(element.Location.X, element.Location.Y + speed);
                    }
                }
                else
                {
                    int delta = distance - speed * last;
                    for (int j = 0; j < elements.Count; ++j)
                    {
                        VisualElem element = elements[j];
                        element.Location = new Point(element.Location.X, element.Location.Y + delta);
                    }
                }
                count++;
                game.Refresh();
            };

            timer.Disposed += delegate
            {
                if (AnimationEnd != null)
                    AnimationEnd();
            };

            game.Refresh();

            timer.Start();
        }

        public event AnimationEndHandler AnimationEnd = null;
    }
}
