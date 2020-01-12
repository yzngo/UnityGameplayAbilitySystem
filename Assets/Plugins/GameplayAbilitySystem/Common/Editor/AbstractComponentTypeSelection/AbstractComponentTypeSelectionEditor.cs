/*
 * Created on Mon Nov 04 2019
 *
 * The MIT License (MIT)
 * Copyright (c) 2019 Sahil Jain
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial
 * portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameplayAbilitySystem.Common.Editor {

    public class AbilitySystemDisplayNameAttribute : System.Attribute {
        public string Name { get; set; }
        public AbilitySystemDisplayNameAttribute(string name) {
            this.Name = name;
        }
    }

    public abstract class AbstractComponentTypeSelectionEditor<T> : UnityEditor.Editor {
        private VisualElement m_RootElement;
        private VisualTreeAsset m_ModulesVisualTree;
        private List<T> components;

        const string baseAssetPath = "Assets/Plugins/GameplayAbilitySystem/Common/Editor/AbstractComponentTypeSelection/";

        // Start is called before the first frame update
        public void OnEnable() {

            m_RootElement = new VisualElement();
            m_ModulesVisualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    baseAssetPath + "/AbstractComponentTypeSelectionEditor.uxml"
                );
            var stylesheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>(
                    baseAssetPath + "/AbstractComponentTypeSelectionEditor.uss"
                );
            m_RootElement.styleSheets.Add(stylesheet);

            // // Cleanup any strings which correspond to types that no longer exist.
            // var allTypes = new ComponentCollector().GetAllTypes(System.AppDomain.CurrentDomain);
            // var serializedTypeStrings = new List<string>();
            // var componentSerialized = serializedObject.FindProperty("Component");
            // var count = componentSerialized.arraySize;
            // for (var i = count - 1; i >= 0; i--) {
            //     var type = componentSerialized.GetArrayElementAtIndex(i).stringValue;
            //     if (!allTypes.Any(x => x.AssemblyQualifiedName == type)) {
            //         componentSerialized.DeleteArrayElementAtIndex(i);
            //     }
            // }
            serializedObject.ApplyModifiedProperties();
            // Go through each item in list and check if it in allTypes.  If it isn't, delete.
        }

        public override VisualElement CreateInspectorGUI() {
            var container = m_RootElement;
            container.Clear();
            m_ModulesVisualTree.CloneTree(container);
            var allTypes = new ComponentCollector().GetAllTypes(System.AppDomain.CurrentDomain).OrderBy(x => x.AssemblyQualifiedName).ToList();
            var uxmlField = container.Q<ObjectField>("Sprite");

            uxmlField.objectType = typeof(Sprite);
            uxmlField.Bind(serializedObject);

            var selectedLabel = container.Q<Label>("selected-type");

            var componentSerialized = serializedObject.FindProperty("Component");
            var serializedTypeString = componentSerialized.stringValue;

            var spritePreview = container.Q<Image>("sprite-preview");
            var sprite = (serializedObject.FindProperty("Sprite").objectReferenceValue as Sprite);
            if (sprite != null) {
                spritePreview.image = sprite.texture;
                spritePreview.scaleMode = ScaleMode.ScaleToFit;
                spritePreview.RemoveFromClassList("hide");
            } else {
                spritePreview.AddToClassList("hide");
            }

            uxmlField.RegisterValueChangedCallback(x => {
                sprite = x.newValue as Sprite;
                spritePreview.visible = false;
                if (sprite != null) {
                    spritePreview.image = sprite.texture;
                    spritePreview.visible = true;
                    spritePreview.scaleMode = ScaleMode.ScaleToFit;
                    spritePreview.RemoveFromClassList("hide");
                } else {
                    spritePreview.AddToClassList("hide");
                }
                container.MarkDirtyRepaint();
            });

            var listView = new ListView();


            Func<VisualElement> makeButtons = () => new Button();
            Action<VisualElement, int> bindItem = (e, i) => {
                var button = (e as Button);
                var type = allTypes[i];
                var displayNameAttribute = (AbilitySystemDisplayNameAttribute)System.Attribute.GetCustomAttribute(type, typeof(AbilitySystemDisplayNameAttribute));
                string displayName = "";
                if (displayNameAttribute != null) {
                    displayName = displayNameAttribute.Name;
                }
                button.text = displayName == "" ? type.FullName : displayName;
                button.tooltip = type.FullName;
                button.clicked += () => listView.selectedIndex = i;
                button.AddToClassList("type-button");
            };

            const int itemHeight = 24;
            listView.itemsSource = allTypes;
            listView.itemHeight = itemHeight;
            listView.makeItem = makeButtons;
            listView.bindItem = bindItem;
            listView.style.flexGrow = 1.0f;
            listView.onSelectionChanged += objects => {
                if (objects.Count < 1) return;

                var type = (objects[0] as Type);
                componentSerialized.stringValue = type.AssemblyQualifiedName;
                // listView.selectedIndex = allTypes.FindIndex(x => componentSerialized.stringValue == x.AssemblyQualifiedName);
                serializedObject.ApplyModifiedProperties();
                var displayNameAttribute = (AbilitySystemDisplayNameAttribute)System.Attribute.GetCustomAttribute(type, typeof(AbilitySystemDisplayNameAttribute));
                string displayName = "";
                if (displayNameAttribute != null) {
                    displayName = displayNameAttribute.Name;
                }
                selectedLabel.text = displayName == "" ? type.FullName : displayName;
                selectedLabel.tooltip = type.FullName;
            };
            listView.selectedIndex = allTypes.FindIndex(x => componentSerialized.stringValue == x.AssemblyQualifiedName);
            if (listView.selectedIndex > -1) {
                var type = allTypes[listView.selectedIndex];
                var displayNameAttribute = (AbilitySystemDisplayNameAttribute)System.Attribute.GetCustomAttribute(type, typeof(AbilitySystemDisplayNameAttribute));
                string displayName = "";
                if (displayNameAttribute != null) {
                    displayName = displayNameAttribute.Name;
                }
                selectedLabel.text = displayName == "" ? type.FullName : displayName;
                selectedLabel.tooltip = type.FullName;
            }

            listView.selectionType = SelectionType.Single;
            listView.AddToClassList("type-list");
            var listContainer = container.Q<VisualElement>("types-list-container");
            listContainer.Add(listView);

            // foreach (var type in allTypes) {
            //     // Look for a "DisplayName" attribute
            //     var displayNameAttribute = (AbilitySystemDisplayNameAttribute)System.Attribute.GetCustomAttribute(type, typeof(AbilitySystemDisplayNameAttribute));
            //     string displayName = "";
            //     if (displayNameAttribute != null) {
            //         displayName = displayNameAttribute.Name;
            //     }

            //     var button = new Button(() => {
            //         componentSerialized.stringValue = type.AssemblyQualifiedName;
            //         serializedObject.ApplyModifiedProperties();
            //         CreateInspectorGUI();
            //     })
            //     { text = displayName == "" ? type.FullName : displayName, tooltip = type.FullName };

            //     // If this type is in the list of selected components, mark it enabled
            //     if (serializedTypeString == type.AssemblyQualifiedName) {
            //         button.AddToClassList("enabled-button");
            //     }

            //     container.Add(button);
            // }

            return container;
        }
        public class ComponentCollector {
            public IEnumerable<System.Type> GetAllTypes(System.AppDomain domain) {
                var componentInterface = typeof(T);
                var types = domain.GetAssemblies()
                            .SelectMany(s => s.GetTypes())
                            .Where(p => componentInterface.IsAssignableFrom(p) && !p.IsInterface);

                return types;
            }

        }
    }
}
