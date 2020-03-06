using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
[System.Serializable]
public class HaloMap {
    public enum GameEngine{
        UNDEFINED=0,
        Retail=0x7,
        CustomEdition=0x261,
        Xbox=0x5,
        Halo2=0x8,//both Xbox and Vista
        Halo3=0xB,//also ODST
        Halo4=0xC//also Reach
    }
    public enum MapType{
        UNDEFINED,
        Campaign=0x0,
        Multiplayer=0x1,
        UI=0x2,
        Shared=0x3,
    }
    const int tagOffset=0x1d8;
    public long detectedBytes;
    public long scanForTagsFoundAt;
    public bool isBigEndian=false;
    public Header header;
    public TagBlock tagBlock;
    public List<Tag> tags;
    public string nextString;
    public HaloMap(string path){
        BinaryReader reader;
        reader = new BinaryReader(File.Open(path,FileMode.Open));

        string integrity = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(4));
        reader.BaseStream.Seek(0,SeekOrigin.End);
        detectedBytes=reader.BaseStream.Position;
        isBigEndian=integrity.Equals("head");
        if(isBigEndian){
            //switch to BigEndian
            reader.Close();
            reader = new BigEndianReader(File.Open(path,FileMode.Open));
        }else if(!integrity.Equals("daeh")){
            Debug.LogError("Header Integrity failed! Found:"+integrity);
            return;
        }
        reader.BaseStream.Seek(0,SeekOrigin.Begin);
        header=new Header(reader);
        reader.BaseStream.Seek(0,SeekOrigin.Begin);
        reader.ScanUntilFound(1952540531);
        scanForTagsFoundAt=reader.BaseStream.Position-36;
        if(header.tagDataOffset>detectedBytes){
            Debug.LogWarning("EOF for tagOffset. Using scanned offset");
            reader.BaseStream.Seek(scanForTagsFoundAt,SeekOrigin.Begin);
            //Debug.LogFormat("Found sgat at:"+reader.BaseStream.Position+"(0x"+(reader.BaseStream.Position-header.tagDataOffset).ToString("X")+" offset)");
        }else{
            reader.BaseStream.Seek(header.tagDataOffset,SeekOrigin.Begin);
        }
        long tagArrayLocation=reader.BaseStream.Position;
        tagBlock= new TagBlock(reader,header.gameEngine);
        
