using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace binaryTree.BinaryTree
{
    public class Node
    {
        public int data;      //自己本身值
        public int index;      //自己本身值
        public Node left;     //左结点
        public Node right;     //右结点
        public Node()
        {
        }
        public Node(int data, int index, Node left, Node right)
        {
            this.data = data;
            this.index = index;
            this.left = left;
            this.right = right;
        }
        public int getData()
        {
            return data;
        }
        public void setData(int data)
        {
            this.data = data;
        }
        public Node getLeft()
        {
            return left;
        }
        public void setLeft(Node left)
        {
            this.left = left;
        }
        public Node getRight()
        {
            return right;
        }
        public void setRight(Node right)
        {
            this.right = right;
        }
        public int getIndex()
        {
            return index;
        }
        public void setIndex(int index)
        {
            this.index = index;
        }

    }
}
