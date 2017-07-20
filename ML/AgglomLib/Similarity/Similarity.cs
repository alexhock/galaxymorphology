using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgglomLib
{
    public interface Similarity
    {
        double GetDistance(Cluster one, Cluster two);
    }

}
