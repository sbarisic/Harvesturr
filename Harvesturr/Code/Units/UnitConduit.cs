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
    class UnitConduit : GameUnit
    {
        public const string UNIT_NAME = "conduit";
        public const int BUILD_COST = 5;
        public const float ConnectRangePower = 96;

        public int Heat;

        public GameUnit LinkedConduit;

        public UnitConduit(Vector2 Position) : base(UNIT_NAME, Position)
        {
            UpdateInterval = 0.2f;
            CanLinkEnergy = true;
            Heat = 0;

            Sfx_OnDestroy = GameMusic.Sfx_ExplosionSmall;
        }

        public override void SlowUpdate()
        {
            Heat -= 2;

            if (Heat < 0)
                Heat = 0;
        }

        public override void DrawWorld()
        {
            if (IsMouseHover)
            {
                GameEngine.DrawLinkLines(Position, UnitConduit.ConnectRangePower, Color.YELLOW, Enumerable.OfType<UnitConduit>);
            }

            DrawColor = Raylib.ColorFromNormalized(Vector4.Lerp(Vector4.One, new Vector4(1, 0.35f, 0.2f, 1), Heat / 100.0f));
            base.DrawWorld();
        }

        public override void DrawGUI()
        {
            base.DrawGUI();

            if (GameEngine.DrawZoomDetails)
            {
                if (LinkedConduit != null)
                    GameEngine.DrawDashedLine(Position, LinkedConduit.Position, 1, 6, Color.YELLOW, GameEngine.Time * 10);

                if (Heat > 5)
                    GameEngine.DrawBar(Position - GetUnitHeight() / 2, Heat / 100.0f, DrawColor);
            }
            else
            {
                if (LinkedConduit != null)
                    Raylib.DrawLineEx(Position, LinkedConduit.Position, 1, Color.YELLOW);
            }
        }

        public override bool ConsumeEnergyPacket(UnitEnergyPacket Packet)
        {
            Heat++;

            if (Heat > 100)
            {
                Heat = 100;
                Packet.Destroy();
            }

            return base.ConsumeEnergyPacket(Packet);
        }

        public override string ToString()
        {
            return string.Format("Heat: {0}", Heat);
        }
    }

}
