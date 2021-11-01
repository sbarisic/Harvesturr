using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using Raylib_cs;
using Raygui_cs;
using System.Numerics;
using System.Diagnostics;

namespace Harvesturr
{
    static class GUI
    {
        public static int GUIRectHeight = 70;
        public static Color GUIPanelColor;

        public static List<GameTool> GameTools = new List<GameTool>();
        public static GameTool ActiveGameTool;

        public static void Init()
        {
            GUIPanelColor = Raylib.Fade(Color.BLACK, 0.8f);
            GameTools.AddRange(IsGameToolAttribute.CreateAllGameTools());
        }

        public static void UpdateGUI(float Dt)
        {
            ActiveGameTool?.Update(Dt);

            if (Utils.IsInside(new Rectangle(0, 0, GameEngine.ScreenWidth, GameEngine.ScreenHeight - GUIRectHeight), GameEngine.MousePosScreen) && GameMap.IsInBounds(GameEngine.MousePosWorld))
            {
                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
                    ActiveGameTool?.OnWorldMousePress(GameEngine.MousePosWorld, true);

                if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
                    ActiveGameTool?.OnWorldMousePress(GameEngine.MousePosWorld, false);
            }
        }

        public static void DrawScreen()
        {
            Raylib.DrawRectangle(0, GameEngine.ScreenHeight - GUIRectHeight, GameEngine.ScreenWidth, GUIRectHeight, GUIPanelColor);
            Raylib.DrawRectangle(0, 0, GameEngine.ScreenWidth, 24, GUIPanelColor);

            if (GameEngine.DebugView)
            {
                float FrameTime = Raylib.GetFrameTime();
                float FPS = 1.0f / FrameTime;
                Raylib.DrawText(string.Format("{0} / {1} Units, {2:0.000} ms, {3:0.00} FPS", GameEngine.GameUnits.Count(U => U != null), GameEngine.GameUnits.Length, FrameTime, FPS), 2, 2, 20, Color.WHITE);
            }

            string ResourcesText = "R$ " + GameEngine.Resources;
            int TextWidth = Raylib.MeasureText(ResourcesText, 20);
            Raylib.DrawText(ResourcesText, GameEngine.ScreenWidth - TextWidth - 10, 2, 20, Color.GREEN);

            int ButtonCount = 0;
            for (int i = 0; i < GameTools.Count; i++)
            {
                GameTool T = GameTools[i];
                bool Checked = AddButton(ref ButtonCount, T.Name, T.Active);

                if (Checked && !T.Active)
                {
                    for (int j = 0; j < GameTools.Count; j++)
                        GameTools[j].Active = false;

                    T.Active = Checked;
                    T.OnSelected();
                    ActiveGameTool = T;
                }
            }
        }

        public static void DrawWorld()
        {
            ActiveGameTool?.DrawWorld();
        }

        public static bool AddButton(ref int ButtonCount, string Text, bool Active)
        {
            int ButtonHeight = (int)(GUIRectHeight * 0.8f);
            int ButtonWidth = 80;
            int ButtonPadding = 10;

            int ButtonX = ButtonPadding + (ButtonWidth * ButtonCount + ButtonPadding * ButtonCount);
            int ButtonY = (GameEngine.ScreenHeight - GUIRectHeight) + (GUIRectHeight - ButtonHeight) / 2;

            bool Ret = Raygui.GuiToggle(new Rectangle(ButtonX, ButtonY, ButtonWidth, ButtonHeight), Text, Active);
            ButtonCount++;
            return Ret;
        }

        public static void DrawTooltip(Vector2 Pos, string Text, Color? Clr = null, bool Offset = true)
        {
            if (GameEngine.CurrentDrawState == DrawState.NONE)
                throw new Exception("Can not tooltip outside drawing functions");

            if (Offset)
                Pos += new Vector2(8);

            if (GameEngine.CurrentDrawState == DrawState.WORLD)
            {
                Pos = Raylib.GetWorldToScreen2D(Pos, GameEngine.GameCamera);
                Raylib.EndMode2D();
            }

            int FontSize = 20;
            int YPadding = 2;
            int XPadding = 5;

            string[] Lines = Text.Trim().Split('\n');
            int MaxWidth = Lines.Select(L => Raylib.MeasureText(L, FontSize)).Max();


            Raylib.DrawRectangle((int)Pos.X, (int)Pos.Y, MaxWidth + XPadding * 2, Lines.Length * FontSize + (Lines.Length + 2) * YPadding, GUIPanelColor);

            for (int i = 0; i < Lines.Length; i++)
                Raylib.DrawText(Lines[i], (int)Pos.X + XPadding, (int)Pos.Y + YPadding * (i + 1) + FontSize * i, FontSize, Clr ?? Color.WHITE);

            if (GameEngine.CurrentDrawState == DrawState.WORLD)
                Raylib.BeginMode2D(GameEngine.GameCamera);
        }

        public static void DrawBar(Vector2 Pos, float Amt, Color Clr, out int BarHeight)
        {
            if (GameEngine.CurrentDrawState == DrawState.NONE)
                throw new Exception("Can not tooltip outside drawing functions");

            if (GameEngine.CurrentDrawState == DrawState.WORLD)
            {
                Pos = Raylib.GetWorldToScreen2D(Pos, GameEngine.GameCamera);
                Raylib.EndMode2D();
            }

            const int Padding = 2;
            const int Width = 48;
            const int Height = 8;
            BarHeight = Height;

            Pos -= new Vector2(Width / 2, Height / 2);

            if (Amt < 0)
                Amt = 0;
            else if (Amt > 1)
                Amt = 1;

            Raylib.DrawRectangle((int)Pos.X, (int)Pos.Y, Width, Height, GUIPanelColor);
            Raylib.DrawRectangle((int)Pos.X + Padding, (int)Pos.Y + Padding, (int)((Width - Padding * 2) * Amt), Height - Padding * 2, Clr);

            if (GameEngine.CurrentDrawState == DrawState.WORLD)
                Raylib.BeginMode2D(GameEngine.GameCamera);
        }

        public static void DrawBar(Vector2 Pos, float Amt, Color Clr)
        {
            DrawBar(Pos, Amt, Clr, out int BarHeight);
        }
    }
}
