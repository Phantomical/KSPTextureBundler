using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KSPTextureLoader;
using KSPTextureLoader.Format;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace ParallaxEditor
{
    public class KSPTexturePostprocessor : AssetPostprocessor
    {
        public override uint GetVersion() => 12;

        internal static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetsPath
        )
        {
            var config = BuildAssetsConfig.Instance;
            AssetDatabase.StartAssetEditing();

            try
            {
                var paths = importedAssets.ToList();
                for (int i = 0; i < movedAssets.Length; ++i)
                {
                    var original = Path.GetFileNameWithoutExtension(movedFromAssetsPath[i]);
                    var renamed = Path.GetFileNameWithoutExtension(movedAssets[i]);

                    if (original != renamed)
                        paths.Add(renamed);
                }

                var imports = new TextureHandleImpl[paths.Count];

                for (int i = 0; i < paths.Count; ++i)
                {
                    var import = paths[i];
                    if (!import.EndsWith(".dds"))
                        continue;

                    try
                    {
                        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(import);
                        var options = new TextureLoadOptions
                        {
                            Hint = TextureLoadHint.Synchronous,
                            Unreadable = false
                        };

                        var importer = AssetImporter.GetAtPath(import) as IHVImageFormatImporter;

                        if (texture != null)
                        {
                            options.Unreadable = !texture.isReadable;
                            options.Linear = !GraphicsFormatUtility.IsSRGBFormat(texture.graphicsFormat);
                        }
                        else if (importer != null)
                        {
                            options.Unreadable = importer.isReadable;
                        }

                        EditorUtility.DisplayProgressBar(
                            $"Processing DDS Textures ({i}/{paths.Count})",
                            import,
                            (float)i / paths.Count
                        );

                        var handle = new TextureHandleImpl(import, options.Unreadable);
                        foreach (var item in DDSLoader.LoadDDSTexture<Texture>(handle, options))
                            handle.completeHandler?.WaitUntilComplete();

                        var transformed = handle.GetTexture();
                        transformed.filterMode = importer.filterMode;
                        transformed.wrapModeU = importer.wrapModeU;
                        transformed.wrapModeV = importer.wrapModeV;
                        transformed.wrapModeW = importer.wrapModeW;

                        var guid = AssetDatabase.AssetPathToGUID(import);
                        var fileName = Path.GetFileNameWithoutExtension(import);
                        var outPath = Path.Combine(
                            config.TemporaryAssetDirectory,
                            $"{fileName}-{guid}.asset"
                        );

                        var dirPath = Path.GetDirectoryName(outPath);
                        if (!Directory.Exists(dirPath))
                            Directory.CreateDirectory(dirPath);

                        AssetDatabase.CreateAsset(transformed, outPath);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to postprocess {import}");
                        Debug.LogException(e);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.StopAssetEditing();
            }
        }
    }
}
