using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperBot
{
    public class Helpers
    { 
        public static bool IsInBounds(int x, int y)
        {
            if (x < 0) return false;
            if (x >= Params.columns) return false;
            if (y < 0) return false;
            if (y >= Params.rows) return false;

            return true;
        }

        public static List<int[]> GetAllSubsets(int n, int k)
        {
            List<int[]> combinations = new List<int[]>();
            GetSubset(n, k, 0, new int[k], 0, ref combinations);
            return combinations;
        }

        private static void GetSubset(int n, int k, int index, int[] combination, 
            int i, ref List<int[]> combinations)
        {
            if (index == k)
            {
                combinations.Add(combination);
                return;
            }
            if (i >= n) return;
            int[] c1 = new int[k];
            int[] c2 = new int[k];
            combination.CopyTo(c1, 0);
            combination.CopyTo(c2, 0);
            c1[index] = i;
            c2[index] = i;
            GetSubset(n, k, index + 1, c1, i + 1, ref combinations);
            GetSubset(n, k, index, c2, i + 1, ref combinations);
        }
    }
}
