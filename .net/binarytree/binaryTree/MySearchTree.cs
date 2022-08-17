using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace binaryTree
{
    public class MySearchTree<T> where T : struct, IComparable<T>
    {

        public BinaryNode<T> root;

        public MySearchTree()
        {
            root = null;
        }

        public void makeEmpty()
        {
            root = null;
        }

        public bool isEmpty()
        {
            return root == null;
        }

        public bool contains(T x)
        {
            return contains(x, root);
        }

        public T findMin()
        {
            return findMin(root).element;
        }

        public T findMax()
        {
            return findMax(root).element;
        }

        public void insert(T x)
        {
            root = insert(x, root);
        }

        public void remove(T x)
        {
            remove(x, root);
        }

        public void printTree()
        {
            printTree(root);
        }


        /**
         * 如果这个树上的值就是要查找的x，返回true
         * 如果树为空，说明不存在这个值，返回false
         * 如果x小于这个树上的值，就在这个树的左子树上递归查找
         * 如果x大于这个树上的值，就在这个树的右子树上递归查找
         */
        private bool contains(T x, BinaryNode<T> tree)
        {
            if (tree == null)
            {
                return false;
            }
            int compareResult = x.CompareTo(tree.element);
            if (compareResult < 0)
            {
                return contains(x, tree.left);
            }
            else if (compareResult > 0)
            {
                return contains(x, tree.right);
            }
            else
            {
                return true;
            }
        }


        /**
         * 只要有左子树就一直往左找，左子树为空说明这个就是最小值
         */
        private BinaryNode<T> findMin(BinaryNode<T> tree)
        {
            if (tree == null)
            {
                return null;
            }
            else if (tree.left == null)
            {
                return tree;
            }
            else
            {
                return findMin(tree.left);
            }

        }

        /**
         * 只要有右子树就一直往左找，右子树为空说明这个就是最大值
         */
        private BinaryNode<T> findMax(BinaryNode<T> tree)
        {
            if (tree == null)
            {
                return null;
            }
            else if (tree.right == null)
            {
                return tree;
            }
            else
            {
                return findMax(tree.right);
            }
        }

        /**
         * 如果要插入的树是null，说明这个就是要插入的值该放的位置，new一个子树，绑定到对应的父亲上
         * 如果树不为null，说明这个树上有值，拿x和这个值进行比较
         * 如果两个值相等，说明已经有这个值了，可以进行一些处理
         * 如果x小于树上的值，就往该树的左子树上递归插入
         * 如果x大于树上的值，就往该树的右子树上递归插入
         */
        private BinaryNode<T> insert(T x, BinaryNode<T> tree)
        {
            if (tree == null)
            {
                return new BinaryNode<T>(x, null, null);
            }
            int compareResult = x.CompareTo(tree.element);
            if (compareResult < 0)
            {
                tree.left = insert(x, tree.left);
            }
            else if (compareResult > 0)
            {
                tree.right = insert(x, tree.right);
            }
            else
            {
                //说明已经有这个值了。
                Console.WriteLine("已经有这个值了");
            }
            return tree;
        }


        /**
         * 比较x和树的值
         * 如果x小于树的值，在树的左子树中递归删除
         * 如果x大于树的值，在树的右子树中递归删除
         * 如果x等于树的值，那么这个值就是要删除的值。
         * 因为删除一个值就要对树进行重新排列，所以这个位置上不能空。
         * 如果这个树只有一个子树，那么就直接把这个子树放在这个位置上
         * 如果这个树有两个子树，那么需要找到右子树的最小值，将这个最小值赋值在要删除的位置上，
         * 然后递归调用从右子树中删除刚刚找到的这个最小值
         */
        private BinaryNode<T> remove(T x, BinaryNode<T> tree)
        {
            if (tree == null)
            {
                //没有这个树
                return tree;
            }
            int compareResult = x.CompareTo(tree.element);
            if (compareResult < 0)
            {
                tree.left = remove(x, tree.left);
            }
            else if (compareResult > 0)
            {
                tree.right = remove(x, tree.right);
            }
            else if (tree.left != null && tree.right != null)
            {
                tree.element = findMin(tree.right).element;
                tree.right = remove(tree.element, tree.right);
            }
            else
            {
                tree = (tree.left != null) ? tree.left : tree.right;
            }
            return tree;

        }

        private void printTree(BinaryNode<T> node, int Layer = 0)
        {
            //if (tree == null)
            //{
            //    return;
            //}
            //Console.WriteLine(tree.element + " ");
            //printTree(tree.left);
            //printTree(tree.right);

            if (node == null) { return; }
            printTree(node.right, Layer + 3);
            for (int i = 0; i < Layer; i++)
            {
                Console.Write(" ");
            }
            Console.WriteLine(node.element);
            printTree(node.left, Layer + 3);
        }

        public class BinaryNode<T> where T : struct, IComparable<T>
        {
            public T element;
            public BinaryNode<T> left;
            public BinaryNode<T> right;

            public BinaryNode(T element)
            {
                new BinaryNode<T>(element, null, null);
            }

            public BinaryNode(T element, BinaryNode<T> left, BinaryNode<T> right)
            {
                this.element = element;
                this.left = left;
                this.right = right;
            }
        }
    }
}
