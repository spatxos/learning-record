using binaryTree.BinarySearchTree;
using binaryTree.BinaryTree;
using System;
using System.Collections.Generic;

namespace binaryTree
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int[] array = { 6, 7,1, 2,  5,  8, 9,3, 4
                          ,10, 18, 19,11, 12, 15, 16, 17, 13, 14};
            if (array.Length > 0)
            {
                #region 二叉树
                {
                    List<Node> list = TreeHelper.CreateTree(array);
                    TreeHelper.PrintTree(list[0], 0);
                    TreeHelper.Search(list[0], 0);
                    TreeHelper.Search(list[0], 9);
                }
                #endregion
                #region 二叉查找树
                {
                    var mytree = new BinarySearchTree<int>();
                    for (int i = 0; i < array.Length; i++)
                    {
                        mytree.insert(array[i]);
                    }
                    mytree.printTree();
                    Console.WriteLine($"findMin:{mytree.findMin()}");
                    Console.WriteLine($"findMax:{mytree.findMax()}");
                }
                #endregion
            }
            Console.ReadLine();
        }
    }
}
