using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace TestServers {


    public class ProtoMessage {
        private enum MsgState {
            Idle,
            Varint,
            Int32,  // not impl 
            Int64,  // not impl
            Len
        }

        private MsgState state;
        private UInt64 bitCollector;
        private StringBuilder lenContent = new StringBuilder();
        private int currentFieldNr = 0;
        private int index = 0; 

        private Dictionary<int, Object> fields = new Dictionary<int, Object>();

        internal bool IsIdle() {
            return state == MsgState.Idle;
        }

        public void ProcessByte(byte v) {
            if (IsIdle()) {
                byte type = (byte)(v & 0x07);
                int i = (int)(v >> 3);
                SetCurrentFieldNr(i);
                switch (type) {
                    case 0:
                        SetVarint();
                        break;
                    case 1:
                        SetI64();
                        break;
                    case 2:
                        SetLen();
                        break;
                    case 5:
                        SetI32();
                        break;
                }
            } else {
                NextByte(v);
            }

        }

        private void NextByte(byte v) {
            if (state == MsgState.Varint) {
                UInt64 val7 = (ulong)(v & 0x7F);
                bitCollector &= (val7 << (index * 7));
                if ((v & 0x80) == 0) {
                    // last byte -> store Value
                    AddNumField(currentFieldNr++, bitCollector);
                    state = MsgState.Idle;
                } else {
                    index++;
                }
            } else if (state == MsgState.Len) {
                if (index == -1) {
                    index = v;
                } else {
                    lenContent.Append((char)v);
                    index--;
                    if (index == 0) {
                        AddStrField(currentFieldNr++, lenContent.ToString());
                        state = MsgState.Idle;
                    }
                }
            }
            
        }

        private void AddStrField(int v, string str) {
            fields.Add(v, str);
        }

        private void AddNumField(int v, ulong bitCollector) {
            fields.Add(v, bitCollector);
        }

        internal void SetCurrentFieldNr(int i) {
            currentFieldNr = i;
        }

        internal void SetI32() {
            state = MsgState.Int32;
            index = 0;
        }

        internal void SetI64() {
            state = MsgState.Int64;
            index = 0;
        }

        internal void SetLen() {
            state = MsgState.Len;
            index = -1;
            lenContent = new StringBuilder();
        }

        internal void SetVarint() {
            state = MsgState.Varint;
            bitCollector = 0;
            index = 0;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            foreach (var field in fields) {
                sb.Append("/" + field.Key + " - " + field.Value.ToString()+Environment.NewLine);
            }
            return sb.ToString();
        }

        public object? GetField(int v) {
            if (fields.ContainsKey(v)) {
                return fields[v];
            }
            return null;
        }

        internal void SetField(int v, string value) {
            fields[v] = value;
        }

        internal byte[] GetAsBytes() {
            List<byte> output = new();
            foreach (var field in fields) {
                if (field.Value is UInt64 nf){
                    output.Add((byte)(field.Key << 3));
                    byte[] val = MakeVarInt(nf);
                    output.AddRange(val);   
                };

                if (field.Value is string sf) {
                    output.Add((byte)((field.Key << 3) | 0x02));
                    output.AddRange(MakeVarInt((ulong)sf.Length));
                    output.AddRange(Encoding.UTF8.GetBytes(sf));
                }
            }



            return output.ToArray();
        }

        private byte[] MakeVarInt(ulong nf) {
            List<byte> retVal = new();

            while (nf > 128) {
                retVal.Add((byte)((nf & 0x7f) | 0x80));
                nf >>= 7;
            }
            retVal.Add((byte)(nf & 0x7f));
            return retVal.ToArray();
        }
    }
}
