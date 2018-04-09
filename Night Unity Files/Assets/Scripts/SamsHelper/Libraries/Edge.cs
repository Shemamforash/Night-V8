namespace SamsHelper.Libraries
{
    public class Edge<T>
    {
        public readonly T A, B;
        public readonly float Length;
                         
        public Edge(T a, T b)
        {
            A = a;
            B = b;
        }
             
        public bool ConnectsTo(T n)
        {
            return Equals(n, A) || Equals(n, B);
        }

        public T GetOther(T n)
        {
            return Equals(n, A) ? B : A;
        }
    }
}