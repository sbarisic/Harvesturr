﻿using System;
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

    // https://doc.mapeditor.org/en/stable/reference/tmx-map-format/
    struct GameTile
    {
        const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;
        const uint FLIPPED_DIAGONALLY_FLAG = 0x20000000;

        public int ID;

        public bool FlipHorizontal;
        public bool FlipVertical;
        public bool FlipDiagonal;

        public GameTile(int IDRaw)
        {
            int FlipH = (int)(IDRaw & FLIPPED_HORIZONTALLY_FLAG);
            int FlipV = (int)(IDRaw & FLIPPED_VERTICALLY_FLAG);
            int FlipD = (int)(IDRaw & FLIPPED_DIAGONALLY_FLAG);

            ID = IDRaw & ~(FlipH | FlipV | FlipD);

            FlipHorizontal = FlipH > 0;
            FlipVertical = FlipV > 0;
            FlipDiagonal = FlipD > 0;
        }

        public override string ToString()
        {
            return string.Format("{0} - H {1}, V {2}, D {3}", ID, FlipHorizontal, FlipVertical, FlipDiagonal);
        }
    }

    static class GameMap
    {
        static Texture2D TilemapTex;

        static int TileWidth = 32;
        static int TileHeight = 32;

        static int X;
        static int Y;
        static int Width;
        static int Height;

        static GameTile[] Tiles;

        static RenderTexture2D MapCached;
        static Camera2D MapCamera;

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
            Tiles = CSV.ParseIntCSV(ResMgr.LoadMapCSV(MapName, MapName), out Width, out Height).Select(T => new GameTile(T)).ToArray();
            TilemapTex = ResMgr.LoadTexture("tileset");

            //Width = 50;
            //Height = 50;

            X = -(TotalWidth / 2);
            Y = -(TotalHeight / 2);

            MapCached = Raylib.LoadRenderTexture(TotalWidth, TotalHeight);
            MapCamera = new Camera2D(Vector2.Zero, Vector2.Zero, 0, 1);
            CacheDrawWorld();
        }

        public static void Update(float Dt)
        {
        }

        static void CacheDrawWorld()
        {
            Raylib.BeginTextureMode(MapCached);
            Raylib.ClearBackground(Color.PINK);
            //Raylib.BeginMode2D(MapCamera);

            int TilesX = TilemapTex.width / TileWidth;
            // int TilesY = TilemapTex.height / TileHeight;

            for (int i = 0; i < Tiles.Length; i++)
            {
                int TileX = Tiles[i].ID % TilesX;
                int TileY = Tiles[i].ID / TilesX;

                int WorldX = (i % Width) * TileWidth;
                int WorldY = (i / Width) * TileWidth;

                Raylib.DrawTextureRec(TilemapTex, new Rectangle(TileX * TileWidth, TileY * TileHeight, TileWidth, TileHeight), new Vector2(WorldX, WorldY), Color.WHITE);
            }

            //Raylib.EndMode2D();
            Raylib.EndTextureMode();
        }

        public static void DrawWorld()
        {
            Raylib.DrawTexture(MapCached.texture, X, Y, Color.WHITE);
        }

        public static bool IsInBounds(Vector2 Pos)
        {
            return Utils.IsInside(GetBounds(), Pos);
        }

        public static Vector2 RandomPoint()
        {
            Rectangle Bounds = GetBounds();

            Vector2 Pos = new Vector2(Bounds.x, Bounds.y);
            Vector2 Pos2 = Pos + new Vector2(Bounds.width, Bounds.height);

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
