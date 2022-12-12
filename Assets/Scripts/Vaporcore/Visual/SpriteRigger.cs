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

        TextureImporter baseImporter = GetImporter(baseAtlas);
        TextureImporter overrideImporter = GetImporter(overrideAtlas);
        overrideImporter.spriteImportMode = SpriteImportMode.Multiple;

        List<SpriteMetaData> newMetaData = new List<SpriteMetaData>();
        foreach (SpriteMetaData baseData in baseImporter.spritesheet) {
            SpriteMetaData m = new SpriteMetaData();
            m.alignment = baseData.alignment;
            m.border = baseData.border;
            m.name = GetOverrideName(baseData.name);
            m.pivot = baseData.pivot;
            m.rect = baseData.rect;
            newMetaData.Add(m);
        }
        overrideImporter.spritesheet = newMetaData.ToArray();
        Debug.Log(newMetaData.ToArray().Length);
        overrideImporter.SaveAndReimport();
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(overrideAtlas), ImportAssetOptions.ForceUpdate);
        // this isn't getting updated for some reason
        overrideImporter = GetImporter(overrideAtlas);
        Debug.Log(overrideImporter.spritesheet.Count());

        // oghh does this have to be in resources
        // guess so
        // JUST USE THE SLICING METHOD FROM BEFORE
        // AND SAVE THE OVERRIDE TEXTURE AT RUNTIME
        // or wait, fuck, the base won't exist
        // save a serialized dictionary of sprite bases and overrides? would that be too much?
        Dictionary<string, Sprite> baseSprites = LoadSpritesFromTexture(baseAtlas);
        Debug.Log(baseSprites.Count);
        Dictionary<string, Sprite> overrideSprites = LoadSpritesFromTexture(overrideAtlas);
        Debug.Log(overrideSprites.Count);
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

    string GetOverrideName(string spriteName) {
        return overrideAtlas.name + " " + spriteName;
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
    }
}
#endif
