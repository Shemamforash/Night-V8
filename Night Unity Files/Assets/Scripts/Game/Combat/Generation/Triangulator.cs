using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat.Generation
{
    public static class Triangulator
    {
        private static List<Vector3> m_points;

        public static int[] Triangulate(Vector3[] points)
        {
            m_points = new List<Vector3>(points);
            List<int> indices = new List<int>();

            int n = m_points.Count;
            if (n < 3)
                return indices.ToArray();

            int[] V = new int[n];
            if (Area() > 0)
                for (int v = 0; v < n; v++)
                    V[v] = v;
            else
                for (int v = 0; v < n; v++)
                    V[v] = n - 1 - v;

            int nv = n;
            int count = 2 * nv;
            for (int m = 0, v = nv - 1; nv > 2;)
            {
                if (count-- <= 0)
                    return indices.ToArray();

                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;

                if (!Snip(u, v, w, nv, V)) continue;
                int s, t;
                int a = V[u];
                int b = V[v];
                int c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                m++;
                for (s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                nv--;
                count = 2 * nv;
            }

            indices.Reverse();
            return indices.ToArray();
        }

        private static float Area()
        {
            int n = m_points.Count;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector3 pval = m_points[p];
                Vector3 qval = m_points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }

            return A * 0.5f;
        }

        private static bool Snip(int u, int v, int w, int n, int[] V)
        {
            int p;
            Vector3 A = m_points[V[u]];
            Vector3 B = m_points[V[v]];
            Vector3 C = m_points[V[w]];
            if (Mathf.Epsilon > (B.x - A.x) * (C.y - A.y) - (B.y - A.y) * (C.x - A.x))
                return false;
            for (p = 0; p < n; p++)
            {
                if (p == u || p == v || p == w)
                    continue;
                Vector3 P = m_points[V[p]];
                if (InsideTriangle(A, B, C, P))
                    return false;
            }

            return true;
        }

        private static bool InsideTriangle(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p)
        {
            float A = 0.5f * (-p1.y * p2.x + p0.y * (-p1.x + p2.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y);
            int sign = A < 0 ? -1 : 1;
            float s = (p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y) * sign;
            float t = (p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y) * sign;
            return s > 0 && t > 0 && (s + t) < 2 * A * sign;
        }
    }
}