using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperBot
{
    public class NewFlagsCount
    {
        public Point Coords { get; set; }
        public int Count { get; set; }

        public NewFlagsCount(Point coords)
        {
            Coords = coords;
            Count = 1;
        }
    }
}
