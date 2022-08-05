using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Carbosaurus.LCA
{
    public class TreeNode
    {
        public string name { get; set; }

        public List<TreeNode> children { get; set; }

    }
}
