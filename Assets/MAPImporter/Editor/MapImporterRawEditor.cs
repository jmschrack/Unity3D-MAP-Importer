using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using ExposedInspector;
public class MapImporterRawEditor : BaseAssetImporterTabUI{
    bool showTagBlock,showTags;
    bool showFileData,showHeaderData=true;
    List<bool> showIndividualTag;
    Vector2 scrollTags;
    public MapImporterRawEditor(ScriptedImporterEditor panelContainer):base(panelContainer){}
    static GUIStyle good,bad;
    public override void OnEnable(){
        good = new GUIStyle();
        bad = new GUIStyle();
        good.normal.textColor=new Color(0,0.5f,0,1);
        bad.normal.textColor=Color.red;
    }
    GUIStyle ValidStyle(bool isValid){
        return isValid?good:bad;
    }
    HaloMap mapFile{
        get{
            return (target as MAPImporter).mapFile;
        }
    }
    public override void OnInspectorGUI(){
        
        EditorGUILayout.LabelField("Header",EditorStyles.boldLabel);
        showHeaderData=EditorGUILayout.Foldout(showHeaderData,mapFile.header.mapName);
        if(showHeaderData){
            using(new EditorGUI.IndentLevelScope()){
                EditorGUILayout.LabelField("HeaderBegin Integrity:"+mapFile.header.headIntegrity+" 0x"+mapFile.header.headIntegrity.ToByteString(),(mapFile.header.headIntegrityIsValid?good:bad));
                EditorGUILayout.LabelField("GameEngine:"+mapFile.header.gameEngine.ToString()+mapFile.header.subversion);
                EditorGUILayout.LabelField(string.Format("MapType:{0}",mapFile.header.mapType.ToString()));
                EditorGUILayout.LabelField("Header Listed Size:"+mapFile.header.mapFileSize);
                EditorGUILayout.LabelField(string.Format("TagOffset:0x{0} (Listed) vs 0x{1} (Scanned) Diff:0x{2}",mapFile.header.tagDataOffset.ToString("X"),mapFile.scanForTagsFoundAt.ToString("X"),(mapFile.header.tagDataOffset-mapFile.scanForTagsFoundAt).ToString("X")));
                EditorGUILayout.LabelField(string.Format("TagDataSize:{0:n0} Bytes", mapFile.header.tagDataSize));
                EditorGUILayout.LabelField("HeaderEnd Integrity:"+mapFile.header.footIntegrity+" 0x"+mapFile.header.footIntegrity.ToByteString(),ValidStyle(mapFile.header.footIntegrityIsValid));
            }
        }
        showFileData=EditorGUILayout.Foldout(showFileData,"File Data");
        if(showFileData){
            using (new EditorGUI.IndentLevelScope()){
                EditorGUILayout.LabelField(mapFile.isBigEndian?"BigEndian":"LittleEndian");
                EditorGUILayout.LabelField(string.Format("Detected Size:{0:n0} (0x{1:X})",mapFile.detectedBytes,mapFile.detectedBytes));
                EditorGUILayout.LabelField(string.Format("Header Listed Size:{0:n0}",mapFile.header.mapFileSize));
            }
            
        }
        showTagBlock=EditorGUILayout.Foldout(showTagBlock,"Tag Index");
        if(showTagBlock){
            using(new EditorGUI.IndentLevelScope()){
                EditorGUILayout.LabelField(string.Format("TagArrayPointer:{0} 0x{1:X}",mapFile.tagBlock.TagArrayPointer,mapFile.tagBlock.TagArrayPointer));
                EditorGUILayout.LabelField("Mini Tag Array index:"+((System.UInt16)mapFile.tagBlock.TagArrayPointer).ToString());
                EditorGUILayout.LabelField(string.Format("ScenarioTagID:{0}",mapFile.tagBlock.ScenarioTagID));
                EditorGUILayout.LabelField(string.Format("Tag Count:{0}",mapFile.tagBlock.TagCount));
                EditorGUILayout.LabelField(string.Format("ModelPart Count:{0}",mapFile.tagBlock.ModelpartCount));
                EditorGUILayout.LabelField(string.Format("ModelData File Offset:{0} 0x{1:X}",mapFile.tagBlock.ModelDataFileOffset,mapFile.tagBlock.ModelDataFileOffset));
                EditorGUILayout.LabelField(string.Format("ModelPart Count?:{0}",mapFile.tagBlock.ModelpartCount2));
                EditorGUILayout.LabelField(string.Format("Vertex Size:{0}",mapFile.tagBlock.VertexSize));
                EditorGUILayout.LabelField(string.Format("Model Data Size:{0}",mapFile.tagBlock.ModelDataSize));
                EditorGUILayout.LabelField(string.Format("Tags integrity:{0} 0x{1}",mapFile.tagBlock.tagsIntegrity,mapFile.tagBlock.tagsIntegrity.ToByteString()),ValidStyle(mapFile.tagBlock.tagsIntegrityIsValid));
            }
        }
        if(showIndividualTag==null||showIndividualTag.Count!=mapFile.tags.Count){
            showIndividualTag=new List<bool>();
            while(showIndividualTag.Count!=mapFile.tags.Count){
                showIndividualTag.Add(false);
            }
        }
        showTags=EditorGUILayout.Foldout(showTags,"Tags");
        if(showTags){
            scrollTags=EditorGUILayout.BeginScrollView(scrollTags);
            for(int i=0;i<mapFile.tags.Count;i++){
                showIndividualTag[i]=EditorGUILayout.Foldout(showIndividualTag[i],mapFile.tags[i].tagPathText);
                if(showIndividualTag[i]){
                    using(new EditorGUI.IndentLevelScope()){
                        EditorGUILayout.LabelField("TagClass:"+mapFile.tags[i].tagClass);
                        if(mapFile.tags[i].hasSecondaryTagClass){
                            EditorGUILayout.LabelField("2ndTagClass:"+mapFile.tags[i].secondTagClassString);
                        }
                        if(mapFile.tags[i].hasTertiaryTagClass){
                            EditorGUILayout.LabelField("3rdTagClass:"+mapFile.tags[i].tertiaryTagClassString);
                        }
                        EditorGUILayout.LabelField("TagID:"+mapFile.tags[i].tagID.ToString()+" 0x"+mapFile.tags[i].TagIDHex);
                        EditorGUILayout.LabelField(string.Format("TagPath:0x{0:X} as offset: 0x{1:X} found at 0x{2:X}",mapFile.tags[i].tagPath,mapFile.tags[i].tagPathOffset,mapFile.tags[i].tagPathOffset+mapFile.header.tagDataOffset));
                        EditorGUILayout.LabelField("TagPath Result:"+mapFile.tags[i].tagPathText);
                        EditorGUILayout.LabelField("TagData:0x"+mapFile.tags[i].tagData.ToString("X"));
                        EditorGUILayout.LabelField("CETag:"+mapFile.tags[i].CEtagData);
                    }
                }
            }
            EditorGUILayout.EndScrollView();//((UInt16)tagPath)+offset
        }
        EditorGUILayout.LabelField(mapFile.nextString);
        //EditorGUILayout.TextField("Magic offset:",(mapFile.scanForTagsFoundAt-mapFile.header.tagDataOffset).ToString());
        
        
    }
}