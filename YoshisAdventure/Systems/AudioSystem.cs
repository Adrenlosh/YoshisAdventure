using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
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
        private static WaveOutEvent waveOutEvent;
        private static VorbisWaveReader vorbisWaveReader;

        private static WaveFileReader waveFileReader;
        private static WaveOutEvent waveOutEventSFX;

        private static string _currentSongName;

        private static float previousVolume = 1.0f;
        private static bool isMute = false;

        // 添加字段
        private static LoopStream loopStream;

        public static void Initialize(ContentManager content)
        {
            _content = content;
            _audioAssets = new Dictionary<string, Audio>();
            _loadedSoundEffects = new Dictionary<string, SoundEffect>();
            _loadedSongs = new Dictionary<string, Song>();

            waveOutEvent = new WaveOutEvent();
            waveOutEvent.PlaybackStopped += OnPlaybackStopped;

            waveOutEventSFX = new WaveOutEvent();

            LoadAudioConfig();
        }

        public static void Update(GameTime gameTime)
        {
        }

        private static void LoadAudioConfig()
        {
            try
            {
                string configPath = Path.Combine(_content.RootDirectory, "Audio", "Audio.xml");

                if (!File.Exists(configPath))
                {
                    throw new FileNotFoundException($"Audio config file not found: {configPath}");
                }

                XmlDocument doc = new XmlDocument();
                doc.Load(configPath);
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
                            IsLooping = false
                        };

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
                if (!_loadedSoundEffects.TryGetValue(audio.File, out SoundEffect soundEffect))
                {
                    string soundPath = Path.Combine(_content.RootDirectory, "Audio", "SFX", audio.File);
                    soundEffect = _content.Load<SoundEffect>(soundPath);
                    _loadedSoundEffects[audio.File] = soundEffect;
                    if (waveOutEvent != null)
                    {
                        waveFileReader = new WaveFileReader(soundPath);
                        waveOutEventSFX.Init(waveFileReader);
                        waveOutEventSFX.Play();
                    }
                }
            }
        }
        public static void PlaySong(string songName)
        {
            if (_audioAssets.TryGetValue(songName, out Audio audio) && !audio.IsSFX)
            {
                string songPath = Path.Combine(_content.RootDirectory, "Audio", "Song", audio.File);
                vorbisWaveReader = new VorbisWaveReader(songPath);

                loopStream = new LoopStream(vorbisWaveReader)
                {
                    EnableLooping = audio.IsLooping,
                    LoopStart = audio.RepeatStartTime
                };

                waveOutEvent.Init(loopStream);
                waveOutEvent.Play();
                _currentSongName = songName;
            }
        }

        public static void StopSong()
        {
            waveOutEvent.Stop();
        }

        public static void PauseAudio()
        {
            waveOutEvent.Pause();
        }

        public static void ResumeAudio()
        {
            waveOutEvent.Play();
        }

        public static void SetSongVolume(float volume)
        {
            waveOutEvent.Volume = volume;
        }

        public static void SetSoundEffectVolume(float volume)
        {
            waveOutEventSFX.Volume = volume;
        }

        public static void Mute()
        {
            previousVolume = waveOutEvent.Volume;
            waveOutEvent.Volume = 0;
            isMute = true;
        }

        public static void Unmute()
        {
            waveOutEvent.Volume = previousVolume;
            isMute = false;
        }

        public static void ToggleMute()
        {
            if(isMute)
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

            _loadedSoundEffects.Clear();
            _loadedSongs.Clear();
            _audioAssets.Clear();
            vorbisWaveReader?.Dispose();
            waveOutEvent?.Dispose();
        }

        private static void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            //if (_audioAssets.TryGetValue(_currentSongName, out Audio audio) && !audio.IsSFX)
            //{
            //    if (audio.IsLooping)
            //    {
            //        if (vorbisWaveReader != null)
            //        {
            //            vorbisWaveReader.CurrentTime = audio.RepeatStartTime;
            //            waveOutEvent.Play();
            //        }
            //    }
            //}
        }
    }
}