using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperBot
{
    public class Board
    {
        public Field[,] Fields { get; private set; }
        #region Initialization
        public Board()
        {
            Fields = new Field[Params.columns, Params.rows];
            for(int i = 0; i < Fields.GetLength(0); ++i)
            {
                for (int j = 0; j < Fields.GetLength(1); ++j)
                {
                    Fields[i, j] = new Field();
                }
            }
            PlaceMines();
            SetNumbers();
        }

        private void PlaceMines()
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());

            for(int i = 0; i < Params.minesCount; ++i)
            {
                int x = -1;
                int y = -1;
                do
                {
                    x = rand.Next(Params.columns);
                    y = rand.Next(Params.rows);
                } while (Fields[x,y].HasMine);
                Fields[x, y].Number = -1;
            }
        }
        
        private void SetNumbers()
        {
            for (int i = 0; i < Fields.GetLength(0); ++i)
            {
                for (int j = 0; j < Fields.GetLength(1); ++j)
                {
                    if (Fields[i, j].HasMine) continue;
                    CountMines(i, j);
                }
            }       
        }

        private void CountMines(int x, int y)
        {
            int minesCount = 0;
            for (int k = -1; k <= 1; ++k)
            {
                for (int m = -1; m <= 1; ++m)
                {
                    int neighborX = x + k;
                    int neighborY = y + m;
                    if (k == 0 && m == 0) continue;
                    if (!Helpers.IsInBounds(neighborX, neighborY)) continue;

                    if (Fields[neighborX, neighborY].HasMine) ++minesCount;
                }
            }
            Fields[x, y].Number = minesCount;
        }
        #endregion

        #region Game
        public Neighborhood GetNeighborhood(int x, int y)
        {
            Neighborhood neighborhood = new Neighborhood();
            for (int i = -1; i <= 1; ++i)
            {
                for (int j = -1; j <= 1; ++j)
                {
                    int neighborX = x + i;
                    int neighborY = y + j;
                    if (i == 0 && j == 0) continue;
                    if (!Helpers.IsInBounds(neighborX, neighborY)) continue;
                    if (Fields[neighborX, neighborY].Clicked && Fields[neighborX, neighborY].Number == 0)
                        continue;
                    if (Fields[neighborX, neighborY].Flag) ++neighborhood.FlagsCount;
                    else
                    {
                        if (!Fields[neighborX, neighborY].Clicked)
                            neighborhood.FreeNeighbors.Add(new System.Drawing.Point(neighborX, neighborY));
                    }
                }
            }
            return neighborhood;
        }

        public List<Point> GetNumbersAroundFlag(int x, int y)
        {
            List<Point> numbers = new List<Point>();
            for (int i = -1; i <= 1; ++i)
            {
                for (int j = -1; j <= 1; ++j)
                {
                    int neighborX = x + i;
                    int neighborY = y + j;
                    if (i == 0 && j == 0) continue;
                    if (!Helpers.IsInBounds(neighborX, neighborY)) continue;
                    if (!Fields[neighborX, neighborY].Clicked) continue;
                    if (Fields[neighborX, neighborY].Flag) continue;
                    if (Fields[neighborX, neighborY].Number == 0) continue;
                    numbers.Add(new Point(neighborX, neighborY));
                }
            }
            return numbers;
        }
       
        #endregion
    }
}
