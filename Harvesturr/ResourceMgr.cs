using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Raylib_cs;

namespace Harvesturr {
	static class ResMgr {
		//static Dictionary<string, Font> Fonts = new Dictionary<string, Font>();
		static Dictionary<string, Texture2DRef> Textures = new Dictionary<string, Texture2DRef>();
		static Dictionary<string, SoundRef> Sounds = new Dictionary<string, SoundRef>();

		public static void InitFileWatcher() {
			FileSystemWatcher FSW = new FileSystemWatcher("data/");
			FSW.Changed += FSW_Changed;
			FSW.Created += FSW_Changed;
			FSW.Deleted += FSW_Changed;
			FSW.Renamed += FSW_Changed;
			FSW.IncludeSubdirectories = true;
			FSW.EnableRaisingEvents = true;
		}

		private static void FSW_Changed(object sender, FileSystemEventArgs e) {
			if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Renamed) {
				string FullPath = Path.GetFileName(e.FullPath).ToLower();
				string FileName = Path.GetFileNameWithoutExtension(FullPath);

				if (Path.GetExtension(FullPath) == ".png") {
					foreach (KeyValuePair<string, Texture2DRef> Tex in Textures) {
						if (Tex.Key == FileName)
							Tex.Value.MarkForReload = true;
					}
				} else if (Path.GetExtension(FullPath) == ".wav") {
					foreach (KeyValuePair<string, SoundRef> S in Sounds) {
						if (S.Key == FileName)
							S.Value.MarkForReload = true;
					}
				}
			}
		}

		public static Font LoadFont(string Name, int Size) {
			/*if (Fonts.ContainsKey(Name))
				return Fonts[Name];*/

			Font Fnt = Raylib.LoadFontEx("data/fonts/" + Name + ".ttf", Size, null, 250);			
			//Fonts.Add(Name, Fnt);
			return Fnt;
		}

		public static Texture2DRef LoadTexture(string Name) {
			if (Textures.ContainsKey(Name))
				return Textures[Name];

			Texture2DRef Tex = new Texture2DRef("data/textures/" + Name + ".png");

			if (!Tex.IsValid && Name != "error")
				return LoadTexture("error");

			Textures.Add(Name, Tex);
			return Tex;
		}

		public static SoundRef LoadSound(string Name) {
			if (Sounds.ContainsKey(Name))
				return Sounds[Name];

			SoundRef Sfx = new SoundRef("data/sfx/" + Name + ".wav");
			Sounds.Add(Name, Sfx);
			return Sfx;
		}

		public static string[] GetAllMusic() {
			return Directory.EnumerateFiles("data/music/", "*.mp3").Select(Path.GetFileNameWithoutExtension).ToArray();
		}

		public static Music LoadMusic(string Name) {
			Music M = Raylib.LoadMusicStream("data/music/" + Name + ".mp3");
			return M;
		}


		public static string LoadMapCSV(string MapName, string Name) {
			return File.ReadAllText(string.Format("data/maps/{0}/{1}.csv", MapName, Name));
		}
	}

	class Texture2DRef {
		public string FullPath;
		public bool MarkForReload;
		Texture2D Texture;

		public int width {
			get {
				return Texture.width;
			}
		}

		public int height {
			get {
				return Texture.height;
			}
		}

		public bool IsValid {
			get; private set;
		}

		public Texture2DRef(string FullPath) {
			this.FullPath = FullPath;
			Texture = Raylib.LoadTexture(FullPath);

			IsValid = Texture.id > 0;
			MarkForReload = false;
		}

		public void Update() {
			if (MarkForReload) {
				MarkForReload = false;

				Raylib.UnloadTexture(Texture);
				Texture = Raylib.LoadTexture(FullPath);
			}
		}

		public static implicit operator Texture2D(Texture2DRef Ref) {
			Ref.Update();
			return Ref.Texture;
		}
	}

	class SoundRef {
		public string FullPath;
		public bool MarkForReload;
		Sound Sound;

		public SoundRef(string FullPath) {
			MarkForReload = false;

			if (FullPath != null) {
				this.FullPath = FullPath;
				this.Sound = Raylib.LoadSound(FullPath);
			}
		}

		public void Update() {
			if (MarkForReload) {
				MarkForReload = false;

				Raylib.UnloadSound(Sound);
				Sound = Raylib.LoadSound(FullPath);
			}
		}

		public virtual Sound GetSound() {
			Update();
			return Sound;
		}

		public static implicit operator Sound(SoundRef Ref) {
			return Ref.GetSound();
		}
	}

	class MultiSoundRef : SoundRef {
		SoundRef[] Sounds;

		public MultiSoundRef(params SoundRef[] Sounds) : base(null) {
			this.Sounds = Sounds;
		}

		public override Sound GetSound() {
			return Utils.Random(Sounds).GetSound();
		}
	}
}
