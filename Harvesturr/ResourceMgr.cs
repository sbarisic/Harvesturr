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
        static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

        public static Texture2D LoadTexture(string Name)
        {
            if (Textures.ContainsKey(Name))
                return Textures[Name];

            Texture2D Tex = Raylib.LoadTexture("data/textures/" + Name + ".png");
            Textures.Add(Name, Tex);
            return Tex;
        }

        public static string LoadMapCSV(string MapName, string Name)
        {
            return File.ReadAllText(string.Format("data/maps/{0}/{1}.csv", MapName, Name));
        }
    }
}
