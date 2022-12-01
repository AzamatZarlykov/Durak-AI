using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIAgent
{
    public  class Tree
    {
        Node root;
        public Tree()
        {
            root = new Node();
        }

        public Tree(Node root)
        {
            this.root = root;
        }
        // Getter
        public Node GetRoot()
        {
            return root!;
        }
        // Setter
        public void SetRoot(Node node)
        {
            this.root = node;
        }
    }
}
