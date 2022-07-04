#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;

public class RuleTileCreator : MonoBehaviour {
	static RuleTile.TilingRule currentRule;
	static Dictionary<Vector3Int, int> neighborDict = new Dictionary<Vector3Int, int>();
	static List<Sprite> sprites;
	static RuleTile tile;

	[MenuItem("Assets/Create Rule Tile From Texture", true)]
    static bool CanMakeRuleTile() {
        if (Selection.activeObject is Texture2D) {
			Texture2D t = Selection.activeObject as Texture2D;
			return t.width==TiledBlockCreator.destWidth && t.height==TiledBlockCreator.destHeight;
		}
		return false;
    }

	[MenuItem("Assets/Create Rule Tile From Texture")]
	static void CreateRuleTileFromTexture() {
		Initialize();

		string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
		string ruleTilePath = BaseToOutputName(assetPath);

		// check if it's present first
		// true is not working, it's never loaded
		bool saveNew = false;
		tile = AssetDatabase.LoadAssetAtPath(ruleTilePath, typeof(RuleTile)) as RuleTile;
		if(tile == null) {
			saveNew = true;
			tile = ScriptableObject.CreateInstance("RuleTile") as RuleTile;
		} else {
			tile.m_TilingRules.Clear();
		}
	
		sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath).Where(x => x is Sprite).Cast<Sprite>().ToList();
		
		// sprite 12 is the no-neighbor one, they're 0-indexed
		tile.m_DefaultColliderType = UnityEngine.Tilemaps.Tile.ColliderType.Grid;
		tile.m_DefaultSprite = sprites[12];

		AddSprite(0);
		Top(false);
		Left(false);
		Right(true);
		Bottom(true);

		AddSprite(1);
		Top(true);
		Left(false);
		Right(true);
		Bottom(true);

		AddSprite(2);
		Top(true);
		Left(false);
		Right(true);
		Bottom(false);

		AddSprite(3);
		Top(false);
		Left(true);
		Right(true);
		Bottom(true);

		AddSprite(5);
		Top(true);
		Left(true);
		Right(true);
		Bottom(false);

		AddSprite(6);
		Top(true);
		Left(true);
		Right(true);
		Bottom(true);
		TopLeft(false);

		AddSprite(7);
		Top(true);
		Left(true);
		Right(true);
		Bottom(true);
		BottomLeft(false);

		AddSprite(4);
		Top(true);
		Left(true);
		Right(true);
		Bottom(true);


		ApplyLastRule();
		if (saveNew) {
			AssetDatabase.CreateAsset(tile,ruleTilePath);
		}
		AssetDatabase.Refresh();
	}

	static void AddSprite(int spriteIndex) {
		ApplyLastRule();
		neighborDict.Clear();
		currentRule = new RuleTile.TilingRule();
		currentRule.m_Sprites[0] = sprites[spriteIndex];
	}

	static void TopLeft(bool neighborRule) {
		AddRule(0, neighborRule);
	}

	static void Top(bool neighborRule) {
		AddRule(1, neighborRule);
	}

	static void TopRight(bool neighborRule) {
		AddRule(2, neighborRule);
	}

	static void Left(bool neighborRule) {
		AddRule(3, neighborRule);
	}

	static void Right(bool neighborRule) {
		AddRule(4, neighborRule);
	}

	static void BottomLeft(bool neighborRule) {
		AddRule(5, neighborRule);
	}

	static void Bottom(bool neighborRule) {
		AddRule(6, neighborRule);
	}

	static void BottomRight(bool neighborRule) {
		AddRule(7, neighborRule);
	}

	static void AddRule(int neighborIndex, bool neighborRule) {
		AddRule(neighborIndex, neighborRule ? RuleTile.TilingRuleOutput.Neighbor.This : RuleTile.TilingRuleOutput.Neighbor.NotThis);
	}

	static void AddRule(int neighborIndex, int neighborRule) {
		neighborDict.Add(currentRule.m_NeighborPositions[neighborIndex], neighborRule);
	}

	static void MirrorX() {
		currentRule.m_RuleTransform = RuleTile.TilingRuleOutput.Transform.MirrorX;
	}

	static void ApplyLastRule() {
		// rule can be non-null between runs
		if (currentRule == null) return;
		MirrorX();
		currentRule.ApplyNeighbors(neighborDict);
		tile.m_TilingRules.Add(currentRule);
	}

	static void Initialize() {
		currentRule = null;
		neighborDict.Clear();
		tile = null;
	}

	static string BaseToOutputName(string baseAssetPath) {
        string[] splitPath = baseAssetPath.Split('/');
        string originalFileName = splitPath[splitPath.Length-1];
        splitPath[splitPath.Length-1] = originalFileName.Split('.')[0]+"Tile.asset";
        return string.Join('/', splitPath);
    }
}
#endif
