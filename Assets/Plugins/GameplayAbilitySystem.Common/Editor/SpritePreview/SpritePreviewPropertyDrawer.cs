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


using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(SpritePreview))]
public class SpritePreviewPropertyDrawer : PropertyDrawer {
    const string baseAssetPath = "Assets/Plugins/GameplayAbilitySystem/Common/Editor/SpritePreview/";

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        var m_ModulesVisualTree =
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            baseAssetPath + "/Elements.uxml"
            );

        var stylesheet =
            AssetDatabase.LoadAssetAtPath<StyleSheet>(
                baseAssetPath + "/Styles.uss"
            );

        var container = new VisualElement();
        m_ModulesVisualTree.CloneTree(container);
        var prop = property.FindPropertyRelative("Sprite");
        var spriteContainer = container.Q<ObjectField>("Sprite");
        var previewContainer = container.Q<Image>("sprite-preview");
        spriteContainer.objectType = typeof(Sprite);
        spriteContainer.RegisterValueChangedCallback(x => {
            BuildSpritePreview(previewContainer, x.newValue as Sprite);
        });

        if (prop.objectReferenceValue != null) {
            var sprite = (prop.objectReferenceValue as Sprite);
            BuildSpritePreview(previewContainer, sprite);
        }

        container.styleSheets.Add(stylesheet);
        return container;
    }

    private void BuildSpritePreview(Image spritePreviewContainer, Sprite sprite) {
        if (sprite != null) {
            spritePreviewContainer.image = sprite.texture;
            spritePreviewContainer.scaleMode = ScaleMode.ScaleToFit;
            spritePreviewContainer.RemoveFromClassList("hide");
        } else {
            spritePreviewContainer.AddToClassList("hide");
        }
    }
}