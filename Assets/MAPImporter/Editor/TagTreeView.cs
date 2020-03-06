using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
public class TagTreeView : TreeView
{
    [SerializeField]
    Dictionary<string,TreeViewItem> cacheItems;
    TreeViewItem root;
    public TagTreeView(TreeViewState treeViewState,List<HaloMap.Tag> tags):base(treeViewState){
            BuildCache(tags);
            Reload();
        }

    protected override TreeViewItem BuildRoot(){
        root = new TreeViewItem      { id = 0, depth = -1, displayName = "Root" };
        
        foreach(string key in cacheItems.Keys){
            try{
                if(cacheItems[key].parent==null)
                    root.AddChild(cacheItems[key]);
            }catch(System.NullReferenceException e){
                Debug.LogWarningFormat("NullRef for:{0}",key);
            }
            
        }
        SetupDepthsFromParentsAndChildren(root);
        return root;
    }

    public void BuildCache(List<HaloMap.Tag> tags){
        Debug.Log("Building cache");
        cacheItems = new Dictionary<string,TreeViewItem>();
        foreach(HaloMap.Tag tag in tags){
            AddChildPath(tag.tagPathText);
        }
    }

    void AddChildPath(string path){
        if(cacheItems.ContainsKey(path)&&cacheItems[path]!=null)
            return;
        //string[] paths=path.Split('\\');
        List<string> paths = new List<string>();
        string[] names = path.Split('\\');
        int breakIndex=path.IndexOf('\\');
        while(breakIndex>-1){
            paths.Add(path.Substring(0,breakIndex));
            breakIndex=path.IndexOf('\\',breakIndex+1);
        }
        TreeViewItem lastItem=null;
        TreeViewItem temp;
        for(int i=0;i<paths.Count;i++){    
            if(!cacheItems.ContainsKey(paths[i])){
                temp=new TreeViewItem{displayName=names[i]};
                if(lastItem!=null)
                    lastItem.AddChild(temp);
                if(cacheItems.ContainsKey(path))
                    cacheItems[path]=temp;
                else
                    cacheItems.Add(paths[i],lastItem);
                lastItem=temp;
            }else{
                lastItem=cacheItems[paths[i]];
            }
        }
        temp = new TreeViewItem{displayName=names[names.Length-1]};
        if(lastItem!=null)
            lastItem.AddChild(temp);
        if(cacheItems.ContainsKey(path))
            cacheItems[path]=temp;
        else
            cacheItems.Add(path,temp);
    }
}
