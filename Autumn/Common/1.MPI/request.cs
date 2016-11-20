using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
class Request
{
    public Edge ik;
    public int[] row_i;
    public int[] row_k;
    public Request() { }
    public Request(Edge _edge, int[] _row_i, int[] _row_k)
    {
        ik = _edge;
        row_i = _row_i;
        row_k = _row_k;
    }
}


