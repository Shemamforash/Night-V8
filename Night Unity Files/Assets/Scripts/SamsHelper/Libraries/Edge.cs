namespace SamsHelper.Libraries
{
    public class Edge
    {
        public readonly Node A, B;
        public readonly float Length;
                         
        public Edge(Node a, Node b)
        {
            A = a;
            B = b;
            Length = A.Distance(B);
        }
             
        public bool ConnectsTo(Node n)
        {
            return Equals(n.Position, A.Position) || Equals(n.Position, B.Position);
        }

        public bool Equals(Edge other)
        {
            return ConnectsTo(other.A) && ConnectsTo(other.B);
        }
        
        public Node GetOther(Node n)
        {
            return Equals(n, A) ? B : A;
        }
    }
}