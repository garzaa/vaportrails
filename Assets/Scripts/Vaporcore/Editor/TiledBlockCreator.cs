#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class TiledBlockCreator : MonoBehaviour {
    /*
        https://answers.unity.com/questions/949102/how-to-add-context-menu-on-assets-and-asset-folder.html
        https://docs.unity3d.com/ScriptReference/AssetDatabase.CreateAsset.html
        https://answers.unity.com/questions/1617356/how-do-i-add-neighbor-rules-for-ruletiles-by-scrip.html
*/
    // NB: (0, 0) is at the bottom left of a texture

    const int tileSize = 64;
    const int blocksPerTile = 4;
    const int blockSize = tileSize / blocksPerTile;

    static string assetPath;
    static Texture2D src;
    static Texture2D dest;
    static Vector2Int workingTile;

    [MenuItem("Assets/Create Tiled Block")]
    static void CreateTiledBlockFromTexture() {
        PrepareBaseTexture();

        // load the texture
        src = Selection.activeObject as Texture2D;
        int width = src.width;
        int height = src.height;

        // first: create a 9-slice cube
        /*
        (0, 2) (1, 2) (2, 2)
        (0, 1) (1, 1) (2, 1)
        (0, 0) (1, 0) (2, 0)
        */
        dest = MakeDefaultTexture(tileSize * 3, tileSize * 3);

        // 11
        // 10
        SetWorkingTile(v(0, 2));
        FillMiddle();
        TopEdge();
        LeftEdge();
        TopLeftCorner();

        // 11
        // 00
        SetWorkingTile(v(1, 2));
        FillMiddle();
        TopEdge();
        
        // 10
        // 10
        SetWorkingTile(v(0, 1));
        FillMiddle();
        LeftEdge();

        // 10
        // 11
        SetWorkingTile(v(0, 0));
        FillMiddle();
        LeftEdge();
        BottomEdge();
        BottomLeftCorner();

        // 00
        // 11
        SetWorkingTile(v(1, 0));
        FillMiddle();
        BottomEdge();

        // 00
        // 00
        SetWorkingTile(v(1, 1));
        FillMiddle();   

        SaveOutputTexture();
    }

    static Vector2Int v(int x, int y) {
        return new Vector2Int(x, y);
    }

    static void SetWorkingTile(Vector2Int v) {
        workingTile = v;
    }

    static void CopyBlock(Vector2Int sourceBlock, Vector2Int destBlock) {
        /*
        assumes a block with this structure (can extend past 4 quarters, just starts at bottom left)
        (0, 3) (1, 3) (2, 3), (3, 3)
        (0, 2) (1, 2) (2, 2), (3, 2)
        (0, 1) (1, 1) (2, 1), (3, 1)
        (0, 0) (1, 0) (2, 0), (3, 0)
        */
        CopyBlock(sourceBlock, v(1, 1), destBlock);
    }

    static void CopyBlock(Vector2Int sourceBlock, Vector2Int sourceBlockSize, Vector2Int destBlock) {
        CopyPixelBlock(
            sourceBlock.x * blockSize,
            sourceBlock.y * blockSize,
            sourceBlockSize.x * blockSize,
            sourceBlockSize.y * blockSize,
            (destBlock.x * blockSize) + (tileSize * workingTile.x),
            (destBlock.y * blockSize) + (tileSize * workingTile.y)
        );
    }

    static void DirectCopyBlock(Vector2Int position) {
        CopyBlock(position, position);
    }

    static void DirectCopyBlock(Vector2Int position, Vector2Int blockSize) {
        CopyBlock(position, blockSize, position);
    }

    static void CopyPixelSquare(int sourceX, int sourceY, int blockSize, int destX, int destY) {
        CopyPixelBlock(sourceX, sourceY, blockSize, blockSize, destX, destY);
    }

    static void CopyPixelBlock(int sourceX, int sourceY, int blockWidth, int blockHeight, int destX, int destY) {
        Graphics.CopyTexture(src, 0, 0, sourceX, sourceY, blockWidth, blockHeight, dest, 0, 0, destX, destY);
    }

    static Texture2D MakeDefaultTexture(int xSize, int ySize) {
        Texture2D output = new Texture2D(xSize, ySize);
        output.filterMode = FilterMode.Point;
        return output;
    }

    static bool CanMakeTiledBlock() {
        return Selection.activeObject is Texture2D;
    }

    static void FillMiddle() {
        /*
        given a texture with four target tiles, it's mapped like this:
        (1, 0) (1, 1)
        (0, 0) (0, 1)
        and each tiles will have 16 blocks in it
        */

        // fill the middle with the middle texture, straight copy
        CopyBlock(v(1, 1), v(2, 2), v(1, 1));

        // then tile the bottom section at the top
        CopyBlock(v(1, 1), v(2, 1), v(1, 3));
        // and at the bottom
        CopyBlock(v(1, 2), v(2, 1), v(1, 0));

        // then do the sides: start with left middle to right side
        CopyBlock(v(1, 1), v(1, 2), v(3, 1));
        // then right middle to left side
        CopyBlock(v(2, 1), v(1, 2), v(0, 1));

        // then do the corners, again repeating the tiling pattern
        // bottom left to top right
        CopyBlock(v(1, 1), v(3, 3));
        // bottom right to top left
        CopyBlock(v(2, 1), v(0, 3));
        // top left to bottom right
        CopyBlock(v(1, 2), v(3, 0));
        // top right to bottom left
        CopyBlock(v(2, 2), v(0, 0));
    }
    static void TopEdge() {
        DirectCopyBlock(v(1, 3), v(2, 1));
        CopyBlock(v(2, 3), v(0, 3));
        CopyBlock(v(1, 3), v(3, 3));
    }

    static void RightEdge() {
        DirectCopyBlock(v(3, 1), v(1, 2));
        CopyBlock(v(3, 1), v(3, 3));
        CopyBlock(v(3, 2), v(3, 0));
    }

    static void BottomEdge() {
        DirectCopyBlock(v(1, 0), v(2, 1));
        CopyBlock(v(2, 0), v(0, 0));
        CopyBlock(v(1, 0), v(3, 0));
    }

    static void LeftEdge() {
        DirectCopyBlock(v(0, 1), v(1, 2));
        CopyBlock(v(0, 2), v(0, 0));
        CopyBlock(v(0, 1), v(0, 3));
    }

    static void TopLeftCorner() {
        DirectCopyBlock(v(0, 3));
    }

    static void TopRightCorner() {
        DirectCopyBlock(v(3, 3));
    }

    static void BottomRightCorner() {
        DirectCopyBlock(v(3, 0));
    }

    static void BottomLeftCorner() {
        DirectCopyBlock(v(0, 0));
    }

    static void PrepareBaseTexture() {
        assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        // the texture's asset importer setting has to be RGBA32, because that's the format of the default PNG output we create
        TextureImporter t = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        TextureImporterPlatformSettings texset = t.GetDefaultPlatformTextureSettings();
        texset.format=TextureImporterFormat.RGBA32;
        t.SetPlatformTextureSettings(texset);
        // also make sure it's readable
        t.isReadable = true;
        AssetDatabase.ImportAsset(assetPath);
        AssetDatabase.Refresh();
    }

    static void SaveOutputTexture() {
        string[] splitPath = assetPath.Split('/');
        string originalFileName = splitPath[splitPath.Length-1];
        splitPath[splitPath.Length-1] = "generated_" + originalFileName;
        string outputPath = string.Join('/', splitPath);
        byte[] bytes = dest.EncodeToPNG();

        File.WriteAllBytes(outputPath, bytes);

        // then refresh so the new texture shows up
        AssetDatabase.Refresh();
    }
}
#endif
