using UnityEngine;

// UnityEngine.Random only samples uniform distributions; sensor noise needs Gaussian samples.
public static class GaussianRandom
{
    public static float NextGaussian(float mean, float stdDev)
    {
        float u1 = 1f - Random.value; // (0,1], avoids log(0)
        float u2 = Random.value;
        float z = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Sin(2f * Mathf.PI * u2); // Box-Muller
        return mean + stdDev * z;
    }

    public static Vector3 NextGaussian3(float stdDev)
    {
        return new Vector3(
            NextGaussian(0f, stdDev),
            NextGaussian(0f, stdDev),
            NextGaussian(0f, stdDev));
    }
}
