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
    class GameToolBuilder : GameTool
    {
        protected int BuildCost;
        protected bool CurrentLocationValid;

        public GameToolBuilder(string Name, int BuildCost) : base(Name)
        {
            this.BuildCost = BuildCost;
        }

        public override void Update(float Dt)
        {
            CurrentLocationValid = IsValidLocation(GameEngine.MousePosWorld);
        }

        public override void DrawWorld()
        {
            if (ToolGhost == null)
                return;

            Color GhostColor = CurrentLocationValid ? Color.GREEN : Color.RED;
            GameEngine.DrawTextureCentered(ToolGhost, GameEngine.MousePosWorld, Clr: Raylib.Fade(GhostColor, 0.5f));

            if (CurrentLocationValid)
                GUI.DrawTooltip(GameEngine.MousePosWorld, "R$ " + BuildCost, Color.GREEN);
        }

        public virtual bool IsValidLocation(Vector2 MousePosWorld)
        {
            if (ToolGhost == null)
                return true;

            Rectangle CollisionRect = GameEngine.GetBoundingRect(ToolGhost, MousePosWorld);
            int PickCount = GameEngine.Pick(CollisionRect).Where(U => !(U is UnitEnergyPacket)).Count();

            if (PickCount != 0)
                return false;

            return true;
        }

        public override void OnWorldClick(Vector2 WorldPos)
        {
            if (CurrentLocationValid)
            {
                if (GameEngine.TryConsumeResources(BuildCost))
                {
                    OnSpawnSuccess(WorldPos);
                }
            }
        }

        public virtual void OnSpawnSuccess(Vector2 WorldPos)
        {

        }
    }
}
