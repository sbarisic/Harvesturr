using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Harvesturr
{
    static class CSV
    {
        public static int[] ParseIntCSV(string CSVSrc, out int W, out int H)
        {
            string Text = CSVSrc.Trim();
            string[] Lines = Text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            H = Lines.Length;
            W = Lines[0].Count(C => C == ',') + 1;

            int[] Ints = new int[W * H];

            for (int L = 0; L < Lines.Length; L++)
            {
                string Line = Lines[L];
                int[] Columns = Line.Split(new char[] { ',' }).Select(C => int.Parse(C)).ToArray();
                Array.Copy(Columns, 0, Ints, W * L, Columns.Length);
            }

            return Ints;
        }
    }
}
