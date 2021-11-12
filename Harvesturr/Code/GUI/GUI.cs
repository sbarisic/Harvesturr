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
using Flexbox;
using System.Reflection;
using FishMarkupLanguage;

namespace Harvesturr {
    static class GUI {
        // static RaylibDevice Dev;
        public static Vector2 MousePos;
        public static bool MouseLeftDown;
        public static bool MouseLeftPressed;
        public static bool MouseLeftReleased;
        public static bool MouseRightDown;
        public static bool MouseRightPressed;
        public static bool MouseRightReleased;

        // Resources
        public static Texture2DRef TexButton;
        public static Texture2DRef TexPanel;
        public static Texture2DRef TexCheckbox;
        public static Font GUIFont;
        public static Font GUIFontLarge;

        public static GUIState CurrentState;

        public static void Init() {
            TexButton = ResMgr.LoadTexture("button");
            TexPanel = ResMgr.LoadTexture("panel");
            TexCheckbox = ResMgr.LoadTexture("checkbox");

            GUIFont = ResMgr.LoadFont("pixantiqua", 12);
            GUIFontLarge = ResMgr.LoadFont("pixantiqua", 24);
        }

        public static void ChangeState(GUIState NewState) {
            CurrentState = NewState;
            CurrentState.Init();
        }

        public static void UpdateInput(float Dt) {
            MousePos = Raylib.GetMousePosition();
            MouseLeftDown = Raylib.IsMouseButtonDown(MouseButton.MOUSE_LEFT_BUTTON);
            MouseRightDown = Raylib.IsMouseButtonDown(MouseButton.MOUSE_RIGHT_BUTTON);
            MouseLeftPressed = Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON);
            MouseRightPressed = Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON);
            MouseLeftReleased = Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON);
            MouseRightReleased = Raylib.IsMouseButtonReleased(MouseButton.MOUSE_RIGHT_BUTTON);

            CurrentState?.UpdateInput(Dt);
        }

        public static void UpdateGUI(float Dt) {
            if (CurrentState != null) {
                CurrentState.Update(Dt);
            }
        }

        public static void DrawScreen() {
            CurrentState?.DrawScreen();
        }

        public static void DrawWorld() {
            CurrentState?.DrawWorld();
        }

        public static void DrawTooltip(Vector2 Pos, string Text, Color? Clr = null, bool Offset = true) {
            if (GameEngine.CurrentDrawState == DrawState.NONE)
                throw new Exception("Can not tooltip outside drawing functions");

            if (Offset)
                Pos += new Vector2(8);

            if (GameEngine.CurrentDrawState == DrawState.WORLD) {
                Pos = Raylib.GetWorldToScreen2D(Pos, GameEngine.GameCamera);
                Raylib.EndMode2D();
            }

            int FontSize = 20;
            int YPadding = 2;
            int XPadding = 5;

            string[] Lines = Text.Trim().Split('\n');
            int MaxWidth = Lines.Select(L => Raylib.MeasureText(L, FontSize)).Max();


            Raylib.DrawRectangle((int)Pos.X, (int)Pos.Y, MaxWidth + XPadding * 2, Lines.Length * FontSize + (Lines.Length + 2) * YPadding, new Color(0, 0, 0, 200));

            for (int i = 0; i < Lines.Length; i++)
                Raylib.DrawText(Lines[i], (int)Pos.X + XPadding, (int)Pos.Y + YPadding * (i + 1) + FontSize * i, FontSize, Clr ?? Color.WHITE);

            if (GameEngine.CurrentDrawState == DrawState.WORLD)
                Raylib.BeginMode2D(GameEngine.GameCamera);
        }

        public static void DrawBar(Vector2 Pos, float Amt, Color Clr, out int BarHeight) {
            if (GameEngine.CurrentDrawState == DrawState.NONE)
                throw new Exception("Can not tooltip outside drawing functions");

            if (GameEngine.CurrentDrawState == DrawState.WORLD) {
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

            Raylib.DrawRectangle((int)Pos.X, (int)Pos.Y, Width, Height, new Color(0, 0, 0, 200));
            Raylib.DrawRectangle((int)Pos.X + Padding, (int)Pos.Y + Padding, (int)((Width - Padding * 2) * Amt), Height - Padding * 2, Clr);

            if (GameEngine.CurrentDrawState == DrawState.WORLD)
                Raylib.BeginMode2D(GameEngine.GameCamera);
        }

        public static void DrawBar(Vector2 Pos, float Amt, Color Clr) {
            DrawBar(Pos, Amt, Clr, out int BarHeight);
        }

        public static IEnumerable<GUIControl> ParseFML(string FileName) {
            // TODO: Hack
            

            FMLDocument Doc = FML.Parse(FileName);
            Doc.FlattenTemplates();

            foreach (FMLTag T in Doc.Tags)
                yield return FMLTagToGUIControl(T);
        }

        public static GUIControl FMLTagToGUIControl(FMLTag T) {

        }
    }
}
