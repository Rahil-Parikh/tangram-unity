﻿using System;
using System.Linq;
using System.Collections.Generic;
using Mapzen;
using Mapzen.Unity;
using Mapzen.VectorData.Filters;
using UnityEditor;
using UnityEngine;

namespace PluginEditor
{
    public class FilterStyleEditor : EditorBase
    {
        [SerializeField]
        private int selectedLayer;

        [SerializeField]
        private FeatureStyle.Matcher.Type selectedMatcherType;

        [SerializeField]
        private List<LayerStyleEditor> layerStyleEditors;

        [SerializeField]
        private MatcherEditor matcherEditor;

        [SerializeField]
        private FeatureStyle.FilterStyle filterStyle;

        public FeatureStyle.FilterStyle FilterStyle
        {
            get { return filterStyle; }
        }

        private static List<string> mapzenLayers = new List<string>(new string[] {
            "boundaries", "buildings", "earth", "landuse", "places", "pois", "roads", "transit", "water",
        });

        public FilterStyleEditor(FeatureStyle.FilterStyle filterStyle)
            : base()
        {
            this.filterStyle = filterStyle;
            this.layerStyleEditors = new List<LayerStyleEditor>();

            foreach (var layerStyle in filterStyle.LayerStyles)
            {
                layerStyleEditors.Add(new LayerStyleEditor(layerStyle));
            }
        }

        public override void OnInspectorGUI()
        {
            // Default layers
            EditorGUILayout.BeginHorizontal();
            {
                selectedLayer = EditorGUILayout.Popup("Default layer:", selectedLayer, mapzenLayers.ToArray());
                AddLayerStyleLayout(filterStyle, mapzenLayers[selectedLayer]);
            }
            EditorGUILayout.EndHorizontal();

            // Custom layer entry
            EditorGUILayout.BeginHorizontal();
            {
                customFeatureCollection = EditorGUILayout.TextField("Custom layer:", customFeatureCollection);
                AddLayerStyleLayout(filterStyle, customFeatureCollection);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;

            for (int i = layerStyleEditors.Count - 1; i >= 0; i--)
            {
                var editor = layerStyleEditors[i];
                var layerStyling = editor.LayerStyle;

                var state = FoldoutEditor.OnInspectorGUI(editor.GUID.ToString(), layerStyling.LayerName);

                if (state.show)
                {
                    editor.OnInspectorGUI();
                }

                if (state.markedForDeletion)
                {
                    // Remove the layer from the filter styles
                    filterStyle.LayerStyles.Remove(layerStyling);

                    // Remove the associated layer editor
                    layerStyleEditors.RemoveAt(i);
                }
            }

            EditorGUI.indentLevel--;

            var matcherTypeList = Enum.GetValues(typeof(FeatureStyle.Matcher.Type)).Cast<FeatureStyle.Matcher.Type>();
            var matcherTypeStringList = matcherTypeList.Select(type => type.ToString());
            var oldType = selectedMatcherType;

            selectedMatcherType = (FeatureStyle.Matcher.Type)EditorGUILayout.Popup("Matcher:",
                (int)selectedMatcherType, matcherTypeStringList.ToArray());

            if (selectedMatcherType != oldType)
            {
                matcherEditor = new MatcherEditor(new FeatureStyle.Matcher(selectedMatcherType));
            }
            else
            {
                if (selectedMatcherType == FeatureStyle.Matcher.Type.None)
                {
                    matcherEditor = null;
                }
            }

            if (matcherEditor != null)
            {
                matcherEditor.OnInspectorGUI();

                filterStyle.Matcher = matcherEditor.Matcher;
            }
        }
    }
}
