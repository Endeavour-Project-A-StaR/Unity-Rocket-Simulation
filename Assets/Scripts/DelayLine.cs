using System.Collections.Generic;
using UnityEngine;

// Fixed-length FIFO used to simulate sensor read latency: push the newest true
// sample in, get back whatever sample was pushed `delaySamples` calls ago.
public class DelayLine<T>
{
    private readonly Queue<T> buffer = new Queue<T>();
    private readonly int delaySamples;

    public DelayLine(int delaySamples, T initialValue)
    {
        this.delaySamples = Mathf.Max(0, delaySamples);
        for (int i = 0; i < this.delaySamples; i++)
            buffer.Enqueue(initialValue);
    }

    public T PushAndRead(T newSample)
    {
        if (delaySamples == 0) return newSample;

        buffer.Enqueue(newSample);
        return buffer.Dequeue();
    }
}
