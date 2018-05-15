namespace SamsHelper.Libraries
{
    public class Edge<T>
    {
        public readonly Node<T> A, B;
        public readonly float Length;
                         
        public Edge(Node<T> a, Node<T> b)
        {
            A = a;
            B = b;
        }
             
        public bool ConnectsTo(Node<T> n)
        {
            return Equals(n.Position, A.Position) || Equals(n.Position, B.Position);
        }

        public bool Equals(Edge<T> other)
        {
            return ConnectsTo(other.A) && ConnectsTo(other.B);
        }
        
        public Node<T> GetOther(Node<T> n)
        {
            return Equals(n, A) ? B : A;
        }
    }
}