using System;
using Microsoft.Xna.Framework.Audio;

namespace RPG.Core
{
	public class MedievalProceduralAudio
	{
		private readonly int sampleRate = 44100;
		private readonly float[] scaleIntervals = [0, 2, 3, 5, 7, 9, 10, 12]; // Dorian scale intervals (in semitones)
		private readonly Random random = new();

		private float masterVolume = 0.25f;

		public SoundEffect GenerateSong(int durationSeconds = 15)
		{
			int totalSamples = sampleRate * durationSeconds;
			float[] mixBuffer = new float[totalSamples];

			// Generate layers independently
			GenerateDroneLayer(mixBuffer, durationSeconds);
			GenerateMelodyLayer(mixBuffer, durationSeconds);
			GenerateHarmonyLayer(mixBuffer, durationSeconds);
			GenerateRhythmLayer(mixBuffer, durationSeconds);

			// Normalize and convert to PCM
			short[] pcm = new short[totalSamples];
			for (int i = 0; i < totalSamples; i++)
			{
				float sample = Math.Clamp(mixBuffer[i] * masterVolume, -1f, 1f);
				pcm[i] = (short)(sample * short.MaxValue);
			}

			byte[] pcmBytes = new byte[pcm.Length * 2];
			Buffer.BlockCopy(pcm, 0, pcmBytes, 0, pcmBytes.Length);

			return new SoundEffect(pcmBytes, sampleRate, AudioChannels.Mono);
		}

		private void GenerateDroneLayer(float[] buffer, int durationSeconds)
		{
			// Single sustained drone note: D3 (MIDI 50)
			float freq = MidiNoteToFrequency(50);
			int totalSamples = sampleRate * durationSeconds;
			float velocity = 0.2f;

			for (int i = 0; i < totalSamples; i++)
			{
				float t = i / (float)sampleRate;
				float wave = (float)Math.Sin(2 * Math.PI * freq * t);
				// Slow amplitude modulation for a breathing effect
				float ampMod = 0.5f + 0.5f * (float)Math.Sin(2 * Math.PI * 0.1 * t);
				buffer[i] += wave * velocity * ampMod;
			}
		}

		private void GenerateMelodyLayer(float[] buffer, int durationSeconds)
		{
			int totalSamples = sampleRate * durationSeconds;

			int bpm = 90;
			float beatLength = sampleRate * 60f / bpm;

			int currentSample = 0;
			while (currentSample < totalSamples)
			{
				float[] durations = [0.5f, 1f, 1.5f]; // eighth, quarter, dotted quarter
				float noteLengthBeats = durations[random.Next(durations.Length)];
				int noteLengthSamples = (int)(beatLength * noteLengthBeats);

				int noteIndex = random.Next(scaleIntervals.Length);
				float freq = MidiNoteToFrequency(62 + scaleIntervals[noteIndex]); // D4 base

				float velocity = 0.7f + 0.3f * (float)random.NextDouble();

				AddNote(buffer, currentSample, noteLengthSamples, freq, velocity);

				currentSample += noteLengthSamples;
			}
		}

		private void GenerateHarmonyLayer(float[] buffer, int durationSeconds)
		{
			int totalSamples = sampleRate * durationSeconds;

			int bpm = 90;
			float beatLength = sampleRate * 60f / bpm;

			int currentSample = 0;
			while (currentSample < totalSamples)
			{
				// Longer notes for harmony
				float[] durations = [1f, 2f];
				float noteLengthBeats = durations[random.Next(durations.Length)];
				int noteLengthSamples = (int)(beatLength * noteLengthBeats);

				// Harmony note: mostly a 3rd or 5th above melody root D4 (MIDI 62)
				int[] harmonyIntervals = { 4, 7, 11 }; // Major 3rd, Perfect 5th
				float freq = MidiNoteToFrequency(62 + harmonyIntervals[random.Next(harmonyIntervals.Length)]);

				float velocity = 0.4f + 0.2f * (float)random.NextDouble();

				AddNote(buffer, currentSample, noteLengthSamples, freq, velocity);

				currentSample += noteLengthSamples;
			}
		}

		private void GenerateRhythmLayer(float[] buffer, int durationSeconds)
		{
			int totalSamples = sampleRate * durationSeconds;
			int bpm = 90;
			float beatLength = sampleRate * 60f / bpm;

			for (int i = 0; i < durationSeconds * bpm; i++)
			{
				int hitSample = (int)(i * beatLength);
				int hitLength = (int)(0.05f * sampleRate); // short percussive hit

				// Percussive sound = quickly decaying noise burst
				for (int s = 0; s < hitLength && hitSample + s < totalSamples; s++)
				{
					float decay = 1f - s / (float)hitLength;
					float noise = (float)(random.NextDouble() * 2 - 1); // white noise
					buffer[hitSample + s] += noise * decay * 0.15f;
				}
			}
		}

		private void AddNote(float[] buffer, int startSample, int length, float freq, float velocity)
		{
			int endSample = Math.Min(buffer.Length, startSample + length);

			for (int i = startSample; i < endSample; i++)
			{
				int notePos = i - startSample;

				float t = notePos / (float)sampleRate;

				// Waveform with harmonics (fundamental + octave + fifth)
				float sampleValue =
					(float)(Math.Sin(2 * Math.PI * freq * t) * 0.6 +
							Math.Sin(2 * Math.PI * freq * 2 * t) * 0.25 +
							Math.Sin(2 * Math.PI * freq * 3 * t) * 0.15);

				sampleValue *= (float)1f * velocity;

				buffer[i] += sampleValue;
			}
		}

		private static float MidiNoteToFrequency(float midiNote)
		{
			return 440f * (float)Math.Pow(2, (midiNote - 69) / 12.0);
		}
	}
}