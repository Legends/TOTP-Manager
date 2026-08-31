# Project asset provenance

This record covers every raster image embedded by `TOTP.UI.Avalonia.Desktop`. The files are project-owned outputs and are distributed under the repository's [MIT license](../../LICENSE.txt). No downloaded raster artwork is included.

## Application icon

The application icon was generated specifically for TOTP Manager on 2026-08-29 using OpenAI's built-in image-generation tool at the maintainer's request. It was generated without an input or reference image. [`Generate-AppIcon.ps1`](../../scripts/assets/Generate-AppIcon.ps1) resizes a supplied master with high-quality bicubic interpolation and packages independently resized PNG frames at 16, 24, 32, 48, 64, 128, and 256 pixels in the Windows ICO. The production outputs are retained below; discarded candidates remain available in Git history.

Final generation prompt:

> Use case: logo-brand. Asset type: square desktop application icon candidate. Create an original minimal symbol for a secure TOTP authenticator, combining a circular six-segment one-time-code ring with a small shield cutout. Use a crisp, flat, vector-like, geometric, modern native desktop software identity; a single centered mark; a strong silhouette; generous padding; and forms readable at 16x16 pixels. Use deep navy `#0C1C33`, violet `#7D7FF4`, a restrained bright cyan accent, and white only for small negative-space separation. Use a genuinely transparent background. Include no text, letters, digits, watermark, mockup, rounded-square container, 3D rendering, photorealism, padlock cliché, excessive detail, or thin fragile lines.

The generated image was visually reviewed for prohibited text, third-party branding, and small-size legibility before conversion.

## Language flags

`en.png` and `de.png` were rendered inside the project workspace on 2026-08-29 from geometric primitives. They do not derive from downloaded image files:

- `en.png` is a project-rendered representation of the public-domain United States flag design, used for the English locale.
- `de.png` is a project-rendered representation of the public-domain German federal flag design, used for the German locale.

## Reviewed file hashes

| File | SHA-256 |
| --- | --- |
| `TOTP.UI.Avalonia.Desktop/Assets/Icons/app-1024.png` | `66748954507b3f9f9cff87dc23c97134c1d7d029e8275de179b9f3872f2d12b4` |
| `TOTP.UI.Avalonia.Desktop/Assets/Icons/app-128.png` | `26fe7fe9a91c7f2e939c7d794cbade4d1e22090ef3c40a59b8ae9ffb3c9aaf88` |
| `TOTP.UI.Avalonia.Desktop/Assets/Icons/app.ico` | `7a71a423982499c438177e3b58126f003c3ece9a66cb2b91c07dc50a812ab81e` |
| `TOTP.UI.Avalonia.Desktop/Assets/flags/en.png` | `1c2bcc20e5985e5f03a3a440f198b5d08a4ac609e9cebba00b639b0e50fba8fc` |
| `TOTP.UI.Avalonia.Desktop/Assets/flags/de.png` | `2c8f253f3401d18df0a47bd7906102cf78ea7e4a2caac9e4c6f4efebc906de0a` |

Any replacement requires a new provenance record, license review, updated hashes, and visual/build validation.
