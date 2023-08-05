using System;
using System.IO;
using System.Threading;
using NAudio.Wave;

namespace murph9.TabPlayer.Songs
{
    public class AudioController : IDisposable
    {
        private readonly Stream _audioStream;

        //https://github.com/naudio/NAudio/blob/master/NAudio.Core/Wave/WaveStreams/WaveStream.cs
        private readonly WaveFileReader _waveStream;
        
        //https://github.com/naudio/NAudio/blob/master/NAudio.WinMM/WaveOutEvent.cs
        private readonly WaveOutEvent _waveOut;

        private double _songPositionOffset = 0;
        public double SongPosition { get {
            // waveStream.Position is buffer sized and rather slow updating
            double t = (double)(_waveOut.GetPosition() * 1000 /1000f);
            return _songPositionOffset + t / _waveOut.OutputWaveFormat.BitsPerSample / _waveOut.OutputWaveFormat.Channels * 8 / _waveOut.OutputWaveFormat.SampleRate;
        } private set {} }

        public AudioController(Stream audioStream) {
            _audioStream = audioStream;

            _waveStream = new WaveFileReader(_audioStream);
            _waveOut = new WaveOutEvent();
            _waveOut.Init(_waveStream);
            Active = true;
        }

        public bool Playing() => _waveOut.PlaybackState == PlaybackState.Playing;
        public bool Paused() => _waveOut.PlaybackState == PlaybackState.Paused;
        
        public void Play() => _waveOut.Play();
        public void Pause() => _waveOut.Pause();
        public void Stop() => _waveOut.Stop();

        public bool Active { get; private set; }
        
        public void Seek(double pos) {
            pos = Math.Max(0, pos);

            double t = (double)(_waveStream.Position * 1000 /1000f); // this uses the buffer size to fix seek issues, note the comment on this.SongPosition
            var streamPos = t / _waveOut.OutputWaveFormat.BitsPerSample / _waveOut.OutputWaveFormat.Channels * 8 / _waveOut.OutputWaveFormat.SampleRate;

            var newTime = TimeSpan.FromSeconds(pos);
            if (newTime < _waveStream.TotalTime) {
                _waveStream.CurrentTime = TimeSpan.FromSeconds(pos);
                _songPositionOffset -= streamPos - pos;
            }
        }

        public void Dispose()
        {
            Active = false;

            _waveOut?.Dispose();
            _waveStream?.Dispose();
            _audioStream?.Dispose();
            
            GC.SuppressFinalize(this);
        }
    }
}