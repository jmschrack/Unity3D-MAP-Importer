using System.IO;
using System;
public class BigEndianReader : BinaryReader{
    public BigEndianReader(Stream stream):base(stream){}
    public override byte[] ReadBytes(int count){
        
        byte[] ret=base.ReadBytes(count);
        Array.Reverse(ret);
        return ret;
    }
    public override uint ReadUInt32(){
        return base.ReadUInt32().ReverseBytes();
    }
    public override ushort ReadUInt16() {
        return base.ReadUInt16().ReverseBytes();
    }
   /*  public string ReadISO8859String(int count){
        return System.Text.Encoding.UTF8.GetString(ReadBytes(count));
    } */
}