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
    class UnitSolarPanel : GameUnit
    {
        public const string UNIT_NAME = "solarpanel";
        public const int BUILD_COST = 15;

        public UnitSolarPanel(Vector2 Position) : base(UNIT_NAME, Position)
        {
            UpdateInterval = 2;
            //UpdateInterval = 0.5f;
            //UpdateInterval = 0.01f;

            if (GameEngine.DebugFast)
                UpdateInterval = 0.01f;

            CanLinkEnergy = true;
            Sfx_OnDestroy = GameMusic.Sfx_ExplosionBig;
        }

        public override void SlowUpdate()
        {
            UnitConduit RandomTarget = Utils.Random(GameEngine.PickInRange(Position, UnitConduit.ConnectRangePower).OfType<UnitConduit>().ToArray());

            if (RandomTarget == null)
                return;

            GameEngine.Spawn(new UnitEnergyPacket(Position, RandomTarget));
        }

        public override void DrawWorld()
        {
            if (IsMouseHover)
            {
                GameEngine.DrawLinkLines(Position, UnitConduit.ConnectRangePower, Color.YELLOW, Enumerable.OfType<UnitConduit>);
            }

            base.DrawWorld();
        }
    }

}
