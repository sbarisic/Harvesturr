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
            Sfx_OnDestroy = GameMusic.Sfx_ExplosionSmall;
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
                GameMusic.PlaySfx(this, Sfx_FinishBuilding);
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

}
