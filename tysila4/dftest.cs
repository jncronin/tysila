using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using libtysila4.graph;

namespace tysila4
{
    class dftest
    {
        MultiNode[] n;
        Graph ret;

        void AddEdge(int x, int y)
        {
            n[x - 1].AddNext(n[y - 1]);
            n[y - 1].AddPrev(n[x - 1]);

            ret.bbs_after[x - 1].Add(y - 1);
            ret.bbs_before[y - 1].Add(x - 1);
        }

        public Graph gen_test()
        {
            ret = new Graph();

            ret.bbs_after = new List<List<int>>(new List<int>[13]);
            ret.bbs_before = new List<List<int>>(new List<int>[13]);
            ret.bb_starts = new List<BaseNode>(new BaseNode[13]);
            ret.bb_ends = new List<BaseNode>(new BaseNode[13]);

            n = new MultiNode[13];

            for (int i = 0; i < 13; i++)
            {
                n[i] = new MultiNode();
                n[i].bb = i;
                ret.bb_starts[i] = n[i];
                ret.bb_ends[i] = n[i];
                ret.bbs_after[i] = new List<int>();
                ret.bbs_before[i] = new List<int>();
                ret.blocks.Add(new List<BaseNode> { n[i] });
            }

            AddEdge(1, 2);
            AddEdge(2, 3);
            AddEdge(3, 3);
            AddEdge(3, 4);
            AddEdge(4, 13);
            AddEdge(1, 5);
            AddEdge(5, 6);
            AddEdge(6, 4);
            AddEdge(6, 8);
            AddEdge(8, 13);
            AddEdge(8, 5);
            AddEdge(5, 7);
            AddEdge(7, 8);
            AddEdge(7, 12);
            AddEdge(1, 9);
            AddEdge(9, 10);
            AddEdge(10, 12);
            AddEdge(12, 13);
            AddEdge(9, 11);
            AddEdge(11, 12);

            ret.Starts.Add(n[0]);

            return ret;
        }

        public static void df_test()
        {
            var df = new dftest();
            var g = df.gen_test();

            var dg = libtysila4.graph.DominanceGraph.GenerateDominanceGraph(g, null);
        }
    }
}
