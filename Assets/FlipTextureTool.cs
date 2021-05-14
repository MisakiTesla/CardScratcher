using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FlipTextureTool : Editor
{
    
    [MenuItem("Assets/翻转Texture")]
    static public void ATestCheckArtResFunction()
    {
        var guids = Selection.assetGUIDs;
        foreach (var t in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(t);
            Texture2D texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
            Debug.Log(texture.name+texture.format);
            FlipTexture(texture);
            // var ti = (TextureImporter)TextureImporter.GetAtPath(path);
            //
            // AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();

        }
    }
    
    static void FlipTexture(Texture2D texture2D)
    {
        if (texture2D.isReadable)
        {
            // if (texture2D.format != TextureFormat.RGBA32)
            // {
            //     Debug.LogError("不支持该格式，请设置为RGBA32");
            //     return;
            // }
            var pixels = texture2D.GetPixels().ToList();

            var tex = new Texture2D(texture2D.width,texture2D.height,TextureFormat.RGBA32,true);

            var pixelIndex = 0;
            for (int i = 0; i < texture2D.height; i++)
            {
                var startIndex = i * texture2D.width;
                var pixelsInRow = pixels.GetRange(startIndex, texture2D.width);
                pixelsInRow.Reverse();
                
                tex.SetPixels(0, i,texture2D.width,1, pixelsInRow.ToArray());
                var bytes = tex.EncodeToPNG();
                var path = AssetDatabase.GetAssetPath(texture2D);
                var fileName = Path.GetFileNameWithoutExtension(path);
                fileName = fileName + "_flip";
                var fileExt = Path.GetExtension(path);
                File.WriteAllBytes($"{Path.Combine(Path.GetDirectoryName(path),fileName)}{fileExt}",bytes);
            }
            
        }
        else
        {
            Debug.LogError("请打开 Read/Write Enable!!");
        }
    }

}
