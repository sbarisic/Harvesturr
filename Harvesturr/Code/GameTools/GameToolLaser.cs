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
    class GameToolLaser : GameToolBuilder
    {
        public GameToolLaser() : base("Laser", 10)
        {
            ToolGhost = ResMgr.LoadTexture(UnitLaser.UNIT_NAME);
        }

        public override void DrawWorld()
        {
            GameEngine.DrawLinkLines(GameEngine.MousePosWorld, UnitConduit.ConnectRangePower, Color.YELLOW, Enumerable.OfType<UnitConduit>);
            GameEngine.DrawLinkLines(GameEngine.MousePosWorld, UnitLaser.AttackRangeLaser, Color.RED, Enumerable.OfType<UnitLaser>);
            base.DrawWorld();
        }

        public override void OnSpawnSuccess(Vector2 WorldPos)
        {
            GameEngine.Spawn(new UnitBuildingWIP(WorldPos, typeof(UnitLaser)));
        }
    }
}
