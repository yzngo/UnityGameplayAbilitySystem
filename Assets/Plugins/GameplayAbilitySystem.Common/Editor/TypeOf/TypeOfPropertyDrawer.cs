/*
 * Created on Sun Jan 19 2020
 *
 * The MIT License (MIT)
 * Copyright (c) 2020 Sahil Jain
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
using GameplayAbilitySystem.Common.Editor;
using UnityEditor;
using UnityEngine.UIElements;

public abstract class TypeOfPropertyDrawer<T> : PropertyDrawer {
    const string baseAssetPath = "Assets/Plugins/GameplayAbilitySystem/Common/Editor/TypeOf/";

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        // Create property container element.
        var m_ModulesVisualTree =
    AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
        baseAssetPath + "/Elements.uxml"
    );
        var stylesheet =
            AssetDatabase.LoadAssetAtPath<StyleSheet>(
                baseAssetPath + "/Styles.uss"
            );

        var container = m_ModulesVisualTree.CloneTree(property.propertyPath);
        var prop = property.FindPropertyRelative("_type");
        var listContainer = container.Q<VisualElement>("types-list-container");
        var foldoutLabel = container.Q<Foldout>("selected-type-foldout");
        listContainer.Add(BuildTypeList(prop, (newVal) => {
            var labelText = "No Type Selected";
            var tooltip = "No Type Selected";
            var serializedString = "";
            if (newVal != null) {
                labelText = formatType(newVal);
                tooltip = newVal.FullName;
                serializedString = newVal.AssemblyQualifiedName;
            }

            foldoutLabel.text = labelText;
            foldoutLabel.tooltip = tooltip;
            prop.stringValue = serializedString;
            prop.serializedObject.ApplyModifiedProperties();
        }));
        container.styleSheets.Add(stylesheet);
        container.AddToClassList("parent");
        // Create property fields.
        return container;
    }

    private string formatType(Type type) {
        return "Selected Type: " + GetDisplayNameForType(type);
    }

    private VisualElement BuildTypeList(SerializedProperty property, Action<Type> selectionChanged) {
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
                selectionChanged(null);
                return;
            }
            var type = (objects[0] as Type);
            selectionChanged(type);
        };
        var a = property.stringValue;
        listView.selectedIndex = allTypes.FindIndex(x => property.stringValue == x.AssemblyQualifiedName);
        if (listView.selectedIndex > -1) {
            var type = allTypes[listView.selectedIndex];
            selectionChanged(type);
        } else {
            // If the selected item doesn't exist in parent array, set the serialized value to empty
            selectionChanged(null);
        }
        listView.selectionType = SelectionType.Single;
        listView.AddToClassList("type-list");
        return listView;
    }
    private string GetDisplayNameForType(Type type) {
        var displayNameAttribute = (AbilitySystemDisplayNameAttribute)System.Attribute.GetCustomAttribute(type, typeof(AbilitySystemDisplayNameAttribute));
        string displayName = "";
        if (displayNameAttribute != null) {
            displayName = displayNameAttribute.Name;
        }

        return displayName;
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