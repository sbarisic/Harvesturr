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
            else if (Raylib.IsKeyDown(KeyboardKey.KEY_R))
            {
                PickedUnit?.Destroy();
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
}
