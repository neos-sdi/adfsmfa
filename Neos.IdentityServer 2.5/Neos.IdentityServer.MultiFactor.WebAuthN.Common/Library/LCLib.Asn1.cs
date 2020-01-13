//+-------------------------------------------------------------------------------+
//| Copyright (c) 2003 Liping Dai. All rights reserved.                           |
//| Web: www.lipingshare.com                                                      |
//| Email: lipingshare@yahoo.com                                                  |
//|                                                                               |
//| Copyright and Permission Details:                                             |
//| =================================                                             |
//| Permission is hereby granted, free of charge, to any person obtaining a copy  |
//| of this software and associated documentation files (the "Software"), to deal |
//| in the Software without restriction, including without limitation the rights  |
//| to use, copy, modify, merge, publish, distribute, and/or sell copies of the   |
//| Software, subject to the following conditions:                                |
//|                                                                               |
//| 1. Redistributions of source code must retain the above copyright notice, this|
//| list of conditions and the following disclaimer.                              |
//|                                                                               |
//| 2. Redistributions in binary form must reproduce the above copyright notice,  |
//| this list of conditions and the following disclaimer in the documentation     |
//| and/or other materials provided with the distribution.                        |
//|                                                                               |
//| THE SOFTWARE PRODUCT IS PROVIDED “AS IS” WITHOUT WARRANTY OF ANY KIND,        |
//| EITHER EXPRESS OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED         |
//| WARRANTIES OF TITLE, NON-INFRINGEMENT, MERCHANTABILITY AND FITNESS FOR        |
//| A PARTICULAR PURPOSE.                                                         |
//+-------------------------------------------------------------------------------+
using System;
using System.Collections;
using System.IO;
using System.Text;


namespace Neos.IdentityServer.MultiFactor.WebAuthN.Library.LCLib
{
    /// <summary>
    /// IAsn1Node interface.
    /// </summary>
    public interface IAsn1Node
    {
        /// <summary>
        /// Load data from Stream.
        /// </summary>
        /// <param name="xdata"></param>
        /// <returns>true:Succeed; false:failed.</returns>
        bool LoadData(Stream xdata);

        /// <summary>
        /// Save node data into Stream.
        /// </summary>
        /// <param name="xdata">Stream.</param>
        /// <returns>true:Succeed; false:failed.</returns>
        bool SaveData(Stream xdata);

        /// <summary>
        /// Get parent node.
        /// </summary>
        Asn1Node ParentNode { get; }

        /// <summary>
        /// Add child node at the end of children list.
        /// </summary>
        /// <param name="xdata">Asn1Node</param>
        void AddChild(Asn1Node xdata);

        /// <summary>
        /// Insert a node in the children list before the pointed index.
        /// </summary>
        /// <param name="xdata">Asn1Node</param>
        /// <param name="index">0 based index.</param>
        int InsertChild(Asn1Node xdata, int index);

        /// <summary>
        /// Insert a node in the children list before the pointed node.
        /// </summary>
        /// <param name="xdata">Asn1Node that will be instered in the children list.</param>
        /// <param name="indexNode">Index node.</param>
		/// <returns>New node index.</returns>
		int InsertChild(Asn1Node xdata, Asn1Node indexNode);

        /// <summary>
        /// Insert a node in the children list after the pointed index.
        /// </summary>
        /// <param name="xdata">Asn1Node</param>
        /// <param name="index">0 based index.</param>
		/// <returns>New node index.</returns>
		int InsertChildAfter(Asn1Node xdata, int index);

        /// <summary>
        /// Insert a node in the children list after the pointed node.
        /// </summary>
        /// <param name="xdata">Asn1Node that will be instered in the children list.</param>
        /// <param name="indexNode">Index node.</param>
		/// <returns>New node index.</returns>
		int InsertChildAfter(Asn1Node xdata, Asn1Node indexNode);

        /// <summary>
        /// Remove a child from children node list by index.
        /// </summary>
        /// <param name="index">0 based index.</param>
        /// <returns>The Asn1Node just removed from the list.</returns>
        Asn1Node RemoveChild(int index);

        /// <summary>
        /// Remove the child from children node list.
        /// </summary>
        /// <param name="node">The node needs to be removed.</param>
        /// <returns></returns>
        Asn1Node RemoveChild(Asn1Node node);

        /// <summary>
        /// Get child node count.
        /// </summary>
        long ChildNodeCount { get; }

        /// <summary>
        /// Retrieve child node by index.
        /// </summary>
        /// <param name="index">0 based index.</param>
        /// <returns>0 based index.</returns>
        Asn1Node GetChildNode(int index);

        /// <summary>
        /// Get descendant node by node path.
        /// </summary>
        /// <param name="nodePath">relative node path that refer to current node.</param>
        /// <returns></returns>
        Asn1Node GetDescendantNodeByPath(string nodePath);

        /// <summary>
        /// Get/Set tag value.
        /// </summary>
        byte Tag { get; set; }

        /// <summary>
        /// Get tag name.
        /// </summary>
        string TagName { get; }

        /// <summary>
        /// Get data length. Not included the unused bits byte for BITSTRING.
        /// </summary>
        long DataLength { get; }

        /// <summary>
        /// Get the length field bytes.
        /// </summary>
        long LengthFieldBytes { get; }

        /// <summary>
        /// Get data offset.
        /// </summary>
        long DataOffset { get; }

        /// <summary>
        /// Get unused bits for BITSTRING.
        /// </summary>
        byte UnusedBits { get; }

        /// <summary>
        /// Get/Set node data by byte[], the data length field content and all the 
        /// node in the parent chain will be adjusted.
        /// </summary>
        byte[] Data { get; set; }

        /// <summary>
        /// Get/Set parseEncapsulatedData. This property will be inherited by the 
        /// child nodes when loading data.
        /// </summary>
        bool ParseEncapsulatedData { get; set; }

        /// <summary>
        /// Get the deepness of the node.
        /// </summary>
        long Deepness { get; }

        /// <summary>
        /// Get the path string of the node.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Get the node and all the descendents text description.
        /// </summary>
        /// <param name="startNode">starting node.</param>
        /// <param name="lineLen">line length.</param>
        /// <returns></returns>
        string GetText(Asn1Node startNode, int lineLen);

        /// <summary>
        /// Retrieve the node description.
        /// </summary>
        /// <param name="pureHexMode">true:Return hex string only;
        /// false:Convert to more readable string depending on the node tag.</param>
        /// <returns>string</returns>
        string GetDataStr(bool pureHexMode);

        /// <summary>
        /// Get node label string.
        /// </summary>
        /// <param name="mask">
		/// <code>
		/// SHOW_OFFSET
		/// SHOW_DATA
		/// USE_HEX_OFFSET
		/// SHOW_TAG_NUMBER
		/// SHOW_PATH</code>
		/// </param>
        /// <returns>string</returns>
        string GetLabel(uint mask);

        /// <summary>
        /// Clone a new Asn1Node by current node.
        /// </summary>
        /// <returns>new node.</returns>
        Asn1Node Clone();

        /// <summary>
        /// Clear data and children list.
        /// </summary>
        void ClearAll();
    }

    /// <summary>
    /// Asn1Node, implemented IAsn1Node interface.
    /// </summary>
	public class Asn1Node : IAsn1Node
    {
        private long lengthFieldBytes;
        private byte[] data;
        private ArrayList childNodeList;
        private const int indentStep = 3;
        private bool isIndefiniteLength = false;
        private bool parseEncapsulatedData = true;

        /// <summary>
        /// Default Asn1Node text line length.
        /// </summary>
        public const int defaultLineLen = 80;

        /// <summary>
        /// Minium line length.
        /// </summary>
        public const int minLineLen = 60;

        private Asn1Node(Asn1Node parentNode, long dataOffset)
        {
            Init();
            Deepness = parentNode.Deepness + 1;
            this.ParentNode = parentNode;
            this.DataOffset = dataOffset;
        }

        private void Init()
        {
            childNodeList = new ArrayList();
            data = null;
            DataLength = 0;
            lengthFieldBytes = 0;
            UnusedBits = 0;
            Tag = Asn1Tag.SEQUENCE | Asn1TagClasses.CONSTRUCTED;
            childNodeList.Clear();
            Deepness = 0;
            ParentNode = null;
        }

        private string GetHexPrintingStr(Asn1Node startNode, string baseLine,
            string lStr, int lineLen)
        {
            string nodeStr = "";
            string iStr = GetIndentStr(startNode);
            string dataStr = Asn1Util.ToHexString(data);
            if (dataStr.Length > 0)
            {
                if (baseLine.Length + dataStr.Length < lineLen)
                {
                    nodeStr += baseLine + "'" + dataStr + "'";
                }
                else
                {
                    nodeStr += baseLine + FormatLineHexString(
                        lStr,
                        iStr.Length,
                        lineLen,
                        dataStr
                        );
                }
            }
            else
            {
                nodeStr += baseLine;
            }
            return nodeStr + "\r\n";
        }

        private string FormatLineString(string lStr, int indent, int lineLen, string msg)
        {
            string retval = "";
            indent += indentStep;
            int realLen = lineLen - indent;
            int sLen = indent;
            int currentp;
            for (currentp = 0; currentp < msg.Length; currentp += realLen)
            {
                if (currentp + realLen > msg.Length)
                {
                    retval += "\r\n" + lStr + Asn1Util.GenStr(sLen, ' ') +
                        "'" + msg.Substring(currentp, msg.Length - currentp) + "'";
                }
                else
                {
                    retval += "\r\n" + lStr + Asn1Util.GenStr(sLen, ' ') + "'" +
                        msg.Substring(currentp, realLen) + "'";
                }
            }
            return retval;
        }

        private string FormatLineHexString(string lStr, int indent, int lineLen, string msg)
        {
            string retval = "";
            indent += indentStep;
            int realLen = lineLen - indent;
            int sLen = indent;
            int currentp;
            for (currentp = 0; currentp < msg.Length; currentp += realLen)
            {
                if (currentp + realLen > msg.Length)
                {
                    retval += "\r\n" + lStr + Asn1Util.GenStr(sLen, ' ') +
                        msg.Substring(currentp, msg.Length - currentp);
                }
                else
                {
                    retval += "\r\n" + lStr + Asn1Util.GenStr(sLen, ' ') +
                        msg.Substring(currentp, realLen);
                }
            }
            return retval;
        }


        //PublicMembers

        /// <summary>
        /// Constructor, initialize all the members.
        /// </summary>
        public Asn1Node()
        {
            Init();
            DataOffset = 0;
        }

