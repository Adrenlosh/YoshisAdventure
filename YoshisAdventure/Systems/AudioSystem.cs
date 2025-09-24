using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
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
    public static class AudioSystem
    {
        private static ContentManager _content;
        private static Dictionary<string, Audio> _audioAssets;
        private static Dictionary<string, SoundEffect> _loadedSoundEffects;
        private static Dictionary<string, Song> _loadedSongs;

        private static float previousVolume = 1.0f;
        private static bool isMute = false;

        private static MiniAudioEngine engine;
        private static AudioPlaybackDevice playbackDevice;
        private static SoundPlayer songPlayer;

        private static Dictionary<string, SoundPlayer> _activeSfxPlayers;
        private static List<SoundPlayer> _sfxPlayersPool;

        public static void Initialize(ContentManager content)
        {
            _content = content;
            _audioAssets = new Dictionary<string, Audio>();
            _loadedSoundEffects = new Dictionary<string, SoundEffect>();
            _loadedSongs = new Dictionary<string, Song>();
            _activeSfxPlayers = new Dictionary<string, SoundPlayer>();
            _sfxPlayersPool = new List<SoundPlayer>();

            engine = new MiniAudioEngine();
            var format = AudioFormat.Dvd;
            DeviceInfo defaultDevice = engine.PlaybackDevices.FirstOrDefault(x => x.IsDefault);
            playbackDevice = engine.InitializePlaybackDevice(defaultDevice, format);

            LoadAudioConfig();
        }

        public static void Update(GameTime gameTime)
        {
            CleanupFinishedSfxPlayers();
        }

        private static void CleanupFinishedSfxPlayers()
        {
            var finishedPlayers = new List<string>();

            foreach (var kvp in _activeSfxPlayers)
            {
                if (kvp.Value.State == PlaybackState.Stopped)
                {
                    finishedPlayers.Add(kvp.Key);
                }
            }

            foreach (var key in finishedPlayers)
            {
                var player = _activeSfxPlayers[key];
                player.Stop();
                player.Dispose();
                _activeSfxPlayers.Remove(key);
            }
        }

        private static void LoadAudioConfig()
        {
            try
            {
                string configPath = Path.Combine(_content.RootDirectory, "Audio", "Audio.xml");

                using Stream stream = TitleContainer.OpenStream(configPath);
                using XmlReader xmlReader = XmlReader.Create(stream);
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlReader);

                XmlNodeList songNodes = doc.SelectNodes("//Audio/Songs/Song");
                if (songNodes != null)
                {
                    foreach (XmlNode songNode in songNodes)
                    {
                        var audio = new Audio
                        {
                            Name = songNode.Attributes["name"]?.Value,
                            File = songNode.Attributes["file"]?.Value,
                            IsSFX = false,
                            IsLooping = bool.Parse(songNode.Attributes["repeat"]?.Value ?? "false")
                        };

                        if (songNode.Attributes["volume"] != null)
                        {
                            audio.Volume = float.Parse(songNode.Attributes["volume"].Value);
                        }

                        if (songNode.Attributes["repeatStartSecond"] != null)
                        {
                            int repeatStartSecond = int.Parse(songNode.Attributes["repeatStartSecond"].Value);
                            audio.RepeatStartTime = TimeSpan.FromSeconds(repeatStartSecond);
                        }

                        if (!string.IsNullOrEmpty(audio.Name))
                        {
                            _audioAssets[audio.Name] = audio;
                        }
                    }
                }

                XmlNodeList sfxNodes = doc.SelectNodes("//Audio/SoundEffects/SoundEffect");
                if (sfxNodes != null)
                {
                    foreach (XmlNode sfxNode in sfxNodes)
                    {
                        var audio = new Audio
                        {
                            Name = sfxNode.Attributes["name"]?.Value,
                            File = sfxNode.Attributes["file"]?.Value,
                            IsSFX = true,
                            IsLooping = bool.Parse(sfxNode.Attributes["repeat"]?.Value ?? "false")
                        };

                        if (sfxNode.Attributes["volume"] != null)
                        {
                            audio.Volume = float.Parse(sfxNode.Attributes["volume"].Value);
                        }

                        if (!string.IsNullOrEmpty(audio.Name))
                        {
                            _audioAssets[audio.Name] = audio;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load audio configuration", ex);
            }
        }

        public static void PlaySoundEffect(string soundName)
        {
            if (_audioAssets.TryGetValue(soundName, out Audio audio) && audio.IsSFX)
            {
                StopSoundEffect(soundName);


                string soundPath = Path.Combine(_content.RootDirectory, "Audio", "SFX", audio.File);

                var format = AudioFormat.Dvd;
                var streamProvider = new StreamDataProvider(engine, format, File.OpenRead(soundPath));
                var sfxPlayer = new SoundPlayer(engine, format, streamProvider);

                playbackDevice.MasterMixer.AddComponent(sfxPlayer);
                sfxPlayer.Volume = audio.Volume;
                sfxPlayer.IsLooping = audio.IsLooping;
                sfxPlayer.Play();

                _activeSfxPlayers[soundName] = sfxPlayer;


                if (!audio.IsLooping)
                {
                    sfxPlayer.PlaybackEnded += (s, e) =>
                    {
                        if (_activeSfxPlayers.ContainsKey(soundName))
                        {
                            var player = _activeSfxPlayers[soundName];
                            player.Stop();
                            player.Dispose();
                            _activeSfxPlayers.Remove(soundName);
                        }
                    };
                }
            }

        }
        
        public static void StopSoundEffect(string soundName)
        {
            if (_activeSfxPlayers.TryGetValue(soundName, out SoundPlayer player))
            {
                player.Stop();
                player.Dispose();
                _activeSfxPlayers.Remove(soundName);
            }
        }

        public static void StopAllSoundEffects()
        {
            foreach (var player in _activeSfxPlayers.Values)
            {
                player.Stop();
                player.Dispose();
            }
            _activeSfxPlayers.Clear();
        }

        public static void PlaySong(string songName)
        {
            if (_audioAssets.TryGetValue(songName, out Audio song) && !song.IsSFX)
            {
                StopSong();
                string songPath = Path.Combine(_content.RootDirectory, "Audio", "Song", song.File);

                var format = AudioFormat.Dvd;
                var streamDataProvider = new StreamDataProvider(engine, format, File.OpenRead(songPath));
                songPlayer = new SoundPlayer(engine, format, streamDataProvider);
                playbackDevice.MasterMixer.AddComponent(songPlayer);

                playbackDevice.Start();

                songPlayer.Volume = song.Volume;
                songPlayer.IsLooping = song.IsLooping;
                songPlayer.SetLoopPoints(song.RepeatStartTime);
                songPlayer.Play();

                songPlayer.PlaybackEnded += (s, e) =>
                {
                    if (song.IsLooping && songPlayer != null)
                    {
                        songPlayer.Seek(song.RepeatStartTime);
                        songPlayer.Play();
                    }
                };
            }
        }

        public static void StopSong()
        {
            if (songPlayer != null)
            {
                songPlayer.Stop();
                songPlayer.Dispose();
                songPlayer = null;
            }
        }

        public static void PauseSong()
        {
            songPlayer?.Pause();
        }

        public static void ResumeSong()
        {
            songPlayer?.Play();
        }

        public static void SetSongVolume(float volume)
        {
            if (songPlayer != null)
            {
                songPlayer.Volume = volume;
            }
        }

        public static void SetSoundEffectVolume(float volume)
        {
            foreach (var player in _activeSfxPlayers.Values)
            {
                player.Volume = volume;
            }
        }

        public static void SetMasterVolume(float volume)
        {
            SetSongVolume(volume);
            SetSoundEffectVolume(volume);
        }

        public static void Mute()
        {
            if (!isMute)
            {
                previousVolume = songPlayer?.Volume ?? 1.0f;
                SetMasterVolume(0f);
                isMute = true;
            }
        }

        public static void Unmute()
        {
            if (isMute)
            {
                SetMasterVolume(previousVolume);
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

        public static void Dispose()
        {
            StopAllSoundEffects();
            StopSong();
            foreach (var soundEffect in _loadedSoundEffects.Values)
            {
                soundEffect.Dispose();
            }
            foreach (var song in _loadedSongs.Values)
            {
                song.Dispose();
            }
            playbackDevice?.Stop();
            playbackDevice?.Dispose();
            engine?.Dispose();
            foreach (var player in _sfxPlayersPool)
            {
                player.Dispose();
            }
            _loadedSoundEffects.Clear();
            _loadedSongs.Clear();
            _audioAssets.Clear();
            _activeSfxPlayers.Clear();
            _sfxPlayersPool.Clear();
            _content = null;
        }
    }
}