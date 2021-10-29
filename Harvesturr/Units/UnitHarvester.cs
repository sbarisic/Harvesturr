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
    class UnitHarvester : GameUnit
    {
        public const string UNIT_NAME = "harvester";
        public const int BUILD_COST = 10;
        public const float ConnectRangeHarvest = 64;

        int EnergyCharges;
        float LastEnergyChargeUseTime;

        SoundRef Sfx_Laser;

        public UnitHarvester(Vector2 Position) : base(UNIT_NAME, Position)
        {
            EnergyCharges = 0;
            UpdateInterval = 5;
            CanLinkEnergy = true;

            Sfx_Laser = ResMgr.LoadSound("harvester_laser");
            Sfx_OnDestroy = GameMusic.Sfx_ExplosionSmall;
        }

        public override void Update(float Dt)
        {
            base.Update(Dt);
            DrawColor = Color.WHITE;

            if (EnergyCharges <= 0 && ((GameEngine.Time - LastEnergyChargeUseTime) > UpdateInterval))
                DrawColor = new Color(128, 128, 128, 190);
        }

        public override void SlowUpdate()
        {
            if (EnergyCharges <= 0)
                return;

            UnitMineral[] Minerals = GameEngine.PickInRange(Position, ConnectRangeHarvest).OfType<UnitMineral>().ToArray();
            if (Minerals.Length == 0)
            {
                Destroy();
                GameEngine.AddResource(2);
                return;
            }

            UnitMineral TgtMineral = Utils.Random(Minerals);
            if (TgtMineral == null || TgtMineral.Destroyed)
                return;

            if (TgtMineral.HarvestMineral())
            {
                GameMusic.PlaySfx(this, Sfx_Laser);
                GameEngine.AddEffect(() => Raylib.DrawLineEx(Position, TgtMineral.Position, 2, Color.GREEN), 0.25f);

                EnergyCharges--;

                if (EnergyCharges <= 0)
                {
                    EnergyCharges = 0;
                    LastEnergyChargeUseTime = GameEngine.Time;
                }

                GameEngine.AddResource(1);
            }
        }

        public override void DrawWorld()
        {
            if (IsMouseHover)
            {
                GameEngine.DrawLinkLines(Position, ConnectRangeHarvest, Color.GREEN, Enumerable.OfType<UnitMineral>);
            }

            base.DrawWorld();
        }

        public override bool ConsumeEnergyPacket(UnitEnergyPacket Packet)
        {
            EnergyCharges++;
            Packet.Destroy(true);

            return base.ConsumeEnergyPacket(Packet);
        }

        public override bool CanAcceptEnergyPacket()
        {
            if (AwaitingPacket != null && !AwaitingPacket.Destroyed)
                return false;

            return EnergyCharges <= 1;
        }
    }

}
