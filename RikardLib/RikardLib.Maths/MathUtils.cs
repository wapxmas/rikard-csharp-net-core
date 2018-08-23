using System;
using System.Collections.Generic;
using System.Text;

namespace RikardLib.Maths
{
    public static class MathUtils
    {
        public static double CosineSimilarity(double[] V1, double[] V2)
        {
            double dot = 0.0d;
            double mag1 = 0.0d;
            double mag2 = 0.0d;

            for (int n = 0; n < ((V2.Length < V1.Length) ? V2.Length : V1.Length); n++)
            {
                dot += V1[n] * V2[n];
                mag1 += Math.Pow(V1[n], 2);
                mag2 += Math.Pow(V2[n], 2);
            }

            return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2));
        }
    }
}
