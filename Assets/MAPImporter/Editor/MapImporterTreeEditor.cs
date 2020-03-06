
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor.IMGUI.Controls;
using ExposedInspector;
public class MapImporterTreeEditor : BaseAssetImporterTabUI{
    [SerializeField]
    TreeViewState m_TreeViewState;
    TagTreeView tagTreeView;

    public MapImporterTreeEditor(ScriptedImporterEditor panelContainer):base(panelContainer){}
    HaloMap mapFile{
        get{
            return (target as MAPImporter).mapFile;
        }
    }
    
    public override void OnEnable(){
        // Check whether there is already a serialized view state (state 
        // that survived assembly reloading)
        if (m_TreeViewState == null)
            m_TreeViewState = new TreeViewState ();

        tagTreeView = new TagTreeView(m_TreeViewState,mapFile.tags);
    }
    public override void OnInspectorGUI(){
        Rect r=EditorGUILayout.GetControlRect();
        tagTreeView.OnGUI(new Rect(r.x,r.y,r.width,r.height*10));
    }
}