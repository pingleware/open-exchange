//////////////////////////////////////////////////////////////////////////////
//
// Attribution-NonCommercial-ShareAlike 3.0 Unported (CC BY-NC-SA 3.0)
// Disclaimer
// 
// This file is part of OpEx.
// 
// You are free:
// 	* to Share : to copy, distribute and transmit the work;
//	* to Remix : to adapt the work.
//		
// Under the following conditions:
// 	* Attribution : You must attribute OpEx to Marco De Luca
//     (marco.de.luca@algotradingconsulting.com);
// 	* Noncommercial : You may not use this work for commercial purposes;
// 	* Share Alike : If you alter, transform, or build upon this work,
//     you may distribute the resulting work only under the same or similar
//     license to this one. 
// 	
// With the understanding that: 
// 	* Waiver : Any of the above conditions can be waived if you get permission
//     from the copyright holder. 
// 	* Public Domain : Where the work or any of its elements is in the public
//     domain under applicable law, that status is in no way affected by the
//     license. 
// 	* Other Rights : In no way are any of the following rights affected by
//     the license: 
//		 . Your fair dealing or fair use rights, or other applicable copyright
//          exceptions and limitations; 
//		 . The author's moral rights; 
//		 . Rights other persons may have either in the work itself or in how
//          the work is used, such as publicity or privacy rights. 
//	* Notice : For any reuse or distribution, you must make clear to others
//     the license terms of this work. The best way to do this is with a link
//     to this web page. 
// 
// You should have received a copy of the Legal Code of the CC BY-NC-SA 3.0
// Licence along with OpEx.
// If not, see <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>.
//
//////////////////////////////////////////////////////////////////////////////


﻿//using System;
//using System.IO;
//using System.Runtime.InteropServices;

//namespace OPEX.MDS.Common
//{
//    public delegate void MessageParsedHandler(MessageHeader header);

//    public class MessageParser
//    {
//        private bool _newMsg = true;
//        private int _remainingByte;
//        private MemoryStream _memStream = new MemoryStream();
//        private int _msgLength;

//        public event MessageParsedHandler MessageParsed;

//        private void OnMessageParsed(MessageHeader msgHeader)
//        {
//            MessageParsedHandler handler = MessageParsed;
//            if (handler != null)
//            {
//                handler(msgHeader);
//            }
//        }

//        public byte[] Serialize(MessageHeader obj)
//        {
//            int objectSize = Marshal.SizeOf(obj);
//            obj.MessageLength = objectSize;

//            IntPtr memBuffer = Marshal.AllocHGlobal(objectSize);
//            Marshal.StructureToPtr(obj, memBuffer, false);

//            byte[] byteArray = new byte[objectSize];
//            Marshal.Copy(memBuffer, byteArray, 0, objectSize);

//            Marshal.FreeHGlobal(memBuffer);

//            return byteArray;
//        }

//        private void ConvertToObject(byte[] msgBytes)
//        {
//            int messageType = BitConverter.ToInt32(msgBytes, 4);
//            Type objectType = null;

//            if (messageType == (int)MessageHeaderType.MarketData)
//            {
//                objectType = typeof(MarketDataMessage);
//            }
//            else
//            {
//                throw new ArgumentException("Invalid message type");
//            }

//            int objectSize = Marshal.SizeOf(objectType);
//            IntPtr memBuffer = Marshal.AllocHGlobal(objectSize);
//            MessageHeader hdr = null;

//            try
//            {
//                Marshal.Copy(msgBytes, 0, memBuffer, objectSize);
//                hdr = Marshal.PtrToStructure(memBuffer, objectType) as MessageHeader;
//            }
//            finally
//            {
//                if (memBuffer != null)
//                {
//                    Marshal.FreeHGlobal(memBuffer);
//                }
//            }

//            OnMessageParsed(hdr);
//        }

//        /// <summary>
//        /// Asynchronously deserialises the message.
//        /// Will call back on MessageParsed.
//        /// </summary>
//        /// <param name="msgBytes">The message to deserialise.</param>
//        public void DeSerialize(byte[] msgBytes)
//        {
//            AlignMessageBoundary(msgBytes, 0);
//        }

//        private void AlignMessageBoundary(byte[] recvByte, int offset)
//        {
//            if (offset >= recvByte.Length)
//            {
//                return;
//            }

//            if (_newMsg == true)
//            {
//                _msgLength = BitConverter.ToInt32(recvByte, offset);
//                int msgType = BitConverter.ToInt32(recvByte, offset + 4);

//                if (_msgLength > (recvByte.Length - offset) + 1)
//                {
//                    _newMsg = false;
//                    _remainingByte = _msgLength - recvByte.Length;
//                    _memStream = new MemoryStream();
//                    _memStream.Write(recvByte, offset, recvByte.Length);
//                }
//                else
//                { 
//                    byte[] bytes = new byte[_msgLength];

//                    Array.Copy(recvByte, offset, bytes, 0, _msgLength);
                    
//                    ConvertToObject(bytes);
//                    AlignMessageBoundary(recvByte, offset + _msgLength);
//                }
//            }
//            else
//            {
//                if ( _remainingByte > recvByte.Length)
//                {
//                    _memStream.Write(recvByte, 0, recvByte.Length);
//                    _remainingByte -= recvByte.Length;
//                }
//                else
//                {
//                    _memStream.Write(recvByte, offset, _remainingByte);
//                    byte[] bytes = new byte[_msgLength];
//                    _memStream.Seek(0, SeekOrigin.Begin);
//                    _memStream.Read(bytes, 0, _msgLength);
//                    _memStream.Close();
                    
//                    ConvertToObject(bytes);
//                    _newMsg = true;
//                    AlignMessageBoundary(recvByte, offset+_remainingByte+1);
//                }
//            }
//        }
//    }
//}
