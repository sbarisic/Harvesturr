using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using Raylib_cs;
using System.Numerics;
using System.Diagnostics;

namespace Harvesturr
{
    static class Program
    {
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool SetDllDirectory(string FileName);


        static void Main(string[] args)
        {
            if (!SetDllDirectory("native/" + ((IntPtr.Size == 8) ? "x64" : "x86")))
                throw new Exception("Failed to set DLL directory");

            bool DebugAll = args.Contains("--debug-all");

            GameEngine.DebugView = DebugAll || args.Contains("--debug");
            GameEngine.DebugPerformance = DebugAll || args.Contains("--performance");
            GameEngine.DebugFast = DebugAll || args.Contains("--fast"); // Fast update interval
            GameEngine.DebugFastBuild = DebugAll || args.Contains("--fastbuild"); // Fast structure building

            // Disable this
            GameEngine.DebugView = true;
            GameEngine.DebugFastBuild = true;


            const int Width = 1366;
            const int Height = 768;

            ResMgr.InitFileWatcher();
            Raylib.InitWindow(Width, Height, "Harvesturr");
            Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            Raylib.SetTargetFPS(60);

            GameEngine.GUILoadStyle("jungle");

            GameEngine.GUIPanelColor = Raylib.Fade(Color.BLACK, 0.8f);
            GameEngine.GameCamera = new Camera2D(new Vector2(Width, Height) / 2, Vector2.Zero, 0, 2);

            GameMap.Load("test");

            if (GameEngine.DebugPerformance)
            {
                GameMap.DestroyAllMinerals();
                const float Dist = 50;

                Rectangle Rect = GameMap.GetBounds();
                Vector2 Pos = new Vector2(Rect.x, Rect.y) + new Vector2(10);
                int XCount = (int)(Rect.width / Dist);
                int YCount = (int)(Rect.height / Dist);

                for (int X = 0; X < XCount; X++)
                    for (int Y = 0; Y < YCount; Y++)
                        GameEngine.Spawn(new UnitConduit(Pos + new Vector2(X, Y) * Dist));
            }

            //Spawn(new UnitAlienUfo(Vector2.Zero));

            GameEngine.GameTools.AddRange(IsGameToolAttribute.CreateAllGameTools());
            GameEngine.Resources = 50;

            if (GameEngine.DebugView)
                GameEngine.Resources = 10000000;

            Texture2DRef BackgroundNoise = ResMgr.LoadTexture("background_noise");

            GameEngine.GameTimer.Restart();

            while (!Raylib.WindowShouldClose())
            {
                // float FrameTime = Raylib.GetFrameTime();
                GameEngine.Time = (float)GameEngine.GameTimer.Elapsed.TotalSeconds;

                GameEngine.ScreenWidth = Raylib.GetScreenWidth();
                GameEngine.ScreenHeight = Raylib.GetScreenHeight();

                GameEngine.Lockstep(GameEngine.Time, 1.0f / 60, 1.0f / 10);
                /*if (FrameTime < 0.5f)
					Update(FrameTime);
				else
					Console.WriteLine("Skipping update, frame time {0} s", FrameTime);*/

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.PURPLE);
                Raylib.DrawTextureTiled(BackgroundNoise, new Rectangle(0, 0, BackgroundNoise.width, BackgroundNoise.height), new Rectangle(0, 0, GameEngine.ScreenWidth, GameEngine.ScreenHeight), Vector2.Zero, 0, 1, Color.WHITE);

                Raylib.BeginMode2D(GameEngine.GameCamera);
                GameEngine.CurrentDrawState = DrawState.WORLD;
                GameEngine.DrawWorld();
                Raylib.EndMode2D();

                GameEngine.CurrentDrawState = DrawState.SCREEN;
                GameEngine.DrawScreen();
                Raylib.EndDrawing();

                GameEngine.CurrentDrawState = DrawState.NONE;
            }

            Raylib.CloseWindow();
        }
    }
}