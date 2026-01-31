using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match3Game.Extensions
{
    static class ListExtension
    {
        public static int[,] ToArrays(this List<int[]> list) 
        {
            int[,] arrays = new int[list.Count, list[0].Length];
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < list[i].Length; j++)
                {
                    arrays[i, j] = list[i][j];
                }
            }
            return arrays;
        }
    }
}
