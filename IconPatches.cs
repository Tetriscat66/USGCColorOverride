using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace USGCColorOverride {
	[Harmony]
	internal class IconPatches {
		// Fist icon colors. Includes support for whiplash icon even though it's unused in game 
		[HarmonyPatch(typeof(FistControl), nameof(FistControl.UpdateFistIcon))]
		[HarmonyPostfix]
		private static void PostUpdateFistIcon(FistControl __instance) {
			Color color = new Color(0, 0, 0, 0);
			switch(__instance.currentVarNum) {
				case 0:
					if(Plugin.ColorOverrides.TryGetValue("T_Feedbacker_Palette", out Texture2D value)) {
						color = value.GetPixelFromTopLeft((int)WeaponVariant.BlueVariant, Plugin.IconRow);
					}
					break;
				case 1:
					if(Plugin.ColorOverrides.TryGetValue("v2_armtex_Palette", out Texture2D value2)) {
						color = value2.GetPixelFromTopLeft((int)WeaponVariant.RedVariant, Plugin.IconRow);
					}
					break;
				case 2:
					if(Plugin.ColorOverrides.TryGetValue("T_GreenArm_Palette", out Texture2D value3)) {
						color = value3.GetPixelFromTopLeft((int)WeaponVariant.GreenVariant, Plugin.IconRow);
					}
					break;
				case 3:
					break;
			}
			if(color.a == 0)
				return;
			color.a = 1;
			__instance.fistIconColor = color;
		}

		// Weapon icon colors 
		[HarmonyPatch(typeof(WeaponHUD), nameof(WeaponHUD.UpdateImage))]
		[HarmonyPostfix]
		private static void PostUpdateImage(WeaponHUD __instance, Sprite icon, Sprite glowIcon, int variation) {
			string key = $"{UltraSkinsPatches.ComponentToKey(UnityEngine.Object.FindObjectOfType<WeaponIcon>())}_Palette";
			if(Plugin.ColorOverrides.TryGetValue(key, out Texture2D value)) {
				Color color = value.GetPixelFromTopLeft(variation, Plugin.IconRow);
				if(color.a == 0.0f)
					return;
				color.a = 1.0f;
				__instance.img.color = color;
				__instance.glowImg.color = color;
			}
		}

		// Does a bit more than just weapon icon colors... 
		[HarmonyPatch(typeof(WeaponIcon), nameof(WeaponIcon.UpdateIcon))]
		[HarmonyPostfix]
		private static void PostUpdateIcon(WeaponIcon __instance) {
			string key = $"{UltraSkinsPatches.ComponentToKey(__instance)}_Palette";
			if(Plugin.ColorOverrides.TryGetValue(key, out Texture2D value)) {
				bool flag1 = false;
				Color color = value.GetPixelFromTopLeft(__instance.variationColor, Plugin.EmissivesRow);
				if(color.a == 0.0f) {
					flag1 = true;
				} else {
					color.a = 1.0f;
				}
				bool flag2 = false;
				Color color2 = value.GetPixelFromTopLeft((int)WeaponVariant.GoldVariant, Plugin.ExtrasRow2);
				if(color2.a == 0.0f) {
					flag2 = true;
				} else {
					color2.a = 1.0f;
				}

				// this is where a lot of recoloring happens -- everything in vanilla that is based on variant color 
				MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
				foreach(Renderer obj in __instance.variationColoredRenderers) {
					//Plugin.Logger.LogInfo("obj.name: " + obj.name);
					bool flag = obj.name.Contains("Spear") ? flag2 : flag1;
					if(flag) // this mess is just to skip renderers if the correct color in the palette is transparent 
						continue;
					Color c = obj.name.Contains("Spear") ? color2.Clone() : color.Clone(); // color2 is specifically for the screwdriver drill 
					obj.GetPropertyBlock(materialPropertyBlock);
					if(obj.sharedMaterial.HasProperty("_EmissiveColor")) {
						materialPropertyBlock.SetColor("_EmissiveColor", c);
					} else {
						materialPropertyBlock.SetColor("_Color", c);
					}
					obj.SetPropertyBlock(materialPropertyBlock);
				}
				foreach(Image image in __instance.variationColoredImages) {
					//Plugin.Logger.LogInfo("image.name: " + image.name);
					image.color = color.Clone();
				}
			}
		}
	}
}
