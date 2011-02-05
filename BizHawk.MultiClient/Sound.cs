﻿using System;
using BizHawk.Emulation.Sound;
using SlimDX.DirectSound;
using SlimDX.Multimedia;

namespace BizHawk.MultiClient
{
    public class Sound : IDisposable
    {
        public bool SoundEnabled = true;
        public bool Muted = false;
        private bool disposed = false;

        private SecondarySoundBuffer DSoundBuffer;
        private byte[] SoundBuffer;
        private const int BufferSize = 4410 * 2 * 2; // 1/10th of a second, 2 bytes per sample, 2 channels;
        private int SoundBufferPosition;

        private BufferedAsync semisync = new BufferedAsync();
        
        public Sound(IntPtr handle, DirectSound device)
        {
            device.SetCooperativeLevel(handle, CooperativeLevel.Priority);

            var format = new WaveFormat();
            format.SamplesPerSecond = 44100;
            format.BitsPerSample = 16;
            format.Channels = 2;
            format.FormatTag = WaveFormatTag.Pcm;
            format.BlockAlignment = 4;
            format.AverageBytesPerSecond = format.SamplesPerSecond * format.Channels * (format.BitsPerSample / 8);

            var desc = new SoundBufferDescription();
            desc.Format = format;
            desc.Flags = BufferFlags.GlobalFocus | BufferFlags.Software | BufferFlags.GetCurrentPosition2;
            desc.SizeInBytes = BufferSize;

            DSoundBuffer = new SecondarySoundBuffer(device, desc);
            SoundBuffer = new byte[BufferSize];

            disposed = false;
        }

        public void StartSound()
        {
            if (disposed) throw new ObjectDisposedException("Sound");
            if (SoundEnabled == false) return;

            DSoundBuffer.Write(SoundBuffer, 0, LockFlags.EntireBuffer);

            DSoundBuffer.CurrentPlayPosition = 0;
            DSoundBuffer.Play(0, PlayFlags.Looping);
        }

        public void StopSound()
        {
            for (int i = 0; i < SoundBuffer.Length; i++)
                SoundBuffer[i] = 0;
            DSoundBuffer.Write(SoundBuffer, 0, LockFlags.EntireBuffer);
            DSoundBuffer.Stop();
        }

        public void Dispose()
        {
            if (disposed) return;
            if (DSoundBuffer != null && DSoundBuffer.Disposed == false)
            {
                DSoundBuffer.Dispose();
                DSoundBuffer = null;
            }
        }

		static int circularDist(int from, int to, int size)
		{
			if(size == 0)
				return 0;
			int diff = (to - from);
			while(diff < 0)
				diff += size;
			return diff;
		}

		int soundoffset;
		int SNDDXGetAudioSpace()
		{
			int playcursor = DSoundBuffer.CurrentPlayPosition;
			int writecursor = DSoundBuffer.CurrentWritePosition;

			int curToWrite = circularDist(soundoffset, writecursor, BufferSize);
			int curToPlay = circularDist(soundoffset, playcursor, BufferSize);

			if(curToWrite < curToPlay)
				return 0; // in-between the two cursors. we shouldn't write anything during this time.

			return curToPlay / 4;
		}

        public void UpdateSound(ISoundProvider soundProvider)
        {
            if (SoundEnabled == false || disposed)
                return;

			int samplesNeeded = SNDDXGetAudioSpace()*2;
			if (samplesNeeded == 0)
				return;

            short[] samples = new short[samplesNeeded];
			//Console.WriteLine(samplesNeeded/2);

            if (soundProvider != null && Muted == false)
            {
                semisync.BaseSoundProvider = soundProvider;
                semisync.GetSamples(samples);
            }

			int cursor = soundoffset;
			for (int i = 0; i < samples.Length; i++)
			{
				short s = samples[i];
				SoundBuffer[cursor++] = (byte)(s & 0xFF);
				SoundBuffer[cursor++] = (byte)(s >> 8);

				if (cursor >= SoundBuffer.Length)
					cursor = 0;
			}

            DSoundBuffer.Write(SoundBuffer, 0, LockFlags.EntireBuffer);

			soundoffset += samplesNeeded * 2;
			soundoffset %= BufferSize;
        }
    }
}