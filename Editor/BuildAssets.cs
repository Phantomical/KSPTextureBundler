using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ParallaxEditor
{
    public static class BuildAssets
    {
        [MenuItem("Parallax/Build Asset Bundles")]
        public static void BuildBundles()
        {
            var config = BuildAssetsConfig.Instance;

            if (!Directory.Exists(config.OutputPath))
                Directory.CreateDirectory(config.OutputPath);

            var bundleDefs = new List<AssetBundleBuild>();
            try
            {
                foreach (var entry in config.Entries)
                {
                    foreach (var bundle in GenerateAssetBundles(entry))
                        bundleDefs.Add(bundle);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            var options =
                BuildAssetBundleOptions.StrictMode
                // Only allow loading assets by full path.
                // This avoids some file size overhead.
                | BuildAssetBundleOptions.DisableLoadAssetByFileName
                | BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;

            // We need to enable at least one of these or else the whole bundle
            // gets compressed using LZMA, which is way too slow.
            if (config.EnableLZ4Compression)
                options |= BuildAssetBundleOptions.ChunkBasedCompression;
            else
                options |= BuildAssetBundleOptions.UncompressedAssetBundle;

            AssetDatabase.SaveAssets();

            BuildPipeline.BuildAssetBundles(
                config.OutputPath,
                bundleDefs.ToArray(),
                options,
                // This should be compatible with any other desktop platform
                // _as long as the asset bundle only contains textures_
                BuildTarget.StandaloneWindows64
            );
        }

        static IEnumerable<AssetBundleBuild> GenerateAssetBundles(BuildAssetEntry entry)
        {
            var config = BuildAssetsConfig.Instance;
            var excludes = new HashSet<string>(
                entry.Exclude.SelectMany(exclude =>
                    Directory.EnumerateFiles(entry.InputPath, exclude, SearchOption.AllDirectories)
                )
            );

            var outdir = Path.GetDirectoryName(Path.Combine(config.OutputPath, entry.OutputName));
            if (!Directory.Exists(outdir))
                Directory.CreateDirectory(outdir);

            var files = GetMatchingFiles(entry.InputPath, "*.dds", "*.png", "*.jpg").ToList();
            var frac = 1f / files.Count;

            var assets = new List<KeyValuePair<string, string>>();
            int index = 0;

            foreach (var file in files)
            {
                EditorUtility.DisplayProgressBar(
                    $"Processing {entry.InputPath} ({index + 1}/{files.Count})",
                    file,
                    index * frac
                );
                index += 1;

                if (excludes.Contains(file))
                {
                    Debug.Log($"Excluding {file}");
                    continue;
                }

                var ext = Path.GetExtension(file);

                string relative = file.StripPrefix(entry.InputPath)
                    .StripPrefix("\\")
                    .StripPrefix("/");
                string name = entry.AssetNamePrefix is null
                    ? file
                    : Path.Combine(entry.AssetNamePrefix, relative);
                name = NormalizePath(name);

                string path;
                if (ext == ".dds")
                    path = GetTransformedDDSAsset(file);
                else
                    path = file;

                assets.Add(new KeyValuePair<string, string>(path, name));
            }

            var names = assets.Select(pair => pair.Value).ToArray();
            var paths = assets.Select(pair => pair.Key).ToArray();

            if (paths.Length == 0)
                yield break;

            yield return new AssetBundleBuild
            {
                assetBundleName = entry.OutputName,
                assetNames = paths,
                addressableNames = names,
            };
        }

        static IEnumerable<string> GetMatchingFiles(string dir, params string[] globs)
        {
            return globs.SelectMany(glob =>
                Directory.EnumerateFiles(dir, glob, SearchOption.AllDirectories)
            );
        }

        static string NormalizePath(string path)
        {
            // Normalize all \ separators to /, then convert the name to lowercase.
            // This matches what is exported by the asset bundle script:
            // - The script normalizes all path separators to be /
            // - Unity converts all asset bundle names to lowercase.

            return path.Replace('\\', '/').ToLowerInvariant();
        }

        static string GetTransformedDDSAsset(string assetPath)
        {
            var config = BuildAssetsConfig.Instance;

            var guid = AssetDatabase.AssetPathToGUID(assetPath);

            var fileName = Path.GetFileNameWithoutExtension(assetPath);
            var transformedPath = Path.Combine(
                config.TemporaryAssetDirectory,
                $"{fileName}-{guid}.asset"
            );

            if (File.Exists(transformedPath))
                return transformedPath;

            var fallbackPath = Path.Combine(
                config.TemporaryAssetDirectory,
                $"{fileName}-{guid}.fallback.asset"
            );

            var texture = TextureLoader.LoadTexture(assetPath);
            AssetDatabase.CreateAsset(texture, fallbackPath);

            return fallbackPath;
        }
    }
}
