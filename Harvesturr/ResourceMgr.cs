using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Raylib_cs;

namespace Harvesturr
{
    static class ResMgr
    {
        static Dictionary<string, Texture2DRef> Textures = new Dictionary<string, Texture2DRef>();

        public static void InitFileWatcher()
        {
            FileSystemWatcher FSW = new FileSystemWatcher("data/");
            FSW.Changed += FSW_Changed;
            FSW.Created += FSW_Changed;
            FSW.Deleted += FSW_Changed;
            FSW.Renamed += FSW_Changed;
            FSW.IncludeSubdirectories = true;
            FSW.EnableRaisingEvents = true;
        }

        private static void FSW_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Renamed)
            {
                string FullPath = Path.GetFileName(e.FullPath).ToLower();
                string FileName = Path.GetFileNameWithoutExtension(FullPath);

                if (Path.GetExtension(FullPath) != ".png")
                    return;


                foreach (KeyValuePair<string, Texture2DRef> Tex in Textures)
                {
                    if (Tex.Key == FileName)
                        Tex.Value.MarkForReload = true;
                }
            }
        }

        public static Texture2DRef LoadTexture(string Name)
        {
            if (Textures.ContainsKey(Name))
                return Textures[Name];

            Texture2DRef Tex = new Texture2DRef("data/textures/" + Name + ".png");
            Textures.Add(Name, Tex);
            return Tex;
        }

        public static string LoadMapCSV(string MapName, string Name)
        {
            return File.ReadAllText(string.Format("data/maps/{0}/{1}.csv", MapName, Name));
        }
    }

    class Texture2DRef
    {
        public string FullPath;
        public bool MarkForReload;
        Texture2D Texture;

        public int width
        {
            get
            {
                return Texture.width;
            }
        }

        public int height
        {
            get
            {
                return Texture.height;
            }
        }

        public Texture2DRef(string FullPath)
        {
            this.FullPath = FullPath;
            this.Texture = Raylib.LoadTexture(FullPath);
            MarkForReload = false;
        }

        public void Update()
        {
            if (MarkForReload)
            {
                MarkForReload = false;

                Raylib.UnloadTexture(Texture);
                Texture = Raylib.LoadTexture(FullPath);
            }
        }

        public static implicit operator Texture2D(Texture2DRef Ref)
        {
            Ref.Update();
            return Ref.Texture;
        }
    }
}
