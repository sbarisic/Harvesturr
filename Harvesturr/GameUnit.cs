using Raylib_cs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace Harvesturr
{
    class GameUnit
    {
        public Vector2 Position;
        public string Name;
        public bool Pickable;
        public bool LightningOnDestroy;

        public bool IsMouseHover;

        public int MaxHealth;
        public int Health;
        public bool CanLinkEnergy;
        public float UpdateInterval;
        public float NextUpdateTime;
        public UnitEnergyPacket AwaitingPacket;

        protected Texture2DRef UnitTex;
        protected Color DrawColor;

        protected SoundRef Sfx_OnDestroy;

        public bool Destroyed
        {
            get;
            private set;
        }

        public GameUnit(string UnitName, Vector2 Position)
        {
            this.Position = Position;
            CanLinkEnergy = false;
            UpdateInterval = 1;
            UnitTex = ResMgr.LoadTexture(UnitName);
            Name = UnitName;
            Destroyed = false;
            DrawColor = Color.WHITE;
            Pickable = true;
            MaxHealth = 100;
            Health = MaxHealth;
            LightningOnDestroy = false;
        }

        public Rectangle GetBoundingRect()
        {
            return GameEngine.GetBoundingRect(UnitTex, Position);
        }

        public Vector2 GetUnitWidth()
        {
            return new Vector2(UnitTex.width, 0);
        }

        public Vector2 GetUnitHeight()
        {
            return new Vector2(0, UnitTex.height);
        }

        public Vector2 GetUnitSize()
        {
            return GetUnitWidth() + GetUnitHeight();
        }

        public virtual void Destroy(bool SuppressSfx = false)
        {
            if (Destroyed)
                return;

            if (!SuppressSfx)
                GameEngine.PlaySfx(this, Sfx_OnDestroy);

            Destroyed = true;

            if (LightningOnDestroy)
                GameEngine.AddLightningEffect(Position, Color.SKYBLUE);
        }

        public virtual void Update(float Dt)
        {
            if (Health <= 0)
            {
                Destroy();
                return;
            }

            if (NextUpdateTime < GameEngine.Time)
            {
                NextUpdateTime = GameEngine.Time + UpdateInterval;
                SlowUpdate();
            }
        }

        public virtual void ReceiveDamage(GameUnit AttackingUnit, int Damage)
        {
            Health -= Damage;
        }

        public virtual void SlowUpdate()
        {
        }

        public virtual void DrawWorld()
        {
            GameEngine.DrawTextureCentered(UnitTex, Position, Clr: DrawColor);
        }

        public virtual void DrawGUI()
        {
            if (GameEngine.DrawZoomDetails && Health < MaxHealth)
                GameEngine.DrawBar(Position - GetUnitHeight(), Health / (float)MaxHealth, Color.GREEN);
        }

        // Return true to find next target from current position, return false to destroy
        public virtual bool ConsumeEnergyPacket(UnitEnergyPacket Packet)
        {
            return true;
        }

        public virtual bool CanAcceptEnergyPacket()
        {
            return false;
        }
    }

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

            Sfx_OnDestroy = GameEngine.SfxGetExplosion();
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
            Sfx_OnDestroy = GameEngine.SfxGetExplosion();
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
                GameEngine.PlaySfx(this, Sfx_Laser);
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
            Sfx_OnDestroy = GameEngine.SfxGetExplosion(true);
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

    class UnitEnergyPacket : GameUnit
    {
        public const string UNIT_NAME = "energy";

        public GameUnit Previous;
        public GameUnit Target;

        public UnitEnergyPacket(Vector2 Position, GameUnit Target) : base(UNIT_NAME, Position)
        {
            this.Target = Target;
            Pickable = false;
            LightningOnDestroy = true;

            Sfx_OnDestroy = ResMgr.LoadSound("energy_packet_explode");
        }

        public UnitEnergyPacket(UnitConduit Source, GameUnit Target) : this(Source.Position, Target)
        {
            Previous = Source;
        }

        public override void Update(float Dt)
        {
            base.Update(Dt);
            float MoveSpeed = 96;

            if (Target.Destroyed)
            {
                Destroy();
                return;
            }

            Vector2 TargetPos = Target.Position;

            if (Vector2.Distance(Position, TargetPos) < 2)
            {
                OnTargetReached(Target);
                return;
            }

            Vector2 Dir = Vector2.Normalize(TargetPos - Position);
            Position += Dir * MoveSpeed * Dt;
        }

        void OnTargetReached(GameUnit Target)
        {
            this.Target = null;
            GameUnit Next = null;
            bool PickNextTargetAnyway = Target.ConsumeEnergyPacket(this);

            if (Destroyed)
                return;

            if (Target is UnitConduit || PickNextTargetAnyway)
            {
                Next = GameEngine.PickNextEnergyPacketTarget(Target, Target, Previous);

                if (Next == null)
                    Next = Previous;

                Previous = Target;
            }

            if (Next != null && Next.Destroyed)
                Next = null;

            if (Next == null)
            {
                if (this.Target == null)
                    Destroy();

                return;
            }

            this.Target = Next;
            this.Target.AwaitingPacket = this;
        }
    }

    class UnitBuildingWIP : GameUnit
    {
        static object GetField(Type BaseBuildingType, string FieldName)
        {
            if (BaseBuildingType.BaseType != typeof(GameUnit))
                throw new Exception("BaseBuildingType needs to be derived from GameUnit");

            return BaseBuildingType.GetField(FieldName, BindingFlags.Public | BindingFlags.Static).GetValue(null);
        }

        Type BaseBuildingType;
        int MaxBuildCost;
        int BuildCostRemaining;

        SoundRef Sfx_FinishBuilding;

        public UnitBuildingWIP(Vector2 Position, Type BaseBuildingType) : base((string)GetField(BaseBuildingType, "UNIT_NAME") + "_wip", Position)
        {
            this.BaseBuildingType = BaseBuildingType;
            BuildCostRemaining = MaxBuildCost = (int)GetField(BaseBuildingType, "BUILD_COST");
            CanLinkEnergy = true;

            if (GameEngine.DebugFastBuild)
            {
                MaxBuildCost = 1;
                BuildCostRemaining = 1;
            }

            Sfx_FinishBuilding = ResMgr.LoadSound("building_finish_constructing");
            Sfx_OnDestroy = GameEngine.SfxGetExplosion();
        }

        public override void DrawGUI()
        {
            if (GameEngine.DrawZoomDetails)
                GameEngine.DrawBar(Position - GetUnitHeight() - new Vector2(0, 4), 1.0f - ((float)BuildCostRemaining / MaxBuildCost), Color.ORANGE);

            base.DrawGUI();
        }

        public override void DrawWorld()
        {
            base.DrawWorld();
        }

        public override bool ConsumeEnergyPacket(UnitEnergyPacket Packet)
        {
            Packet.Destroy(true);
            BuildCostRemaining--;

            if (BuildCostRemaining <= 0)
            {
                GameEngine.PlaySfx(this, Sfx_FinishBuilding);
                Destroy(true);

                GameUnit Unit = (GameUnit)Activator.CreateInstance(BaseBuildingType, Position);
                GameEngine.Spawn(Unit);

                // Relink all energy packets to constructed building
                foreach (UnitEnergyPacket P in GameEngine.PickInRange(Position, UnitConduit.ConnectRangePower, true).Select(U => U as UnitEnergyPacket).Where(U => U != null && U.Target == this))
                {
                    P.Target = Unit;
                }

                // Relink all linked conduits back to this unit
                foreach (UnitConduit C in GameEngine.PickInRange(Position, UnitConduit.ConnectRangePower).Select(U => U as UnitConduit).Where(U => U != null && U.LinkedConduit == this))
                {
                    C.LinkedConduit = Unit;
                }
            }

            return false;
        }

        public override bool CanAcceptEnergyPacket()
        {
            return true;
        }
    }

    class UnitLaser : GameUnit
    {
        public const string UNIT_NAME = "laser";
        public const int BUILD_COST = 10;
        public const int SINGLE_LASER_DAMAGE = 1;

        public const float AttackRangeLaser = 128;

        int EnergyCharges;
        float LastEnergyChargeUseTime;

        GameUnitAlien Target;

        public UnitLaser(Vector2 Position) : base(UNIT_NAME, Position)
        {
            EnergyCharges = 0;
            UpdateInterval = 0.1f;
            CanLinkEnergy = true;
            
            Sfx_OnDestroy = GameEngine.SfxGetExplosion(true);
        }

        public override void Update(float Dt)
        {
            base.Update(Dt);
            DrawColor = Color.WHITE;

            if (EnergyCharges <= 0 && ((GameEngine.Time - LastEnergyChargeUseTime) > UpdateInterval))
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
