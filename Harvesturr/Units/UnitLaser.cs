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

    class UnitLaser : GameUnit
    {
        public const string UNIT_NAME = "laser";
        public const int BUILD_COST = 10;
        public const int SINGLE_LASER_DAMAGE = 1;

        public const float AttackRangeLaser = 128;

        int EnergyCharges;
        GameUnitAlien Target;

        public UnitLaser(Vector2 Position) : base(UNIT_NAME, Position)
        {
            EnergyCharges = 0;
            UpdateInterval = 0.1f;
            CanLinkEnergy = true;

            Sfx_OnDestroy = GameMusic.Sfx_ExplosionBig;
        }

        public override void Update(float Dt)
        {
            base.Update(Dt);
            DrawColor = Color.WHITE;

            if (EnergyCharges <= 0)
                DrawColor = new Color(128, 128, 128, 190);
        }

        public int CalculateLaserDamage()
        {
            return SINGLE_LASER_DAMAGE;
        }

        public override void SlowUpdate()
        {
            if (EnergyCharges <= 0)
                return;

            if (Target == null)
            {
                GameUnitAlien[] Aliens = GameEngine.PickInRange(Position, AttackRangeLaser).OfType<GameUnitAlien>().ToArray();
                Target = Utils.Random(Aliens);

                if (Target == null)
                    return;

            }
            else if (Target.Destroyed)
            {
                Target = null;
                return;
            }

            EnergyCharges--;
            Target.ReceiveDamage(this, CalculateLaserDamage());
        }

        public override void DrawWorld()
        {
            if (IsMouseHover)
            {
                GameEngine.DrawLinkLines(Position, AttackRangeLaser, Color.RED, Enumerable.OfType<UnitLaser>);
            }

            if (Target != null && EnergyCharges > 0)
            {
                Raylib.DrawLineEx(Position, Target.Position, 1, Color.RED);
            }

            base.DrawWorld();
        }

        public override bool ConsumeEnergyPacket(UnitEnergyPacket Packet)
        {
            if (EnergyCharges > 60)
                return true;

            EnergyCharges += 25;
            Packet.Destroy(true);

            return base.ConsumeEnergyPacket(Packet);
        }

        public override bool CanAcceptEnergyPacket()
        {
            if (AwaitingPacket != null && !AwaitingPacket.Destroyed)
                return false;

            return EnergyCharges <= 50;
        }
    }
}
