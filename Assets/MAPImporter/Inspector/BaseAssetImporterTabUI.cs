// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor;
namespace ExposedInspector{
    public abstract class BaseAssetImporterTabUI
    {
        ScriptedImporterEditor m_PanelContainer = null;

        protected AssetImporterEditor panelContainer { get { return m_PanelContainer; } }

        public SerializedObject serializedObject
        {
            get { return m_PanelContainer.serializedObject; }
        }

        public Object[] targets
        {
            get { return m_PanelContainer.targets; }
        }

        public Object target
        {
            get { return m_PanelContainer.target; }
        }

        public Object assetTarget
        {
            get { return m_PanelContainer.target; }
        }

        public Object[] assetTargets
        {
            get { return m_PanelContainer.targets; }
        }
/* 
        public int referenceTargetIndex
        {
            get { return m_PanelContainer.in; }
            set { m_PanelContainer.referenceTargetIndex = value; }
        } */

        protected Func<Object, Object> Instantiate;
        protected Action<Object> DestroyImmediate;

        public BaseAssetImporterTabUI(ScriptedImporterEditor panelContainer)
        {
            m_PanelContainer = panelContainer;

            Instantiate = obj => AssetImporterEditor.Instantiate(obj);
            DestroyImmediate = obj => AssetImporterEditor.DestroyImmediate(obj);
        }

        public abstract void OnEnable();
        public virtual void OnDisable() {}

        internal virtual void PreApply() {}


        internal virtual void PostApply() {}

        internal virtual void ResetValues() {}

        public abstract void OnInspectorGUI();

        internal virtual bool HasModified()
        {
            return serializedObject.hasModifiedProperties;
        }

        // The preview functionality is implemented as expected in the Editor class
        // Classes that derive from the AssetImporterPanel can override part of the functionality,
        // but overall we should still fall back to the implementation in the editor
        public virtual void OnPreviewSettings()
        {
        }

        public virtual void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            m_PanelContainer.OnPreviewGUI(r, background);
        }

        public virtual bool HasPreviewGUI()
        {
            return true;
        }

        internal virtual void OnDestroy()
        {
        }

        public void Repaint()
        {
            m_PanelContainer.Repaint();
        }
    }

}