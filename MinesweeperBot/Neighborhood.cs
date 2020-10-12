using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperBot
{
    public class Neighborhood
    { 
        public List<Point> FreeNeighbors { get; set; }
        public int FlagsCount { get; set; }

        public Neighborhood()
        {
            FreeNeighbors = new List<Point>();
            FlagsCount = 0;
        }
    }
}
