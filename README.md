# BPlusTreeCollection
BPlusTreeCollection - Faster range search

# What is B+ Tree?
A B+ tree is an advanced form of a self-balancing tree in which all the values are present in the leaf level.

# Properties of a B+ Tree
1. All data will be in the leaf nodes and will have duplicated data in internal nodes. Internal nodes can only store the key values.
2. All nodes will have node size, it depends on configuration (here I have taken, 1024 values in a node)
3. This also called as max degree. Node will have a max of node size - 1 values underneath it.
4. The leaf nodes of a B+ tree are linked together in the form of a singly linked lists to make the search queries more efficient.

# How it works?

![image](https://github.com/psangrama/BPlusTreeCollection/assets/113549457/f0ad8ec8-b117-4f94-8f88-0421d0ae81c7)

Better visualization for understanding, refer https://www.cs.usfca.edu/~galles/visualization/BPlusTree.html

My Primary Intension  behind creating this.

