using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public static class AudioGenerator
{
    private const int SampleRate = 44100;

    [MenuItem("PacStudent/Generate Audio Clips")]
    public static void GenerateAllAudio()
    {
        string dir = Path.Combine(Application.dataPath, "Audio Clips");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        // 1. Intro Music (BGM - upbeat arcade intro melody ~3.5s)
        CreateGameIntroMusic(Path.Combine(dir, "Background music – game intro.wav"));

        // 2. StartScene Music (BGM - catchy title loop ~4.0s)
        CreateStartSceneMusic(Path.Combine(dir, "Background music – StartScene.wav"));

        // 3. Ghosts Normal State BGM (retro pulsing siren beat ~3.0s)
        CreateGhostsNormalMusic(Path.Combine(dir, "Background music – ghosts normal state.wav"));

        // 4. Ghosts Scared State BGM (fast nervous wobble rhythm ~3.0s)
        CreateGhostsScaredMusic(Path.Combine(dir, "Background music – ghosts scared state.wav"));

        // 5. Ghost Dead BGM (fast cyber ascending beeps/whine ~2.5s)
        CreateGhostDeadMusic(Path.Combine(dir, "Background music – at least one ghost dead.wav"));

        // 6. SFX PacStudent moving (waka-like melodic bleep cycle 0.3s)
        CreatePacStudentMovingSFX(Path.Combine(dir, "SFX – PacStudent moving.wav"));

        // 7. SFX PacStudent eats pellet (crisp short bite/coin blip 0.1s)
        CreateEatPelletSFX(Path.Combine(dir, "SFX – PacStudent eats pellet.wav"));

        // 8. SFX PacStudent eats ghost (heavy chomping crunch reward 0.5s)
        CreateEatGhostSFX(Path.Combine(dir, "SFX – PacStudent eats ghost.wav"));

        // 9. SFX PacStudent eats bonus cherry (bright 2-tone melodic chime 0.4s)
        CreateEatCherrySFX(Path.Combine(dir, "SFX – PacStudent eats bonus cherry.wav"));

        // 10. SFX PacStudent collides with wall (short thud bounce 0.15s)
        CreateWallCollideSFX(Path.Combine(dir, "SFX – PacStudent collides with wall.wav"));

        // 11. SFX PacStudent death animation (descending retro synth explosion ~1.5s)
        CreateDeathSFX(Path.Combine(dir, "SFX – PacStudent death animation.wav"));

        AssetDatabase.Refresh();
        Debug.Log("Successfully generated all 11 Audio Clips in Assets/Audio Clips!");
    }

    private static void CreateGameIntroMusic(string path)
    {
        float duration = 3.2f;
        int sampleCount = (int)(SampleRate * duration);
        float[] samples = new float[sampleCount];

        // Notes for catchy fanfare: C4, E4, G4, C5, B4, G4, A4, B4, C5 (C major arpeggio fanfare)
        float[] freqs = { 261.63f, 329.63f, 392.00f, 523.25f, 493.88f, 392.00f, 440.00f, 493.88f, 523.25f, 659.25f, 783.99f, 1046.50f };
        float noteLen = duration / freqs.Length;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            int noteIndex = Mathf.Clamp((int)(t / noteLen), 0, freqs.Length - 1);
            float freq = freqs[noteIndex];
            float noteT = t - (noteIndex * noteLen);
            float env = Mathf.Exp(-noteT * 3.5f) * 0.4f;

            // Square wave + Sub bass triangle
            float phase = t * freq * 2f * Mathf.PI;
            float square = Mathf.Sign(Mathf.Sin(phase)) * 0.5f;
            float tri = Mathf.PingPong(t * (freq * 0.5f) * 4f, 1f) - 0.5f;

            samples[i] = (square * 0.6f + tri * 0.4f) * env;
        }

        SaveWav(path, samples);
    }

    private static void CreateStartSceneMusic(string path)
    {
        float duration = 4.0f;
        int sampleCount = (int)(SampleRate * duration);
        float[] samples = new float[sampleCount];

        float[] chords = { 330f, 392f, 493f, 587f, 440f, 523f, 659f, 783f };
        float step = duration / chords.Length;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            int idx = Mathf.Clamp((int)(t / step), 0, chords.Length - 1);
            float f = chords[idx];
            float localT = t % step;
            float env = Mathf.Sin(localT / step * Mathf.PI) * 0.35f;

            float pulse = Mathf.Sign(Mathf.Sin(t * f * 2f * Mathf.PI)) * 0.4f;
            float arpHarm = Mathf.Sin(t * f * 3f * 2f * Mathf.PI) * 0.2f;

            samples[i] = (pulse + arpHarm) * env;
        }

        SaveWav(path, samples);
    }

    private static void CreateGhostsNormalMusic(string path)
    {
        // Continuous looping siren beat
        float duration = 2.5f;
        int sampleCount = (int)(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            // Siren pitch sweep from 180Hz to 320Hz oscillating at 2.4Hz
            float sweep = 240f + 70f * Mathf.Sin(t * 2.4f * 2f * Mathf.PI);
            float sig = Mathf.Sin(t * sweep * 2f * Mathf.PI);

            // Add rhythmic low-pass tick
            float beat = Mathf.Exp(-(t % 0.25f) * 20f) * 0.2f;
            float subBass = Mathf.Sin(t * 80f * 2f * Mathf.PI) * beat;

            samples[i] = sig * 0.25f + subBass * 0.35f;
        }

        SaveWav(path, samples);
    }

    private static void CreateGhostsScaredMusic(string path)
    {
        // High tension fast oscillation
        float duration = 2.0f;
        int sampleCount = (int)(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float freq = 440f + 180f * Mathf.Sin(t * 6f * 2f * Mathf.PI);
            float sig = Mathf.Sign(Mathf.Sin(t * freq * 2f * Mathf.PI)) * 0.2f;
            samples[i] = sig;
        }

        SaveWav(path, samples);
    }

    private static void CreateGhostDeadMusic(string path)
    {
        // Ascending alert whine
        float duration = 1.5f;
        int sampleCount = (int)(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float sweep = 600f + (t / duration) * 900f;
            float pulse = Mathf.Sin(t * sweep * 2f * Mathf.PI) * 0.3f;
            float stutter = Mathf.Sin(t * 16f * 2f * Mathf.PI) > 0 ? 1f : 0.2f;
            samples[i] = pulse * stutter;
        }

        SaveWav(path, samples);
    }

    private static void CreatePacStudentMovingSFX(string path)
    {
        float duration = 0.22f;
        int sampleCount = (int)(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            // Modulated waka bleep: 300Hz up to 600Hz and back
            float f = 320f + 280f * Mathf.Sin((t / duration) * Mathf.PI);
            float env = Mathf.Sin((t / duration) * Mathf.PI) * 0.45f;
            samples[i] = Mathf.Sign(Mathf.Sin(t * f * 2f * Mathf.PI)) * env;
        }

        SaveWav(path, samples);
    }

    private static void CreateEatPelletSFX(string path)
    {
        float duration = 0.12f;
        int sampleCount = (int)(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float f = 580f + (t / duration) * 400f;
            float env = Mathf.Exp(-t * 30f) * 0.5f;
            samples[i] = Mathf.Sin(t * f * 2f * Mathf.PI) * env;
        }

        SaveWav(path, samples);
    }

    private static void CreateEatGhostSFX(string path)
    {
        float duration = 0.5f;
        int sampleCount = (int)(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float f = 200f + Mathf.PingPong(t * 1200f, 600f);
            float env = (1f - (t / duration)) * 0.6f;
            float noise = (UnityEngine.Random.value * 2f - 1f) * 0.3f;
            samples[i] = (Mathf.Sign(Mathf.Sin(t * f * 2f * Mathf.PI)) * 0.7f + noise) * env;
        }

        SaveWav(path, samples);
    }

    private static void CreateEatCherrySFX(string path)
    {
        float duration = 0.35f;
        int sampleCount = (int)(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float f = (t < duration * 0.5f) ? 784f : 1046.5f; // G5 then C6
            float env = Mathf.Exp(-(t % (duration * 0.5f)) * 12f) * 0.5f;
            samples[i] = (Mathf.Sin(t * f * 2f * Mathf.PI) + 0.3f * Mathf.Sin(t * f * 2f * 2f * Mathf.PI)) * env;
        }

        SaveWav(path, samples);
    }

    private static void CreateWallCollideSFX(string path)
    {
        float duration = 0.15f;
        int sampleCount = (int)(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float f = 120f * Mathf.Exp(-t * 20f);
            float env = Mathf.Exp(-t * 25f) * 0.6f;
            samples[i] = (Mathf.Sin(t * f * 2f * Mathf.PI) + (UnityEngine.Random.value * 2f - 1f) * 0.2f) * env;
        }

        SaveWav(path, samples);
    }

    private static void CreateDeathSFX(string path)
    {
        float duration = 1.6f;
        int sampleCount = (int)(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            // Descending sweep with arpeggiated chromatic stutter
            float f = Mathf.Max(60f, 900f * Mathf.Exp(-t * 2.5f));
            float wobble = Mathf.Sin(t * 30f * 2f * Mathf.PI);
            float env = Mathf.Max(0f, 1f - (t / duration)) * 0.6f;
            samples[i] = Mathf.Sign(Mathf.Sin(t * (f + wobble * 40f) * 2f * Mathf.PI)) * env;
        }

        SaveWav(path, samples);
    }

    private static void SaveWav(string filePath, float[] samples)
    {
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        using (var writer = new BinaryWriter(fileStream))
        {
            int byteRate = SampleRate * 2; // 16-bit mono = 2 bytes per sample
            int dataSize = samples.Length * 2;

            // RIFF chunk
            writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + dataSize);
            writer.Write(new char[4] { 'W', 'A', 'V', 'E' });

            // fmt chunk
            writer.Write(new char[4] { 'f', 'm', 't', ' ' });
            writer.Write(16); // Chunk size
            writer.Write((short)1); // PCM format
            writer.Write((short)1); // Mono channel
            writer.Write(SampleRate);
            writer.Write(byteRate);
            writer.Write((short)2); // Block align (1 channel * 2 bytes)
            writer.Write((short)16); // Bits per sample

            // data chunk
            writer.Write(new char[4] { 'd', 'a', 't', 'a' });
            writer.Write(dataSize);

            for (int i = 0; i < samples.Length; i++)
            {
                short sampleInt = (short)(Mathf.Clamp(samples[i], -1f, 1f) * 32767f);
                writer.Write(sampleInt);
            }
        }
    }
}
