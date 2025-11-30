using System;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ParallaxEditor
{
    [Serializable]
    public class BuildAssetEntry
    {
        /// <summary>
        /// The input path within the unity project.
        /// </summary>
        ///
        /// <remarks>
        /// One asset bundle will be built for each subdirectory within this
        /// path.
        /// </remarks>
        [SerializeField]
        public string InputPath;

        /// <summary>
        /// The output folder that the asset bundles should be emitted under
        /// within the unity project.
        /// </summary>
        [SerializeField]
        public string OutputName = "bundle.unity3d";

        /// <summary>
        /// A prefix path that will be prepended to the output asset names
        /// within the bundle.
        /// </summary>
        ///
        /// <remarks>
        /// Use this to make it so that the textures have the same paths as
        /// they would if they were directly in GameData. If a texture is at
        ///
        ///   <InputPath>/path/to/texture.dds
        ///
        /// within the solution then it will be named
        ///
        ///   <AssetNamePrefix>/path/to/texture.dds
        ///
        /// in the asset bundle.
        ///
        /// If not set then it will use the input path.
        /// </remarks>
        [SerializeField]
        public string AssetNamePrefix = "";

        /// <summary>
        /// Paths to exclude from the asset bundle.
        /// </summary>
        [SerializeField]
        public string[] Exclude = Array.Empty<string>();
    }

    public class BuildAssetsConfig : ScriptableObject
    {
        [SerializeField]
        public string OutputPath = "AssetBundles";

        [SerializeField]
        public string InputPath = "Assets";

        [SerializeField]
        public string TemporaryAssetDirectory = "Assets/KSP-Texture-Bundler/Temp";

        [SerializeField]
        public bool EnableLZ4Compression = true;

        [SerializeField]
        public bool FlipTextures = true;

        [SerializeField]
        public bool CrunchCompression = false;

        [SerializeField]
        public BuildAssetEntry[] Entries = Array.Empty<BuildAssetEntry>();

        private const string SettingsAssetPath = "Assets/KSP-Texture-Bundler/Settings.asset";
        public static BuildAssetsConfig Instance
        {
            get
            {
                var settings = AssetDatabase.LoadAssetAtPath<BuildAssetsConfig>(SettingsAssetPath);
                if (settings == null)
                {
                    settings = CreateInstance<BuildAssetsConfig>();
                    Directory.CreateDirectory(Path.GetDirectoryName(SettingsAssetPath));
                    AssetDatabase.CreateAsset(settings, SettingsAssetPath);
                    AssetDatabase.SaveAssets();
                }

                return settings;
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new SettingsProvider(
                "KSP Texture Bundler/Asset Bundle Settings (v2)",
                SettingsScope.Project
            )
            {
                label = "Asset Bundle Settings",
                activateHandler = (_, root) =>
                {
                    var so = new SerializedObject(Instance);

                    var stylePath = AssetDatabase.GUIDToAssetPath(
                        "47b926191a3468f459a77161f973e910"
                    );
                    var templatePath = AssetDatabase.GUIDToAssetPath(
                        "a5b106dba8020eb4eb9e50195aac4f41"
                    );

                    var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylePath);
                    var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(templatePath);

                    var tree = template.CloneTree();
                    tree.styleSheets.Add(styleSheet);
                    root.Add(tree);
                    root.Bind(so);
                },
                deactivateHandler = AssetDatabase.SaveAssets,
            };
        }
    }
}