        /// <summary>
        /// Get/Set isIndefiniteLength.
        /// </summary>
        public bool IsIndefiniteLength
        {
            get
            {
                return isIndefiniteLength;
            }
            set
            {
                isIndefiniteLength = value;
            }
        }

        /// <summary>
        /// Clone a new Asn1Node by current node.
        /// </summary>
        /// <returns>new node.</returns>
        public Asn1Node Clone()
        {
            MemoryStream ms = new MemoryStream();
            this.SaveData(ms);
            ms.Position = 0;
            Asn1Node node = new Asn1Node();
            node.LoadData(ms);
            return node;
        }

        /// <summary>
        /// Get/Set tag value.
        /// </summary>
        public byte Tag { get; set; }

        /// <summary>
        /// Load data from byte[].
        /// </summary>
        /// <param name="byteData">byte[]</param>
        /// <returns>true:Succeed; false:failed.</returns>
        public bool LoadData(byte[] byteData)
        {
            bool retval = true;
            try
            {
                MemoryStream ms = new MemoryStream(byteData)
                {
                    Position = 0
                };
                retval = LoadData(ms);
                ms.Close();
            }
            catch
            {
                retval = false;
            }
            return retval;
        }

        /// <summary>
        /// Retrieve all the node count in the node subtree.
        /// </summary>
        /// <param name="node">starting node.</param>
        /// <returns>long integer node count in the node subtree.</returns>
        public static long GetDescendantNodeCount(Asn1Node node)
        {
            long count = 0;
            count += node.ChildNodeCount;
            for (int i = 0; i < node.ChildNodeCount; i++)
            {
                count += GetDescendantNodeCount(node.GetChildNode(i));
            }
            return count;
        }

        /// <summary>
        /// Load data from Stream. Start from current position.
        /// This function sets requireRecalculatePar to false then calls InternalLoadData 
        /// to complish the task.
        /// </summary>
        /// <param name="xdata">Stream</param>
        /// <returns>true:Succeed; false:failed.</returns>
        public bool LoadData(Stream xdata)
        {
            bool retval = false;
            try
            {
                RequireRecalculatePar = false;
                retval = InternalLoadData(xdata);
                return retval;
            }
            finally
            {
                RequireRecalculatePar = true;
                RecalculateTreePar();
            }
        }

        /// <summary>
        /// Call SaveData and return byte[] as result instead stream.
        /// </summary>
        /// <returns></returns>
        public byte[] GetRawData()
        {
            MemoryStream ms = new MemoryStream();
            SaveData(ms);
            byte[] retval = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(retval, 0, (int)ms.Length);
            ms.Close();
            return retval;
        }

