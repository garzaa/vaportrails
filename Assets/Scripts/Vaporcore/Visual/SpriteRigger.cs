#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

public class SpriteRigger : MonoBehaviour {
	// todo: save the sprites,  then override them with the base atlas?
	// but how do you check which one corresponds to which in the base atlas
	// after it's already been set
	public Texture2D baseAtlas;
	public Texture2D overrideAtlas;

	public void ApplyAtlas() {
        if (!baseAtlas || !overrideAtlas) {
            return;
        }

        Dictionary<string, Sprite> overrideSprites = LoadSpritesFromTexture(overrideAtlas);
        // NOW look through the sprite renderer children
        // and get the corresponding names
        // or even just indices in the sprite slicer
        foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>()) {
            // only do the ones matching the base texture
            // then apply the override
            if (spriteRenderer.sprite.texture.name == baseAtlas.name) {
                spriteRenderer.sprite = overrideSprites[GetOverrideName(spriteRenderer.sprite.name)];
            }
        }
    }

    public void ResetAtlas() {
        Dictionary<string, Sprite> baseSprites = LoadSpritesFromTexture(baseAtlas);
        foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>()) {
            if (spriteRenderer.sprite.texture.name == overrideAtlas.name) {
                spriteRenderer.sprite = baseSprites[GetBaseName(spriteRenderer.sprite.name)];
            }
        }
    }

    string GetOverrideName(string baseName) {
        return overrideAtlas.name + " " + baseName;
    }

    string GetBaseName(string overrideName) {
        List<string> l = new List<string>(overrideName.Split(" "));
        l.RemoveAt(0);
        return string.Join(' ', l);
    }

    TextureImporter GetImporter(Texture2D t) {
        return AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(t)) as TextureImporter;
    }

    Dictionary<string, Sprite> LoadSpritesFromTexture(Texture2D t) {
        return AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(t))
            .OfType<Sprite>()
            .ToDictionary(sprite => sprite.name, sprite => sprite);
    }
}

[CustomEditor(typeof(SpriteRigger))]
public class SpriteRiggerInspector : Editor {
    public override void OnInspectorGUI() {
        base.DrawDefaultInspector();
        SpriteRigger spriteRigger = target as SpriteRigger;

        if (GUILayout.Button("Apply Atlas")) {
            spriteRigger.ApplyAtlas();  
        }

        if (GUILayout.Button("Reset Atlas")) {
            spriteRigger.ResetAtlas();
        }
    }
}
#endif
