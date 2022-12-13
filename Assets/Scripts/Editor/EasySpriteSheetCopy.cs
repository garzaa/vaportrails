using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;

#pragma warning disable CS0618

public class EasySpriteSheetCopy{
	
	private class CopySpriteClipboard{
		public bool clipboardSet => spriteMetaData != null;
		public List<SpriteMetaData> spriteMetaData = null;
	}
	
	private static CopySpriteClipboard clipboard = new CopySpriteClipboard();

	[MenuItem("Assets/Copy Sprite Slices", false, 150)]
	private static void CopySpriteSlices() {
		
		TextureImporter spriteImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(Selection.activeObject as Texture2D)) as TextureImporter;
		
		clipboard.spriteMetaData = new List<SpriteMetaData>();

		foreach (SpriteMetaData template in spriteImporter.spritesheet){
			SpriteMetaData meta = new SpriteMetaData();
			meta.name = template.name;
			meta.rect = template.rect;
			meta.pivot = template.pivot;
			meta.alignment = template.alignment;
			meta.border = template.border;
			clipboard.spriteMetaData.Add(meta);
		}
	}
	
	[MenuItem("Assets/Paste Sprite Slices", false, 151)]
	private static void PasteSpriteSlices() {
		TextureImporter currentImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(Selection.activeObject as Texture2D)) as TextureImporter;
		string currentName = (Selection.activeObject as Texture2D).name;

		for (int i=0; i< clipboard.spriteMetaData.Count; i++) {
			SpriteMetaData metaData = clipboard.spriteMetaData[i];
			metaData.name = currentName + " " + metaData.name;
			clipboard.spriteMetaData[i] = metaData;
		}
		
		currentImporter.spritesheet = clipboard.spriteMetaData.ToArray();
		ForceSpriteImporterRefresh(currentImporter);
	}

	static void ForceSpriteImporterRefresh(TextureImporter importer) {
		// this has to happen for the editor to pick up the new changes for some insane reason
		// set it to single, reimport/refresh, then set back to multiple and reimport/refresh AGAIN

		importer.spriteImportMode = SpriteImportMode.Single;
		AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
		AssetDatabase.Refresh();
		
		importer.spriteImportMode = SpriteImportMode.Multiple;
		AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
		AssetDatabase.Refresh();
	}
	
	[MenuItem("Assets/Copy Sprite Slices", true)]
	static bool ValidateTextureType(MenuCommand command) {
		return Selection.activeObject is Texture2D;
    }

	[MenuItem("Assets/Paste Sprite Slices", true)]
	static bool ValidateClipboard(MenuCommand command){
		return Selection.activeObject is Texture2D && clipboard.clipboardSet;
	}
}
