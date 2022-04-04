﻿using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Custom_Texture_Importer.Utils.Libs;

public class Oodle
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SetDllDirectory(string lpPathName);

    public Oodle(string DllPath = null)
    {
        if (DllPath != null)
            SetDllDirectory(DllPath);
    }

    [DllImport("oo2core_5_win64.dll")]
    private static extern int OodleLZ_Compress(OodleFormat Format, byte[] Buffer, long BufferSize,
        byte[] OutputBuffer, OodleCompressionLevel Level, uint a, uint b, uint c);

    [DllImport("oo2core_5_win64.dll")]
    private static extern int OodleLZ_Decompress(byte[] Buffer, long BufferSize, byte[] OutputBuffer,
        long OutputBufferSize, uint a, uint b, uint c, uint d, uint e, uint f, uint g, uint h, uint i, int ThreadModule);

    public uint GetCompressedBounds(uint BufferSize)
    {
        return BufferSize + 274 * ((BufferSize + 0x3FFFF) / 0x40000);
    }

    private uint CompressStream(byte[] Buffer, uint BufferSize, ref byte[] OutputBuffer, uint OutputBufferSize,
        OodleFormat Format, OodleCompressionLevel Level)
    {
        if (Buffer.Length > 0 && BufferSize > 0 && OutputBuffer.Length > 0 && OutputBufferSize > 0)
            return (uint)OodleLZ_Compress(Format, Buffer, BufferSize, OutputBuffer, Level, 0, 0, 0);

        return 0;
    }

    private uint DecompressStream(byte[] Buffer, uint BufferSize, ref byte[] OutputBuffer, uint OutputBufferSize)
    {
        if (Buffer.Length > 0 && BufferSize > 0 && OutputBuffer.Length > 0 && OutputBufferSize > 0)
            return (uint)OodleLZ_Decompress(Buffer, BufferSize, OutputBuffer, OutputBufferSize, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        return 0;
    }

    public byte[] Compress(byte[] Buffer)
    {
        var MaxLength = GetCompressedBounds((uint)Buffer.Length);
        var OutputBuffer = new byte[MaxLength];

        var CompressedSize = CompressStream(Buffer, (uint)Buffer.Length, ref OutputBuffer, MaxLength,
            OodleFormat.Kraken, OodleCompressionLevel.Optimal5);

        var tempBuffer = new byte[CompressedSize];
        Array.Copy(OutputBuffer, tempBuffer, CompressedSize);

        if (CompressedSize > 0)
            // Remove all bytes after the compressed size
            return tempBuffer;

        throw new InvalidDataException("Unable to compress buffer.");
    }
}

public enum OodleFormat : uint
{
    LZH = 0,
    LZHLW = 1,
    LZNIB = 2,
    None = 3,
    LZB16 = 4,
    LZBLW = 5,
    LZA = 6,
    LZNA = 7,
    Kraken = 8,
    Mermaid = 9,
    BitKnit = 10,
    Selkie = 11,
    Hydra = 12,
    Leviathan = 13
}

public enum OodleCompressionLevel : uint
{
    None = 0,
    SuperFast = 1,
    VeryFast = 2,
    Fast = 3,
    Normal = 4,
    Optimal1 = 5,
    Optimal2 = 6,
    Optimal3 = 7,
    Optimal4 = 8,
    Optimal5 = 9
}