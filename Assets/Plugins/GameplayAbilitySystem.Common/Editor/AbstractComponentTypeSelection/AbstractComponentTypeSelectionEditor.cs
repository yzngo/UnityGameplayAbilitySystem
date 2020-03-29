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

            serializedObject.ApplyModifiedProperties();
        }
        public override VisualElement CreateInspectorGUI() {
            var container = m_RootElement;
            container.Clear();
            m_ModulesVisualTree.CloneTree(container);
            Image spritePreview;
            Sprite sprite;
            BuildSpritePreview(container, out spritePreview, out sprite);
            BuildSpriteSelection(container, spritePreview, sprite);
            var componentSerialized = serializedObject.FindProperty("Component");
            var serializedTypeString = componentSerialized.stringValue;
            var selectedLabel = container.Q<Label>("selected-type");
            BuildTypeList(container, selectedLabel, componentSerialized);
            return container;
        }
        private void BuildSpriteSelection(VisualElement container, Image spritePreview, Sprite sprite) {
            var uxmlField = container.Q<ObjectField>("Sprite");
            uxmlField.objectType = typeof(Sprite);
            uxmlField.Bind(serializedObject);
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
        }
        private void BuildSpritePreview(VisualElement container, out Image spritePreview, out Sprite sprite) {
            spritePreview = container.Q<Image>("sprite-preview");
            sprite = (serializedObject.FindProperty("Sprite").objectReferenceValue as Sprite);
            if (sprite != null) {
                spritePreview.image = sprite.texture;
                spritePreview.scaleMode = ScaleMode.ScaleToFit;
                spritePreview.RemoveFromClassList("hide");
            } else {
                spritePreview.AddToClassList("hide");
            }
        }
        private void BuildTypeList(VisualElement container, Label selectedLabel, SerializedProperty componentSerialized) {
            var allTypes = new ComponentCollector().GetAllTypes(System.AppDomain.CurrentDomain).OrderBy(x => x.AssemblyQualifiedName).ToList();
            var listView = new ListView();
            Func<VisualElement> makeButtons = () => new Button();
            Action<VisualElement, int> bindItem = (e, i) => {
                var button = (e as Button);
                var type = allTypes[i];
                string displayName = GetDisplayNameForType(type);
                button.text = displayName == "" ? type.FullName : displayName;
                button.tooltip = type.FullName;
                button.clicked += () => {
                    // Do a toggle selection.  If this item is already selecte, deselect it
                    // If something else is selected, select this one
                    if (listView.selectedIndex == i) {
                        listView.selectedIndex = -1;
                    } else {
                        listView.selectedIndex = i;
                    }
                };
                button.AddToClassList("type-button");
            };
            const int itemHeight = 24;
            listView.itemsSource = allTypes;
            listView.itemHeight = itemHeight;
            listView.makeItem = makeButtons;
            listView.bindItem = bindItem;
            listView.style.flexGrow = 1.0f;
            listView.onSelectionChanged += objects => {
                // No object is selected
                if (objects.Count < 1) {
                    ClearSelectedTypeLabel(selectedLabel, componentSerialized);
                    return;
                }
                var type = (objects[0] as Type);
                ApplySerializedStringValue(serializedObject, componentSerialized, type.AssemblyQualifiedName);
                SetSelectedTypeLabel(selectedLabel, type);
            };
            listView.selectedIndex = allTypes.FindIndex(x => componentSerialized.stringValue == x.AssemblyQualifiedName);
            if (listView.selectedIndex > -1) {
                var type = allTypes[listView.selectedIndex];
                SetSelectedTypeLabel(selectedLabel, type);
            } else {
                // If the selected item doesn't exist in parent array, set the serialized value to empty
                ClearSelectedTypeLabel(selectedLabel, componentSerialized);
            }
            listView.selectionType = SelectionType.Single;
            listView.AddToClassList("type-list");
            var listContainer = container.Q<VisualElement>("types-list-container");
            listContainer.Add(listView);
        }
        void ClearSelectedTypeLabel(Label selectedLabel, SerializedProperty componentSerialized) {
            ApplySerializedStringValue(serializedObject, componentSerialized, "");
            selectedLabel.text = "(No item selected)";
            selectedLabel.tooltip = "";
        }

        void SetSelectedTypeLabel(Label selectedLabel, Type type) {
            var displayName = GetDisplayNameForType(type);
            selectedLabel.text = displayName == "" ? type.FullName : displayName;
            selectedLabel.tooltip = type.FullName;
        }
        private string GetDisplayNameForType(Type type) {
            var displayNameAttribute = (AbilitySystemDisplayNameAttribute)System.Attribute.GetCustomAttribute(type, typeof(AbilitySystemDisplayNameAttribute));
            string displayName = "";
            if (displayNameAttribute != null) {
                displayName = displayNameAttribute.Name;
            }

            return displayName;
        }

        private void ApplySerializedStringValue(SerializedObject serializedObject, SerializedProperty serializedProperty, string value) {
            serializedProperty.stringValue = value;
            serializedObject.ApplyModifiedProperties();
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
