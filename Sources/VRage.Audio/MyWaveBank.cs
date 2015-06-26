﻿using System;
using System.Collections.Generic;
using System.IO;
using VRage.Data.Audio;
using VRage.FileSystem;
using VRage.Library.Utils;

namespace VRage.Audio
{
    class MyWaveBank : IDisposable
    {
        Dictionary<string, MyInMemoryWave> m_waves = new Dictionary<string, MyInMemoryWave>();

        public int Count { get { return m_waves.Count; } }

        public bool Add(MySoundData cue, MyAudioWave cueWave)
        {
            string[] files = { cueWave.Start, cueWave.Loop, cueWave.End };
            SharpDX.Multimedia.WaveFormatEncoding encoding = SharpDX.Multimedia.WaveFormatEncoding.Unknown;
            bool result = true;
            int i = 0;
            foreach (var waveFilename in files)
            {
                i++;
                if (string.IsNullOrEmpty(waveFilename))
                    continue;

                var fsPath = Path.IsPathRooted(waveFilename) ? waveFilename : Path.Combine(MyFileSystem.ContentPath, "Audio", waveFilename);
                var exists = MyFileSystem.FileExists(fsPath);
                result |= exists;
                if (exists)
                {
                    try
                    {
                        MyInMemoryWave wave = new MyInMemoryWave(cue, fsPath);
                        if (i != 2)
                            wave.Buffer.LoopCount = 0;
                        m_waves[waveFilename] = wave;

                        // check the formats
                        if (encoding == SharpDX.Multimedia.WaveFormatEncoding.Unknown)
                        {
                            encoding = wave.WaveFormat.Encoding;
                        }

                        // check the formats
                        if (wave.WaveFormat.Encoding == SharpDX.Multimedia.WaveFormatEncoding.Unknown)
                        {
                            if (MyAudio.OnSoundError != null)
                            {
                                var msg = string.Format("Unknown audio encoding '{0}', '{1}'", cue.SubtypeId.ToString(), waveFilename);
                                MyAudio.OnSoundError(cue, msg);
                            }
                            result = false;
                        }

                        // 3D sounds must be mono
                        if (cueWave.Type == MySoundDimensions.D3 && wave.WaveFormat.Channels != 1)
                        {
                            if (MyAudio.OnSoundError != null)
                            {
                                var msg = string.Format("3D sound '{0}', '{1}' must be in mono, got {2} channels", cue.SubtypeId.ToString(), waveFilename, wave.WaveFormat.Channels);
                                MyAudio.OnSoundError(cue, msg);
                            }
                            result = false;
                        }

                        // all parts of the sound must have the same encoding
                        if (wave.WaveFormat.Encoding != encoding)
                        {
                            if (MyAudio.OnSoundError != null)
                            {
                                var msg = string.Format("Inconsistent sound encoding in '{0}', '{1}', got '{2}', expected '{3}'", cue.SubtypeId.ToString(), waveFilename, wave.WaveFormat.Encoding, encoding);
                                MyAudio.OnSoundError(cue, msg);
                            }
                            result = false;
                        }
                    }
                    catch (Exception e)
                    {
                        if (MyAudio.OnSoundError != null)
                        {
                            var msg = string.Format("Unable to load audio file: '{0}', '{1}': {2}", cue.SubtypeId.ToString(), waveFilename, e.ToString());
                            MyAudio.OnSoundError(cue, msg);
                        }
                        result = false;
                    }
                    // Second catch shouldn't be needed according to http://stackoverflow.com/questions/5345436/net-exception-catch-block
                    // all non-exceptions will be wrapped as type derived from Exception and caught above.
                    //catch
                    //{
                    //    if (MyAudio.OnSoundError != null)
                    //    {
                    //        var msg = string.Format("Unable to load audio file: '{0}', '{1}': {2}", cue.SubtypeId.ToString(), waveFilename, "Something went horribly wrong");
                    //        MyAudio.OnSoundError(cue, msg);
                    //    }
                    //    result = false;
                    //}
                }
                else
                {
                    if (MyAudio.OnSoundError != null)
                    {
                        var msg = string.Format("Unable to find audio file: '{0}', '{1}'", cue.SubtypeId.ToString(), waveFilename);
                        MyAudio.OnSoundError(cue, msg);
                    }
                    result = false;
                }
            }
            return result;
        }

        public void Dispose()
        {
            foreach (var wave in m_waves)
            {
                wave.Value.Dispose();
            }
        }

        public MyInMemoryWave GetWave(string filename)
        {
            if (string.IsNullOrEmpty(filename) || !m_waves.ContainsKey(filename))
                return null;

            return m_waves[filename];
        }

        public List<MyWaveFormat> GetWaveFormats()
        {
            List<MyWaveFormat> output = new List<MyWaveFormat>();
            foreach (var wave in m_waves)
            {
                MyWaveFormat myWaveFormat = new MyWaveFormat()
                {
                    Encoding = wave.Value.WaveFormat.Encoding,
                    Channels = wave.Value.WaveFormat.Channels,
                    SampleRate = wave.Value.WaveFormat.SampleRate,
                    WaveFormat = wave.Value.WaveFormat
                };
                if (!output.Contains(myWaveFormat))
                    output.Add(myWaveFormat);
                else
                {

                }
            }
            return (output);
        }
    }
}
