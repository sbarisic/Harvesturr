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
using NuklearDotNet;

namespace Harvesturr {
    unsafe class RaylibTexture {
        public Texture2D Texture;
        public Image Image;

        public RaylibTexture(int W, int H, IntPtr Data) {
            /*Image = Raylib.GenImageColor(W, H, Color.ORANGE);

			for (int y = 0; y < H; y++)
				for (int x = 0; x < W; x++) {
					NkColor C = ((NkColor*)Data)[y * W + x];

					Raylib.ImageDrawPixel(ref Image, x, y, new Color(C.R, C.G, C.B, C.A));
				}*/

            Image = new Image();
            Image.width = W;
            Image.height = H;
            Image.mipmaps = 1;
            Image.format = PixelFormat.PIXELFORMAT_UNCOMPRESSED_R8G8B8A8;
            Image.data = Data;

            Texture = Raylib.LoadTextureFromImage(Image);
            Raylib.SetTextureFilter(Texture, TextureFilter.TEXTURE_FILTER_POINT);
            Raylib.SetTextureWrap(Texture, TextureWrap.TEXTURE_WRAP_CLAMP);
        }
    }

    unsafe class RaylibDevice : NuklearDeviceTex<RaylibTexture>, IFrameBuffered {
        RenderTexture2D RT;

        public RaylibDevice() {
            CreateRT();
        }

        void CreateRT() {
            int W = Raylib.GetScreenWidth();
            int H = Raylib.GetScreenHeight();
            RT = Raylib.LoadRenderTexture(W, H);
        }

        public override RaylibTexture CreateTexture(int W, int H, IntPtr Data) {
            return new RaylibTexture(W, H, Data);
        }

        public void BeginBuffering() {
            Raylib.BeginTextureMode(RT);
            Raylib.ClearBackground(Color.BLANK);
        }

        NkVertex[] Verts;
        ushort[] Inds;

        public override void SetBuffer(NkVertex[] VertexBuffer, ushort[] IndexBuffer) {
            Verts = VertexBuffer;
            Inds = IndexBuffer;
        }

        static void rlColor(NkColor Clr) {
            Rlgl.rlColor4f(Clr.R / 255.0f, Clr.G / 255.0f, Clr.B / 255.0f, Clr.A / 255.0f);
        }

        static void DrawVert(NkVertex V) {
            rlColor(V.Color);
            Rlgl.rlTexCoord2f(V.UV.X, V.UV.Y);
            Rlgl.rlVertex2f(V.Position.X, V.Position.Y);
        }

        static void Draw(NkVertex v1, NkVertex v2, NkVertex v3) {
            DrawVert(v1);
            DrawVert(v3);
            DrawVert(v2);
            DrawVert(v2);
        }

        public override void Render(NkHandle Userdata, RaylibTexture Texture, NkRect ClipRect, uint Offset, uint Count) {
            Rlgl.rlDisableBackfaceCulling();
            Raylib.BeginScissorMode((int)ClipRect.X, (int)ClipRect.Y, (int)ClipRect.W, (int)ClipRect.H);
            {
                Rlgl.rlSetTexture(Texture.Texture.id);
                Rlgl.rlCheckRenderBatchLimit((int)Count);

                Rlgl.rlBegin(Rlgl.RL_QUADS);
                for (int i = 0; i < Count; i += 3) {
                    NkVertex V1 = Verts[Inds[Offset + i]];
                    NkVertex V2 = Verts[Inds[Offset + i + 1]];
                    NkVertex V3 = Verts[Inds[Offset + i + 2]];

                    Draw(V1, V2, V3);
                }
                Rlgl.rlEnd();

                Rlgl.rlSetTexture(0);
            }
            Raylib.EndScissorMode();
            Rlgl.rlEnableBackfaceCulling();
        }

        public void EndBuffering() {
            Raylib.EndTextureMode();
        }

        public void RenderFinal() {
            if (Raylib.IsWindowResized()) {
                Raylib.UnloadRenderTexture(RT);
                CreateRT();
                NuklearAPI.QueueForceUpdate();
            }

            Raylib.DrawTextureRec(RT.texture, new Rectangle(0, RT.texture.height, RT.texture.width, -RT.texture.height), Vector2.Zero, Color.WHITE);
        }
    }
}
