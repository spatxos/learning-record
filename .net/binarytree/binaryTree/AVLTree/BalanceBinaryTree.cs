using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace binaryTree.AVLTree
{
    using binaryTree.BinaryTree;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace BalanceBinaryTree
    {
        class TreeNode
        {
            public int data;
            public TreeNode left;
            public TreeNode right;
            public int height;//该节点的高度
            public TreeNode(int data)
            {
                this.data = data;
                left = null;
                right = null;
                height = 0;
            }
        }
        class Tree
        {
            public TreeNode root = null;
            public int Height(TreeNode root)
            {
                if (root == null)
                    return -1;
                int left_height = Height(root.left);
                int right_height = Height(root.right);

                return 1 + (left_height > right_height ? left_height : right_height);
            }
            public TreeNode FindMin(TreeNode root)
            {
                if (root == null)
                    return null;

                TreeNode min_node = null;

                if (root.left != null)
                    min_node = FindMin(root.left);
                else
                    min_node = root;

                return min_node;
            }
            /// <summary>
            ///         3       --root
            ///        / \
            ///       ||  6     --root.right
            ///          / \
            ///         ||  8
            /// </summary>
            public TreeNode SingleRotate_Left(ref TreeNode root)
            {
                TreeNode r_right = root.right;
                root.right = r_right.left;
                r_right.left = root;

                root.height = Height(root);
                r_right.height = Height(r_right);

                return r_right;
            }
            /// <summary>
            ///         6       --root
            ///        / \
            ///       4  ||     --root.left
            ///      / \
            ///     3  ||
            /// </summary>
            public TreeNode SingleRotate_Right(ref TreeNode root)
            {
                TreeNode r_left = root.left;

                root.left = r_left.right;
                r_left.right = root;

                root.height = Height(root);
                r_left.height = Height(r_left);

                return r_left;
            }
            /// <summary>
            ///         6
            ///        / \
            ///       4  ||
            ///      / \
            ///     ||  5
            ///        / \
            ///       || ||
            /// </summary>
            public TreeNode DoubleRotate_LeftRight(ref TreeNode root)
            {
                root.left = SingleRotate_Left(ref root.left);
                return SingleRotate_Right(ref root);
            }
            /// <summary>
            ///         6
            ///        / \
            ///       ||  8
            ///          / \
            ///         7  ||
            ///        / \
            ///       || ||
            /// </summary>
            public TreeNode DoubleRotate_RightLeft(ref TreeNode root)
            {
                root.right = SingleRotate_Right(ref root.right);
                return SingleRotate_Left(ref root);
            }
            public TreeNode Insert(ref TreeNode root, int data)
            {
                if (root == null)
                {
                    root = new TreeNode(data);
                }
                else if (data < root.data)
                {
                    root.left = Insert(ref root.left, data);
                    if (Height(root.left) - Height(root.right) == 2)
                    {
                        if (data < root.left.data)
                        {
                            root = SingleRotate_Right(ref root);
                        }
                        else
                        {
                            root = DoubleRotate_LeftRight(ref root);
                        }
                    }
                }
                else if (data >= root.data)
                {
                    root.right = Insert(ref root.right, data);
                    if (Height(root.right) - Height(root.left) == 2)
                    {
                        if (data >= root.right.data)
                        {
                            root = SingleRotate_Left(ref root);
                        }
                        else
                        {
                            root = DoubleRotate_RightLeft(ref root);
                        }
                    }
                }
                root.height = Height(root);
                return root;
            }
            public TreeNode Remove(ref TreeNode root, int data)
            {
                if (root == null)
                {
                    return null;
                }
                else if (data < root.data)
                {
                    root.left = Remove(ref root.left, data);
                    if (Height(root.right) - Height(root.left) == 2)
                    {
                        if (Height(root.right.right) >= Height(root.right.left))
                        {
                            root = SingleRotate_Left(ref root);
                        }
                        else
                        {
                            root = DoubleRotate_RightLeft(ref root);
                        }
                    }
                }
                else if (data > root.data)
                {
                    root.right = Remove(ref root.right, data);
                    if (Height(root.left) - Height(root.right) == 2)
                    {
                        if (Height(root.left.left) >= Height(root.left.right))
                        {
                            root = SingleRotate_Right(ref root);
                        }
                        else
                        {
                            root = DoubleRotate_LeftRight(ref root);
                        }
                    }
                }
                else
                {
                    if (root.left != null && root.right != null)
                    {
                        root.data = FindMin(root.right).data;//用后继替换
                        root.right = Remove(ref root.right, root.data);//在右边删除替换节点
                    }
                    else
                    {
                        root = root.left != null ? root.left : root.right;//如果如果左右都是空，那么root也是null。如果只有一个子节点，那么等与这个子节点
                    }
                }

                if (root != null)//加这一层判断是因为，root可能是叶子结点被删除后变成的null
                {
                    root.height = Height(root);
                }
                return root;
            }
            public void PreOrder(TreeNode root)
            {
                Stack<TreeNode> stack = new Stack<TreeNode>();
                TreeNode current = root;
                while (current != null || stack.Count > 0)
                {
                    while (current != null)
                    {
                        Console.Write(current.data + " ");
                        stack.Push(current);
                        current = current.left;
                    }
                    if (stack.Count > 0)
                    {
                        current = stack.Pop().right;
                    }
                }
            }
            public void InOrder(TreeNode root)
            {
                Stack<TreeNode> stack = new Stack<TreeNode>();
                TreeNode current = root;
                while (current != null || stack.Count > 0)
                {
                    while (current != null)
                    {
                        stack.Push(current);
                        current = current.left;
                    }
                    if (stack.Count > 0)
                    {
                        current = stack.Pop();
                        Console.Write(current.data + " ");
                        current = current.right;
                    }
                }
            }
            public void PostOrder(TreeNode root)
            {
                Stack<TreeNode> stack = new Stack<TreeNode>();
                TreeNode current = root;
                TreeNode visited = null;
                while (current != null)
                {
                    stack.Push(current);
                    current = current.left;
                }
                while (stack.Count > 0)
                {
                    current = stack.Pop();
                    if (current.right == null || current.right == visited)
                    {
                        Console.Write(current.data + " ");
                        visited = current;
                    }
                    else
                    {
                        stack.Push(current);
                        current = stack.Peek().right;
                        while (current != null)
                        {
                            stack.Push(current);
                            current = current.left;
                        }
                    }
                }
            }
            /// <summary>
            /// 打印二叉树
            /// </summary>
            /// <param name="node"></param>
            /// <param name="Layer"></param>
            public void PrintTree(TreeNode node, int Layer)
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
        }
    }

}
