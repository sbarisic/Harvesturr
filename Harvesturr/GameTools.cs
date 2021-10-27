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
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class IsGameToolAttribute : Attribute
    {
        public IsGameToolAttribute()
        {
        }

        public static IEnumerable<GameTool> CreateAllGameTools()
        {
            Type[] AllTypes = Assembly.GetExecutingAssembly().GetTypes();

            foreach (var T in AllTypes)
            {
                bool HasAttribute = T.GetCustomAttribute<IsGameToolAttribute>() != null;

                if (HasAttribute)
                {
                    GameTool ToolInstance = Activator.CreateInstance(T) as GameTool;
                    yield return ToolInstance;
                }
            }
        }
    }

    class GameTool
    {
        public string Name;
        public bool Active;

        protected Texture2DRef ToolGhost;

        protected Vector2 MouseClickPos;
        protected bool InMouseClick;

        public GameTool(string Name)
        {
            this.Name = Name;
            this.Active = false;
        }

        public virtual void OnSelected()
        {
            Console.WriteLine("Selected {0}", Name);
        }

        public virtual void OnWorldMousePress(Vector2 WorldPos, bool Press)
        {
            if (Press)
                OnWorldClick(WorldPos);

            if (Press)
            {
                MouseClickPos = WorldPos;
                InMouseClick = true;
            }
            else if (!Press && InMouseClick)
            {
                InMouseClick = false;

                Vector2 EndPos = GameEngine.MousePosWorld;
                if (MouseClickPos != EndPos)
                    OnMouseDrag(MouseClickPos, EndPos, EndPos - MouseClickPos);
            }
        }

        public virtual void OnMouseDrag(Vector2 WorldStart, Vector2 WorldEnd, Vector2 DragNormal)
        {
        }

        public virtual void OnWorldClick(Vector2 WorldPos)
        {
        }

        public virtual void Update(float Dt)
        {
        }

        public virtual void DrawWorld()
        {
        }
    }

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
                GameEngine.DrawTooltip(GameEngine.MousePosWorld, "R$ " + BuildCost, Color.GREEN);
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

    [IsGameTool]
    class GameToolPicker : GameTool
    {
        Color DragLineColor;

        public GameToolPicker() : base("Picker")
        {
            DragLineColor = new Color(50, 200, 100, 180);
        }

        public override void OnWorldClick(Vector2 WorldPos)
        {
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "new Vector2({0:F3}f, {1:F3}f)", WorldPos.X, WorldPos.Y));
            //GameEngine.AddLightningEffect(WorldPos, Color.SKYBLUE);

            GameUnit PickedUnit = GameEngine.Pick(GameEngine.MousePosWorld).FirstOrDefault();

            if (PickedUnit is GameUnitAlien Alien)
            {
                const float PickForce = 256;
                Alien.ApplyForce(Utils.Random(new Vector2(-PickForce), new Vector2(PickForce)));
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_Q))
            {
                UnitAlienUfo NewAlien = new UnitAlienUfo(WorldPos);
                GameEngine.Spawn(NewAlien);
            }
        }

        public override void OnMouseDrag(Vector2 WorldStart, Vector2 WorldEnd, Vector2 DragNormal)
        {
            UnitConduit UnitA = GameEngine.Pick(WorldStart).FirstOrDefault() as UnitConduit;
            GameUnit UnitB = GameEngine.Pick(WorldEnd).FirstOrDefault();

            if (UnitB != null && !UnitB.CanLinkEnergy)
            {
                UnitB = null;
            }

            if (UnitA == UnitB)
                UnitB = null;

            if (UnitA != null)
            {
                if (UnitB != null && Vector2.Distance(UnitA.Position, UnitB.Position) < UnitConduit.ConnectRangePower)
                    UnitA.LinkedConduit = UnitB;
                else
                    UnitA.LinkedConduit = null;
            }
        }

        public override void DrawWorld()
        {
            if (InMouseClick)
            {
                Raylib.DrawLineEx(MouseClickPos, GameEngine.MousePosWorld, 1, DragLineColor);


                if (GameEngine.Raycast(MouseClickPos, GameEngine.MousePosWorld, out RaycastResult Res))
                {
                    GameEngine.DrawCircle(Res.Intersection.HitPoint, 3, Color.RED);
                }

            }


            //Raylib.DrawLineEx(MouseClickPos, GameEngine.MousePosWorld, 1, DragLineColor);

            GameUnit PickedUnit = GameEngine.Pick(GameEngine.MousePosWorld).FirstOrDefault();

            if (PickedUnit == null || PickedUnit is GameUnitAlien)
                return;

            GameEngine.DrawTooltip(GameEngine.MousePosWorld, PickedUnit.ToString());
        }
    }

    [IsGameTool]
    class GameToolConduit : GameToolBuilder
    {
        public GameToolConduit() : base("Conduit", 5)
        {
            ToolGhost = ResMgr.LoadTexture(UnitConduit.UNIT_NAME);
        }

        public override void DrawWorld()
        {
            base.DrawWorld();
            GameEngine.DrawLinkLines(GameEngine.MousePosWorld, UnitConduit.ConnectRangePower, Color.YELLOW, F => F.Where(U => U.CanLinkEnergy));
        }

        public override void OnSpawnSuccess(Vector2 WorldPos)
        {
            GameEngine.Spawn(new UnitBuildingWIP(WorldPos, typeof(UnitConduit)));
        }
    }

    [IsGameTool]
    class GameToolHarvester : GameToolBuilder
    {
        public GameToolHarvester() : base("Harvester", 8)
        {
            ToolGhost = ResMgr.LoadTexture(UnitHarvester.UNIT_NAME);
        }

        public override void DrawWorld()
        {
            GameEngine.DrawLinkLines(GameEngine.MousePosWorld, UnitConduit.ConnectRangePower, Color.YELLOW, Enumerable.OfType<UnitConduit>);
            GameEngine.DrawLinkLines(GameEngine.MousePosWorld, UnitHarvester.ConnectRangeHarvest, Color.GREEN, Enumerable.OfType<UnitMineral>);
            base.DrawWorld();
        }

        public override void OnSpawnSuccess(Vector2 WorldPos)
        {
            GameEngine.Spawn(new UnitBuildingWIP(WorldPos, typeof(UnitHarvester)));
        }
    }

    [IsGameTool]
    class GameToolSolarPanel : GameToolBuilder
    {
        public GameToolSolarPanel() : base("Solar Panel", 10)
        {
            ToolGhost = ResMgr.LoadTexture(UnitSolarPanel.UNIT_NAME);
        }

        public override void DrawWorld()
        {
            GameEngine.DrawLinkLines(GameEngine.MousePosWorld, UnitConduit.ConnectRangePower, Color.YELLOW, Enumerable.OfType<UnitConduit>);
            base.DrawWorld();
        }

        public override void OnSpawnSuccess(Vector2 WorldPos)
        {
            GameEngine.Spawn(new UnitBuildingWIP(WorldPos, typeof(UnitSolarPanel)));
        }
    }

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
