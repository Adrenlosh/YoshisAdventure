using NAudio.Wave;
using System;

public class LoopStream : WaveStream
{
    private readonly WaveStream sourceStream;

    public TimeSpan LoopStart { get; set; } = TimeSpan.Zero;

    public bool EnableLooping { get; set; } = true;

    public LoopStream(WaveStream sourceStream)
    {
        this.sourceStream = sourceStream;
    }

    public override WaveFormat WaveFormat => sourceStream.WaveFormat;
    public override long Length => sourceStream.Length;
    public override long Position
    {
        get => sourceStream.Position;
        set => sourceStream.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int totalBytesRead = 0;
        while (count > 0)
        {
            int bytesRead = sourceStream.Read(buffer, offset, count);
            if (bytesRead == 0)
            {
                if (EnableLooping)
                {
                    sourceStream.CurrentTime = LoopStart;
                }
                else
                {
                    break;
                }
            }
            totalBytesRead += bytesRead;
            offset += bytesRead;
            count -= bytesRead;
        }
        return totalBytesRead;
    }
}