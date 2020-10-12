using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinesweeperBot
{
    public partial class Form1 : Form   
    {
        Graphics graphics;
        private Board board;
        private bool firstMove;
        private bool failure;
        private Point failurePoint;
        private int minesLeft;
        private int freeFieldsCount;

        private Dictionary<int, string> imageDict;
        public Form1()
        {            
            InitializeComponent();
            pictureBox1.Height = Params.rows * Params.squareSize;
            pictureBox1.Width = Params.columns * Params.squareSize;
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            graphics = Graphics.FromImage(pictureBox1.Image);
            Reset();

            imageDict = new Dictionary<int, string>();
            imageDict.Add(-1, "mine");
            imageDict.Add(0, "zero");
            imageDict.Add(1, "one");
            imageDict.Add(2, "two");
            imageDict.Add(3, "three");
            imageDict.Add(4, "four");
            imageDict.Add(5, "five");
            imageDict.Add(6, "six");
            imageDict.Add(7, "seven");
            imageDict.Add(8, "eight");

            button1.Click += StartGame;
        }

        #region Game
        private void StartGame(object sender, EventArgs e)
        {
            Reset();
            while (!failure)
            {
                if (firstMove)
                {
                    MakeFirstMove();
                    if(failure) return;
                    firstMove = false;
                }
                else
                {
                    if (failure) break;
                    TraverseBoard();
                    RenderBoard();
                }
                RenderBoard();
                if (IsGameFinished()) break;
            }
            RenderBoard();
            if (failure) label3.Text = "Failure!";
            else label3.Text = "Success!";
        }
        private void MakeFirstMove()
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            int x = rand.Next(Params.columns);
            int y = rand.Next(Params.rows);
            LeftClickField(x,y);
            if (board.Fields[x, y].HasMine) failure = true;
        }

        private void LeftClickField(int x, int y)
        {
            if (board.Fields[x, y].Clicked) return;
            if (board.Fields[x, y].Number == 0)
            {
                UncoverArea(x, y);
                return;
            }
            board.Fields[x, y].Clicked = true;
            --freeFieldsCount;
            if (board.Fields[x, y].HasMine)
            {
                //board.Fields[x, y].Clicked = false;
                failure = true;
                failurePoint = new Point(x, y);
            }
        }

        private void UncoverArea(int x, int y)
        {
            Stack<Point> s = new Stack<Point>();
            s.Push(new Point(x, y));
            while(s.Count > 0)
            {
                Point currentPoint = s.Pop();
                board.Fields[currentPoint.X, currentPoint.Y].Clicked = true;
                if (board.Fields[currentPoint.X, currentPoint.Y].Number > 0) continue;
                for(int i = -1; i <= 1; ++i)
                {
                    for(int j = -1; j <= 1; ++j)
                    {
                        if (i == 0 && j == 0) continue;
                        int neighborX = currentPoint.X + i;
                        int neighborY = currentPoint.Y + j;
                        if (!Helpers.IsInBounds(neighborX, neighborY)) continue;
                        if (board.Fields[neighborX, neighborY].Clicked) continue;
                        s.Push(new Point(neighborX, neighborY));
                    }
                }
            }
            freeFieldsCount = GetFreeFields(false).Count();
        }

        private void TraverseBoard()
        {
            bool change = FillCertainNeighborhoods();
            if (!change) Guess();
        }

        public bool FillCertainNeighborhoods()
        {
            bool change = false;
            for (int i = 0; i < Params.columns; ++i)
            {
                for (int j = 0; j < Params.rows; ++j)
                {
                    if (!board.Fields[i, j].Clicked) continue;
                    if (board.Fields[i, j].Flag) continue;

                    Neighborhood neighborhood = board.GetNeighborhood(i, j);
                    if (neighborhood.FreeNeighbors.Count == 0) continue;

                    bool allFlagsPlanted = neighborhood.FlagsCount == board.Fields[i, j].Number;
                    if (allFlagsPlanted)
                    {
                        foreach (var neighbor in neighborhood.FreeNeighbors)
                        {
                            LeftClickField(neighbor.X, neighbor.Y);
                            if (failure) return true;
                        }
                    change = true;
                    }

                    bool allNeigborsShouldBeFlags =
                        neighborhood.FreeNeighbors.Count == board.Fields[i, j].Number - neighborhood.FlagsCount;
                    if (allNeigborsShouldBeFlags)
                    {
                        foreach (var neighbor in neighborhood.FreeNeighbors)
                            PlantFlag(neighbor.X, neighbor.Y);
                        change = true;
                    }
                }
            }
            return change;
        }

        public void Guess()
        {
            float bestChance = 0;
            Point bestPoint = new Point(-1, -1);
            bool bestIsNumber = false;
            for (int i = 0; i < Params.columns; ++i)
            {
                for (int j = 0; j < Params.rows; ++j)
                {
                    if (!board.Fields[i, j].Clicked) continue;
                    if (board.Fields[i, j].Flag) continue;

                    Neighborhood neighborhood = board.GetNeighborhood(i, j);
                    if (neighborhood.FreeNeighbors.Count == 0) continue;

                    int flagsRemaining = board.Fields[i, j].Number - neighborhood.FlagsCount;
                    List<int[]> combinations =
                        Helpers.GetAllSubsets(neighborhood.FreeNeighbors.Count, flagsRemaining);
                    int[] possibleFlagPlacementCombinationCount = new int[neighborhood.FreeNeighbors.Count];
                    bool[] canPlantFlagOnIndex = new bool[neighborhood.FreeNeighbors.Count];

                    List<int[]> legalCombinations = new List<int[]>();

                    foreach(var comb in combinations)
                    {
                        bool isLegal = true;
                        List<NewFlagsCount> numbersNearNewFlags = new List<NewFlagsCount>();
                        foreach(var ind in comb)
                        {
                            int flagX = neighborhood.FreeNeighbors[ind].X;
                            int flagY = neighborhood.FreeNeighbors[ind].Y;
                            List<Point> flagNeighborhood = board.GetNumbersAroundFlag(flagX, flagY);

                            foreach(var numberNearFlag in flagNeighborhood)
                            {
                                NewFlagsCount number = numbersNearNewFlags.Find(n => n.Coords == numberNearFlag);
                                if (number == null)
                                    numbersNearNewFlags.Add(new NewFlagsCount(numberNearFlag));
                                else ++number.Count;
                            }
                        }
                        foreach(var number in numbersNearNewFlags) 
                        {
                            int x = number.Coords.X;
                            int y = number.Coords.Y;
                            int flagCount = board.GetNeighborhood(x, y).FlagsCount;
                            if(flagCount + number.Count > board.Fields[x,y].Number)
                            {
                                isLegal = false;
                                break;
                            }
                        }
                        if (!isLegal) continue;
                        legalCombinations.Add(comb);
                    }
                    int[] combinationsWithFlagCount = new int[neighborhood.FreeNeighbors.Count];
                    foreach(var legalComb in legalCombinations)
                    {
                        foreach (var num in legalComb)
                            ++combinationsWithFlagCount[num];
                    }
                    for(int k = 0; k < combinationsWithFlagCount.Length; ++k)
                    {
                        float chance = (float)combinationsWithFlagCount[k] / legalCombinations.Count; 
                        if(chance > bestChance)
                        {
                            bestChance = chance;
                            bestIsNumber = false;
                            bestPoint = neighborhood.FreeNeighbors[k];
                        }
                        if(1.0f - chance > bestChance)
                        {
                            bestChance = 1.0f - chance;
                            bestIsNumber = true;
                            bestPoint = neighborhood.FreeNeighbors[k];
                        }
                        if (Math.Abs(bestChance - 1.0f) < 0.0000001f)
                        {
                            break;
                        }
                    }
                    if (Math.Abs(bestChance - 1.0f) < 0.0000001f) break;
                }
                if (Math.Abs(bestChance - 1.0f) < 0.0000001f) break;
            }
            List<Point> freeFields = new List<Point>();
            if (bestPoint.X == -1 && bestPoint.Y == -1) freeFields = GetFreeFields(false);
            else freeFields = GetFreeFields(true);

            float chanceForRandomField = 1.0f - ((float)minesLeft / freeFieldsCount);
            if(chanceForRandomField > bestChance)
            {
                bestIsNumber = true;
                Random rand = new Random(Guid.NewGuid().GetHashCode());
                int ind = rand.Next(freeFields.Count);
                bestPoint = freeFields[ind];
            }
            if (bestIsNumber) LeftClickField(bestPoint.X, bestPoint.Y);
            else PlantFlag(bestPoint.X, bestPoint.Y);
        }

        private List<Point> GetFreeFields(bool checkNeighborhood)
        {
            List<Point> freeFields = new List<Point>();
            for (int i = 0; i < Params.columns; ++i)
            {
                for (int j = 0; j < Params.rows; ++j)
                {
                    if (board.Fields[i, j].Clicked) continue;
                    if (checkNeighborhood)
                    {
                        Neighborhood neighborhood = board.GetNeighborhood(i, j);
                        bool corner = (i == 0 && j == 0) ||
                            (i == Params.columns - 1 && j == 0) ||
                            (i == 0 && j == Params.rows - 1) ||
                            (i == Params.columns - 1 && j == Params.rows - 1);
                        bool border = i == 0 ||
                            i == Params.columns - 1 ||
                            j == 0 ||
                            j == Params.rows - 1;
                        if (corner)
                            if (neighborhood.FreeNeighbors.Count != 3) continue;
                        if(!corner && border)
                            if (neighborhood.FreeNeighbors.Count != 5) continue;
                        if(!corner && !border)
                            if(neighborhood.FreeNeighbors.Count != 8) continue;
                    }
                    freeFields.Add(new Point(i,j));
                }
            }
            return freeFields;
        }
        public void PlantFlag(int x, int y)
        {
            if (board.Fields[x, y].Clicked) return;
            board.Fields[x, y].Clicked = true;
            board.Fields[x, y].Flag = true;
            --minesLeft;
            --freeFieldsCount;
        }
        

        private void Reset()
        {
            firstMove = true;
            failure = false;
            failurePoint = new Point(-1, -1);
            board = new Board();
            minesLeft = Params.minesCount;
            freeFieldsCount = Params.rows * Params.columns - Params.minesCount;
        }

        private bool IsGameFinished()
        {
            bool isGameFinished = true;
            for (int i = 0; i < Params.columns; ++i)
            {
                for (int j = 0; j < Params.rows; ++j)
                {
                    if (!board.Fields[i, j].Clicked)
                    {
                        isGameFinished = false;
                        break;
                    }
                }
                if (!isGameFinished) break;
            }
            return isGameFinished;
        }

        #endregion
        #region Rendering
        private void RenderBoard()
        {
            ClearBoard();
            RenderFields();
            RenderDividingLines();
            pictureBox1.Refresh();
            label1.Text = $"Free fields: {freeFieldsCount}";
            label2.Text = $"Mines left: {minesLeft}";
            label3.Text = "";
        }

        private void RenderDividingLines()
        {           
            Pen pen = new Pen(Color.Black);
            for (int i = 1; i < Params.rows; ++i)
            {
                int height = i * Params.squareSize;
                graphics.DrawLine(pen, 0, height, pictureBox1.Width, height);
            }

            for(int i = 1; i < Params.columns; ++i)
            {
                int width = i * Params.squareSize;
                graphics.DrawLine(pen, width, 0, width, pictureBox1.Height);
            }
            pen.Dispose();
        }

        private void RenderFields()
        {           
            for (int i = 0; i < Params.columns; ++i)
            {
                for (int j = 0; j < Params.rows; ++j)
                {
                    if (!board.Fields[i, j].Clicked) continue;
                    string path = "";
                    if(i == failurePoint.X && j == failurePoint.Y)
                    { 
                        path = $"./explosion.png";
                    }   
                    else if (board.Fields[i, j].Flag) path = $"./flag.png";
                    else path = $"./{imageDict[board.Fields[i, j].Number]}.png";
                    Bitmap image = new Bitmap(path);
                    graphics.DrawImage(image, i * Params.squareSize, j * Params.squareSize,
                        Params.squareSize, Params.squareSize);
                }
            }

        }

        private void ClearBoard()
        {
            graphics.Clear(pictureBox1.BackColor);
        }
        #endregion      
    }
}
