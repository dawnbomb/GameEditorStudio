using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEditorStudio
{
    internal class ByteManager
    {

        //The following is written by violentlycar to deal with writing/saving mult-byte hex to files. 
        //it is not used here, but in other files, pretty much every editor uses this.
        //it is used via the following command
        //ByteWriter(value, destination_array, offset)
        //a semi-pratical example is ByteWriter(value, enemydata_array, 0x38 + (treeView1.SelectedNode.Index * 0x38) + 4)

        //I have permission to use this. There is nothing else here other then this converter.
        //Only supports: byte, sbyte, ushort, short, uint, int, string
        //Does not support: Double, long, float, etc.



        static public void ByteWriter<T>(T val, byte[] d_array, int start_offset)
        {
            if (val is byte || val is sbyte) // avoid using this one
            {
                d_array[start_offset] = Convert.ToByte(val);
            }
            else if (val is short)
            {
                short c_val = Convert.ToInt16(val);
                d_array[start_offset] = (byte)(c_val & 0xFF);
                d_array[start_offset + 1] = (byte)((c_val & 0xFF00) >> 8);
            }
            else if (val is ushort)
            {
                ushort c_val = Convert.ToUInt16(val);
                d_array[start_offset] = (byte)(c_val & 0xFF);
                d_array[start_offset + 1] = (byte)((c_val & 0xFF00) >> 8);
            }
            else if (val is int)
            {
                int c_val = Convert.ToInt32(val);
                d_array[start_offset] = (byte)(c_val & 0xFF);
                d_array[start_offset + 1] = (byte)((c_val & 0xFF00) >> 8);
                d_array[start_offset + 2] = (byte)((c_val & 0xFF0000) >> 16);
                d_array[start_offset + 3] = (byte)((c_val & 0xFF000000) >> 24);
            }
            else if (val is uint)
            {
                uint c_val = Convert.ToUInt32(val);
                d_array[start_offset] = (byte)(c_val & 0xFF);
                d_array[start_offset + 1] = (byte)((c_val & 0xFF00) >> 8);
                d_array[start_offset + 2] = (byte)((c_val & 0xFF0000) >> 16);
                d_array[start_offset + 3] = (byte)((c_val & 0xFF000000) >> 24);
            }
            else if (val is string)
            {
                byte[] str_bytes = Encoding.ASCII.GetBytes(Convert.ToString(val));
                for (int x = 0; x < str_bytes.Length; x++)
                {
                    d_array[start_offset + x] = str_bytes[x];
                }
            }
            else
            {
                throw new Exception("Value passed was not one of supported type: byte, sbyte, ushort, short, uint, int, string.");
            }
        }
    }
}
