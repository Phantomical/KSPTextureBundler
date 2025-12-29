using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace KSPTextureLoader
{
    internal static class TextureUtils
    {
        internal static void ApplyGeneric(
            this Texture tex,
            bool updateMipmaps,
            bool makeNoLongerReadable
        )
        {
            switch (tex)
            {
                case Texture2D tex2d:
                    tex2d.Apply(updateMipmaps, makeNoLongerReadable);
                    break;
                case Texture2DArray texture2DArray:
                    texture2DArray.Apply(updateMipmaps, makeNoLongerReadable);
                    break;
                case Texture3D texture3D:
                    texture3D.Apply(updateMipmaps, makeNoLongerReadable);
                    break;
                case Cubemap cubemap:
                    cubemap.Apply(updateMipmaps, makeNoLongerReadable);
                    break;
                case CubemapArray cubemapArray:
                    cubemapArray.Apply(updateMipmaps, makeNoLongerReadable);
                    break;

                default:
                    throw new NotSupportedException(
                        $"Cannot apply a texture of type {tex.GetType().Name}"
                    );
            }
        }

        #region CreateUninitializedTexture
        // This reflects the actual creation flags in
        // https://github.com/Unity-Technologies/UnityCsReference/blob/59b03b8a0f179c0b7e038178c90b6c80b340aa9f/Runtime/Export/Graphics/GraphicsEnums.cs#L626
        //
        // Most of the extra ones here are completely undocumented.
        [Flags]
        internal enum InternalTextureCreationFlags
        {
            None,
            MipChain = 1 << 0,
            DontInitializePixels = 1 << 2,
            DontDestroyTexture = 1 << 3,
            DontCreateSharedTextureData = 1 << 4,
            APIShareable = 1 << 5,
            Crunch = 1 << 6,
        }

        static MethodInfo GetValidateMethod()
        {
            return typeof(Texture).GetMethod(
                "ValidateFormat",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(TextureFormat) },
                Array.Empty<ParameterModifier>()
            );
        }

        /// <summary>
        /// Create a <see cref="Texture2D"/> without initializing its data.
        /// </summary>
        internal static Texture2D CreateUninitializedTexture2D(
            int width,
            int height,
            TextureFormat format = TextureFormat.RGBA32,
            bool mipChain = false,
            bool linear = false,
            InternalTextureCreationFlags flags = InternalTextureCreationFlags.None
        )
        {
            // The code in here exactly matches the behaviour of the Texture2D
            // constructors which directly take a TextureFormat, with one
            // difference: it includes the DontInitializePixels flag.
            //
            // This is necessary because the Texture2D constructors that take
            // GraphicsFormat validate the format differently than those that take
            // TextureFormat, and only the GraphicsFormat constructors allow you to
            // pass TextureCreationFlags.
            //
            // I (@Phantomical) have taken at look at decompiled implementation for
            // Internal_Create_Impl and validated that this works as you would expect.

            if (GraphicsFormatUtility.IsCrunchFormat(format))
                flags |= InternalTextureCreationFlags.Crunch;
            int mipCount = !mipChain ? 1 : -1;

            return CreateUninitializedTexture2D(
                width,
                height,
                mipCount,
                GraphicsFormatUtility.GetGraphicsFormat(format, isSRGB: !linear),
                flags
            );
        }

        internal static Texture2D CreateUninitializedTexture2D(
            int width,
            int height,
            int mipCount,
            GraphicsFormat format,
            InternalTextureCreationFlags flags = InternalTextureCreationFlags.None
        )
        {
            var create = typeof(Texture2D).GetMethod("Internal_Create", BindingFlags.Static | BindingFlags.NonPublic);
            var validate = GetValidateMethod();

            var tex = (Texture2D)FormatterServices.GetUninitializedObject(typeof(Texture2D));
            if (!(bool)validate.Invoke(tex, new object[]{GraphicsFormatUtility.GetTextureFormat(format)}))
                return tex;

            flags |= InternalTextureCreationFlags.DontInitializePixels;
            if (mipCount != 1)
                flags |= InternalTextureCreationFlags.MipChain;

            create.Invoke(
                null,
                new object[] {
                    tex,
                    width,
                    height,
                    mipCount,
                    format,
                    (TextureCreationFlags)flags,
                    IntPtr.Zero
                }
            );

            return tex;
        }

        internal static Texture2DArray CreateUninitializedTexture2DArray(
            int width,
            int height,
            int depth,
            int mipCount,
            GraphicsFormat format,
            InternalTextureCreationFlags flags = InternalTextureCreationFlags.None
        )
        {
            var create = typeof(Texture2DArray).GetMethod("Internal_Create", BindingFlags.Static | BindingFlags.NonPublic);
            var validate = GetValidateMethod();

            var tex = (Texture2DArray)FormatterServices.GetUninitializedObject(typeof(Texture2D));
            if (!(bool)validate.Invoke(null, new object[]{GraphicsFormatUtility.GetTextureFormat(format)}))
                return tex;

            flags |= InternalTextureCreationFlags.DontInitializePixels;
            if (mipCount != 1)
                flags |= InternalTextureCreationFlags.MipChain;

            create.Invoke(
                null,
                new object[] {
                    tex,
                    width,
                    height,
                    depth,
                    mipCount,
                    format,
                    (TextureCreationFlags)flags
                }
            );

            return tex;
        }

        internal static Texture3D CreateUninitializedTexture3D(
            int width,
            int height,
            int depth,
            int mipCount,
            GraphicsFormat format,
            InternalTextureCreationFlags flags = InternalTextureCreationFlags.None
        )
        {
            var create = typeof(Texture3D).GetMethod("Internal_Create", BindingFlags.Static | BindingFlags.NonPublic);
            var validate = GetValidateMethod();

            var tex = (Texture3D)FormatterServices.GetUninitializedObject(typeof(Texture2D));
            if (!(bool)validate.Invoke(tex, new object[]{GraphicsFormatUtility.GetTextureFormat(format)}))
                return tex;

            flags |= InternalTextureCreationFlags.DontInitializePixels;
            if (mipCount != 1)
                flags |= InternalTextureCreationFlags.MipChain;

            create.Invoke(
                null,
                new object[] {
                    tex,
                    width,
                    height,
                    depth,
                    mipCount,
                    format,
                    (TextureCreationFlags)flags
                }
            );

            return tex;
        }

        internal static Cubemap CreateUninitializedCubemap(
            int extent,
            int mipCount,
            GraphicsFormat format,
            InternalTextureCreationFlags flags = InternalTextureCreationFlags.None
        )
        {
            var create = typeof(Cubemap).GetMethod("Internal_Create", BindingFlags.Static | BindingFlags.NonPublic);
            var validate = GetValidateMethod();

            var tex = (Cubemap)FormatterServices.GetUninitializedObject(typeof(Cubemap));

            if (!(bool)validate.Invoke(tex, new object[]{GraphicsFormatUtility.GetTextureFormat(format)}))
                return tex;

            flags |= InternalTextureCreationFlags.DontInitializePixels;
            if (mipCount != 1)
                flags |= InternalTextureCreationFlags.DontInitializePixels;

            create.Invoke(
                null,
                new object[] {
                    tex,
                    extent,
                    mipCount,
                    format,
                    (TextureCreationFlags)flags,
                    IntPtr.Zero
                }
            );

            return tex;
        }

        internal static CubemapArray CreateUninitializedCubemapArray(
            int extent,
            int count,
            int mipCount,
            GraphicsFormat format,
            InternalTextureCreationFlags flags = InternalTextureCreationFlags.None
        )
        {
            var create = typeof(CubemapArray).GetMethod("Internal_Create", BindingFlags.Static | BindingFlags.NonPublic);
            var validate = GetValidateMethod();

            var tex = (CubemapArray)FormatterServices.GetUninitializedObject(typeof(Cubemap));

            if (!(bool)validate.Invoke(tex, new object[]{GraphicsFormatUtility.GetTextureFormat(format)}))
                return tex;

            flags |= InternalTextureCreationFlags.DontInitializePixels;
            if (mipCount != 1)
                flags |= InternalTextureCreationFlags.MipChain;

            create.Invoke(
                null,
                new object[] {
                    tex,
                    extent,
                    count,
                    mipCount,
                    format,
                    (TextureCreationFlags)flags
                }
            );

            return tex;
        }
        #endregion

        #region CloneTexture
        internal static Texture CloneTexture(Texture src)
        {
            switch (src)
            {
                case Texture2D texture2d:
                    return CloneTexture(texture2d);
                case Texture2DArray texture2darray:
                    return CloneTexture(texture2darray);
                case Texture3D texture3d:
                    return CloneTexture(texture3d);
                case Cubemap cubemap:
                    return CloneTexture(cubemap);
                case CubemapArray cubemapArray:
                    return CloneTexture(cubemapArray);
                default:
                    throw new NotImplementedException(
                        $"Cannot clone a texture of type {src.GetType().Name}"
                    );
            }
        }

        internal static Texture2D CloneTexture(Texture2D src)
        {
            var dst = CreateUninitializedTexture2D(
                src.width,
                src.height,
                src.mipmapCount,
                src.graphicsFormat
            );
            if (!src.isReadable)
                dst.Apply(false, true);

            Graphics.CopyTexture(src, dst);
            return dst;
        }

        internal static Texture2DArray CloneTexture(Texture2DArray src)
        {
            var dst = CreateUninitializedTexture2DArray(
                src.width,
                src.height,
                src.depth,
                src.mipmapCount,
                src.graphicsFormat
            );
            if (!src.isReadable)
                dst.Apply(false, true);

            Graphics.CopyTexture(src, dst);
            return dst;
        }

        internal static Texture3D CloneTexture(Texture3D src)
        {
            var dst = CreateUninitializedTexture3D(
                src.width,
                src.height,
                src.depth,
                src.mipmapCount,
                src.graphicsFormat
            );
            if (!src.isReadable)
                dst.Apply(false, true);

            Graphics.CopyTexture(src, dst);
            return dst;
        }

        internal static Cubemap CloneTexture(Cubemap src)
        {
            var dst = CreateUninitializedCubemap(src.width, src.mipmapCount, src.graphicsFormat);
            if (!src.isReadable)
                dst.Apply(false, true);

            Graphics.CopyTexture(src, dst);
            return dst;
        }

        internal static CubemapArray CloneTexture(CubemapArray src)
        {
            var dst = CreateUninitializedCubemapArray(
                src.width,
                src.cubemapCount,
                src.mipmapCount,
                src.graphicsFormat
            );
            if (!src.isReadable)
                dst.Apply(false, true);

            Graphics.CopyTexture(src, dst);
            return dst;
        }
        #endregion

        #region Cubemap

        static int GetSupportedMipMapLevels(int cubedim, GraphicsFormat format)
        {
            var tzcnt = TrailingZeroCount(cubedim);

            // We cannot subdivide compressed formats to smaller than supported by
            // their block size.
            if (GraphicsFormatUtility.IsCompressedFormat(format))
                tzcnt -= TrailingZeroCount((int)GraphicsFormatUtility.GetBlockHeight(format));

            if (tzcnt < 1)
                tzcnt = 1;

            return tzcnt;
        }

        internal static Cubemap ConvertTexture2dToCubemap(Texture2D src)
        {
            var cubedim = src.width / 4;
            if (src.width != cubedim * 4 || src.height != cubedim * 3)
                throw new Exception(
                    "2D texture was not in the right format for a cubemap. Dimensions need to be 4*cubedim x 3*cubedim."
                );

            var mips = GetSupportedMipMapLevels(cubedim, src.graphicsFormat);
            if (mips > src.mipmapCount)
                mips = src.mipmapCount;

            Cubemap cube = CreateUninitializedCubemap(
                cubedim,
                mips,
                GraphicsFormatUtility.GetGraphicsFormat(TextureFormat.RGBA32, false)
            );

            cube.SetPixels(
                src.GetPixels(2 * cubedim, 2 * cubedim, cubedim, cubedim),
                CubemapFace.NegativeY
            );
            cube.SetPixels(
                src.GetPixels(3 * cubedim, cubedim, cubedim, cubedim),
                CubemapFace.PositiveX
            );
            cube.SetPixels(
                src.GetPixels(2 * cubedim, cubedim, cubedim, cubedim),
                CubemapFace.PositiveZ
            );
            cube.SetPixels(src.GetPixels(cubedim, cubedim, cubedim, cubedim), CubemapFace.NegativeX);
            cube.SetPixels(src.GetPixels(0, cubedim, cubedim, cubedim), CubemapFace.NegativeZ);
            cube.SetPixels(src.GetPixels(2 * cubedim, 0, cubedim, cubedim), CubemapFace.PositiveY);

            cube.name = src.name;
            return cube;
        }

        #endregion

        static unsafe int TrailingZeroCount(int value)
        {
            uint v = (uint)value;
            if (v == 0)
                return 32;

            float f = v & -v;
            uint r = *(uint*)&f;
            return (int)(r >> 23) - 0x7F;
        }
    }
}
