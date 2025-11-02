using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Providers;
using SoundFlow.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using YoshisAdventure.Models;

namespace YoshisAdventure.Systems
{
    public static class SongSystem
    {
        private static ContentManager _content;
        private static MiniAudioEngine engine;
        private static AudioPlaybackDevice playbackDevice;
        private static SoundPlayer soundPlayer;
        private static Dictionary<string, Song> _songs;
        private static float previousVolume = 1.0f;
        private static bool isMute = false;

        private static Stream stream;

        public static void Initialize(ContentManager content, MiniAudioEngine engine, AudioPlaybackDevice playbackDevice)
        {
            _content = content;
            _songs = new Dictionary<string, Song>();
            SongSystem.engine = engine;
            SongSystem.playbackDevice = playbackDevice;
            LoadConfig();
        }

        private static void LoadConfig()
        {
            string filePath = Path.Combine(_content.RootDirectory, "Audio", "Audio.xml");
            using Stream stream = TitleContainer.OpenStream(filePath);
            using XmlReader xmlReader = XmlReader.Create(stream);
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlReader);
            XmlNodeList songNodes = doc.SelectNodes("//Audio/Songs/Song");
            if (songNodes != null)
            {
                foreach (XmlNode songNode in songNodes)
                {
                    var song = new Song
                    {
                        Name = songNode.Attributes["name"]?.Value,
                        File = songNode.Attributes["file"]?.Value,
                        IsLooping = bool.Parse(songNode.Attributes["repeat"]?.Value ?? "false")
                    };

                    if (songNode.Attributes["volume"] != null)
                    {
                        song.Volume = float.Parse(songNode.Attributes["volume"].Value);
                    }

                    if (songNode.Attributes["repeatStartSecond"] != null)
                    {
                        int repeatStartSecond = int.Parse(songNode.Attributes["repeatStartSecond"].Value);
                        song.RepeatStartTime = TimeSpan.FromSeconds(repeatStartSecond);
                    }

                    if (!string.IsNullOrEmpty(song.Name))
                    {
                        _songs[song.Name] = song;
                    }
                }
            }
        }

        public static void Play(string songName)
        {
            if (_songs.TryGetValue(songName, out Song song))
            {
                Stop();
                string songPath = Path.Combine(_content.RootDirectory, "Audio", "Song", song.File);
                stream = TitleContainer.OpenStream(songPath);
                soundPlayer = new SoundPlayer(engine, AudioFormat.Dvd, new StreamDataProvider(engine, AudioFormat.Dvd, stream));
                playbackDevice.MasterMixer.AddComponent(soundPlayer);
                soundPlayer.Volume = song.Volume;
                soundPlayer.IsLooping = song.IsLooping;
                soundPlayer.SetLoopPoints(song.RepeatStartTime);
                soundPlayer.Play();
                soundPlayer.PlaybackEnded += (s, e) =>
                {
                    if (song.IsLooping && soundPlayer != null)
                    {
                        soundPlayer.Seek(song.RepeatStartTime);
                        soundPlayer.Play();
                    }
                };
            }
        }

        public static void Stop()
        {
            if (soundPlayer != null)
            {
                soundPlayer.Stop();
                soundPlayer.Dispose();
                soundPlayer = null;
                stream?.Dispose();
            }
        }

        public static void Pause()
        {
            soundPlayer?.Pause();
        }

        public static void Resume()
        {
            soundPlayer?.Play();
        }

        public static void SetVolume(float volume)
        {
            if (soundPlayer != null)
            {
                soundPlayer.Volume = volume;
            }
        }

        public static void Mute()
        {
            if (!isMute)
            {
                previousVolume = soundPlayer?.Volume ?? 1.0f;
                SetVolume(0f);
                isMute = true;
            }
        }

        public static void Unmute()
        {
            if (isMute)
            {
                SetVolume(previousVolume);
                isMute = false;
            }
        }

        public static void ToggleMute()
        {
            if (isMute)
            {
                Unmute();
            }
            else
            {
                Mute();
            }
        }

        public static float GetSpeed()
        {
            return soundPlayer.PlaybackSpeed;
        }

        public static void SetSpeed(float speed = 3f)
        {
            soundPlayer.PlaybackSpeed = speed;
        }

        public static void NormalSpeed()
        {
            soundPlayer.PlaybackSpeed = 1;
        }

        public static void Dispose()
        {
            Stop();
            playbackDevice?.Dispose();
            engine?.Dispose();
            _songs?.Clear();
            stream?.Dispose();
        }
    }
}