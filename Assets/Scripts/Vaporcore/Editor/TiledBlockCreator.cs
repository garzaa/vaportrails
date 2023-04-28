#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class TiledBlockCreator : MonoBehaviour {
    static int tileSize = 16;
    static int blocksPerTile = 2;
    static int blockSize = tileSize / blocksPerTile;
    static int xTiles = 7;
    static int yTiles = 3;
    
    static int templateSize = 2 * tileSize;
    public static int destWidth = xTiles * tileSize;
    public static int destHeight = yTiles * tileSize;

    static string assetPath;
    static Texture2D src;
    static Texture2D dest;
    static Vector2Int workingTile;

    [MenuItem("Assets/Create Tiled Block", true)]
    static bool CanMakeTiledBlock() {
        if (Selection.activeObject is Texture2D) {
            Texture2D t = Selection.activeObject as Texture2D;
            return t.width == templateSize && t.height == templateSize;
        }
        return false;
    }

    [MenuItem("Assets/Create Tiled Block")]
    static void CreateTiledBlockFromTexture() {
        assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);

        PrepareBaseTexture();
        LoadBaseTexture();
        SetParameters();
        CreateOutputTexture();
        CreateBlocks();
        CleanOldOutput();
        SaveOutputTexture();
        SliceOutputTexture();
    }

    static void CleanOldOutput() {
        AssetDatabase.DeleteAsset(BaseToOutputName(assetPath));
    }

    static void LoadBaseTexture() {
        src = Selection.activeObject as Texture2D;
    }

    static void SetParameters() {
        // tile size can be variable
        tileSize = src.width / 2;
        blockSize = tileSize / blocksPerTile;
        
        templateSize = 2 * tileSize;
        destWidth = xTiles * tileSize;
        destHeight = yTiles * tileSize;
    }

    static void CreateOutputTexture() {
        /*
        (0, 2) (1, 2) (2, 2), (3, 2)
        (0, 1) (1, 1) (2, 1), (3, 1)
        (0, 0) (1, 0) (2, 0), (3, 0) ... etc
        */
        dest = MakeDefaultTexture(tileSize * xTiles, tileSize * yTiles);
    }

    static void CreateBlocks() {        
        // 1 - edge, 0 - enclosed
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

        // inner corner - top edge to side edge
        SetWorkingTile(v(2, 2));
        FillMiddle();
        InnerTopLeftCorner();

        // inner corner - bottom edge to side edge
        SetWorkingTile(v(2, 1));
        FillMiddle();
        InnerBottomLeftCorner();

        // end cap for a 1-high row
        SetWorkingTile(v(2, 0));
        TopEdge();
        BottomEdge();
        TopLeftCorner();
        BottomLeftCorner();

        // top cap for a 1-wide column
        SetWorkingTile(v(3, 2));
        LeftEdge();
        RightEdge();
        TopLeftCorner();
        TopRightCorner();

        // bottom cap for a 1-wide column
        SetWorkingTile(v(3, 1));
        LeftEdge();
        RightEdge();
        BottomLeftCorner();
        BottomRightCorner();

        // center for a 1-high row
        SetWorkingTile(v(3, 0));
        TopEdge();
        BottomEdge();

        // zero-neighbor
        SetWorkingTile(v(4, 2));
        TopLeftCorner();
        TopRightCorner();
        BottomLeftCorner();
        BottomRightCorner();

        // middle of a 1-wide column
        SetWorkingTile(v(4, 1));
        LeftEdge();
        RightEdge();

        SetWorkingTile(v(4, 0));
        FillMiddle();
        InnerTopLeftCorner();
        RightEdge();

        SetWorkingTile(v(5, 2));
        FillMiddle();
        InnerTopLeftCorner();
        InnerTopRightCorner();

        SetWorkingTile(v(5, 1));
        FillMiddle();
        InnerBottomLeftCorner();
        InnerBottomRightCorner();

        SetWorkingTile(v(5, 0));
        FillMiddle();
        InnerTopLeftCorner();
        InnerBottomLeftCorner();

        SetWorkingTile(v(6, 2));
        FillMiddle();
        TopEdge();
        InnerBottomLeftCorner();

        SetWorkingTile(v(6, 1));
        FillMiddle();
        BottomEdge();
        InnerTopLeftCorner();

        SetWorkingTile(v(6, 0));
        LeftEdge();
        TopEdge();
        TopLeftCorner();
        InnerBottomRightCorner();
    }

    static Vector2Int v(int x, int y) {
        return new Vector2Int(x, y);
    }

    static void SetWorkingTile(Vector2Int v) {
        workingTile = v;
    }

    static void CopyBlock(Vector2Int sourceBlock, Vector2Int destBlock) {
        /*
        assumes a block with this structure
        (0, 1) (1, 1)
        (0, 0) (1, 0)
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

    static void CopyPixelBlock(int sourceX, int sourceY, int blockWidth, int blockHeight, int destX, int destY) {
        Graphics.CopyTexture(src, 0, 0, sourceX, sourceY, blockWidth, blockHeight, dest, 0, 0, destX, destY);
    }

    static Texture2D MakeDefaultTexture(int xSize, int ySize) {
        Texture2D output = new Texture2D(xSize, ySize);
        output.filterMode = FilterMode.Point;
        return output;
    }

    static void FillMiddle() {
        /*
        given a texture with four target tiles, it's mapped like this:
        (1, 0) (1, 1)
        (0, 0) (0, 1)
        and each tile will have 4 blocks in it
        */

        // fill the middle with the middle texture, straight copy
        CopyBlock(v(1, 1), v(2, 2), v(0, 0));
    }

    static void TopEdge() {
        CopyBlock(v(1, 3), v(2, 1), v(0, 1));
    }

    static void RightEdge() {
        CopyBlock(v(0, 1), v(1, 2), v(1, 0));
        MirrorBlock(v(1, 0));
        MirrorBlock(v(1, 1));
    }

    static void BottomEdge() {
        CopyBlock(v(1, 0), v(2, 1), v(0, 0));
    }

    static void LeftEdge() {
        CopyBlock(v(0, 1), v(1, 2), v(0, 0));
    }

    static void TopLeftCorner() {
        CopyBlock(v(0, 3), v(0, 1));
    }

    static void TopRightCorner() {
        CopyBlock(v(0, 3), v(1, 1));
        MirrorBlock(v(1, 1));
    }

    static void BottomRightCorner() {
        CopyBlock(v(0, 0), v(1, 0));
        MirrorBlock(v(1, 0));
    }

    static void BottomLeftCorner() {
        CopyBlock(v(0, 0), v(0, 0));
    }

    static void InnerTopLeftCorner() {
        CopyBlock(v(3, 1), v(0, 1));
    }

    static void InnerTopRightCorner() {
        CopyBlock(v(3, 1), v(1, 1));
        MirrorBlock(v(1, 1));
    }

    static void InnerBottomLeftCorner() {
        CopyBlock(v(3, 0), v(0, 0));
    }

    static void InnerBottomRightCorner() {
        CopyBlock(v(3, 0), v(1, 0));
        MirrorBlock(v(1, 0));
    }

    static void MirrorBlock(Vector2Int targetBlock) {
        // translate the block's origin into texture space
        // first offset by tile, then look ino the specific block
        Vector2Int origin = (tileSize * workingTile) + (targetBlock * blockSize);
        Color[] pixels = dest.GetPixels(origin.x, origin.y, blockSize, blockSize);

        // then go and flip each row, swap the first element with the nth, second with n-1, etc
        int diff;
        Color other;        
        // swap pixels in the left half with the right half
        for (int i=0; i<pixels.Length; i++) {
            diff = (blockSize - (i%blockSize)*2 - 1);
            if (i%blockSize < blockSize/2) {
                other = pixels[i + diff];
                pixels[i + diff] = pixels[i];
                pixels[i] = other;
            }
        }
        dest.SetPixels(origin.x, origin.y, blockSize, blockSize, pixels);

        dest.Apply();
    }

    static void PrepareBaseTexture() {
        // the texture's asset importer setting has to be RGBA32, because that's the format of the default PNG dest tex we create
        TextureImporter t = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        TextureImporterPlatformSettings texset = t.GetDefaultPlatformTextureSettings();
        texset.format = TextureImporterFormat.RGBA32;
        t.SetPlatformTextureSettings(texset);
        // also make sure it's readable
        t.isReadable = true;
        AssetDatabase.ImportAsset(assetPath);
        AssetDatabase.Refresh();
    }

    static void SaveOutputTexture() {
        File.WriteAllBytes(BaseToOutputName(assetPath), dest.EncodeToPNG());
        // then refresh so the new texture shows up
        AssetDatabase.Refresh();
    }

    static string BaseToOutputName(string baseAssetPath) {
        string[] splitPath = baseAssetPath.Split('/');
        string originalFileName = splitPath[splitPath.Length-1];
        splitPath[splitPath.Length-1] = originalFileName.Split('.')[0]+"_generated.png";
        return string.Join('/', splitPath);
    }

    static void SliceOutputTexture() {
        string outputPath = BaseToOutputName(assetPath);
        TextureImporter ti = AssetImporter.GetAtPath(outputPath) as TextureImporter;
        ti.spriteImportMode = SpriteImportMode.Multiple;
        List<SpriteMetaData> metaData = new List<SpriteMetaData>();
        int spriteNum = 0;
        for (int x=0; x<dest.width; x+=tileSize) {
            for (int y=dest.height; y>0; y-=tileSize) {
                SpriteMetaData meta = new SpriteMetaData();
                meta.pivot = new Vector2(0.5f, 0.5f);
                meta.alignment = 9;
                meta.name = spriteNum.ToString();
                meta.rect = new Rect(x, y-tileSize, tileSize, tileSize);
                metaData.Add(meta);

                spriteNum++;
            }
        }
        ti.spritesheet = metaData.ToArray();
        AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();
    }
}
#endif
