# UltraSkins GC Custom Palettes

An addon mod for UltraSkins GC, allowing you to add custom color palettes for individual weapon skins. These let you do things like use a different emissive color or weapon icon color for each variant for a given weapon skin.

## About palettes: 

Palettes are 4 pixels wide by 5 pixels tall `.png` files that are used in order to change certain colors of weapons and arms. Pixels in a palette will affect different things depending on their placement. Refer to the visual aides provided in the `Resources` section for where to place each pixel color. 

## Creating a palette: 

To create a palette, simply add a 4x5 pixel `.png` file to your skin, named with the original skin's texture name, followed by `_Palette`; an identical naming scheme to the existing `_ID` and `_Emissive` sprites. The following palettes are supported: 
```
- T_PistolNew_Palette.png
- T_MinosRevolver_Palette.png
- T_ShotgunNew_Palette.png
- T_ImpactHammer_Palette.png
- T_Chainsaw_Palette_.png
- T_NailgunNew_Palette.png
- T_SawbladeLauncher_Palette.png
- T_Railcannon_Palette.png
- T_RocketLauncher_Palette.png
- T_MainArm_Palette.png
- T_Feedbacker_Palette.png
- v2_armtex_Palette.png
- T_GreenArm_Palette.png
- T_Washer_Palette.png
- T_Vacuum_Palette.png
- T_MainArm_White_Palette.png
```



If the weapon has an `_ID` color for custom color palettes, these `_Palette` files will apply to them too. 

The general trend for pixel placement that palettes follow is using the top row for emissive colors, the bottom row for weapon icon colors, and rows in between for various other nescessary colors. Each column tends to be dedicated to one variant color (blue, green, red, and gold, in that order), though many skins will use the gold variant column as needed for additional extra colors. 

Pixels in a palette will always be treated as either fully transparent or fully opaque. Any translucent pixels are treated as and forced to be opaque. Empty pixels will cause the mod to not overwrite anything affected by the pixel's color, letting you specify that the vanilla color should be used instead.

## Credits/Thanks 

Thank you Cintra_ ([CintraSkins](https://thunderstore.io/c/ultrakill/p/CintraSkins/) on Thunderstore) for your suggestions, proofreading and editing of the readme, and for beta testing this mod. 

## Reporting Bugs 

If you find any bugs, you can report them to me either through making an issue on the [GitHub](https://github.com/Tetriscat66/USGCColorOverride/) repository for this mod or through the New Blood Discord server in `#ultrakill-modding`, the Ultrakill Discord server in `#ultramodding`, or BobTheCorn's Discord server in `#modding-chat` (the Discord server linked to by UltraSkins GC). Be sure to ping me (`@syla_tetriscat66_0469`) if you're using Discord so that I see your message. 

## Resources 

The Palette guide maps for each supported skin are attached below. They can also be found and downloaded [here](https://github.com/Tetriscat66/SylasThunderstoreImages/tree/86fa5f3235e14982e33d9245129389f2851c2ff2/USGCColorOverride/Palette%20Guide%20Maps).

`T_PistolNew_Palette.png` and `T_MinosRevolver_Palette.png`

![](https://github.com/Tetriscat66/SylasThunderstoreImages/blob/86fa5f3235e14982e33d9245129389f2851c2ff2/USGCColorOverride/Palette%20Guide%20Maps/Revolvers%20Palette%20Map.png?raw=true)

<br>

`T_ShotgunNew_Palette.png`

![](https://github.com/Tetriscat66/SylasThunderstoreImages/blob/86fa5f3235e14982e33d9245129389f2851c2ff2/USGCColorOverride/Palette%20Guide%20Maps/Shotgun%20Palette%20Map.png?raw=true)

<br>

`T_ImpactHammer_Palette.png`

![](https://github.com/Tetriscat66/SylasThunderstoreImages/blob/86fa5f3235e14982e33d9245129389f2851c2ff2/USGCColorOverride/Palette%20Guide%20Maps/Impact%20Hammer%20Palette%20Map.png?raw=true)

<br>

`T_Chainsaw_Palette_.png`

![](https://github.com/Tetriscat66/SylasThunderstoreImages/blob/86fa5f3235e14982e33d9245129389f2851c2ff2/USGCColorOverride/Palette%20Guide%20Maps/Chainsaw%20Palette%20Map.png?raw=true)

<br>

`T_NailgunNew_Palette.png` and `T_SawbladeLauncher_Palette.png`

![](https://github.com/Tetriscat66/SylasThunderstoreImages/blob/86fa5f3235e14982e33d9245129389f2851c2ff2/USGCColorOverride/Palette%20Guide%20Maps/Nailgun%20Sawblade%20Launcher%20Palette%20Map.png?raw=true)

<br>

`T_Railcannon_Palette.png`

![](https://github.com/Tetriscat66/SylasThunderstoreImages/blob/86fa5f3235e14982e33d9245129389f2851c2ff2/USGCColorOverride/Palette%20Guide%20Maps/Railcannon%20Palette%20Map.png?raw=true)

<br>

`T_RocketLauncher_Palette.png`

![](https://github.com/Tetriscat66/SylasThunderstoreImages/blob/86fa5f3235e14982e33d9245129389f2851c2ff2/USGCColorOverride/Palette%20Guide%20Maps/Rocket%20Launcher%20Palette%20Map.png?raw=true)

<br>

`T_MainArm_Palette.png`

![](https://github.com/Tetriscat66/SylasThunderstoreImages/blob/86fa5f3235e14982e33d9245129389f2851c2ff2/USGCColorOverride/Palette%20Guide%20Maps/Right%20Arm%20Palette%20Map.png?raw=true)

<br>

`T_Feedbacker_Palette.png`

![](https://github.com/Tetriscat66/SylasThunderstoreImages/blob/86fa5f3235e14982e33d9245129389f2851c2ff2/USGCColorOverride/Palette%20Guide%20Maps/Feedbacker%20Palette%20Map.png?raw=true)

<br>

`v2_armtex_Palette.png`

![](https://github.com/Tetriscat66/SylasThunderstoreImages/blob/86fa5f3235e14982e33d9245129389f2851c2ff2/USGCColorOverride/Palette%20Guide%20Maps/Knuckleblaster%20Palette%20Map.png?raw=true)

<br>

`T_GreenArm_Palette.png`

![](https://github.com/Tetriscat66/SylasThunderstoreImages/blob/86fa5f3235e14982e33d9245129389f2851c2ff2/USGCColorOverride/Palette%20Guide%20Maps/Whiplash%20Palette%20Map.png?raw=true)

<br>

`T_Washer_Palette.png`

![](https://github.com/Tetriscat66/SylasThunderstoreImages/blob/86fa5f3235e14982e33d9245129389f2851c2ff2/USGCColorOverride/Palette%20Guide%20Maps/Washer%20Palette%20Map.png?raw=true)

<br>

`T_Vacuum_Palette.png`

![](https://github.com/Tetriscat66/SylasThunderstoreImages/blob/86fa5f3235e14982e33d9245129389f2851c2ff2/USGCColorOverride/Palette%20Guide%20Maps/Vacuum%20Palette%20Map.png?raw=true)

<br>

`T_MainArm_White_Palette.png`

![](https://github.com/Tetriscat66/SylasThunderstoreImages/blob/86fa5f3235e14982e33d9245129389f2851c2ff2/USGCColorOverride/Palette%20Guide%20Maps/Spawner%20Arm%20Palette%20Map.png?raw=true)
