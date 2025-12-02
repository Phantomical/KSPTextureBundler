# KSP Texture Bundler
This is a package for the unity editor that is meant to allow you to easily
create asset bundles that are compatible with Parallax Continued and also
(in the future) other graphics mods in the KSP ecosystem.

## Installation
To install the KSP texture bundler:
* Open the Unity Package Manager in your Project (menu: Window > Package Manager)
* Click the + (Add) button at the top-left corner of the window.
* Chose Add package from git URL...
* Enter <https://github.com/Phantomical/KSPTextureBundler.git> as the URL.
* Click Add. The package manager downloads and installs the package's "main" branch.

Once installed it will add:
* A new entry in the project settings (KSP Texture Bundler > Asset Bundle Settings)
* A new menu entry (menu: KSP Texture Bundler > Build Asset Bundles)

## Getting Started
### Configuration
Before doing anything else you should open up the project settings window
(menu: edit > Project Settings) and go to the Asset Bundle Settings screen.

![the settings screen](./docs/settings.png)

Create asset bundle entries for all of the asset bundles you intend to
create for your mod. The entries you need to fill out are:
* **Input Path** - A directory containing assets to include in your asset
  bundle. The names of the assets within the bundle will be relative to
  this directory.

  If your assets are at `Assets/MyPlanetTextures` then that's what you should
  enter here.

* **Output Name** - The path to place the resulting bundle within the
  configured output directory (the Output Path option at the top of the
  settings).

  As an example, `my-planet-textures.unity3d` will result the asset bundle being
  created at `<output dir>/my-planet-textures.unity3d`. Unity will always convert
  the path you enter here to lowercase, but the asset building script will
  attempt to create a directory with the appropriate casing first.

* **Asset Name Prefix** - A directory prefix to prepend to the names of all the
  assets within the bundle. This is useful if you're trying to use the same
  asset names both inside and outside of the bundle.

  If your planet texture is at `Assets/MyPlanetTextures/planet.dds` and you
  want the final path in the asset bundle to be `MyPlanetTextures/planet.dds`
  then you should set this to `MyPlanetTextures`. The final path is constructed
  as `<prefix>/<path within input dir>`.

* **Exclude** - Files to exclude from the bundle. This should be in the form
  of file name globs, not paths.

> ![INFO]
>
> Opening the settings window will create a settings asset at
> `KSP-Texture-Bundler/Settings.asset`. This file is YAML internally, so if you
> would rather edit the settings using a text editor then you can just open up
> the file directly.

### Importing & Building
Once you have your settings set up the way you want them you should import your
texture assets. If they are already in the project then you will need to reimport
them. _This will take a while_. If you are mostly dealing with 4k textures expect
a couple minutes to import everything, for 16k textures this will take longer.

After you have everything imported the way you want, you can build the asset
bundles by hitting (menu: KSP Texture Bundler > Build Asset Bundles). The bundles
will be placed under the directory you configured in the settings. By default this
is `AssetBundles`.

## Common Warnings and Errors
* If you are importing L8 or R16 dds textures then you will see
  ![unsupported DDS file](./docs/unsupported-dds-file.png)

  in the output logs. This is harmless, unity doesn't know how to import these
  but this package manually imports them.

* DDS textures whose width/height are not multiples of 4 seem to cause errors
  when they are imported into KSP (with DX11, I don't know about with other
  APIs). The postprocessing step will automatically resize any 1x1 textures to
  4x4 but larger textures should be excluded from the asset bundle.

## Limitations
* Unity 2019.4 cannot create asset bundles that are larger than 4GB in size when
  uncompressed. If your textures are larger than 4GB total then you will need to
  split them up into multiple asset bundles.
* Unity 2019.4 does not allow you export a source texture (i.e. anything but DDS)
  as larger than 8192x8192. If you want to use 16k textures then you will need to
  convert them to dds textures outside of unity and import the dds textures.
* There is no support for bundling cubemaps directly at this time. You should
  continue to use parallax's format for cubemaps packed into a single 2D texture.

## Best Practices
* When textures are imported they will use the import settings as defined in the
  unity editor. Make sure that your import settings match what you want.

  By default, parallax imports textures like so:
  * All textures have `isReadable` set to false (except cubemaps, see below)
  * Bump, displacement, influence, wind, and height textures are linear (i.e.
    have `sRGB` set to false).

  These are a good starting point for your own configuration.

  Cubemaps must have `isReadable` set to true so that parallax can read the
  pixels from them to build the actual cubemap.

* Ideally you should have one asset bundle for your entire mod. This will avoid
  situations where you need to keep some assets as loose textures because it is
  needed for multiple asset bundles.

* If you are using DXT1 and DXT5 textures then you can use crunch compression
  to reduce their size on disk. For non-dds textures, you can set this directly
  in the import settings. For dds textures you can set the "Crunch Compression"
  option in the settings.

  Be aware that this will make imports quite a bit slower. I have also seen
  issues where crunch compression doesn't work properly, which is why it is
  disabled by default.

## Texture Flipping
When you export loose textures for import into parallax you want then to be
oriented so that north is down. However, when you import those textures via
an asset bundle some, but not all, of them need to be flipped so that they
are oriented north-up.

When texture flipping is enabled then KSP Texture Bundler will flip all imported
textures vertically except in the following cases:
* BC7 textures are not flipped
* textures that unity doesn't know how to parse (L8/R16) are also not flipped.

This is intended to make it so that textures end up being identical to those in
parallax when they are imported.

If have a custom build script generating your release textures and want to do
the flipping yourself outside of unity then you will need to make sure that
you are following the same flipping conventions.

