using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperBot
{
    public class Field
    {
        public int Number { get; set; } // -1 == mine

        public bool Clicked { get; set; }

        public bool Flag { get; set; }

        public bool HasMine => Number == -1;
    }
}
