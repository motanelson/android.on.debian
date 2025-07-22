using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;

class Program
{
    const int SampleRate = 44100;
    const short BitsPerSample = 16;
    const short NumChannels = 1;
    const double Volume = 0.3;
    const double NoteDurationSec = 0.3;
    const double PauseDurationSec = 0.05;

    static readonly Dictionary<char, int[]> Chords = new()
    {
        ['0'] = new[] { 60, 64, 67 },
        ['1'] = new[] { 62, 65, 69 },
        ['2'] = new[] { 64, 67, 71 },
        ['3'] = new[] { 65, 69, 72 },
        ['4'] = new[] { 67, 71, 74 },
        ['5'] = new[] { 69, 72, 76 },
        ['6'] = new[] { 71, 74, 77 },
        ['7'] = new[] { 72, 76, 79 },
        ['8'] = new[] { 74, 77, 81 },
        ['9'] = new[] { 76, 79, 83 }
    };

    static readonly Dictionary<char, int> Notes = new()
    {
        ['A'] = 69, ['B'] = 71, ['C'] = 60, ['D'] = 62,
        ['E'] = 64, ['F'] = 65, ['G'] = 67, ['H'] = 70
    };

    static void Main()
    {
        Console.ForegroundColor = ConsoleColor.Black;
        Console.BackgroundColor = ConsoleColor.DarkYellow;

        Console.Write("Nome do ficheiro .txt: ");
        string inputFile = Console.ReadLine()?.Trim() ?? "";

        if (!File.Exists(inputFile))
        {
            Console.WriteLine("Ficheiro não encontrado.");
            return;
        }

        string content = File.ReadAllText(inputFile).ToUpper();
        string outputFile = Path.ChangeExtension(inputFile, ".wav");

        using var stream = new MemoryStream();
        foreach (char ch in content)
        {
            if (Chords.ContainsKey(ch))
                AppendNote(stream, Chords[ch], NoteDurationSec);
            else if (Notes.ContainsKey(ch))
                AppendNote(stream, new[] { Notes[ch] }, NoteDurationSec);
            else
                continue;

            AppendSilence(stream, PauseDurationSec);
        }

        byte[] audioData = stream.ToArray();
        using var outFile = new FileStream(outputFile, FileMode.Create);
        WriteWavHeader(outFile, audioData.Length);
        outFile.Write(audioData, 0, audioData.Length);

        Console.WriteLine($"✅ Ficheiro WAV gerado: {outputFile}");
    }

    static void AppendNote(MemoryStream ms, int[] midiNotes, double durationSec)
    {
        int samples = (int)(SampleRate * durationSec);
        for (int i = 0; i < samples; i++)
        {
            double t = i / (double)SampleRate;
            double sample = midiNotes.Select(n => Math.Sin(2 * Math.PI * MidiToFreq(n) * t)).Sum();
            sample = sample / midiNotes.Length * Volume;
            short s = (short)(sample * short.MaxValue);
            ms.WriteByte((byte)(s & 0xFF));
            ms.WriteByte((byte)((s >> 8) & 0xFF));
        }
    }

    static void AppendSilence(MemoryStream ms, double durationSec)
    {
        int samples = (int)(SampleRate * durationSec);
        for (int i = 0; i < samples; i++)
        {
            ms.WriteByte(0);
            ms.WriteByte(0);
        }
    }

    static double MidiToFreq(int midiNote)
    {
        return 440.0 * Math.Pow(2, (midiNote - 69) / 12.0);
    }

    static void WriteWavHeader(Stream stream, int dataLength)
    {
        int byteRate = SampleRate * NumChannels * BitsPerSample / 8;
        int blockAlign = NumChannels * BitsPerSample / 8;
        int chunkSize = 36 + dataLength;

        using var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, leaveOpen: true);
        writer.Write("RIFF".ToCharArray());
        writer.Write(chunkSize);
        writer.Write("WAVE".ToCharArray());

        writer.Write("fmt ".ToCharArray());
        writer.Write(16); // Subchunk1Size
        writer.Write((short)1); // AudioFormat (PCM)
        writer.Write((short)NumChannels);
        writer.Write(SampleRate);
        writer.Write(byteRate);
        writer.Write((short)blockAlign);
        writer.Write((short)BitsPerSample);

        writer.Write("data".ToCharArray());
        writer.Write(dataLength);
    }
}
