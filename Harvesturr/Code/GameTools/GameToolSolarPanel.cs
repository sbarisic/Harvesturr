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
    class GameToolSolarPanel : GameToolBuilder
    {
        public GameToolSolarPanel() : base("Solar Panel", 10)
        {
            ToolGhost = ResMgr.LoadTexture(UnitSolarPanel.UNIT_NAME);
        }

        public override void DrawWorld()
        {
            GameEngine.DrawLinkLines(GameEngine.MousePosWorld, UnitConduit.ConnectRangePower, Color.YELLOW, Enumerable.OfType<UnitConduit>);
            base.DrawWorld();
        }

        public override void OnSpawnSuccess(Vector2 WorldPos)
        {
            GameEngine.Spawn(new UnitBuildingWIP(WorldPos, typeof(UnitSolarPanel)));
        }
    }
}
