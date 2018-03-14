using System;
using System.Collections.Generic;

public class PerlinNoise
{
    /// <summary>
    /// Based on the reference Java Code written by Perlin in 2002 (http://mrl.nyu.edu/~perlin/noise/)
    /// Enhanced with https://gamedev.stackexchange.com/questions/58664/2d-and-3d-perlin-noise-terrain-generation
    /// </summary>
    private static double _frequency = 1.0;

    private static double _lacunarity = 2.0;
    private static int _octaveCount = 6;
    private static double _persistence = 0.5;

    private static int[] _p = new int[512];

    private static readonly List<int> _permutation = new List<int>();

    public static void Generate(float frequency = 1f, float lacunarity = 2f, float persistence = 0.5f, int octaves = 6)
    {
        _frequency = frequency;
        _lacunarity = lacunarity;
        _persistence = persistence;
        _octaveCount = octaves;

        for (int i = 0; i < 256; i++)
        {
            _permutation.Add(i);
        }
        Shuffle(_permutation);
        for (int i = 0; i < 256; i++) _p[256 + i] = _p[i] = _permutation[i];
    }

    private static double Noise(double x, double y)
    {
        double z = 1;
        // FIND UNIT CUBE THAT CONTAINS POINT.
        int X = (int)Math.Floor(x) & 255,
            Y = (int)Math.Floor(y) & 255,
            Z = (int)Math.Floor(z) & 255;

        // FIND RELATIVE X,Y,Z OF POINT IN CUBE.
        x -= Math.Floor(x);
        y -= Math.Floor(y);
        z -= Math.Floor(z);

        // COMPUTE FADE CURVES
        // FOR EACH OF X,Y,Z.
        double u = Fade(x),
            v = Fade(y),
            w = Fade(z);

        // HASH COORDINATES OF
        // THE 8 CUBE CORNERS,
        int A = _p[X] + Y,
            AA = _p[A] + Z,
            AB = _p[A + 1] + Z,
            B = _p[X + 1] + Y,
            BA = _p[B] + Z,
            BB = _p[B + 1] + Z;


        // Blend results from 8 corners of cube
        return Lerp(w, Lerp(v, Lerp(u, Grad(_p[AA], x, y, z),
                    Grad(_p[BA], x - 1, y, z)),
                Lerp(u, Grad(_p[AB], x, y - 1, z),
                    Grad(_p[BB], x - 1, y - 1, z))),
            Lerp(v, Lerp(u, Grad(_p[AA + 1], x, y, z - 1),
                    Grad(_p[BA + 1], x - 1, y, z - 1)),
                Lerp(u, Grad(_p[AB + 1], x, y - 1, z - 1),
                    Grad(_p[BB + 1], x - 1, y - 1, z - 1))));
    }

    //already returns between 0 and 1
    public static double GetValue(double x, double y)
    {
        double sum = 0;
        double currentFrequency = _frequency;
        double amplitude = 1;
        for (int i = 0; i < _octaveCount; ++i)
        {
            sum += amplitude * Noise(x * currentFrequency, y * currentFrequency);
            currentFrequency *= _lacunarity;
            amplitude /= _persistence;
        }

        if (sum < -1f)
        {
            sum = -1f;
        }
        else if (sum > 1f)
        {
            sum = 1f;
        }
        sum = (sum + 1f) / 2f;
        return sum;
    }

    private static double Fade(double t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static double Lerp(double t, double a, double b)
    {
        return a + t * (b - a);
    }

    private static double Grad(int hash, double x, double y, double z)
    {
        int h = hash & 15; // CONVERT LO 4 BITS OF HASH CODE
        double u = h < 8 ? x : y,
            // INTO 12 GRADIENT DIRECTIONS.
            v = h < 4 ? y : h == 12 || h == 14 ? x : z;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    private static void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}