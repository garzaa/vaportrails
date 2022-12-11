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

        baseImporter.spritesheet.CopyTo(overrideImporter.spritesheet, 0);
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(overrideAtlas));
        AssetDatabase.Refresh();

        // oghh does this have to be in resources
        Dictionary<string, Sprite> baseSprites = Resources.LoadAll<Sprite>(baseAtlas.name).ToDictionary(s => s.name, s => s);
        Dictionary<string, Sprite> overrideSprites = Resources.LoadAll<Sprite>(overrideAtlas.name).ToDictionary(s => s.name, s => s);

        // NOW look through the sprite renderer children
        // and get the corresponding names
        // or even just indices in the sprite slicer
        foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>()) {
            // only do the ones matching the base texture
            // then apply the override
            if (spriteRenderer.sprite.texture.name == baseAtlas.name) {
                spriteRenderer.sprite = overrideSprites[spriteRenderer.sprite.name];
            }
        }
    }

    TextureImporter GetImporter(Texture2D t) {
        return AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(t)) as TextureImporter;
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
