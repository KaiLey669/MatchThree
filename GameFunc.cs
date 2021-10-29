using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchThree
{
    public class GameFunc
    {
        int fieldWidth;
        int fieldHeight;
        int elemTypeCount;
        int score;
        int[,] matrix;

        public delegate void ElementRemoveHandler(int x, int y);
        public event ElementRemoveHandler ElementRemoved;

        public delegate void MatchesRemoveHandler();
        public event MatchesRemoveHandler MatchesRemoved;

        public delegate void ElementsFallHandler(List<Index> elements);
        public event ElementsFallHandler ElementsFalled;

        public GameFunc(int fieldWidth, int fieldHeight, int elemTypeCount)
        {
            this.fieldWidth = fieldWidth;
            this.fieldHeight = fieldHeight;
            this.elemTypeCount = elemTypeCount;
            matrix = new int[fieldWidth, fieldHeight];
            score = 0;
        }

        public void FillMatrix()
        {
            Random random = new Random();

            for (int i = 0; i < fieldHeight; i++)
            {
                for (int j = 0; j < fieldWidth; j++)
                {
                    bool checkRepeat;
                    int cellValue;

                    do
                    {
                        checkRepeat = false;
                        cellValue = (int)random.Next(0, elemTypeCount);

                        if (j >= 2 && (matrix[i, j - 2] == matrix[i, j - 1] && matrix[i, j - 1] == cellValue))
                            checkRepeat = true;
                        else if (i >= 2 && (matrix[i - 2, j] == matrix[i - 1, j] && matrix[i - 1, j] == cellValue))
                            checkRepeat = true;

                    } while (checkRepeat != false);

                    matrix[i, j] = cellValue;
                }
            }
        }

        public bool RemoveLine()
        {
            List<Line> lines = new List<Line>();
            Queue<Bonus> qBonuses = new Queue<Bonus>();
            int[,] tempMatrix = (int[,])matrix.Clone();
            int length;

            for (int i = 0; i < fieldHeight; i++)
            {
                for (int j = 0; j < fieldWidth; j++)
                {
                    if (tempMatrix[i, j] == -1)
                        continue;

                    length = 1;
                    for(int k = j + 1; k < fieldWidth; k++)
                    {
                        if (tempMatrix[i, j] % 5 == tempMatrix[i, k] % 5 )
                        {
                            length++;
                        }
                        else
                            break;
                    }

                    if (length >= 3)
                    {
                        for (int k = j; k < j + length; k++)
                        {
                            if(tempMatrix[i, k] >= 5 && tempMatrix[i, k] <= 9)
                                qBonuses.Enqueue(new Bomb(new Index(k, i)));
                            if (tempMatrix[i, k] >= 10 && tempMatrix[i, k] <= 14)
                                qBonuses.Enqueue(new LineDest(new Index(k, i), false));
                            if (tempMatrix[i, k] >= 15 && tempMatrix[i, k] <= 19)
                                qBonuses.Enqueue(new LineDest(new Index(k, i), true));
                            tempMatrix[i, k] = -1;
                        }
                            
                        lines.Add(new Line(new Index(j, i), new Index(j + length - 1, i)));
                    }
                }
            }

            tempMatrix = (int[,])matrix.Clone();
            for (int i = 0; i < fieldHeight; i++)
            {
                for (int j = 0; j < fieldWidth; j++)
                {
                    if (tempMatrix[i, j] == -1)
                        continue;

                    length = 1;
                    for (int k = i + 1; k < fieldHeight; k++)
                    {
                        if (tempMatrix[i, j] % 5 == tempMatrix[k, j] % 5) 
                        {
                            length++;
                        }
                        else
                            break;
                    }

                    if (length >= 3)
                    {
                        for (int k = i; k < i + length; k++)
                        {
                            if(tempMatrix[k, j] >= 5 && tempMatrix[k, j] <= 9)
                                qBonuses.Enqueue(new Bomb(new Index(j, k)));
                            if (tempMatrix[k, j] >= 10 && tempMatrix[k, j] <= 14)
                                qBonuses.Enqueue(new LineDest(new Index(j, k), false));
                            if (tempMatrix[k, j] >= 15 && tempMatrix[k, j] <= 19)
                                qBonuses.Enqueue(new LineDest(new Index(j, k), true));
                            tempMatrix[k, j] = -1;
                        }
                            
                        lines.Add(new Line(new Index(j, i), new Index(j, i + length - 1)));
                    }               
                }
            }

            if(lines.Count == 0)
                return false;

            int mult = 5;
            bool direction; //false - horizontal, true - vertical
            foreach (Line line in lines)
            {
                int count = 0;
                int colorNumber = matrix[line.Begin.I, line.Begin.J];
                if (line.Begin.I == line.End.I)
                {
                    for (int i = line.Begin.J; i <= line.End.J; ++i)
                    {
                        matrix[line.Begin.I, i] = -1;
                        if (ElementRemoved != null)
                            ElementRemoved(i, line.End.I);
                        count++;
                    }
                    //Bomb spawn
                    if (line.End.J - line.Begin.J + 1 >= 5)
                        matrix[line.Begin.I, line.Begin.J] = colorNumber + 5;
                    //LineDest spawn
                    if (line.End.J - line.Begin.J + 1 == 4)
                    {
                        Random random = new Random();
                        direction = Convert.ToBoolean((int)random.Next(0,1));
                        if(direction)
                            matrix[line.Begin.I, line.Begin.J] = colorNumber + 15;
                        else
                            matrix[line.Begin.I, line.Begin.J] = colorNumber + 10;
                    }
                }
                else
                {
                    for (int i = line.Begin.I; i <= line.End.I; ++i)
                    {
                        matrix[i, line.Begin.J] = -1;
                        if (ElementRemoved != null)
                            ElementRemoved(line.Begin.J, i);
                        count++;
                    }
                    //Bomb spawn
                    if (line.End.I - line.Begin.I + 1 >= 5)
                    {
                        matrix[line.Begin.I, line.Begin.J] = colorNumber + 5;
                    }
                    //LineDest spawn
                    if (line.End.I - line.Begin.I + 1 == 4)
                    {
                        Random random = new Random();
                        direction = Convert.ToBoolean((int)random.Next(0, 1));
                        if (direction)
                            matrix[line.Begin.I, line.Begin.J] = colorNumber + 15;
                        else
                            matrix[line.Begin.I, line.Begin.J] = colorNumber + 10;
                    }
                }
                    
                score += (count - 1) * mult;
            }

            while (qBonuses.Count != 0)
            {
                Bonus tempBonus = qBonuses.Dequeue();
                if (tempBonus is Bomb)
                {
                    for (int i = tempBonus.Center.I - 1; i <= tempBonus.Center.I + 1; i++)
                        for (int j = tempBonus.Center.J - 1; j <= tempBonus.Center.J + 1; j++)
                            if (i >= 0 && j >= 0)
                            {
                                if (matrix[i, j] >= 5 && matrix[i, j] <= 9)
                                    qBonuses.Enqueue(new Bomb(new Index(j,i)));
                                if (matrix[i, j] >= 10 && matrix[i, j] <= 14)
                                    qBonuses.Enqueue(new LineDest(new Index(j, i), false));
                                if (matrix[i, j] >= 15 && matrix[i, j] <= 19)
                                    qBonuses.Enqueue(new LineDest(new Index(j, i), true));
 
                                matrix[i, j] = -1;
                            }
                }

                if (tempBonus is LineDest)
                {
                    LineDest lineDest = (LineDest)tempBonus;
                    if (lineDest.IsVert)
                    {
                        for (int i = 0; i < fieldHeight; i++)
                        {
                            if (matrix[i, lineDest.Center.J] >= 5 && matrix[i, lineDest.Center.J] <= 9)
                                qBonuses.Enqueue(new Bomb(new Index(lineDest.Center.J, i)));
                            if (matrix[i, lineDest.Center.J] >= 10 && matrix[i, lineDest.Center.J] <= 14)
                                qBonuses.Enqueue(new LineDest(new Index(lineDest.Center.J, i), false));
                            if (matrix[i, lineDest.Center.J] >= 15 && matrix[i, lineDest.Center.J] <= 19)
                                qBonuses.Enqueue(new LineDest(new Index(lineDest.Center.J, i), true));

                            matrix[i, lineDest.Center.J] = -1;
                        }
                    }
                    else 
                    {
                        for (int j = 0; j < fieldWidth; j++)
                        {
                            if (matrix[lineDest.Center.I, j] >= 5 && matrix[lineDest.Center.I, j] <= 9)
                                qBonuses.Enqueue(new Bomb(new Index(j, lineDest.Center.I)));
                            if (matrix[lineDest.Center.I, j] >= 10 && matrix[lineDest.Center.I, j] <= 14)
                                qBonuses.Enqueue(new LineDest(new Index(j, lineDest.Center.I), false));
                            if (matrix[lineDest.Center.J, j] >= 15 && matrix[lineDest.Center.I, j] <= 19)
                                qBonuses.Enqueue(new LineDest(new Index(j, lineDest.Center.I), true));

                            matrix[lineDest.Center.I, j] = -1;
                        }
                    }
                }
            }
 
            if (MatchesRemoved != null)
                MatchesRemoved();

            return true;
        }

        public bool Fall()
        {
            List<Index> elements = new List<Index>();
            //сдвигаем каждый элемент вниз, если есть место
            for (int i = fieldHeight - 2; i >= 0; i--)
            {
                for (int j = fieldWidth - 1; j >= 0; j--)
                {
                    if (matrix[i + 1, j] == -1)
                    {
                        matrix[i + 1, j] = matrix[i, j];
                        matrix[i, j] = -1;
                        elements.Add(new Index(j, i + 1));
                    }
                }
            }
                
            //добавляем новые элементы сверху
            Random random = new Random();
            for (int i = 0; i < fieldWidth; i++)
            {
                if (matrix[0, i] == -1)
                {
                    matrix[0, i] = (int)random.Next(0, elemTypeCount);
                    elements.Add(new Index(i, 0));
                }
            }

            if (ElementsFalled != null && elements.Count != 0)
                ElementsFalled(elements);

            return elements.Count != 0;
        }

        public void Swap(Index a, Index b)
        {
            int temp = matrix[a.I, a.J];
            matrix[a.I, a.J] = matrix[b.I, b.J];
            matrix[b.I, b.J] = temp;
        }

        public int GetScore()
        {
            return score;
        }

        public int GetValue(int j, int i)
        {
            return matrix[i, j];
        }
    }

    public class Line
    {
        Index begin, end;

        public Line()
        {
            begin = new Index();
            end = new Index();
        }

        public Line(Index newBegin, Index newEnd)
        {
            begin = newBegin;
            end = newEnd;
        }

        public Index Begin 
        { 
            get { return begin; } 
            
            set { begin = new Index(value); } 
        }
        public Index End
        { 
            get { return end; } 
            
            set { end = new Index(value); } 
        }
    }

    abstract class Bonus
    { 
        public abstract Index Center { get; set; }  
        public bool IsVert { get; set; }
    }

    class Bomb : Bonus
    {
        Index center;

        public Bomb(Index newCenter)
        {
            center = newCenter;
        }

        public override Index Center
        {
            get { return center; }

            set { center = new Index(value); }
        }
    }

    class LineDest : Bonus
    {
        Index center;
        bool isVert;

        public LineDest(Index newCenter, bool direction)
        {
            center = newCenter;
            isVert = direction;
        }

        public override Index Center
        {
            get { return center; }

            set { center = new Index(value); }
        }

        public bool IsVert
        {
            get { return isVert; }

            set { isVert = value; }
        }
    }

    public class Index
    {
        int x, y;

        public Index()
        {
            x = 0;
            y = 0;
        }

        public Index(int j, int i)
        {
            x = j;
            y = i;
        }

        public Index(Index t)
        {
            x = t.x;
            y = t.y;
        }

        public int J 
        {
            get { return x; }

            set { x = value; }
        }

        public int I
        {
            get { return y; }
           
            set { y = value; }
        }

    }


}