        /// <summary>
        /// Get if data is empty.
        /// </summary>
        public bool IsEmptyData
        {
            get
            {
                if (data == null) return true;
                if (data.Length < 1)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Save node data into Stream.
        /// </summary>
        /// <param name="xdata">Stream.</param>
        /// <returns>true:Succeed; false:failed.</returns>
        public bool SaveData(Stream xdata)
        {
            bool retval = true;
            long nodeCount = ChildNodeCount;
            xdata.WriteByte(Tag);
            int tmpLen = Asn1Util.DERLengthEncode(xdata, (ulong)DataLength);
            if ((Tag) == Asn1Tag.BIT_STRING)
            {
                xdata.WriteByte(UnusedBits);
            }
            if (nodeCount == 0)
            {
                if (data != null)
                {
                    xdata.Write(data, 0, data.Length);
                }
            }
            else
            {
                Asn1Node tempNode;
                int i;
                for (i = 0; i < nodeCount; i++)
                {
                    tempNode = GetChildNode(i);
                    retval = tempNode.SaveData(xdata);
                }
            }
            return retval;
        }

        /// <summary>
        /// Clear data and children list.
        /// </summary>
        public void ClearAll()
        {
            data = null;
            Asn1Node tempNode;
            for (int i = 0; i < childNodeList.Count; i++)
            {
                tempNode = (Asn1Node)childNodeList[i];
                tempNode.ClearAll();
            }
            childNodeList.Clear();
            RecalculateTreePar();
        }


        /// <summary>
        /// Add child node at the end of children list.
        /// </summary>
        /// <param name="xdata">the node that will be add in.</param>
        public void AddChild(Asn1Node xdata)
        {
            childNodeList.Add(xdata);
            RecalculateTreePar();
        }

        /// <summary>
        /// Insert a node in the children list before the pointed index.
        /// </summary>
        /// <param name="xdata">Asn1Node</param>
        /// <param name="index">0 based index.</param>
		/// <returns>New node index.</returns>
		public int InsertChild(Asn1Node xdata, int index)
        {
            childNodeList.Insert(index, xdata);
            RecalculateTreePar();
            return index;
        }

        /// <summary>
        /// Insert a node in the children list before the pointed node.
        /// </summary>
        /// <param name="xdata">Asn1Node that will be instered in the children list.</param>
        /// <param name="indexNode">Index node.</param>
		/// <returns>New node index.</returns>
		public int InsertChild(Asn1Node xdata, Asn1Node indexNode)
        {
            int index = childNodeList.IndexOf(indexNode);
            childNodeList.Insert(index, xdata);
            RecalculateTreePar();
            return index;
        }

        /// <summary>
        /// Insert a node in the children list after the pointed node.
        /// </summary>
        /// <param name="xdata">Asn1Node</param>
        /// <param name="indexNode">Index node.</param>
		/// <returns>New node index.</returns>
		public int InsertChildAfter(Asn1Node xdata, Asn1Node indexNode)
        {
            int index = childNodeList.IndexOf(indexNode) + 1;
            childNodeList.Insert(index, xdata);
            RecalculateTreePar();
            return index;
        }

        /// <summary>
        /// Insert a node in the children list after the pointed node.
        /// </summary>
        /// <param name="xdata">Asn1Node that will be instered in the children list.</param>
        /// <param name="index">0 based index.</param>
		/// <returns>New node index.</returns>
		public int InsertChildAfter(Asn1Node xdata, int index)
        {
            int xindex = index + 1;
            childNodeList.Insert(xindex, xdata);
            RecalculateTreePar();
            return xindex;
        }

        /// <summary>
        /// Remove a child from children node list by index.
        /// </summary>
        /// <param name="index">0 based index.</param>
        /// <returns>The Asn1Node just removed from the list.</returns>
        public Asn1Node RemoveChild(int index)
        {
            Asn1Node retval = null;
            if (index < (childNodeList.Count - 1))
            {
                retval = (Asn1Node)childNodeList[index + 1];
            }
            childNodeList.RemoveAt(index);
            if (retval == null)
            {
                if (childNodeList.Count > 0)
                {
                    retval = (Asn1Node)childNodeList[childNodeList.Count - 1];
                }
                else
                {
                    retval = this;
                }
            }
            RecalculateTreePar();
            return retval;
        }

        /// <summary>
        /// Remove the child from children node list.
        /// </summary>
        /// <param name="node">The node needs to be removed.</param>
        /// <returns></returns>
        public Asn1Node RemoveChild(Asn1Node node)
        {
            Asn1Node retval = null;
            int i = childNodeList.IndexOf(node);
            retval = RemoveChild(i);
            return retval;
        }

        /// <summary>
        /// Get child node count.
        /// </summary>
        public long ChildNodeCount
        {
            get
            {
                return childNodeList.Count;
            }
        }

        /// <summary>
        /// Retrieve child node by index.
        /// </summary>
        /// <param name="index">0 based index.</param>
        /// <returns>0 based index.</returns>
        public Asn1Node GetChildNode(int index)
        {
            Asn1Node retval = null;
            if (index < ChildNodeCount)
            {
                retval = (Asn1Node)childNodeList[index];
            }
            return retval;
        }



        /// <summary>
        /// Get tag name.
        /// </summary>
        public string TagName
        {
            get
            {
                return Asn1Util.GetTagName(Tag);
            }
        }

        /// <summary>
        /// Get parent node.
        /// </summary>
        public Asn1Node ParentNode { get; private set; }

        /// <summary>
        /// Get the node and all the descendents text description.
        /// </summary>
        /// <param name="startNode">starting node.</param>
        /// <param name="lineLen">line length.</param>
        /// <returns></returns>
        public string GetText(Asn1Node startNode, int lineLen)
        {
            string nodeStr = "";
            string baseLine = "";
            string dataStr = "";
            const string lStr = "      |      |       | ";
            string oid, oidName;
            switch (Tag)
            {
                case Asn1Tag.BIT_STRING:
                    baseLine =
                        string.Format("{0,6}|{1,6}|{2,7}|{3} {4} UnusedBits:{5} : ",
                        DataOffset,
                        DataLength,
                        lengthFieldBytes,
                        GetIndentStr(startNode),
                        TagName,
                        UnusedBits
                        );
                    dataStr = Asn1Util.ToHexString(data);
                    if (baseLine.Length + dataStr.Length < lineLen)
                    {
                        if (dataStr.Length < 1)
                        {
                            nodeStr += baseLine + "\r\n";
                        }
                        else
                        {
                            nodeStr += baseLine + "'" + dataStr + "'\r\n";
                        }
                    }
                    else
                    {
                        nodeStr += baseLine + FormatLineHexString(
                            lStr,
                            GetIndentStr(startNode).Length,
                            lineLen,
                            dataStr + "\r\n"
                            );
                    }
                    break;
                case Asn1Tag.OBJECT_IDENTIFIER:
                    Oid xoid = new Oid();
                    oid = xoid.Decode(new MemoryStream(data));
                    oidName = "foo";//xoid.GetOidName(oid);
                    nodeStr += string.Format("{0,6}|{1,6}|{2,7}|{3} {4} : {5} [{6}]\r\n",
                        DataOffset,
                        DataLength,
                        lengthFieldBytes,
                        GetIndentStr(startNode),
                        TagName,
                        oidName,
                        oid
                        );
                    break;
                case Asn1Tag.RELATIVE_OID:
                    RelativeOid xiod = new RelativeOid();
                    oid = xiod.Decode(new MemoryStream(data));
                    oidName = "";
                    nodeStr += string.Format("{0,6}|{1,6}|{2,7}|{3} {4} : {5} [{6}]\r\n",
                        DataOffset,
                        DataLength,
                        lengthFieldBytes,
                        GetIndentStr(startNode),
                        TagName,
                        oidName,
                        oid
                        );
                    break;
                case Asn1Tag.PRINTABLE_STRING:
                case Asn1Tag.IA5_STRING:
                case Asn1Tag.UNIVERSAL_STRING:
                case Asn1Tag.VISIBLE_STRING:
                case Asn1Tag.NUMERIC_STRING:
                case Asn1Tag.UTC_TIME:
                case Asn1Tag.UTF8_STRING:
                case Asn1Tag.BMPSTRING:
                case Asn1Tag.GENERAL_STRING:
                case Asn1Tag.GENERALIZED_TIME:
                    baseLine =
                        string.Format("{0,6}|{1,6}|{2,7}|{3} {4} : ",
                        DataOffset,
                        DataLength,
                        lengthFieldBytes,
                        GetIndentStr(startNode),
                        TagName
                        );
                    if (Tag == Asn1Tag.UTF8_STRING)
                    {
                        UTF8Encoding unicode = new UTF8Encoding();
                        dataStr = unicode.GetString(data);
                    }
                    else
                    {
                        dataStr = Asn1Util.BytesToString(data);
                    }
                    if (baseLine.Length + dataStr.Length < lineLen)
                    {
                        nodeStr += baseLine + "'" + dataStr + "'\r\n";
                    }
                    else
                    {
                        nodeStr += baseLine + FormatLineString(
                            lStr,
                            GetIndentStr(startNode).Length,
                            lineLen,
                            dataStr) + "\r\n";
                    }
                    break;
                case Asn1Tag.INTEGER:
                    if (data != null && DataLength < 8)
                    {
                        nodeStr += string.Format("{0,6}|{1,6}|{2,7}|{3} {4} : {5}\r\n",
                            DataOffset,
                            DataLength,
                            lengthFieldBytes,
                            GetIndentStr(startNode),
                            TagName,
                            Asn1Util.BytesToLong(data).ToString()
                            );
                    }
                    else
                    {
                        baseLine =
                            string.Format("{0,6}|{1,6}|{2,7}|{3} {4} : ",
                            DataOffset,
                            DataLength,
                            lengthFieldBytes,
                            GetIndentStr(startNode),
                            TagName
                            );
                        nodeStr += GetHexPrintingStr(startNode, baseLine, lStr, lineLen);
                    }
                    break;
                default:
                    if ((Tag & Asn1Tag.TAG_MASK) == 6) // Visible string for certificate
                    {
                        baseLine =
                            string.Format("{0,6}|{1,6}|{2,7}|{3} {4} : ",
                            DataOffset,
                            DataLength,
                            lengthFieldBytes,
                            GetIndentStr(startNode),
                            TagName
                            );
                        dataStr = Asn1Util.BytesToString(data);
                        if (baseLine.Length + dataStr.Length < lineLen)
                        {
                            nodeStr += baseLine + "'" + dataStr + "'\r\n";
                        }
                        else
                        {
                            nodeStr += baseLine + FormatLineString(
                                lStr,
                                GetIndentStr(startNode).Length,
                                lineLen,
                                dataStr) + "\r\n";
                        }
                    }
                    else
                    {
                        baseLine =
                            string.Format("{0,6}|{1,6}|{2,7}|{3} {4} : ",
                            DataOffset,
                            DataLength,
                            lengthFieldBytes,
                            GetIndentStr(startNode),
                            TagName
                            );
                        nodeStr += GetHexPrintingStr(startNode, baseLine, lStr, lineLen);
                    }
                    break;
            };
            if (childNodeList.Count >= 0)
            {
                nodeStr += GetListStr(startNode, lineLen);
            }
            return nodeStr;
        }

        /// <summary>
        /// Get the path string of the node.
        /// </summary>
        public string Path { get; private set; } = "";

        /// <summary>
        /// Retrieve the node description.
        /// </summary>
        /// <param name="pureHexMode">true:Return hex string only;
        /// false:Convert to more readable string depending on the node tag.</param>
        /// <returns>string</returns>
        public string GetDataStr(bool pureHexMode)
        {
            const int lineLen = 32;
            string dataStr = "";
            if (pureHexMode)
            {
                dataStr = Asn1Util.FormatString(Asn1Util.ToHexString(data), lineLen, 2);
            }
            else
            {
                switch (Tag)
                {
                    case Asn1Tag.BIT_STRING:
                        dataStr = Asn1Util.FormatString(Asn1Util.ToHexString(data), lineLen, 2);
                        break;
                    case Asn1Tag.OBJECT_IDENTIFIER:
                        Oid xoid = new Oid();
                        dataStr = xoid.Decode(new MemoryStream(data));
                        break;
                    case Asn1Tag.RELATIVE_OID:
                        RelativeOid roid = new RelativeOid();
                        dataStr = roid.Decode(new MemoryStream(data));
                        break;
                    case Asn1Tag.PRINTABLE_STRING:
                    case Asn1Tag.IA5_STRING:
                    case Asn1Tag.UNIVERSAL_STRING:
                    case Asn1Tag.VISIBLE_STRING:
                    case Asn1Tag.NUMERIC_STRING:
                    case Asn1Tag.UTC_TIME:
                    case Asn1Tag.BMPSTRING:
                    case Asn1Tag.GENERAL_STRING:
                    case Asn1Tag.GENERALIZED_TIME:
                        dataStr = Asn1Util.BytesToString(data);
                        break;
                    case Asn1Tag.UTF8_STRING:
                        UTF8Encoding utf8 = new UTF8Encoding();
                        dataStr = utf8.GetString(data);
                        break;
                    case Asn1Tag.INTEGER:
                        dataStr = Asn1Util.FormatString(Asn1Util.ToHexString(data), lineLen, 2);
                        break;
                    default:
                        if ((Tag & Asn1Tag.TAG_MASK) == 6) // Visible string for certificate
                        {
                            dataStr = Asn1Util.BytesToString(data);
                        }
                        else
                        {
                            dataStr = Asn1Util.FormatString(Asn1Util.ToHexString(data), lineLen, 2);
                        }
                        break;
                };
            }
            return dataStr;
        }

        /// <summary>
        /// Get node label string.
        /// </summary>
        /// <param name="mask">
        /// <code>
		/// SHOW_OFFSET
		/// SHOW_DATA
		/// USE_HEX_OFFSET
		/// SHOW_TAG_NUMBER
		/// SHOW_PATH</code>
		/// </param>
        /// <returns>string</returns>
        public string GetLabel(uint mask)
        {
            string nodeStr = "";
            string dataStr = "";
            string offsetStr = "";
            if ((mask & TagTextMask.USE_HEX_OFFSET) != 0)
            {
                if ((mask & TagTextMask.SHOW_TAG_NUMBER) != 0)
                    offsetStr = string.Format("(0x{0:X2},0x{1:X6},0x{2:X4})", Tag, DataOffset, DataLength);
                else
                    offsetStr = string.Format("(0x{0:X6},0x{1:X4})", DataOffset, DataLength);
            }
            else
            {
                if ((mask & TagTextMask.SHOW_TAG_NUMBER) != 0)
                    offsetStr = string.Format("({0},{1},{2})", Tag, DataOffset, DataLength);
                else
                    offsetStr = string.Format("({0},{1})", DataOffset, DataLength);
            }
            string oid, oidName;
            switch (Tag)
            {
                case Asn1Tag.BIT_STRING:
                    if ((mask & TagTextMask.SHOW_OFFSET) != 0)
                    {
                        nodeStr += offsetStr;
                    }
                    nodeStr += " " + TagName + " UnusedBits: " + UnusedBits.ToString();
                    if ((mask & TagTextMask.SHOW_DATA) != 0)
                    {
                        dataStr = Asn1Util.ToHexString(data);
                        nodeStr += ((dataStr.Length > 0) ? " : '" + dataStr + "'" : "");
                    }
                    break;
                case Asn1Tag.OBJECT_IDENTIFIER:
                    Oid xoid = new Oid();
                    oid = xoid.Decode(data);
                    oidName = "bar";// xoid.GetOidName(oid);
                    if ((mask & TagTextMask.SHOW_OFFSET) != 0)
                    {
                        nodeStr += offsetStr;
                    }
                    nodeStr += " " + TagName;
                    nodeStr += " : " + oidName;
                    if ((mask & TagTextMask.SHOW_DATA) != 0)
                    {
                        nodeStr += ((oid.Length > 0) ? " : '" + oid + "'" : "");
                    }
                    break;
                case Asn1Tag.RELATIVE_OID:
                    RelativeOid roid = new RelativeOid();
                    oid = roid.Decode(data);
                    oidName = "";
                    if ((mask & TagTextMask.SHOW_OFFSET) != 0)
                    {
                        nodeStr += offsetStr;
                    }
                    nodeStr += " " + TagName;
                    nodeStr += " : " + oidName;
                    if ((mask & TagTextMask.SHOW_DATA) != 0)
                    {
                        nodeStr += ((oid.Length > 0) ? " : '" + oid + "'" : "");
                    }
                    break;
                case Asn1Tag.PRINTABLE_STRING:
                case Asn1Tag.IA5_STRING:
                case Asn1Tag.UNIVERSAL_STRING:
                case Asn1Tag.VISIBLE_STRING:
                case Asn1Tag.NUMERIC_STRING:
                case Asn1Tag.UTC_TIME:
                case Asn1Tag.UTF8_STRING:
                case Asn1Tag.BMPSTRING:
                case Asn1Tag.GENERAL_STRING:
                case Asn1Tag.GENERALIZED_TIME:
                    if ((mask & TagTextMask.SHOW_OFFSET) != 0)
                    {
                        nodeStr += offsetStr;
                    }
                    nodeStr += " " + TagName;
                    if ((mask & TagTextMask.SHOW_DATA) != 0)
                    {
                        if (Tag == Asn1Tag.UTF8_STRING)
                        {
                            UTF8Encoding unicode = new UTF8Encoding();
                            dataStr = unicode.GetString(data);
                        }
                        else
                        {
                            dataStr = Asn1Util.BytesToString(data);
                        }
                        nodeStr += ((dataStr.Length > 0) ? " : '" + dataStr + "'" : "");
                    }
                    break;
                case Asn1Tag.INTEGER:
                    if ((mask & TagTextMask.SHOW_OFFSET) != 0)
                    {
                        nodeStr += offsetStr;
                    }
                    nodeStr += " " + TagName;
                    if ((mask & TagTextMask.SHOW_DATA) != 0)
                    {
                        if (data != null && DataLength < 8)
                        {
                            dataStr = Asn1Util.BytesToLong(data).ToString();
                        }
                        else
                        {
                            dataStr = Asn1Util.ToHexString(data);
                        }
                        nodeStr += ((dataStr.Length > 0) ? " : '" + dataStr + "'" : "");
                    }
                    break;
                default:
                    if ((mask & TagTextMask.SHOW_OFFSET) != 0)
                    {
                        nodeStr += offsetStr;
                    }
                    nodeStr += " " + TagName;
                    if ((mask & TagTextMask.SHOW_DATA) != 0)
                    {
                        if ((Tag & Asn1Tag.TAG_MASK) == 6) // Visible string for certificate
                        {
                            dataStr = Asn1Util.BytesToString(data);
                        }
                        else
                        {
                            dataStr = Asn1Util.ToHexString(data);
                        }
                        nodeStr += ((dataStr.Length > 0) ? " : '" + dataStr + "'" : "");
                    }
                    break;
            };
            if ((mask & TagTextMask.SHOW_PATH) != 0)
            {
                nodeStr = "(" + Path + ") " + nodeStr;
            }
            return nodeStr;
        }

        /// <summary>
        /// Get data length. Not included the unused bits byte for BITSTRING.
        /// </summary>
        public long DataLength { get; private set; }

        /// <summary>
        /// Get the length field bytes.
        /// </summary>
        public long LengthFieldBytes
        {
            get
            {
                return lengthFieldBytes;
            }
        }

        /// <summary>
        /// Get/Set node data by byte[], the data length field content and all the 
        /// node in the parent chain will be adjusted.
        /// <br></br>
        /// It return all the child data for constructed node.
        /// </summary>
        public byte[] Data
        {
            get
            {
                MemoryStream xdata = new MemoryStream();
                long nodeCount = ChildNodeCount;
                if (nodeCount == 0)
                {
                    if (data != null)
                    {
                        xdata.Write(data, 0, data.Length);
                    }
                }
                else
                {
                    Asn1Node tempNode;
                    for (int i = 0; i < nodeCount; i++)
                    {
                        tempNode = GetChildNode(i);
                        tempNode.SaveData(xdata);
                    }
                }
                byte[] tmpData = new byte[xdata.Length];
                xdata.Position = 0;
                xdata.Read(tmpData, 0, (int)xdata.Length);
                xdata.Close();
                return tmpData;
            }
            set
            {
                SetData(value);
            }
        }

        /// <summary>
        /// Get the deepness of the node.
        /// </summary>
        public long Deepness { get; private set; }

        /// <summary>
        /// Get data offset.
        /// </summary>
        public long DataOffset { get; private set; }

        /// <summary>
        /// Get unused bits for BITSTRING.
        /// </summary>
        public byte UnusedBits { get; set; }


        /// <summary>
        /// Get descendant node by node path.
        /// </summary>
        /// <param name="nodePath">relative node path that refer to current node.</param>
        /// <returns></returns>
        public Asn1Node GetDescendantNodeByPath(string nodePath)
        {
            Asn1Node retval = this;
            if (nodePath == null) return retval;
            nodePath = nodePath.TrimEnd().TrimStart();
            if (nodePath.Length < 1) return retval;
            string[] route = nodePath.Split('/');
            try
            {
                for (int i = 1; i < route.Length; i++)
                {
                    retval = retval.GetChildNode(Convert.ToInt32(route[i]));
                }
            }
            catch
            {
                retval = null;
            }
            return retval;
        }

        /// <summary>
        /// Get node by OID.
        /// </summary>
        /// <param name="oid">OID.</param>
        /// <param name="startNode">Starting node.</param>
        /// <returns>Null or Asn1Node.</returns>
        static public Asn1Node GetDecendantNodeByOid(string oid, Asn1Node startNode)
        {
            Asn1Node retval = null;
            Oid xoid = new Oid();
            for (int i = 0; i < startNode.ChildNodeCount; i++)
            {
                Asn1Node childNode = startNode.GetChildNode(i);
                int tmpTag = childNode.Tag & Asn1Tag.TAG_MASK;
                if (tmpTag == Asn1Tag.OBJECT_IDENTIFIER)
                {
                    if (oid == xoid.Decode(childNode.Data))
                    {
                        retval = childNode;
                        break;
                    }
                }
                retval = GetDecendantNodeByOid(oid, childNode);
                if (retval != null) break;
            }
            return retval;
        }

        /// <summary>
        /// Constant of tag field length.
        /// </summary>
        public const int TagLength = 1;

        /// <summary>
        /// Constant of unused bits field length.
        /// </summary>
        public const int BitStringUnusedFiledLength = 1;

        /// <summary>
        /// Tag text generation mask definition.
        /// </summary>
        public class TagTextMask
        {
            /// <summary>
            /// Show offset.
            /// </summary>
            public const uint SHOW_OFFSET = 0x01;

            /// <summary>
            /// Show decoded data.
            /// </summary>
            public const uint SHOW_DATA = 0x02;

            /// <summary>
            /// Show offset in hex format.
            /// </summary>
            public const uint USE_HEX_OFFSET = 0x04;

            /// <summary>
            /// Show tag.
            /// </summary>
            public const uint SHOW_TAG_NUMBER = 0x08;

            /// <summary>
            /// Show node path.
            /// </summary>
            public const uint SHOW_PATH = 0x10;
        }

        /// <summary>
        /// Set/Get requireRecalculatePar. RecalculateTreePar function will not do anything
        /// if it is set to false. 
        /// </summary>
        protected bool RequireRecalculatePar { get; set; } = true;

        //ProtectedMembers

        /// <summary>
        /// Find root node and recalculate entire tree length field, 
        /// path, offset and deepness.
        /// </summary>
        protected void RecalculateTreePar()
        {
            if (!RequireRecalculatePar) return;
            Asn1Node rootNode;
            for (rootNode = this; rootNode.ParentNode != null;)
            {
                rootNode = rootNode.ParentNode;
            }
            ResetBranchDataLength(rootNode);
            rootNode.DataOffset = 0;
            rootNode.Deepness = 0;
            long subOffset = rootNode.DataOffset + TagLength + rootNode.lengthFieldBytes;
            ResetChildNodePar(rootNode, subOffset);
        }

        /// <summary>
        /// Recursively set all the node data length.
        /// </summary>
        /// <param name="node"></param>
        /// <returns>node data length.</returns>
        protected static long ResetBranchDataLength(Asn1Node node)
        {
            long retval = 0;
            long childDataLength = 0;
            if (node.ChildNodeCount < 1)
            {
                if (node.data != null)
                    childDataLength += node.data.Length;
            }
            else
            {
                for (int i = 0; i < node.ChildNodeCount; i++)
                {
                    childDataLength += ResetBranchDataLength(node.GetChildNode(i));
                }
            }
            node.DataLength = childDataLength;
            if (node.Tag == Asn1Tag.BIT_STRING)
                node.DataLength += BitStringUnusedFiledLength;
            ResetDataLengthFieldWidth(node);
            retval = node.DataLength + TagLength + node.lengthFieldBytes;
            return retval;
        }

        /// <summary>
        /// Encode the node data length field and set lengthFieldBytes and dataLength.
        /// </summary>
        /// <param name="node">The node needs to be reset.</param>
        protected static void ResetDataLengthFieldWidth(Asn1Node node)
        {
            MemoryStream tempStream = new MemoryStream();
            Asn1Util.DERLengthEncode(tempStream, (ulong)node.DataLength);
            node.lengthFieldBytes = tempStream.Length;
            tempStream.Close();
        }

        /// <summary>
        /// Recursively set all the child parameters, except dataLength.
        /// dataLength is set by ResetBranchDataLength.
        /// </summary>
        /// <param name="xNode">Starting node.</param>
        /// <param name="subOffset">Starting node offset.</param>
        protected void ResetChildNodePar(Asn1Node xNode, long subOffset)
        {
            int i;
            if (xNode.Tag == Asn1Tag.BIT_STRING)
            {
                subOffset++;
            }
            Asn1Node tempNode;
            for (i = 0; i < xNode.ChildNodeCount; i++)
            {
                tempNode = xNode.GetChildNode(i);
                tempNode.ParentNode = xNode;
                tempNode.DataOffset = subOffset;
                tempNode.Deepness = xNode.Deepness + 1;
                tempNode.Path = xNode.Path + '/' + i.ToString();
                subOffset += TagLength + tempNode.lengthFieldBytes;
                ResetChildNodePar(tempNode, subOffset);
                subOffset += tempNode.DataLength;
            }
        }

        /// <summary>
        /// Generate all the child text from childNodeList.
        /// </summary>
        /// <param name="startNode">Starting node.</param>
        /// <param name="lineLen">Line length.</param>
        /// <returns>Text string.</returns>
        protected string GetListStr(Asn1Node startNode, int lineLen)
        {
            string nodeStr = "";
            int i;
            Asn1Node tempNode;
            for (i = 0; i < childNodeList.Count; i++)
            {
                tempNode = (Asn1Node)childNodeList[i];
                nodeStr += tempNode.GetText(startNode, lineLen);
            }
            return nodeStr;
        }

        /// <summary>
        /// Generate the node indent string.
        /// </summary>
        /// <param name="startNode">The node.</param>
        /// <returns>Text string.</returns>
        protected string GetIndentStr(Asn1Node startNode)
        {
            string retval = "";
            long startLen = 0;
            if (startNode != null)
            {
                startLen = startNode.Deepness;
            }
            for (long i = 0; i < Deepness - startLen; i++)
            {
                retval += "   ";
            }
            return retval;
        }

        /// <summary>
        /// Decode ASN.1 encoded node Stream data.
        /// </summary>
        /// <param name="xdata">Stream data.</param>
        /// <returns>true:Succeed, false:Failed.</returns>
        protected bool GeneralDecode(Stream xdata)
        {
            bool retval = false;
            long nodeMaxLen;
            nodeMaxLen = xdata.Length - xdata.Position;
            Tag = (byte)xdata.ReadByte();
            long start, end;
            start = xdata.Position;
            DataLength = Asn1Util.DerLengthDecode(xdata, ref isIndefiniteLength);
            if (DataLength < 0) return retval; // Node data length can not be negative.
            end = xdata.Position;
            lengthFieldBytes = end - start;
            if (nodeMaxLen < (DataLength + TagLength + lengthFieldBytes))
            {
                return retval;
            }
            if (ParentNode == null || ((ParentNode.Tag & Asn1TagClasses.CONSTRUCTED) == 0))
            {
                if ((Tag & Asn1Tag.TAG_MASK) <= 0 || (Tag & Asn1Tag.TAG_MASK) > 0x1E) return retval;
            }
            if (Tag == Asn1Tag.BIT_STRING)
            {
                // First byte of BIT_STRING is unused bits.
                // BIT_STRING data does not include this byte.

                // Fixed by Gustaf Björklund.
                if (DataLength < 1) return retval; // We cannot read less than 1 - 1 bytes.

                UnusedBits = (byte)xdata.ReadByte();
                data = new byte[DataLength - 1];
                xdata.Read(data, 0, (int)(DataLength - 1));
            }
            else
            {
                data = new byte[DataLength];
                xdata.Read(data, 0, (int)(DataLength));
            }
            retval = true;
            return retval;
        }

        /// <summary>
        /// Decode ASN.1 encoded complex data type Stream data.
        /// </summary>
        /// <param name="xdata">Stream data.</param>
        /// <returns>true:Succeed, false:Failed.</returns>
        protected bool ListDecode(Stream xdata)
        {
            bool retval = false;
            long originalPosition = xdata.Position;
            long childNodeMaxLen;
            try
            {
                childNodeMaxLen = xdata.Length - xdata.Position;
                Tag = (byte)xdata.ReadByte();
                long start, end, offset;
                start = xdata.Position;
                DataLength = Asn1Util.DerLengthDecode(xdata, ref isIndefiniteLength);
                if (DataLength < 0 || childNodeMaxLen < DataLength)
                {
                    return retval;
                }
                end = xdata.Position;
                lengthFieldBytes = end - start;
                offset = DataOffset + TagLength + lengthFieldBytes;
                Stream secData;
                byte[] secByte;
                if (Tag == Asn1Tag.BIT_STRING)
                {
                    // First byte of BIT_STRING is unused bits.
                    // BIT_STRING data does not include this byte.
                    UnusedBits = (byte)xdata.ReadByte();
                    DataLength--;
                    offset++;
                }
                if (DataLength <= 0) return retval; // List data length cann't be zero.
                secData = new MemoryStream((int)DataLength);
                secByte = new byte[DataLength];
                xdata.Read(secByte, 0, (int)(DataLength));
                if (Tag == Asn1Tag.BIT_STRING) DataLength++;
                secData.Write(secByte, 0, secByte.Length);
                secData.Position = 0;
                while (secData.Position < secData.Length)
                {
                    Asn1Node node = new Asn1Node(this, offset)
                    {
                        parseEncapsulatedData = this.parseEncapsulatedData
                    };
                    start = secData.Position;
                    if (!node.InternalLoadData(secData)) return retval;
                    AddChild(node);
                    end = secData.Position;
                    offset += end - start;
                }
                retval = true;
            }
            finally
            {
                if (!retval)
                {
                    xdata.Position = originalPosition;
                    ClearAll();
                }
            }
            return retval;
        }

        /// <summary>
        /// Set the node data and recalculate the entire tree parameters.
        /// </summary>
        /// <param name="xdata">byte[] data.</param>
        protected void SetData(byte[] xdata)
        {
            if (childNodeList.Count > 0)
            {
                throw new Exception("Constructed node can't hold simple data.");
            }
            else
            {
                data = xdata;
                if (data != null)
                    DataLength = data.Length;
                else
                    DataLength = 0;
                RecalculateTreePar();
            }
        }

        /// <summary>
        /// Load data from Stream. Start from current position.
        /// </summary>
        /// <param name="xdata">Stream</param>
        /// <returns>true:Succeed; false:failed.</returns>
        protected bool InternalLoadData(Stream xdata)
        {
            bool retval = true;
            ClearAll();
            byte xtag;
            long curPosition = xdata.Position;
            xtag = (byte)xdata.ReadByte();
            xdata.Position = curPosition;
            int maskedTag = xtag & Asn1Tag.TAG_MASK;
            if (((xtag & Asn1TagClasses.CONSTRUCTED) != 0)
                || (parseEncapsulatedData
                && ((maskedTag == Asn1Tag.BIT_STRING)
                || (maskedTag == Asn1Tag.EXTERNAL)
                || (maskedTag == Asn1Tag.GENERAL_STRING)
                || (maskedTag == Asn1Tag.GENERALIZED_TIME)
                || (maskedTag == Asn1Tag.GRAPHIC_STRING)
                || (maskedTag == Asn1Tag.IA5_STRING)
                || (maskedTag == Asn1Tag.OCTET_STRING)
                || (maskedTag == Asn1Tag.PRINTABLE_STRING)
                || (maskedTag == Asn1Tag.SEQUENCE)
                || (maskedTag == Asn1Tag.SET)
                || (maskedTag == Asn1Tag.T61_STRING)
                || (maskedTag == Asn1Tag.UNIVERSAL_STRING)
                || (maskedTag == Asn1Tag.UTF8_STRING)
                || (maskedTag == Asn1Tag.VIDEOTEXT_STRING)
                || (maskedTag == Asn1Tag.VISIBLE_STRING)))
                )
            {
                if (!ListDecode(xdata))
                {
                    if (!GeneralDecode(xdata))
                    {
                        retval = false;
                    }
                }
            }
            else
            {
                if (!GeneralDecode(xdata)) retval = false;
            }
            return retval;
        }

        /// <summary>
        /// Get/Set parseEncapsulatedData. This property will be inherited by the 
        /// child nodes when loading data.
        /// </summary>
        public bool ParseEncapsulatedData
        {
            get
            {
                return parseEncapsulatedData;
            }
            set
            {
                if (parseEncapsulatedData == value) return;
                byte[] tmpData = Data;
                parseEncapsulatedData = value;
                ClearAll();
                if ((Tag & Asn1TagClasses.CONSTRUCTED) != 0 || parseEncapsulatedData)
                {
                    MemoryStream ms = new MemoryStream(tmpData)
                    {
                        Position = 0
                    };
                    bool isLoaded = true;
                    while (ms.Position < ms.Length)
                    {
                        Asn1Node tempNode = new Asn1Node
                        {
                            ParseEncapsulatedData = parseEncapsulatedData
                        };
                        if (!tempNode.LoadData(ms))
                        {
                            ClearAll();
                            isLoaded = false;
                            break;
                        }
                        AddChild(tempNode);
                    }
                    if (!isLoaded)
                    {
                        Data = tmpData;
                    }
                }
                else
                {
                    Data = tmpData;
                }
            }
        }
    }

    /// <summary>
    /// ASN.1 encoded data parser.
    /// This a higher level class which unilized Asn1Node class functionality to 
    /// provide functions for ASN.1 encoded files. 
    /// </summary>
    public class Asn1Parser
    {
        private Asn1Node rootNode = new Asn1Node();

        /// <summary>
        /// Get/Set parseEncapsulatedData. Reloading data is required after this property is reset.
        /// </summary>
        bool ParseEncapsulatedData
        {
            get
            {
                return rootNode.ParseEncapsulatedData;
            }
            set
            {
                rootNode.ParseEncapsulatedData = value;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
		public Asn1Parser()
        {
        }

        /// <summary>
        /// Get raw ASN.1 encoded data.
        /// </summary>
		public byte[] RawData { get; private set; }

        /// <summary>
        /// Load ASN.1 encoded data from a file.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public void LoadData(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open);
            RawData = new byte[fs.Length];
            fs.Read(RawData, 0, (int)fs.Length);
            fs.Close();
            MemoryStream ms = new MemoryStream(RawData);
            LoadData(ms);
        }

        /// <summary>
        /// Load PEM formated file.
        /// </summary>
        /// <param name="fileName">PEM file name.</param>
        public void LoadPemData(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            fs.Close();
            string dataStr = Asn1Util.BytesToString(data);
            if (Asn1Util.IsPemFormated(dataStr))
            {
                Stream ms = Asn1Util.PemToStream(dataStr);
                ms.Position = 0;
                LoadData(ms);
            }
            else
            {
                throw new Exception("It is a invalid PEM file: " + fileName);
            }
        }

        /// <summary>
        /// Load ASN.1 encoded data from Stream.
        /// </summary>
        /// <param name="stream">Stream data.</param>
        public void LoadData(Stream stream)
        {
            stream.Position = 0;
            if (!rootNode.LoadData(stream))
            {
                throw new Exception("Failed to load data.");
            }
            RawData = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(RawData, 0, RawData.Length);
        }

        /// <summary>
        /// Save data into a file.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public void SaveData(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create);
            rootNode.SaveData(fs);
            fs.Close();
        }

        /// <summary>
        /// Get root node.
        /// </summary>
        public Asn1Node RootNode
        {
            get
            {
                return rootNode;
            }
        }

        /// <summary>
        /// Get a node by path string.
        /// </summary>
        /// <param name="nodePath">Path string.</param>
        /// <returns>Asn1Node or null.</returns>
        public Asn1Node GetNodeByPath(string nodePath)
        {
            return rootNode.GetDescendantNodeByPath(nodePath);
        }

        /// <summary>
        /// Get a node by OID.
        /// </summary>
        /// <param name="oid">OID string.</param>
        /// <returns>Asn1Node or null.</returns>
        public Asn1Node GetNodeByOid(string oid)
        {
            return Asn1Node.GetDecendantNodeByOid(oid, rootNode);
        }

        /// <summary>
        /// Generate node text header. This method is used by GetNodeText to put heading.
        /// </summary>
        /// <param name="lineLen">Line length.</param>
        /// <returns>Header string.</returns>
        static public string GetNodeTextHeader(int lineLen)
        {
            string header = string.Format("Offset| Len  |LenByte|\r\n");
            header += "======+======+=======+" + Asn1Util.GenStr(lineLen + 10, '=') + "\r\n";
            return header;
        }

        /// <summary>
        /// Generate the root node text description.
        /// </summary>
        /// <returns>Text string.</returns>
        public override string ToString()
        {
            return GetNodeText(rootNode, 100);
        }

        /// <summary>
        /// Generate node text description. It uses GetNodeTextHeader to generate
        /// the heading and Asn1Node.GetText to generate the node text.
        /// </summary>
        /// <param name="node">Target node.</param>
        /// <param name="lineLen">Line length.</param>
        /// <returns>Text string.</returns>
        public static string GetNodeText(Asn1Node node, int lineLen)
        {
            string nodeStr = GetNodeTextHeader(lineLen);
            nodeStr += node.GetText(node, lineLen);
            return nodeStr;
        }
    }

    /// <summary>
    /// Define ASN.1 tag constants.
    /// </summary>
    /// 
    public class Asn1Tag
    {
        /// <summary>
        /// Tag mask constant value.
        /// </summary>
        public const byte TAG_MASK = 0x1F;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte BOOLEAN = 0x01;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte INTEGER = 0x02;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte BIT_STRING = 0x03;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte OCTET_STRING = 0x04;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte TAG_NULL = 0x05;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte OBJECT_IDENTIFIER = 0x06;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte OBJECT_DESCRIPTOR = 0x07;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte EXTERNAL = 0x08;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte REAL = 0x09;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte ENUMERATED = 0x0a;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte UTF8_STRING = 0x0c;

        /// <summary>
        /// Relative object identifier.
        /// </summary>
        public const byte RELATIVE_OID = 0x0d;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte SEQUENCE = 0x10;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte SET = 0x11;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte NUMERIC_STRING = 0x12;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte PRINTABLE_STRING = 0x13;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte T61_STRING = 0x14;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte VIDEOTEXT_STRING = 0x15;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte IA5_STRING = 0x16;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte UTC_TIME = 0x17;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte GENERALIZED_TIME = 0x18;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte GRAPHIC_STRING = 0x19;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte VISIBLE_STRING = 0x1a;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte GENERAL_STRING = 0x1b;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte UNIVERSAL_STRING = 0x1C;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte BMPSTRING = 0x1E;	/* 30: Basic Multilingual Plane/Unicode string */

        /// <summary>
        /// Constructor.
        /// </summary>
        public Asn1Tag()
        {
        }
    };

    /// <summary>
    /// Define ASN.1 tag class constants.
    /// </summary>
    /// 
    public class Asn1TagClasses
    {
        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte CLASS_MASK = 0xc0;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte UNIVERSAL = 0x00;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte CONSTRUCTED = 0x20;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte APPLICATION = 0x40;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte CONTEXT_SPECIFIC = 0x80;

        /// <summary>
        /// Constant value.
        /// </summary>
        public const byte PRIVATE = 0xc0;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Asn1TagClasses()
        {
        }
    };

    /// <summary>
    /// Utility functions.
    /// </summary>
    public class Asn1Util
    {

        /// <summary>
        /// Check if the string is ASN.1 encoded hex string.
        /// </summary>
        /// <param name="dataStr">The string.</param>
        /// <returns>true:Yes, false:No.</returns>
        public static bool IsAsn1EncodedHexStr(string dataStr)
        {
            bool retval = false;
            try
            {
                byte[] data = HexStrToBytes(dataStr);
                if (data.Length > 0)
                {
                    Asn1Node node = new Asn1Node();
                    retval = node.LoadData(data);
                }
            }
            catch
            {
                retval = false;
            }
            return retval;
        }

        /// <summary>
        /// Format a string to have certain line length and character group length.
        /// Sample result FormatString(xstr,32,2):
        /// <code>07 AE 0B E7 84 5A D4 6C 6A BD DF 8F 89 88 9E F1</code>
        /// </summary>
        /// <param name="inStr">source string.</param>
        /// <param name="lineLen">line length.</param>
        /// <param name="groupLen">group length.</param>
        /// <returns></returns>
        public static string FormatString(string inStr, int lineLen, int groupLen)
        {
            char[] tmpCh = new char[inStr.Length * 2];
            int i, c = 0, linec = 0;
            int gc = 0;
            for (i = 0; i < inStr.Length; i++)
            {
                tmpCh[c++] = inStr[i];
                gc++;
                linec++;
                if (gc >= groupLen && groupLen > 0)
                {
                    tmpCh[c++] = ' ';
                    gc = 0;
                }
                if (linec >= lineLen)
                {
                    tmpCh[c++] = '\r';
                    tmpCh[c++] = '\n';
                    linec = 0;
                }
            }
            string retval = new string(tmpCh);
            retval = retval.TrimEnd('\0');
            retval = retval.TrimEnd('\n');
            retval = retval.TrimEnd('\r');
            return retval;
        }

        /// <summary>
        /// Generate a string by duplicating <see cref="char"/> xch.
        /// </summary>
        /// <param name="len">duplicate times.</param>
        /// <param name="xch">the duplicated character.</param>
        /// <returns></returns>
        public static string GenStr(int len, char xch)
        {
            char[] ch = new char[len];
            for (int i = 0; i < len; i++)
            {
                ch[i] = xch;
            }
            return new string(ch);
        }

        /// <summary>
        /// Convert byte array to a <see cref="long"/> integer.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static long BytesToLong(byte[] bytes)
        {
            long tempInt = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                tempInt = tempInt << 8 | bytes[i];
            }
            return tempInt;
        }

        /// <summary>
        /// Convert a ASCII byte array to string, also filter out the null characters.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string BytesToString(byte[] bytes)
        {
            string retval = "";
            if (bytes == null || bytes.Length < 1) return retval;
            char[] cretval = new char[bytes.Length];
            for (int i = 0, j = 0; i < bytes.Length; i++)
            {
                if (bytes[i] != '\0')
                {
                    cretval[j++] = (char)bytes[i];
                }
            }
            retval = new string(cretval);
            retval = retval.TrimEnd('\0');
            return retval;
        }

        /// <summary>
        /// Convert ASCII string to byte array.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static byte[] StringToBytes(string msg)
        {
            byte[] retval = new byte[msg.Length];
            for (int i = 0; i < msg.Length; i++)
            {
                retval[i] = (byte)msg[i];
            }
            return retval;
        }

        /// <summary>
        /// Compare source and target byte array.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsEqual(byte[] source, byte[] target)
        {
            if (source == null) return false;
            if (target == null) return false;
            if (source.Length != target.Length) return false;
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] != target[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// Constant hex digits array.
        /// </summary>
        static char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7',
                                    '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

        /// <summary>
        /// Convert a byte array to hex string.
        /// </summary>
        /// <param name="bytes">source array.</param>
        /// <returns>hex string.</returns>
		public static string ToHexString(byte[] bytes)
        {
            if (bytes == null) return "";
            char[] chars = new char[bytes.Length * 2];
            int b, i;
            for (i = 0; i < bytes.Length; i++)
            {
                b = bytes[i];
                chars[i * 2] = hexDigits[b >> 4];
                chars[i * 2 + 1] = hexDigits[b & 0xF];
            }
            return new string(chars);
        }

        /// <summary>
        /// Check if the character is a valid hex digits.
        /// </summary>
        /// <param name="ch">source character.</param>
        /// <returns>true:Valid, false:Invalid.</returns>
        public static bool IsValidHexDigits(char ch)
        {
            bool retval = false;
            for (int i = 0; i < hexDigits.Length; i++)
            {
                if (hexDigits[i] == ch)
                {
                    retval = true;
                    break;
                }
            }
            return retval;
        }

        /// <summary>
        /// Get hex digits value.
        /// </summary>
        /// <param name="ch">source character.</param>
        /// <returns>hex digits value.</returns>
        public static byte GetHexDigitsVal(char ch)
        {
            byte retval = 0;
            for (int i = 0; i < hexDigits.Length; i++)
            {
                if (hexDigits[i] == ch)
                {
                    retval = (byte)i;
                    break;
                }
            }
            return retval;
        }

        /// <summary>
        /// Convert hex string to byte array.
        /// </summary>
        /// <param name="hexStr">Source hex string.</param>
        /// <returns>return byte array.</returns>
        public static byte[] HexStrToBytes(string hexStr)
        {
            hexStr = hexStr.Replace(" ", "");
            hexStr = hexStr.Replace("\r", "");
            hexStr = hexStr.Replace("\n", "");
            hexStr = hexStr.ToUpper();
            if ((hexStr.Length % 2) != 0) throw new Exception("Invalid Hex string: odd length.");
            int i;
            for (i = 0; i < hexStr.Length; i++)
            {
                if (!IsValidHexDigits(hexStr[i]))
                {
                    throw new Exception("Invalid Hex string: included invalid character [" +
                        hexStr[i] + "]");
                }
            }
            int bc = hexStr.Length / 2;
            byte[] retval = new byte[bc];
            int b1, b2, b;
            for (i = 0; i < bc; i++)
            {
                b1 = GetHexDigitsVal(hexStr[i * 2]);
                b2 = GetHexDigitsVal(hexStr[i * 2 + 1]);
                b = ((b1 << 4) | b2);
                retval[i] = (byte)b;
            }
            return retval;
        }

        /// <summary>
        /// Check if the source string is a valid hex string.
        /// </summary>
        /// <param name="hexStr">source string.</param>
        /// <returns>true:Valid, false:Invalid.</returns>
        public static bool IsHexStr(string hexStr)
        {
            byte[] bytes = null;
            try
            {
                bytes = HexStrToBytes(hexStr);
            }
            catch
            {
                return false;
            }
            if (bytes == null || bytes.Length < 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private const string PemStartStr = "-----BEGIN";
        private const string PemEndStr = "-----END";
        /// <summary>
        /// Check if the source string is PEM formated string.
        /// </summary>
        /// <param name="pemStr">source string.</param>
        /// <returns>true:Valid, false:Invalid.</returns>
        public static bool IsPemFormated(string pemStr)
        {
            byte[] data = null;
            try
            {
                data = PemToBytes(pemStr);
            }
            catch
            {
                return false;
            }
            return (data.Length > 0);
        }

        /// <summary>
        /// Check if a file is PEM formated.
        /// </summary>
        /// <param name="fileName">source file name.</param>
        /// <returns>true:Yes, false:No.</returns>
        public static bool IsPemFormatedFile(string fileName)
        {
            bool retval = false;
            try
            {
                FileStream fs = new FileStream(fileName, FileMode.Open);
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                fs.Close();
                string dataStr = BytesToString(data);
                retval = IsPemFormated(dataStr);
            }
            catch
            {
                retval = false;
            }
            return retval;
        }

        /// <summary>
        /// Convert PEM formated string into <see cref="Stream"/> and set the Stream position to 0.
        /// </summary>
        /// <param name="pemStr">source string.</param>
        /// <returns>output stream.</returns>
        public static Stream PemToStream(string pemStr)
        {
            byte[] bytes = PemToBytes(pemStr);
            MemoryStream retval = new MemoryStream(bytes)
            {
                Position = 0
            };
            return retval;
        }

        /// <summary>
        /// Convert PEM formated string into byte array.
        /// </summary>
        /// <param name="pemStr">source string.</param>
        /// <returns>output byte array.</returns>
        public static byte[] PemToBytes(string pemStr)
        {
            byte[] retval = null;
            string[] lines = pemStr.Split('\n');
            string base64Str = "";
            bool started = false, ended = false;
            string cline = "";
            for (int i = 0; i < lines.Length; i++)
            {
                cline = lines[i].ToUpper();
                if (cline == "") continue;
                if (cline.Length > PemStartStr.Length)
                {
                    if (!started && cline.Substring(0, PemStartStr.Length) == PemStartStr)
                    {
                        started = true;
                        continue;
                    }
                }
                if (cline.Length > PemEndStr.Length)
                {
                    if (cline.Substring(0, PemEndStr.Length) == PemEndStr)
                    {
                        ended = true;
                        break;
                    }
                }
                if (started)
                {
                    base64Str += lines[i];
                }
            }
            if (!(started && ended))
            {
                throw new Exception("'BEGIN'/'END' line is missing.");
            }
            base64Str = base64Str.Replace("\r", "");
            base64Str = base64Str.Replace("\n", "");
            base64Str = base64Str.Replace("\n", " ");
            retval = Convert.FromBase64String(base64Str);
            return retval;
        }

        /// <summary>
        /// Convert byte array to PEM formated string.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string BytesToPem(byte[] data)
        {
            return BytesToPem(data, "");
        }

        /// <summary>
        /// Retrieve PEM file heading.
        /// </summary>
        /// <param name="fileName">source file name.</param>
        /// <returns>heading string.</returns>
        public static string GetPemFileHeader(string fileName)
        {
            try
            {
                FileStream fs = new FileStream(fileName, FileMode.Open);
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                fs.Close();
                string dataStr = BytesToString(data);
                return GetPemHeader(dataStr);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Retrieve PEM heading from a PEM formated string.
        /// </summary>
        /// <param name="pemStr">source string.</param>
        /// <returns>heading string.</returns>
        public static string GetPemHeader(string pemStr)
        {
            string[] lines = pemStr.Split('\n');
            bool started = false;
            string cline = "";
            for (int i = 0; i < lines.Length; i++)
            {
                cline = lines[i].ToUpper().Replace("\r", "");
                if (cline == "") continue;
                if (cline.Length > PemStartStr.Length)
                {
                    if (!started && cline.Substring(0, PemStartStr.Length) == PemStartStr)
                    {
                        started = true;
                        string retstr = lines[i].Substring(PemStartStr.Length,
                                lines[i].Length -
                                PemStartStr.Length).Replace("-----", "");
                        return retstr.Replace("\r", "");
                    }
                }
                else
                {
                    continue;
                }
            }
            return "";
        }

        /// <summary>
        /// Convert byte array to PEM formated string and set the heading as pemHeader.
        /// </summary>
        /// <param name="data">source array.</param>
        /// <param name="pemHeader">PEM heading.</param>
        /// <returns>PEM formated string.</returns>
        public static string BytesToPem(byte[] data, string pemHeader)
        {
            if (pemHeader == null || pemHeader.Length < 1)
            {
                pemHeader = "ASN.1 Editor Generated PEM File";
            }
            string retval = "";
            if (pemHeader.Length > 0 && pemHeader[0] != ' ')
            {
                pemHeader = " " + pemHeader;
            }
            retval = Convert.ToBase64String(data);
            retval = FormatString(retval, 64, 0);
            retval = "-----BEGIN" + pemHeader + "-----\r\n" +
                     retval +
                     "\r\n-----END" + pemHeader + "-----\r\n";
            return retval;
        }

        /// <summary>
        /// Calculate how many bits is enough to hold ivalue.
        /// </summary>
        /// <param name="ivalue">source value.</param>
        /// <returns>bits number.</returns>
        public static int BitPrecision(ulong ivalue)
        {
            if (ivalue == 0) return 0;
            int l = 0, h = 8 * 4; // 4: sizeof(ulong)
            while (h - l > 1)
            {
                int t = (int)(l + h) / 2;
                if ((ivalue >> t) != 0)
                    l = t;
                else
                    h = t;
            }
            return h;
        }

        /// <summary>
        /// Calculate how many bytes is enough to hold the value.
        /// </summary>
        /// <param name="value">input value.</param>
        /// <returns>bytes number.</returns>
        public static int BytePrecision(ulong value)
        {
            int i;
            for (i = 4; i > 0; --i) // 4: sizeof(ulong)
                if ((value >> (i - 1) * 8) != 0)
                    break;
            return i;
        }

        /// <summary>
        /// ASN.1 DER length encoder.
        /// </summary>
        /// <param name="xdata">result output stream.</param>
        /// <param name="length">source length.</param>
        /// <returns>result bytes.</returns>
        public static int DERLengthEncode(Stream xdata, ulong length)
        {
            int i = 0;
            if (length <= 0x7f)
            {
                xdata.WriteByte((byte)length);
                i++;
            }
            else
            {
                xdata.WriteByte((byte)(BytePrecision(length) | 0x80));
                i++;
                for (int j = BytePrecision((ulong)length); j > 0; --j)
                {
                    xdata.WriteByte((byte)(length >> (j - 1) * 8));
                    i++;
                }
            }
            return i;
        }

        /// <summary>
        /// ASN.1 DER length decoder.
        /// </summary>
        /// <param name="bt">Source stream.</param>
        /// <param name="isIndefiniteLength">Output parameter.</param>
        /// <returns>Output length.</returns>
        public static long DerLengthDecode(Stream bt, ref bool isIndefiniteLength)
        {
            isIndefiniteLength = false;
            long length = 0;
            byte b;
            b = (byte)bt.ReadByte();
            if ((b & 0x80) == 0)
            {
                length = b;
            }
            else
            {
                long lengthBytes = b & 0x7f;
                if (lengthBytes == 0)
                {
                    isIndefiniteLength = true;
                    long sPos = bt.Position;
                    return -2; // Indefinite length.
                }
                length = 0;
                while (lengthBytes-- > 0)
                {
                    if ((length >> (8 * (4 - 1))) > 0) // 4: sizeof(long)
                    {
                        return -1; // Length overflow.
                    }
                    b = (byte)bt.ReadByte();
                    length = (length << 8) | b;
                }
            }
            return length;
        }

        /// <summary>
        /// Decode tag value to return tag name.
        /// </summary>
        /// <param name="tag">input tag.</param>
        /// <returns>tag name.</returns>
        static public string GetTagName(byte tag)
        {
            string retval = "";
            if ((tag & Asn1TagClasses.CLASS_MASK) != 0)
            {
                switch (tag & Asn1TagClasses.CLASS_MASK)
                {
                    case Asn1TagClasses.CONTEXT_SPECIFIC:
                        retval += "CONTEXT SPECIFIC (" + ((int)(tag & Asn1Tag.TAG_MASK)).ToString() + ")";
                        break;
                    case Asn1TagClasses.APPLICATION:
                        retval += "APPLICATION (" + ((int)(tag & Asn1Tag.TAG_MASK)).ToString() + ")";
                        break;
                    case Asn1TagClasses.PRIVATE:
                        retval += "PRIVATE (" + ((int)(tag & Asn1Tag.TAG_MASK)).ToString() + ")";
                        break;
                    case Asn1TagClasses.CONSTRUCTED:
                        retval += "CONSTRUCTED (" + ((int)(tag & Asn1Tag.TAG_MASK)).ToString() + ")";
                        break;
                    case Asn1TagClasses.UNIVERSAL:
                        retval += "UNIVERSAL (" + ((int)(tag & Asn1Tag.TAG_MASK)).ToString() + ")";
                        break;
                }
            }
            else
            {
                switch (tag & Asn1Tag.TAG_MASK)
                {
                    case Asn1Tag.BOOLEAN:
                        retval += "BOOLEAN";
                        break;
                    case Asn1Tag.INTEGER:
                        retval += "INTEGER";
                        break;
                    case Asn1Tag.BIT_STRING:
                        retval += "BIT STRING";
                        break;
                    case Asn1Tag.OCTET_STRING:
                        retval += "OCTET STRING";
                        break;
                    case Asn1Tag.TAG_NULL:
                        retval += "NULL";
                        break;
                    case Asn1Tag.OBJECT_IDENTIFIER:
                        retval += "OBJECT IDENTIFIER";
                        break;
                    case Asn1Tag.OBJECT_DESCRIPTOR:
                        retval += "OBJECT DESCRIPTOR";
                        break;
                    case Asn1Tag.RELATIVE_OID:
                        retval += "RELATIVE-OID";
                        break;
                    case Asn1Tag.EXTERNAL:
                        retval += "EXTERNAL";
                        break;
                    case Asn1Tag.REAL:
                        retval += "REAL";
                        break;
                    case Asn1Tag.ENUMERATED:
                        retval += "ENUMERATED";
                        break;
                    case Asn1Tag.UTF8_STRING:
                        retval += "UTF8 STRING";
                        break;
                    case (Asn1Tag.SEQUENCE):
                        retval += "SEQUENCE";
                        break;
                    case (Asn1Tag.SET):
                        retval += "SET";
                        break;
                    case Asn1Tag.NUMERIC_STRING:
                        retval += "NUMERIC STRING";
                        break;
                    case Asn1Tag.PRINTABLE_STRING:
                        retval += "PRINTABLE STRING";
                        break;
                    case Asn1Tag.T61_STRING:
                        retval += "T61 STRING";
                        break;
                    case Asn1Tag.VIDEOTEXT_STRING:
                        retval += "VIDEOTEXT STRING";
                        break;
                    case Asn1Tag.IA5_STRING:
                        retval += "IA5 STRING";
                        break;
                    case Asn1Tag.UTC_TIME:
                        retval += "UTC TIME";
                        break;
                    case Asn1Tag.GENERALIZED_TIME:
                        retval += "GENERALIZED TIME";
                        break;
                    case Asn1Tag.GRAPHIC_STRING:
                        retval += "GRAPHIC STRING";
                        break;
                    case Asn1Tag.VISIBLE_STRING:
                        retval += "VISIBLE STRING";
                        break;
                    case Asn1Tag.GENERAL_STRING:
                        retval += "GENERAL STRING";
                        break;
                    case Asn1Tag.UNIVERSAL_STRING:
                        retval += "UNIVERSAL STRING";
                        break;
                    case Asn1Tag.BMPSTRING:
                        retval += "BMP STRING";
                        break;
                    default:
                        retval += "UNKNOWN TAG";
                        break;
                };
            }
            return retval;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
		private Asn1Util()
        {
            //Private constructor.
        }

    }

    /// <summary>
    /// Summary description for BinaryDump.
    /// </summary>
    public class BinaryDump
    {
        public byte[] Data { get; set; } = null;

        public int OffsetWidth { get; set; } = 3;

        public int DataWidth { get; set; } = 16;


        public BinaryDump()
        {
        }

        public static string Dump(byte[] data, int offsetWidth, int dataWidth)
        {
            string retval = "";
            int offset = 0;
            for (offset = 0; offset < data.Length; offset++)
            {

            }
            return retval;
        }
    }

    /// <summary>
    /// BinaryView class. It is used to calculate hex view parameters.
    /// </summary>
    public class BinaryView
    {

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BinaryView()
        {
            CalculatePar();
        }

        /// <summary>
        /// Set hex view parameters. It calls <see cref="CalculatePar"/> to get the parameters.
        /// <code>
        /// Parameters Definition:
        /// 000000  30 82 05 32 30 82 04 1A  A0 03 02 01 02 02 0A 1F  0..20...........
        /// 000010  CE 8F 20 00 00 00 00 00  22 30 0D 06 09 2A 86 48  .. ....."0...*.H
        /// |----|offsetWidth                                         |--dataWidth --|
        /// |----- hexWidth ---------------------------------------|
        /// |----- totalWidth -------------------------------------------------------|
        /// </code>
        /// </summary>
        /// <param name="offsetWidth">input</param>
        /// <param name="dataWidth">input</param>
        public void SetPar(int offsetWidth, int dataWidth)
        {
            OffsetWidth = offsetWidth;
            DataWidth = dataWidth;
            CalculatePar();
        }

        /// <summary>
        /// Constructor, it calls <see cref="SetPar"/> to set the parameters.
        /// </summary>
        /// <param name="offsetWidth">input</param>
        /// <param name="dataWidth">input</param>
		public BinaryView(int offsetWidth, int dataWidth)
        {
            SetPar(offsetWidth, dataWidth);
        }

        /// <summary>
        /// Get offsetWidth.
        /// </summary>
        public int OffsetWidth { get; private set; } = 6;

        /// <summary>
        /// Get dataWidth.
        /// </summary>
        public int DataWidth { get; private set; } = 16;

        /// <summary>
        /// Get totalWidth.
        /// </summary>
        public int TotalWidth { get; private set; }

        /// <summary>
        /// Get hexWidth.
        /// </summary>
        public int HexWidth { get; private set; }

        /// <summary>
        /// Calculate hex view parameters.
        /// </summary>
        protected void CalculatePar()
        {
            TotalWidth = OffsetWidth + 2 + DataWidth * 3 + ((DataWidth / 8) - 1) + 1 + DataWidth;
            HexWidth = TotalWidth - DataWidth;
        }

        /// <summary>
        /// Generate hex view text string by calling <see cref="GetBinaryViewText"/>.
        /// </summary>
        /// <param name="data">source byte array.</param>
        /// <returns>output string.</returns>
        public string GenerateText(byte[] data)
        {
            return GetBinaryViewText(data, OffsetWidth, DataWidth);
        }

        /// <summary>
        /// Calculate the byte <see cref="ByteLocation"/> by offset.
        /// </summary>
        /// <param name="byteOffset"></param>
        /// <param name="loc"></param>
        public void GetLocation(int byteOffset, ByteLocation loc)
        {
            int colOff = byteOffset - byteOffset / DataWidth * DataWidth;
            int line = byteOffset / DataWidth;
            int col = OffsetWidth + 2 + colOff * 3;
            int colLen = 3;
            int totOff = line * TotalWidth + line + col;
            int col2 = HexWidth + colOff;
            int totOff2 = line * TotalWidth + line + col2;
            int colLen2 = 1;
            loc.hexOffset = totOff;
            loc.hexColLen = colLen;
            loc.line = line;
            loc.chOffset = totOff2;
            loc.chColLen = colLen2;
        }

        /// <summary>
        /// Generate "Detail" hex view text.
        /// </summary>
        /// <param name="data">source byte array.</param>
        /// <param name="offsetWidth">offset text width.</param>
        /// <param name="dataWidth">data text width</param>
        /// <returns>detail hex view string.</returns>
        public static string GetBinaryViewText(byte[] data, int offsetWidth, int dataWidth)
        {
            string retval = "";
            string offForm = "{0:X" + offsetWidth + "}  ";
            int i, lineStart, lineEnd;
            int line = 0, offset = 0;
            int totalWidth = offsetWidth + 2 + dataWidth * 3 + ((dataWidth / 8) - 1) + 1 + dataWidth;
            int hexWidth = totalWidth - dataWidth;
            string dumpStr = "";
            string lineStr = "";
            for (offset = 0; offset < data.Length;)
            {
                lineStr = string.Format(offForm, (line++) * dataWidth);
                lineStart = offset;
                for (i = 0; i < dataWidth; i++)
                {
                    lineStr += string.Format("{0:X2} ", data[offset++]);
                    if (offset >= data.Length) break;
                    if ((i + 1) % 8 == 0 && i != 0 && (i + 1) < dataWidth) lineStr += " ";
                }
                lineStr += " ";
                lineEnd = offset;
                lineStr = lineStr.PadRight(hexWidth, ' ');
                for (i = lineStart; i < lineEnd; i++)
                {
                    if (data[i] < 32 || data[i] > 128)
                        lineStr += '.';
                    else
                        lineStr += (char)data[i];
                }
                lineStr = lineStr.PadRight(totalWidth, ' ');
                dumpStr += lineStr + "\r\n";
            }
            retval = dumpStr;
            return retval;
        }

    }

    /// <summary>
    /// ByteLocation class is used by <see cref="BinaryView"/> to transfer 
    /// location parameters.
    /// </summary>
    public class ByteLocation
    {
        /// <summary>
        /// line number.
        /// </summary>
        public int line = 0;

        /// <summary>
        /// Hex encoded data length.
        /// </summary>
        public int hexColLen = 3;

        /// <summary>
        /// Hex encoded data offset.
        /// </summary>
        public int hexOffset = 0;

        /// <summary>
        /// Character length.
        /// </summary>
        public int chColLen = 1;

        /// <summary>
        /// Character offset.
        /// </summary>
        public int chOffset = 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ByteLocation()
        {
        }
    }

    /// <summary>
    /// Summary description for OID.
    /// This class is used to encode and decode OID strings.
    /// </summary>
    public class Oid
    {
        /// <summary>
        /// Encode OID string to byte array.
        /// </summary>
        /// <param name="oidStr">source string.</param>
        /// <returns>encoded array.</returns>
        public byte[] Encode(string oidStr)
        {
            MemoryStream ms = new MemoryStream();
            Encode(ms, oidStr);
            ms.Position = 0;
            byte[] retval = new byte[ms.Length];
            ms.Read(retval, 0, retval.Length);
            ms.Close();
            return retval;
        }

        /// <summary>
        /// Decode OID byte array to OID string.
        /// </summary>
        /// <param name="data">source byte array.</param>
        /// <returns>result OID string.</returns>
        public string Decode(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data)
            {
                Position = 0
            };
            string retval = Decode(ms);
            ms.Close();
            return retval;
        }

        /// <summary>
        /// Encode OID string and put result into <see cref="Stream"/>
        /// </summary>
        /// <param name="bt">output stream.</param>
        /// <param name="oidStr">source OID string.</param>
        public virtual void Encode(Stream bt, string oidStr) //TODO
        {
            string[] oidList = oidStr.Split('.');
            if (oidList.Length < 2) throw new Exception("Invalid OID string.");
            ulong[] values = new ulong[oidList.Length];
            for (int i = 0; i < oidList.Length; i++)
            {
                values[i] = Convert.ToUInt64(oidList[i]);
            }
            bt.WriteByte((byte)(values[0] * 40 + values[1]));
            for (int i = 2; i < values.Length; i++)
                EncodeValue(bt, values[i]);
        }

        /// <summary>
        /// Decode OID <see cref="Stream"/> and return OID string.
        /// </summary>
        /// <param name="bt">source stream.</param>
        /// <returns>result OID string.</returns>
        public virtual string Decode(Stream bt)
        {
            string retval = "";
            byte b;
            ulong v = 0;
            b = (byte)bt.ReadByte();
            retval += Convert.ToString(b / 40);
            retval += "." + Convert.ToString(b % 40);
            while (bt.Position < bt.Length)
            {
                try
                {
                    DecodeValue(bt, ref v);
                    retval += "." + v.ToString();
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to decode OID value: " + e.Message);
                }
            }
            return retval;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Oid()
        {
        }

        /// <summary>
        /// Encode single OID value.
        /// </summary>
        /// <param name="bt">output stream.</param>
        /// <param name="v">source value.</param>
        protected void EncodeValue(Stream bt, ulong v)
        {
            for (int i = (Asn1Util.BitPrecision(v) - 1) / 7; i > 0; i--)
            {
                bt.WriteByte((byte)(0x80 | ((v >> (i * 7)) & 0x7f)));
            }
            bt.WriteByte((byte)(v & 0x7f));
        }

        /// <summary>
        /// Decode single OID value.
        /// </summary>
        /// <param name="bt">source stream.</param>
        /// <param name="v">output value</param>
        /// <returns>OID value bytes.</returns>
        protected int DecodeValue(Stream bt, ref ulong v)
        {
            byte b;
            int i = 0;
            v = 0;
            while (true)
            {
                b = (byte)bt.ReadByte();
                i++;
                v <<= 7;
                v += (ulong)(b & 0x7f);
                if ((b & 0x80) == 0)
                    return i;
            }
        }
    }

    /// <summary>
    /// Summary description for RelativeOid.
    /// </summary>
    public class RelativeOid : Oid
    {
        /// <summary>
        /// Constructor.
        /// </summary>
		public RelativeOid()
        {
        }

        /// <summary>
        /// Encode relative OID string and put result into <see cref="Stream"/>
        /// </summary>
        /// <param name="bt">output stream.</param>
        /// <param name="oidStr">source OID string.</param>
        public override void Encode(Stream bt, string oidStr)
        {
            string[] oidList = oidStr.Split('.');
            ulong[] values = new ulong[oidList.Length];
            for (int i = 0; i < oidList.Length; i++)
            {
                values[i] = Convert.ToUInt64(oidList[i]);
            }
            for (int i = 0; i < values.Length; i++)
                EncodeValue(bt, values[i]);
        }

        /// <summary>
        /// Decode relative OID <see cref="Stream"/> and return OID string.
        /// </summary>
        /// <param name="bt">source stream.</param>
        /// <returns>result OID string.</returns>
        public override string Decode(Stream bt)
        {
            string retval = "";
            ulong v = 0;
            bool isFirst = true;
            while (bt.Position < bt.Length)
            {
                try
                {
                    DecodeValue(bt, ref v);
                    if (isFirst)
                    {
                        retval = v.ToString();
                        isFirst = false;
                    }
                    else
                    {
                        retval += "." + v.ToString();
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to decode OID value: " + e.Message);
                }
            }
            return retval;
        }
    }
}
