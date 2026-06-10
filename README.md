# Low End Volumetric Clouds Mod

A performance-optimized, lightweight 3D true volumetric cloud engine designed specifically for Kerbal Space Program (KSP) running on low-end PCs, older dedicated GPUs, and integrated graphics processors (like Intel HD/UHD Graphics and AMD Radeon Vega iGPUs).

Now you can fly your rockets through deep cloud canyons, brave swirling weather cells, and dive into massive atmospheric air cavities without sacrificing your frame rates!

## 📉 Low-End Optimization Secrets
Standard true volumetric cloud mods run heavy 3D calculations that crush budget graphics cards. This mod maintains high frame rates using smart math tricks:
*   **Quarter-Resolution Rendering**: Clouds are rendered into a lightweight, downscaled buffer (1/4 of your native resolution) and seamlessly composited back onto your screen, cutting GPU fill-rate demands by up to 85%.
*   **Ultra-Low Sample Bounds**: Raymarching step paths are strictly capped at 12 layers (compared to the standard 64+ layers used by high-end mods).
*   **Early-Exit Cavity Performance Gateways**: The raymarching engine reads a static 2D Weather Map before computing pixels. When your ship hovers over a deep cloud canyon or eye of a storm, the GPU skips execution entirely, dropping rendering costs in empty spaces to absolute zero.

## 🪐 Supported Planetary Bodies
This mod features a dynamic, planet-aware engine loop that reads your vessel’s location and instantly adapts its weather profiles at runtime:
*   **Kerbin**: Crisp, fluffy white-and-grey atmospheric storm decks with dramatic vertical canyon walls.
*   **Jool**: Thick, cinematic, toxic green gas-giant cloud decks layered with sweeping rotational cyclone storm walls.
*   **Atmosphere-less Bodies (Mun, Minmus, etc.)**: The rendering pipeline automatically deactivates to preserve 100% of your hardware performance.

## ⚙️ How to Install
This mod is packaged natively to be managed and routed flawlessly by **CKAN**.
1. Open your **CKAN** client application.
2. Search for `Low End Volumetric Clouds Mod` in the global index.
3. Check the box and click **Apply Changes**. CKAN will handle the path allocations and file routing automatically for both Steam and portable standalone installs!

## 📜 License & Open Source
This project is completely open-source and distributed under the **GNU GPLv3** license. You are free to inspect our low-end optimization techniques, share the mod, or modify the source code, provided that all derivative works remain entirely open-source under the exact same license terms.
