using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace binaryTree.BinaryTree
{
    public class TreeHelper
    {
        /// <summary>
        /// 构建二叉树
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static List<Node> CreateTree(int[] array)
        {
            List<Node> list = new List<Node>();
            for (int i = 0; i < array.Length; i++)
            {
                Node node = new Node(array[i], -1, null, null);    //创建结点，每一个结点的左结点和右结点为null
                list.Add(node); // list中存着每一个结点
            }
            // 构建二叉树
            if (list.Count > 0)
            {
                for (int i = 0; i < array.Length / 2 - 1; i++)
                {       // i表示的是根节点的索引，从0开始
                    if (list[2 * i + 1] != null)
                    {
                        // 左结点
                        list[i].left = list[2 * i + 1];
                        list[i].index = i;
                    }
                    if (list[2 * i + 2] != null)
                    {
                        // 右结点
                        list[i].right = list[2 * i + 2];
                        list[i].index = i;
                    }
                }
                // 判断最后一个根结点：因为最后一个根结点可能没有右结点，所以单独拿出来处理
                int lastIndex = array.Length / 2 - 1;
                // 左结点
                list[lastIndex].left = list[lastIndex * 2 + 1];
                list[lastIndex].index = lastIndex;
                // 右结点，如果数组的长度为奇数才有右结点
                if (array.Length % 2 == 1)
                {
                    list[lastIndex].right = list[lastIndex * 2 + 2];
                }
            }
            return list;
        }
        /// <summary>
        /// 打印二叉树
        /// </summary>
        /// <param name="node"></param>
        /// <param name="Layer"></param>
        public static void PrintTree(Node node, int Layer)
        {
            if (node == null) { return; }
            PrintTree(node.right, Layer + 3);
            for (int i = 0; i < Layer; i++)
            {
                Console.Write(" ");
            }
            Console.WriteLine(node.data);
            PrintTree(node.left, Layer + 3);
        }

        /// <summary>
        /// 搜索二叉树
        /// </summary>
        /// <param name="node"></param>
        /// <param name="Layer"></param>
        public static void Search(Node node,int data)
        {
            if (node == null) { return; }
            Search(node.right, data);
            if(node.data == data)
            {
                Console.WriteLine($"search：{data}:index={node.index}");
            }
            Search(node.left, data);
        }
    }
}