        tagArrayLocation+=((UInt16)tagBlock.TagArrayPointer);
        Debug.Log("Parsed TagBlock. "+tagArrayLocation.ToString("X"));
        reader.BaseStream.Seek(tagArrayLocation,SeekOrigin.Begin);
        Debug.Log("Beginning parse");
        tags=new List<Tag>();
        try{
        for(int i=0;i<tagBlock.TagCount;i++){
            tags.Add(new Tag(reader,(0x10000)+header.tagDataOffset));
        }
        }catch(EndOfStreamException e){
            Debug.LogError("Couldn't parse tags. Invalid offset. Purging tags for safety.");
            tags.Clear();
        }
        //Debug.Log("NextString:0x"+reader.BaseStream.Position.ToString("X"));
        //Debug.Log("Offset from Tagblock:0x"+(reader.BaseStream.Position-header.tagDataOffset).ToString("X"));
        //nextString=reader.ReadCString();
        //reader.BaseStream.Seek(header.tagDataOffset,SeekOrigin.Begin);
        //tagBlock.tagsIntegrity=reader.ReadUTF8String(4);
        //
        
        
        reader.Close();
        reader=null;
    }
    [System.Serializable]
    public class Header{
        
        public string headIntegrity;
        public bool headIntegrityIsValid;

        public GameEngine gameEngine;
        void SetGameEngine(uint i){
            switch(i){
                case 0x5:
                gameEngine=GameEngine.Xbox;
                break;
                case 0x7:
                gameEngine=GameEngine.Retail;
                break;
                case 0x8:
                gameEngine=GameEngine.Halo2;
                break;
                case 0x261:
                gameEngine=GameEngine.CustomEdition;
                break;
                case 0xB:
                gameEngine=GameEngine.Halo3;
                break;
                case 0xC:
                gameEngine=GameEngine.Halo4;
                break;
                default:
                Debug.LogWarning("Unknown game engine:0x"+i.ToString("X"));
                break;
            }
        }
        bool isHalo1{get{return gameEngine==GameEngine.CustomEdition||gameEngine==GameEngine.Xbox||gameEngine==GameEngine.Retail;}}
        public string subversion{get{
            switch(gameEngine){
                case GameEngine.Halo2:
                    if(mapBuild.Contains("main"))
                        return "Vista";
                break;
                case GameEngine.Halo3:
                    if(mapBuild.Contains("atlas_relea"))
                        return "ODST";
                break;
                case GameEngine.Halo4:
                    if(mapBuild.Contains("omaha_relea"))
                        return "Reach";
                break;
                default:
                    return "";
            }
            return "";
        }}
        public uint spacerData1;
        public uint mapFileSize;
        public uint tagDataOffset;
        public uint tagDataSize;
        public string mapName;
        public string mapBuild;
        public MapType mapType;
        public uint crc32;
        public string footIntegrity;
        public bool footIntegrityIsValid;
        
        public Header(BinaryReader reader){
            //headIntegrity=reader.ReadUTF8String(4);
            headIntegrity=System.Text.Encoding.UTF8.GetString(reader.ReadBytes(4));
            headIntegrityIsValid=headIntegrity.Equals("daeh");
            gameEngine=(GameEngine)reader.ReadUInt32();
            mapFileSize=reader.ReadUInt32();
            spacerData1=reader.ReadUInt32();
            tagDataOffset=reader.ReadUInt32();
            if(gameEngine==GameEngine.Halo2)
                tagDataOffset-=0x8;
            tagDataSize=reader.ReadUInt32();
            if(isHalo1)
                readHalo1(reader);
            else{
                readHalo3(reader);
            }
            footIntegrity=System.Text.Encoding.UTF8.GetString(reader.ReadBytes(4));
            footIntegrityIsValid=footIntegrity.Equals("toof");
        }
        private void readHalo1(BinaryReader reader){
            reader.ReadChars(8);
            mapName=reader.ReadUTF8String(32);
            mapBuild=reader.ReadUTF8String(32);
            mapType=(MapType)reader.ReadUInt16();
            reader.ReadChars(2);//more spacer
            crc32=reader.ReadUInt32();
            reader.ReadChars(1940);//I feel like there's a joke here
            //reader.ReadUntilData();
            
        }

        private void readHalo3(BinaryReader reader){
           
            reader.BaseStream.Position=0x11C;
            if(gameEngine==GameEngine.Halo2)
                reader.ReadUntilData();
            mapBuild=reader.ReadUTF8String(32);
            mapType=(MapType)reader.ReadUInt16();
            reader.ReadBytes(2);
            crc32=reader.ReadUInt32();
            reader.BaseStream.Position=0x18C;
            if(gameEngine==GameEngine.Halo2){
                reader.BaseStream.Position=0x198;
            }
            mapName=reader.ReadCString();
            if(gameEngine==GameEngine.Halo2){
                reader.BaseStream.Position=0x7FC;
            }else
                reader.BaseStream.Position=0x2FFC;
            
        }
    }
    [System.Serializable]
    public class TagBlock{
        public uint TagArrayPointer;
        public uint ScenarioTagID;
        public uint TagCount;//Max:65535
        public uint ModelpartCount;
        public uint ModelDataFileOffset;
        public uint ModelpartCount2;
        public uint VertexSize;
        public uint ModelDataSize;
        public string tagsIntegrity;
        public bool tagsIntegrityIsValid;
        
        public TagBlock(){}
        public TagBlock(BinaryReader reader,GameEngine gameType){
            TagArrayPointer=reader.ReadUInt32();
            ScenarioTagID=reader.ReadUInt32();
            reader.ReadUInt32();
            TagCount=reader.ReadUInt32();
            ModelpartCount=reader.ReadUInt32();
            ModelDataFileOffset=reader.ReadUInt32();
            ModelpartCount2=reader.ReadUInt32();
            VertexSize=reader.ReadUInt32();
            ModelDataSize=reader.ReadUInt32();
            tagsIntegrity=reader.ReadUTF8String(4);
            tagsIntegrityIsValid=tagsIntegrity.Equals("sgat");
        }
        
    }


    [System.Serializable]
    public class Tag{
        public string tagClass;
        public uint secondTagClass;
        public uint tertiaryTagClass;
        [SerializeField]
        private uint _tagID;
        public UInt16 tagID{
            get{
                return (UInt16)_tagID;
            }
        }

        public uint tagPath;
        public uint tagPathOffset{
            get{return tagPath-memoryOffset;}
        }
        public string tagPathText;
        public uint tagData;
        public uint CEtagData;
        const uint memoryOffset=0x40450000;

        public Tag(BinaryReader reader,long offset){
            
            tagClass=reader.ReadReverseUTF8String(4);
            secondTagClass=reader.ReadUInt32();
            tertiaryTagClass=reader.ReadUInt32();
            _tagID=reader.ReadUInt32();
            
            tagPath=reader.ReadUInt32();
            tagData=reader.ReadUInt32();
            CEtagData=reader.ReadUInt32();
            reader.ReadBytes(4);
            long resumePos=reader.BaseStream.Position;
            try{
                reader.BaseStream.Seek(tagPathOffset+offset,SeekOrigin.Begin);
                tagPathText=reader.ReadCString();
            }catch(EndOfStreamException e){
                Debug.LogWarning("Couldn't seek to path");
            }
            reader.BaseStream.Seek(resumePos,SeekOrigin.Begin);
        }
        public bool isTagIDNull{
            get{
                return _tagID==0xFFFFFFFF;
            }
        }
        public string TagIDHex{
            get{
                return _tagID.ToString("X");
            }
        }
        public bool hasSecondaryTagClass{
            get{
                return secondTagClass!=4294967295;
            }
        }
        public bool hasTertiaryTagClass{
            get{
                return tertiaryTagClass!=0xFFFFFFFF;
            }
        }
        public string secondTagClassString{
            get{
                return System.Text.Encoding.UTF8.GetString(BitConverter.GetBytes(secondTagClass.ReverseBytes()));
            }
        }
        public string tertiaryTagClassString{
            get{
                return System.Text.Encoding.UTF8.GetString(BitConverter.GetBytes(secondTagClass.ReverseBytes()));
            }
        }
    }
}