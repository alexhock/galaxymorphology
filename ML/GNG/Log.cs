using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNG
{
    public static class Log
    {
        public static void Cout(this string str, params object[] args)
        {
#if DEBUG
     //       Console.WriteLine(str, args);
#endif
        }
    }
}
