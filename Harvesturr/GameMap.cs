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

    // https://doc.mapeditor.org/en/stable/reference/tmx-map-format/
    struct GameTile
    {
        const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;
        const uint FLIPPED_DIAGONALLY_FLAG = 0x20000000;

        public int ID;

        public bool FlipH;
        public bool FlipV;
        public bool FlipD;

        public GameTile(int IDRaw)
        {
            FlipH = (IDRaw & FLIPPED_HORIZONTALLY_FLAG) > 0;
            FlipV = (IDRaw & FLIPPED_VERTICALLY_FLAG) > 0;
            FlipD = (IDRaw & FLIPPED_DIAGONALLY_FLAG) > 0;

            ID = IDRaw & ~((FlipH ? 1 : 0) | (FlipV ? 1 : 0) | (FlipD ? 1 : 0));
        }
    }

    static class GameMap
    {
        static Texture2D MapTex;

        static int TileWidth = 32;
        static int TileHeight = 32;

        static int X;
        static int Y;
        static int Width;
        static int Height;

        static int[] Tiles;

        static int TotalWidth
        {
            get
            {
                return Width * TileWidth;
            }
        }

        static int TotalHeight
        {
            get
            {
                return Height * TileHeight;
            }
        }

        public static Rectangle GetBounds()
        {
            return new Rectangle(X, Y, TotalWidth, TotalHeight);
        }

        public static void Load(string MapName)
        {
            string MapFile = Path.Combine("data/maps", MapName, MapName + ".csv");
            Tiles = CSV.ParseIntCSV(MapFile, out Width, out Height);

            MapTex = ResMgr.LoadTexture(MapName);

            //Width = 50;
            //Height = 50;

            X = -(TotalWidth / 2);
            Y = -(TotalHeight / 2);
        }

        public static void Update(float Dt)
        {
        }

        public static void DrawWorld()
        {
            Raylib.DrawTexture(MapTex, X, Y, Color.WHITE);
        }

        public static bool IsInBounds(Vector2 Pos)
        {
            return Utils.IsInside(new Rectangle(X, Y, Width, Height), Pos);
        }

        public static Vector2 RandomPoint()
        {
            Vector2 Pos = new Vector2(X, Y);
            Vector2 Pos2 = Pos + new Vector2(Width, Height);

            return Utils.Random(Pos, Pos2);
        }

        public static Vector2 RandomMineralPoint()
        {
            Vector2 Pt = Vector2.Zero;

            while (Vector2.Distance(Vector2.Zero, Pt) < 120)
                Pt = RandomPoint();

            return Pt;
        }
    }
}
