using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using System.IO.Compression;
using UnityEditor.Experimental.AssetImporters;
[ScriptedImporter(1,"map")]
public class MAPImporter : ScriptedImporter
{
    public HaloMap mapFile;
    public string MapName;
    public HaloMap.GameEngine engineType;
    public string subtype;
    public HaloMap.MapType mapType;
    public long mapByteSize;
    
    public long mapLength;
    public long TagBlock;
    public long TagSize;
    public string tagIntegrity;
    public List<string> data;
    public override void OnImportAsset(AssetImportContext ctx){
        GameObject mapObject = new GameObject();
        mapFile = new HaloMap(ctx.assetPath);
        MapName=mapFile.header.mapName;
        engineType=mapFile.header.gameEngine;
        subtype=mapFile.header.subversion;
        mapByteSize=mapFile.detectedBytes;
        mapLength=mapFile.header.mapFileSize;
        mapType=mapFile.header.mapType;
        data = new List<string>();
        TagBlock=mapFile.header.tagDataOffset;
        TagSize=mapFile.header.tagDataSize;
        tagIntegrity=mapFile.tagBlock.tagsIntegrity;
        data.Add(mapFile.header.mapBuild);
        ctx.AddObjectToAsset(mapObject.name,mapObject);
        ctx.SetMainObject(mapObject);
    }

    
}
