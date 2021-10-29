using Raylib_cs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Harvesturr
{
    class UnitMineral : GameUnit
    {
        public const string UNIT_NAME_Megamineral = "megamineral";
        public const string UNIT_NAME_Mineral = "mineral";

        //public const int ConnectRangeHarvest = 64;
        public int MineralCount;

        public UnitMineral(Vector2 Position, bool Megamineral = false) : base(Megamineral ? UNIT_NAME_Megamineral : UNIT_NAME_Mineral, Position)
        {
            if (Megamineral)
                MineralCount = Utils.Random(300, 400);
            else
                MineralCount = Utils.Random(20, 40);
            //MineralCount = 5;
        }

        public bool HarvestMineral()
        {
            bool Success = false;

            if (MineralCount > 0)
            {
                MineralCount--;
                Success = true;
            }

            if (MineralCount <= 0)
                Destroy();

            return Success;
        }

        public override string ToString()
        {
            return "Minerals: " + MineralCount;
        }
    }

}
