using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
class Edge
{
    public int from, to, value;
    public Edge(int _from, int _to, int _value)
    {
        from = _from;
        to = _to;
        value = _value;
    }
}


