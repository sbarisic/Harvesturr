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

        public const int SINGLE_LASER_DMG = 1;
        public const float SINGLE_LASER_RNG = 64;

        public bool IsAttacking;

        bool CalculationDirty;
        int AttackDamage;
        float AttackRange;

        int MaxEnergyCharges;
        int EnergyCharges;

        GameUnitAlien Target;
        UnitLaser LinkedLaser;

        public float GetAttackRange
        {
            get
            {
                return AttackRange;
            }
        }

        public UnitLaser GetLinkedLaser
        {
            get
            {
                if (LinkedLaser != null && LinkedLaser.Destroyed)
                    return null;

                return LinkedLaser;
            }
        }

        public UnitLaser(Vector2 Position) : base(UNIT_NAME, Position)
        {
            MaxEnergyCharges = 60;
            EnergyCharges = 0;

            UpdateInterval = 0.1f;
            CanLinkEnergy = true;
            CalculationDirty = true;

            CalculateRangeAndDamage();
            Sfx_OnDestroy = GameMusic.Sfx_ExplosionBig;
        }

        public bool ContainsLaserInLinkChain(UnitLaser Laser)
        {
            if (GetLinkedLaser == Laser)
                return true;

            if (GetLinkedLaser != null && GetLinkedLaser.ContainsLaserInLinkChain(Laser))
                return true;

            return false;
        }

        public virtual void LinkLaser(UnitLaser NewLaser)
        {
            if (NewLaser == null || NewLaser == this)
            {
                UnitLaser OldLaser = LinkedLaser;
                LinkedLaser = null;

                if (OldLaser != null && !OldLaser.Destroyed)
                    OldLaser.CalculationDirty = true;

                CalculationDirty = true;
                return;
            }

            if (NewLaser.GetLinkedLaser == this)
                NewLaser.LinkLaser(null);

            if (NewLaser.ContainsLaserInLinkChain(this))
                LinkedLaser = null;
            else
            {
                LinkedLaser = NewLaser;
                LinkedLaser.CalculationDirty = true;
            }
        }

        IEnumerable<UnitLaser> GetLinkedLasers()
        {
            IEnumerable<UnitLaser> Units = GameEngine.GetAllGameUnits().Select(U => U as UnitLaser);

            foreach (UnitLaser L in Units)
            {
                if (L == null)
                    continue;

                if (L.GetLinkedLaser == this)
                    yield return L;
            }
        }

        public void CalculateRangeAndDamage()
        {
            if (!CalculationDirty)
                return;
            CalculationDirty = false;

            AttackDamage = SINGLE_LASER_DMG;

            foreach (UnitLaser L in GetLinkedLasers())
            {
                L.CalculateRangeAndDamage();
                AttackDamage += L.AttackDamage;
            }

            AttackRange = SINGLE_LASER_RNG + (SINGLE_LASER_RNG * (AttackDamage / SINGLE_LASER_DMG - 1) * 0.2f);

            if (LinkedLaser != null && !LinkedLaser.Destroyed)
                LinkedLaser.CalculationDirty = true;
        }

        public override void Update(float Dt)
        {
            if (LinkedLaser != null && LinkedLaser.Destroyed)
                LinkedLaser = null;

            CalculateRangeAndDamage();

            base.Update(Dt);
            DrawColor = Color.WHITE;

            if (EnergyCharges <= 0)
                DrawColor = new Color(128, 128, 128, 190);
        }

        public override void SlowUpdate()
        {
            if (EnergyCharges <= 0)
                return;

            // Don't do anything if linked
            if (LinkedLaser != null)
            {
                IsAttacking = LinkedLaser.IsAttacking;
                return;
            }

            // Find target
            if (Target == null)
            {
                GameUnitAlien[] Aliens = GameEngine.PickInRange(Position, AttackRange).OfType<GameUnitAlien>().ToArray();
                Target = Utils.Random(Aliens);
            }

            // Attack target or unlink target if too far/destroyed
            if (Target != null && !Target.Destroyed)
            {
                EnergyCharges--;
                Target.ReceiveDamage(this, AttackDamage);
            }

            if (Target != null && Target.Destroyed)
                Target = null;

            if (Target != null && Vector2.Distance(Position, Target.Position) > AttackRange)
                Target = null;

            IsAttacking = Target != null;
        }

        public override void DrawWorld()
        {
            if (IsMouseHover || GameEngine.DebugDrawLaserRange)
            {
                GameEngine.DrawLinkLines(Position, AttackRange, Color.RED);

                if (IsMouseHover && GameEngine.DebugDrawLaserRange)
                    GameEngine.DrawLinkLines(Position, AttackRange + 1, Color.RED);
            }

            if (Target != null && EnergyCharges > 0)
            {
                Raylib.DrawLineEx(Position, Target.Position, 1, Color.RED);
            }

            base.DrawWorld();
        }

        public override void Destroy(bool SuppressSfx = false)
        {
            base.Destroy(SuppressSfx);

            if (LinkedLaser != null && !LinkedLaser.Destroyed)
                LinkedLaser.CalculationDirty = true;
        }

        public override void DrawGUI()
        {
            base.DrawGUI();

            if (GameEngine.DrawZoomDetails)
            {
                if (LinkedLaser != null)
                {
                    if (LinkedLaser.IsAttacking)
                    {
                        Raylib.DrawLineEx(Position, LinkedLaser.Position, 1, Color.RED);
                    }
                    else
                    {
                        GameEngine.DrawDashedLine(Position, LinkedLaser.Position, 1, 6, Color.RED, GameEngine.Time * 10);
                    }
                }

                GameEngine.DrawBar(Position - GetUnitHeight() - new Vector2(0, 4), (float)EnergyCharges / MaxEnergyCharges, Color.YELLOW);
            }
            else
            {
                if (LinkedLaser != null)
                {
                    Raylib.DrawLineEx(Position, LinkedLaser.Position, LinkedLaser.IsAttacking ? 2 : 1, Color.RED);
                }
            }
        }

        public override bool ConsumeEnergyPacket(UnitEnergyPacket Packet)
        {
            if (EnergyCharges >= MaxEnergyCharges)
                return true;

            EnergyCharges += 25;
            Packet.Destroy(true);

            return base.ConsumeEnergyPacket(Packet);
        }

        public override bool CanAcceptEnergyPacket()
        {
            if (AwaitingPacket != null && !AwaitingPacket.Destroyed)
                return false;

            return EnergyCharges < MaxEnergyCharges;
        }
    }
}
