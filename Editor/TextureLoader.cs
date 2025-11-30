using System;
using System.IO;
using DDSHeaders;
using UnityEngine;

namespace ParallaxEditor
{
    public static class TextureLoader
    {
        struct TextureMetadata
        {
            public int width;
            public int height;
            public TextureFormat format;
            public bool mips;
            public bool linear;
        }

        public static Texture2D LoadTexture(string path, bool unreadable = true)
        {
            const int DDS_HEADER_SIZE = 128;

            using (var reader = new FileStream(path, FileMode.Open))
            {
                if (reader.Length < DDS_HEADER_SIZE)
                    throw new Exception("DDS texture is too small to contain a valid header");

                var br = new BinaryReader(reader);
                uint magicID = br.ReadUInt32();
                if (magicID != DDSValues.uintMagic)
                    throw new Exception("DDS texture is invalid");

                var header = new DDSHeader(br);
                var header10 =
                    header.ddspf.dwFourCC == DDSValues.uintDX10 ? new DDSHeaderDX10(br) : null;

                var textureData = GetTextureMetadata(header, header10);
                var texture = new Texture2D(
                    textureData.width,
                    textureData.height,
                    textureData.format,
                    textureData.mips,
                    textureData.linear
                );

                byte[] data = new byte[reader.Length - reader.Position];
                if (reader.Read(data, 0, data.Length) != data.Length)
                    throw new Exception("File was modified while it was being read");

                texture.LoadRawTextureData(data);
                texture.Apply(false, unreadable);

                return texture;
            }
        }

        static TextureMetadata GetTextureMetadata(DDSHeader header, DDSHeaderDX10 header10)
        {
            if (header.dwSize != 124)
                throw new Exception("DDS texture has an invalid header size");

            var metadata = new TextureMetadata()
            {
                height = (int)header.dwHeight,
                width = (int)header.dwWidth,
                mips = header.dwMipMapCount > 1,
            };

            metadata.format = GetTextureFormat(header, header10, out metadata.linear);
            return metadata;
        }

        static TextureFormat GetTextureFormat(
            DDSHeader header,
            DDSHeaderDX10 header10,
            out bool linear
        )
        {
            var fourCC = header.ddspf.dwFourCC;
            var dwFlags = header.ddspf.dwFlags;
            linear = false;

            if (fourCC == DDSValues.uintDXT1)
                return TextureFormat.DXT1;
            if (fourCC == DDSValues.uintDXT5)
                return TextureFormat.DXT5;
            if ((dwFlags & 0x40) != 0 && fourCC == 0) // DDPF_ALPHAPIXELS (standard L8)
            {
                linear = true;
                return TextureFormat.Alpha8;
            }
            if ((dwFlags & 0x20000) != 0 && fourCC == 0) // DDPF_FOURCC with no FourCC (alternate L8)
            {
                linear = true;
                if (header.ddspf.dwRGBBitCount == 16)
                    return TextureFormat.R16;
                return TextureFormat.R8;
            }
            if (fourCC == DDSValues.uintDX10)
            {
                linear = IsDxgiFormatLinear(header10.dxgiFormat);
                switch (header10.dxgiFormat)
                {
                    case DXGI_FORMAT.DXGI_FORMAT_A8_UNORM:
                        return TextureFormat.Alpha8;
                    case DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC4_SNORM:
                        return TextureFormat.BC4;
                    case DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM:
                        return TextureFormat.BC5;
                    case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM_SRGB:
                        return TextureFormat.BC7;
                    case DXGI_FORMAT.DXGI_FORMAT_R16_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_R16_SNORM:
                        return TextureFormat.R16;

                    default:
                        throw new Exception(
                            $"Unsupported DDS texture format {header10.dxgiFormat}"
                        );
                }
            }

            throw new Exception($"Unsupported DDS texture format");
        }

        static bool IsDxgiFormatLinear(DXGI_FORMAT format)
        {
            switch (format)
            {
                case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_D16_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R16_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_A8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R1_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8_B8G8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_G8R8_G8B8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_B5G6R5_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_B5G5R5A1_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_B4G4R4A4_UNORM:
                    return true;

                default:
                    return false;
            }
        }
    }
}
