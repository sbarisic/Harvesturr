using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Raylib_cs;

namespace Harvesturr
{
    static class GameMusic
    {
        public static MultiSoundRef Sfx_ExplosionSmall;
        public static MultiSoundRef Sfx_ExplosionBig;
        public static MultiSoundRef Sfx_Hit;

        static Dictionary<string, Music> Sfx_Music;

        static bool PlayingMusic;
        static Music CurrentMusic;

        public static void Init()
        {
            Raylib.InitAudioDevice();

            Sfx_ExplosionSmall = new MultiSoundRef(ResMgr.LoadSound("explosion_small_1"), ResMgr.LoadSound("explosion_small_2"));
            Sfx_ExplosionBig = new MultiSoundRef(ResMgr.LoadSound("explosion_big_1"), ResMgr.LoadSound("explosion_big_2"));
            Sfx_Hit = new MultiSoundRef(ResMgr.LoadSound("hit_1"), ResMgr.LoadSound("hit_2"), ResMgr.LoadSound("hit_3"));

            // TODO: Music
            // https://www.newgrounds.com/search/conduct/audio?sort=score-desc&suitabilities=e%2Ct&match=tags&tags=chiptune

            // https://vectx.newgrounds.com/audio/
            // https://soundcloud.com/user-652567881

            string[] MusicNames = ResMgr.GetAllMusic();
            Sfx_Music = new Dictionary<string, Music>();

            foreach (string Name in MusicNames)
                Sfx_Music.Add(Name, ResMgr.LoadMusic(Name));

            //PlayMusic(Sfx_Music[Utils.Random(MusicNames)]);
        }

        public static void Update()
        {
            if (PlayingMusic)
            {
                Raylib.SetMusicVolume(CurrentMusic, 0.1f);
                Raylib.UpdateMusicStream(CurrentMusic);
            }
        }

        public static void PlaySfx(GameUnit Unit, SoundRef Sfx)
        {
            PlaySfx(Unit.Position, Sfx);
        }

        public static void PlaySfx(Vector2 Pos, SoundRef Sfx)
        {
            const float SoundFalloffDist = 600;

            if (Sfx == null)
                return;

            Sound Snd = Sfx;
            float ZoomMul = 1;
            float Volume = 1;

            if (GameEngine.Zoom < 2)
                ZoomMul = GameEngine.Zoom / 3;

            float Dist = Vector2.Distance(GameEngine.GameCamera.target, Pos);
            // Console.WriteLine("Sound dist: {0}", Dist);

            Volume = (SoundFalloffDist - Dist) / SoundFalloffDist;
            Volume = Volume * ZoomMul;

            if (Volume > 0.001f)
            {
                Raylib.SetSoundVolume(Snd, Volume);
                Raylib.PlaySoundMulti(Snd);
            }
        }

        public static void PlayMusic(Music M)
        {
            float Volume = 0.1f;
            CurrentMusic = M;

            Raylib.PlayMusicStream(CurrentMusic);
            Raylib.SetMusicVolume(CurrentMusic, Volume);
            PlayingMusic = true;
        }
    }
}
