using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace ParallaxEditor
{
    public class KSPTexturePostprocessor : AssetPostprocessor
    {
        public override uint GetVersion() => 11;

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

                for (int i = 0; i < paths.Count; ++i)
                {
                    var import = paths[i];
                    if (!import.EndsWith(".dds"))
                        continue;

                    try
                    {
                        bool manual = false;
                        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(import);
                        if (texture == null)
                        {
                            texture = TextureLoader.LoadTexture(import, false);
                            manual = true;
                        }

                        EditorUtility.DisplayProgressBar(
                            $"Processing DDS Textures ({i}/{paths.Count})",
                            import,
                            (float)i / paths.Count
                        );

                        Texture2D transformed;
                        if (NeedsPostprocess(texture, manual))
                            transformed = PostprocessIHVTexture(texture, manual);
                        else
                            transformed = CopyTexture(texture);

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
                        break;
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.StopAssetEditing();
            }
        }

        static Texture2D CopyTexture(Texture2D texture)
        {
            var copy = new Texture2D(
                texture.width,
                texture.height,
                texture.format,
                texture.mipmapCount > 1,
                !GraphicsFormatUtility.IsSRGBFormat(texture.graphicsFormat)
            );
            copy.Apply(false, !texture.isReadable);

            Graphics.CopyTexture(texture, copy);

            return copy;
        }

        static Texture2D PostprocessIHVTexture(Texture2D texture, bool manual)
        {
            var config = BuildAssetsConfig.Instance;
            var isLinear = !GraphicsFormatUtility.IsSRGBFormat(texture.graphicsFormat);
            var rtLinear = isLinear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB;
            var rtFormat = GetRenderTextureFormat(texture.graphicsFormat);

            var flipped = new Texture2D(
                Math.Max(texture.width, 4),
                Math.Max(texture.height, 4),
                GetUncompressedFormat(texture.graphicsFormat),
                texture.mipmapCount,
                isLinear
            );

            var rt = new RenderTexture(texture.width, texture.height, 0, rtFormat, rtLinear);

            if (config.FlipTextures && NeedsFlip(texture, manual))
                Graphics.Blit(
                    texture,
                    rt,
                    scale: new Vector2(1f, -1f),
                    offset: new Vector2(0f, 1f)
                );
            else
                Graphics.Blit(texture, rt);

            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            flipped.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, true);
            RenderTexture.active = prev;
            rt.Release();

            var newFormat = config.CrunchCompression
                ? GetCompressedFormat(texture.graphicsFormat, texture.width, texture.height)
                : texture.format;

            if (flipped.format != newFormat)
            {
                EditorUtility.CompressTexture(flipped, newFormat, TextureCompressionQuality.Best);
            }

            flipped.Apply(false, !texture.isReadable);
            return flipped;
        }

        static TextureFormat GetUncompressedFormat(GraphicsFormat gformat)
        {
            var format = GraphicsFormatUtility.GetTextureFormat(gformat);
            if (GraphicsFormatUtility.IsASTCFormat(gformat))
                throw new NotImplementedException("ASTC formats are not supported");

            switch (format)
            {
                case TextureFormat.DXT1:
                case TextureFormat.DXT1Crunched:
                    return TextureFormat.RGB24;
                case TextureFormat.DXT5:
                case TextureFormat.DXT5Crunched:
                    return TextureFormat.RGBA32;
                case TextureFormat.BC4:
                    return TextureFormat.R8;
                case TextureFormat.BC5:
                    return TextureFormat.RG16;
                case TextureFormat.BC7:
                    return TextureFormat.RGBA32;
                default:
                    return format;
            }
        }

        static TextureFormat GetCompressedFormat(GraphicsFormat gformat, int width, int height)
        {
            var format = GraphicsFormatUtility.GetTextureFormat(gformat);
            if (GraphicsFormatUtility.IsASTCFormat(gformat))
                throw new NotImplementedException("ASTC formats are not supported");

            var config = BuildAssetsConfig.Instance;

            if (!config.CrunchCompression)
                return format;

            // Crunch compression requires that the width/height are a multiple of 4
            if (width % 4 != 0 || height % 4 != 0)
                return format;

            switch (format)
            {
                case TextureFormat.DXT1:
                    return TextureFormat.DXT1Crunched;
                case TextureFormat.DXT5:
                    return TextureFormat.DXT5Crunched;

                default:
                    return format;
            }
        }

        static RenderTextureFormat GetRenderTextureFormat(GraphicsFormat gformat)
        {
            var format = GraphicsFormatUtility.GetTextureFormat(gformat);
            if (GraphicsFormatUtility.IsASTCFormat(gformat))
                throw new NotImplementedException("ASTC formats are not supported");

            var rtformat = GraphicsFormatUtility.GetRenderTextureFormat(gformat);
            if (rtformat != (RenderTextureFormat)29)
                return rtformat;

            switch (format)
            {
                case TextureFormat.BC4:
                    return RenderTextureFormat.R8;
                case TextureFormat.BC5:
                    return RenderTextureFormat.RG16;

                default:
                    return RenderTextureFormat.ARGB32;
            }
        }

        static void Swap<T>(ref T a, ref T b) => (b, a) = (a, b);

        // Imported textures are inconsistent in whether they actually need to
        // be flipped from the default north-down that parallax uses.
        //
        // This method overrides flipping in the following cases:
        // - manually loaded textures are loaded the same way that parallax
        //   loads textures and do not need to be flipped.
        // - BC7 doesn't need to be flipped for some reason.
        internal static bool NeedsFlip(Texture2D texture, bool manual)
        {
            if (manual)
                return false;

            if (texture.format == TextureFormat.BC7)
                return false;

            return true;
        }

        internal static bool NeedsPostprocess(Texture2D texture, bool manual)
        {
            var config = BuildAssetsConfig.Instance;
            if (config.FlipTextures && NeedsFlip(texture, manual))
                return true;

            if (config.CrunchCompression)
            {
                if (texture.format == TextureFormat.DXT1 || texture.format == TextureFormat.DXT5)
                    return true;
            }

            if (texture.width == 1 && texture.height == 1)
                return true;

            return false;
        }
    }
}
