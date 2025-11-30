using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace ParallaxEditor
{
    #if false
    internal static class TextureUtils
    {
        // Postprocess assets according to the config.
        public static string CreatePostprocessedAsset(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath);

            throw new NotImplementedException();
        }

        // DDS textures need to be flipped vertically so that they
        static string CreatePostprocessedAsset(string assetPath, IHVImageFormatImporter importer)
        {
            var config = BuildAssetsConfig.instance;
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

            if (!config.CrunchCompression && !config.EnableLZ4Compression)
                return assetPath;

            switch (texture.format)
            {
                case TextureFormat.DXT1:
                case TextureFormat.DXT1Crunched:
                    return ProcessDXT1(texture)
            }
        }

        static Texture2D ProcessDXT1(Texture2D texture, IHVImageFormatImporter importer)
        {
            var config = BuildAssetsConfig.instance;

            var linear =
                GraphicsFormatUtility.GetLinearFormat(texture.graphicsFormat)
                == texture.graphicsFormat;

            var copy = new Texture2D(
                texture.width,
                texture.height,
                TextureFormat.RGB24,
                texture.mipmapCount,
                linear
            );

            var pixels = texture.GetPixels32();
            var width = texture.width;
            var height = texture.height;

            if (config.FlipDDSTextures)
            {
                for (int loy = 0, hiy = height - 1; loy < hiy; loy++, hiy--)
                {
                    var loStart = loy * width;
                    var hiStart = hiy * width;

                    for (int x = 0; x < width; ++x)
                        Swap(ref pixels[loStart + x], ref pixels[hiStart + x]);
                }
            }

            copy.SetPixels32(pixels);
            copy.Apply(true);
            var format = config.CrunchCompression ? TextureFormat.DXT1 : TextureFormat.DXT1Crunched;
            EditorUtility.CompressTexture(copy, format, TextureCompressionQuality.Best);
            copy.Apply(false);

            return copy;
        }

        static void Swap<T>(ref T a, ref T b) => (b, a) = (a, b);
    }
    #endif
}
