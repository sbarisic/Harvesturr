using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Raylib_cs;
using System.Globalization;

namespace Harvesturr
{
    [IsGameTool]
    class GameToolConduit : GameToolBuilder
    {
        public GameToolConduit() : base("Conduit", 5)
        {
            ToolGhost = ResMgr.LoadTexture(UnitConduit.UNIT_NAME);
        }

        public override void DrawWorld()
        {
            base.DrawWorld();
            GameEngine.DrawLinkLines(GameEngine.MousePosWorld, UnitConduit.ConnectRangePower, Color.YELLOW, F => F.Where(U => U.CanLinkEnergy));
        }

        public override void OnSpawnSuccess(Vector2 WorldPos)
        {
            GameEngine.Spawn(new UnitBuildingWIP(WorldPos, typeof(UnitConduit)));
        }
    }
}
