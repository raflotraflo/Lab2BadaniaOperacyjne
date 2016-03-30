using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lab2BadaniaOperacyjne
{
    public class KEdge
    {
        public int Source { get; set; }
        public int Target { get; set; }
        public int Value { get; set; }

        public KEdge(int source, int value, int target)
        {
            Source = source;
            Target = target;
            Value = value;
        }
    }
}
