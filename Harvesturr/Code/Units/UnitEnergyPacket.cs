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

}
