using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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
    public static class SFXSystem
    {
        private static ContentManager _content;
        private static MiniAudioEngine engine;
        private static AudioPlaybackDevice playbackDevice;
        private static Dictionary<string, SFX> _SFXs;
        private static float previousVolume = 1.0f;
        private static bool isMute = false;

        private static Dictionary<string, SoundPlayer> _activeSfxPlayers;
        private static List<SoundPlayer> _sfxPlayersPool;

        public static void Initialize(ContentManager content, MiniAudioEngine engine, AudioPlaybackDevice playbackDevice)
        {
            _content = content;
            _SFXs = new Dictionary<string, SFX>();
            _activeSfxPlayers = new Dictionary<string, SoundPlayer>();
            _sfxPlayersPool = new List<SoundPlayer>();
            SFXSystem.engine = engine;
            SFXSystem.playbackDevice = playbackDevice;
            LoadConfig();
        }

        private static void LoadConfig()
        {
            string filePath = Path.Combine(_content.RootDirectory, "Audio", "Audio.xml");
            using Stream stream = TitleContainer.OpenStream(filePath);
            using XmlReader xmlReader = XmlReader.Create(stream);
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlReader);
            XmlNodeList sfxNodes = doc.SelectNodes("//Audio/SoundEffects/SoundEffect");
            if (sfxNodes != null)
            {
                foreach (XmlNode sfxNode in sfxNodes)
                {
                    var sfx = new SFX
                    {
                        Name = sfxNode.Attributes["name"]?.Value,
                        File = sfxNode.Attributes["file"]?.Value,
                    };

                    if (sfxNode.Attributes["volume"] != null)
                    {
                        sfx.Volume = float.Parse(sfxNode.Attributes["volume"].Value);
                    }

                    if (!string.IsNullOrEmpty(sfx.Name))
                    {
                        _SFXs[sfx.Name] = sfx;
                    }
                }
            }
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
                playbackDevice.MasterMixer.RemoveComponent(player);
            }
        }

        public static void Play(string sfxName)
        {
            if (_SFXs.TryGetValue(sfxName, out SFX sfx))
            {
                Stop(sfxName);
                string soundPath = Path.Combine(_content.RootDirectory, "Audio", "SFX", sfx.File);
                var streamProvider = new StreamDataProvider(engine, AudioFormat.Dvd, File.OpenRead(soundPath));
                var sfxPlayer = new SoundPlayer(engine, AudioFormat.Dvd, streamProvider);
                playbackDevice.MasterMixer.AddComponent(sfxPlayer);
                sfxPlayer.Volume = sfx.Volume;
                sfxPlayer.Play();
                _activeSfxPlayers[sfxName] = sfxPlayer;
            }
            else
            {
                throw new ArgumentException(nameof(sfxName));
            }
        }

        public static void Stop(string sfxName)
        {
            if (_activeSfxPlayers.TryGetValue(sfxName, out SoundPlayer player))
            {
                player.Stop();
                player.Dispose();
                _activeSfxPlayers.Remove(sfxName);
            }
        }

        public static void StopAll()
        {
            foreach (var player in _activeSfxPlayers.Values)
            {
                player.Stop();
                player.Dispose();
            }
            _activeSfxPlayers.Clear();
        }

        public static void SetVolume(float volume)
        {
            foreach (var player in _activeSfxPlayers.Values)
            {
                player.Volume = volume;
            }
        }

        public static void Mute()
        {
            if (!isMute)
            {
                previousVolume = _activeSfxPlayers.Values.FirstOrDefault()?.Volume ?? 1.0f;
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

        public static bool IsPlaying(string sfxName)
        {
            return _activeSfxPlayers.ContainsKey(sfxName) && _activeSfxPlayers[sfxName].State == PlaybackState.Playing;
        }   

        public static void Dispose()
        {
            StopAll();
            playbackDevice?.Dispose();
            engine?.Dispose();
            _activeSfxPlayers?.Clear();
            _sfxPlayersPool?.Clear();
            _SFXs?.Clear();
        }
    }
}
