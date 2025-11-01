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
        private static Dictionary<string, byte[]> _SFXCache;
        private static float previousVolume =1.0f;
        private static bool isMute = false;

        private class PlaybackInstance
        {
            public SoundPlayer Player { get; set; }
            public StreamDataProvider Provider { get; set; }
            public Stream Stream { get; set; }
        }

        private static Dictionary<string, PlaybackInstance> _activeSfxPlayers;
        private static List<SoundPlayer> _sfxPlayersPool;

        public static void Initialize(ContentManager content, MiniAudioEngine engine, AudioPlaybackDevice playbackDevice)
        {
            _content = content;
            _SFXs = new Dictionary<string, SFX>();
            _SFXCache = new Dictionary<string, byte[]>();
            _activeSfxPlayers = new Dictionary<string, PlaybackInstance>();
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

                    if (sfxNode.Attributes["singleInstance"] != null)
                    {
                        sfx.SingleInstance = bool.Parse(sfxNode.Attributes["singleInstance"].Value);
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
                if (kvp.Value.Player.State == PlaybackState.Stopped)
                {
                    finishedPlayers.Add(kvp.Key);
                }
            }

            foreach (var key in finishedPlayers)
            {
                var instance = _activeSfxPlayers[key];
                try
                {
                    instance.Player.Stop();
                }
                catch { }
                try
                {
                    playbackDevice.MasterMixer.RemoveComponent(instance.Player);
                }
                catch { }
                instance.Player.Dispose();
                instance.Provider?.Dispose();
                instance.Stream?.Dispose();
                _activeSfxPlayers.Remove(key);
            }
        }

        public static void Play(string sfxName)
        {
            if (_SFXs.ContainsKey(sfxName) && _SFXs[sfxName].SingleInstance && _activeSfxPlayers.ContainsKey(sfxName))
                return;

            if (_SFXs.TryGetValue(sfxName, out SFX sfx))
            {
                if (sfx.SingleInstance)
                {
                    Stop(sfxName);
                }

                if (!_SFXCache.ContainsKey(sfxName))
                {
                    string sfxPath = Path.Combine(_content.RootDirectory, "Audio", "SFX", sfx.File);
                    using Stream stream = TitleContainer.OpenStream(sfxPath);
                    using var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    _SFXCache.Add(sfxName, ms.ToArray());
                }

                var buffer = _SFXCache[sfxName];
                var playbackStream = new MemoryStream(buffer, writable: false);
                var streamProvider = new StreamDataProvider(engine, AudioFormat.Dvd, playbackStream);
                var sfxPlayer = new SoundPlayer(engine, AudioFormat.Dvd, streamProvider);

                playbackDevice.MasterMixer.AddComponent(sfxPlayer);
                sfxPlayer.Volume = sfx.Volume;
                sfxPlayer.Play();

                var instance = new PlaybackInstance
                {
                    Player = sfxPlayer,
                    Provider = streamProvider,
                    Stream = playbackStream
                };

                _activeSfxPlayers[sfxName] = instance;
            }
            else
            {
                throw new ArgumentException(nameof(sfxName));
            }
        }

        public static void Stop(string sfxName)
        {
            if (_activeSfxPlayers.TryGetValue(sfxName, out PlaybackInstance instance))
            {
                try
                {
                    instance.Player.Stop();
                }
                catch { }
                try
                {
                    playbackDevice.MasterMixer.RemoveComponent(instance.Player);
                }
                catch { }
                instance.Player.Dispose();
                instance.Provider?.Dispose();
                instance.Stream?.Dispose();
                _activeSfxPlayers.Remove(sfxName);
            }
        }

        public static void StopAll()
        {
            foreach (var instance in _activeSfxPlayers.Values)
            {
                try
                {
                    instance.Player.Stop();
                }
                catch { }
                try
                {
                    playbackDevice.MasterMixer.RemoveComponent(instance.Player);
                }
                catch { }
                instance.Player.Dispose();
                instance.Provider?.Dispose();
                instance.Stream?.Dispose();
            }
            _activeSfxPlayers.Clear();
        }

        public static void SetVolume(float volume)
        {
            foreach (var instance in _activeSfxPlayers.Values)
            {
                instance.Player.Volume = volume;
            }
        }

        public static void Mute()
        {
            if (!isMute)
            {
                previousVolume = _activeSfxPlayers.Values.FirstOrDefault()?.Player.Volume ??1.0f;
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
            return _activeSfxPlayers.ContainsKey(sfxName) && _activeSfxPlayers[sfxName].Player.State == PlaybackState.Playing;
        }

        public static void Dispose()
        {
            StopAll();
            playbackDevice?.Dispose();
            engine?.Dispose();
            _activeSfxPlayers?.Clear();
            _sfxPlayersPool?.Clear();
            _SFXs?.Clear();
            _SFXCache?.Clear();
        }
    }
}
