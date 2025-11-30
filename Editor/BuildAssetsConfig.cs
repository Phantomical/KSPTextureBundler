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
        public string AssetNamePrefix;

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
        public string TemporaryAssetDirectory = "Assets/Editor/Parallax-Editor-Temp";

        [SerializeField]
        public bool EnableLZ4Compression = true;

        [SerializeField]
        public bool FlipTextures = true;

        [SerializeField]
        public bool CrunchCompression = true;

        [SerializeField]
        public BuildAssetEntry[] Entries = Array.Empty<BuildAssetEntry>();

        private const string SettingsAssetPath = "Assets/Editor/Parallax-Editor-Settings.asset";
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
                "Parallax/Asset Bundle Settings (v2)",
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

                    //     Instance.hideFlags &= ~HideFlags.NotEditable;
                    //     var so = new SerializedObject(Instance);

                    //     var container = new VisualElement();
                    //     container.style.flexDirection = FlexDirection.Column;
                    //     if (styleSheet != null)
                    //         container.styleSheets.Add(styleSheet);

                    //     var label = new Label("Asset Bundle Settings");
                    //     label.AddToClassList("section-header");
                    //     container.Add(label);
                    //     container.Add(
                    //         new PropertyField(
                    //             so.FindProperty(nameof(AssetBundleExtension)),
                    //             "Asset Bundle Extension"
                    //         )
                    //         {
                    //             tooltip = "The file extension to use for asset bundles.",
                    //         }
                    //     );
                    //     container.Add(
                    //         new PropertyField(so.FindProperty(nameof(OutputPath)), "Output Path")
                    //         {
                    //             tooltip =
                    //                 "The path within the project that the asset bundles will be emitted to. "
                    //                 + "Invidual asset bundle output paths are relative to this path.",
                    //         }
                    //     );
                    //     container.Add(
                    //         new PropertyField(
                    //             so.FindProperty(nameof(EnableLZ4Compression)),
                    //             "Enable LZ4 Compression"
                    //         )
                    //         {
                    //             tooltip =
                    //                 "Enable chunk-based LZ4 compression for the resulting asset bundles.",
                    //         }
                    //     );
                    //     container.Add(
                    //         new PropertyField(
                    //             so.FindProperty(nameof(TemporaryAssetDirectory)),
                    //             "Temporary Asset Directory"
                    //         )
                    //         {
                    //             tooltip =
                    //                 "The location to store temporary flipped/compressed copies of assets. "
                    //                 + "This must be somewhere under Assets but you can otherwise ignore the contents "
                    //                 + "of this directory.",
                    //         }
                    //     );

                    //     label = new Label("Texture Import Settings");
                    //     label.AddToClassList("section-header");
                    //     container.Add(label);
                    //     container.Add(
                    //         new PropertyField(
                    //             so.FindProperty(nameof(FlipTextures)),
                    //             "Flip Textures Vertically"
                    //         )
                    //         {
                    //             tooltip =
                    //                 "If you import textures directly without asset bundles then "
                    //                 + "you will find that they are flipped vertically. If this option is "
                    //                 + "set then they will be converted on the fly before they are added to "
                    //                 + "the asset bundle.",
                    //         }
                    //     );
                    //     container.Add(
                    //         new PropertyField(
                    //             so.FindProperty(nameof(CrunchCompression)),
                    //             "Crunch Compression"
                    //         )
                    //         {
                    //             tooltip = "Enable crunch compression on DXT1 and DXT5 textures",
                    //         }
                    //     );

                    //     label = new Label("Asset Bundle Configuration");
                    //     label.AddToClassList("section-header");
                    //     container.Add(label);
                    //     container.Add(
                    //         new PropertyField(
                    //             so.FindProperty(nameof(Entries)),
                    //             "Asset Bundle Configuration"
                    //         )
                    //     );

                    //     root.Add(container);
                    //     root.Bind(so);
                },
                deactivateHandler = AssetDatabase.SaveAssets,
            };
        }
    }
}
