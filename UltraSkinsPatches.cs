using System;
using HarmonyLib;
using Sandbox.Arm;
using ULTRAKILL.Cheats;
using UltraSkins;
using UnityEngine;
using UnityEngine.UI;

namespace USGCColorOverride {
	internal struct Size {
		public int width;
		public int height;

		public Size(int w, int h) {
			width = w;
			height = h;
		}
	}

	[Harmony]
	internal class UltraSkinsPatches {
		// Supported texture names 
		private static string[] ColorOverrideStrings = new string[] {
			"T_PistolNew",
			"T_MinosRevolver",
			"T_ShotgunNew",
			"T_Chainsaw",
			"T_ImpactHammer",
			"T_NailgunNew",
			"T_SawbladeLauncher",
			"T_Railcannon",
			"T_RocketLauncher",
			"T_MainArm",
			"T_Feedbacker",
			"v2_armtex",
			"T_GreenArm",
			"T_Washer",
			"T_Vacuum",
			"T_MainArm_White"
		};

		// for whiplash 
		private static Color lineRendererColor = new Color(0.251f, 0.251f, 0.251f, 1.0f);

		// for _Palette sprites 
		private static Size expectedSize = new Size(4, 5);

		[HarmonyPatch(typeof(ULTRASKINHand), nameof(ULTRASKINHand.LoadTextures))]
		[HarmonyPostfix]
		private static void PostLoadTextures(ref string __result) {
			if(__result != "Success")
				return;
			// setup color overrides -- fill it with the _Palette textures
			Plugin.ColorOverrides.Clear();
			foreach(string str in ColorOverrideStrings) {
				string key = $"{str}_Palette";
				if(ULTRASKINHand.autoSwapCache.TryGetValue(key, out Texture value)) {
					bool canLoad = value.width == expectedSize.width && value.height == expectedSize.height;
					if(canLoad) {
						Plugin.ColorOverrides.TryAdd(key, value as Texture2D);
					} else {
						Plugin.Logger.LogWarning($"Failed to load palette {key}.png. Texture size {value.width}x{value.height} does not match expected size {expectedSize.width}x{expectedSize.height}");
					}
				}
			}
		}

		// Override the emissive variant colors that are changable by TextureOverWatch 
		[HarmonyPatch(typeof(ULTRASKINHand), nameof(ULTRASKINHand.GetVarationColor))]
		[HarmonyPostfix]
		private static void PostGetVariationColor(ref Color __result, TextureOverWatch TOW) {
			Color color = __result;
			bool updateEmissives = false;
			bool updateFistIcon = false;
			//Plugin.Logger.LogInfo("tow for: " + TOW.gameObject.name);
			if(TOW.gameObject.name == "RightArm") { // Right arm 
				WeaponIcon WPI = TOW.GetComponentInParent<WeaponIcon>();
				int variationColor = WPI.variationColor;
				string key = ComponentToKey(WPI, ref variationColor);
				int y =
					(key == "T_MinosRevolver" || key == "T_PistolNew") ? Plugin.EmissivesRow :
					(key == "T_Washer" ? Plugin.ExtrasRow : Plugin.ExtrasRow2);
				//Plugin.Logger.LogInfo(key);
				if(Plugin.ColorOverrides.TryGetValue("T_MainArm_Palette", out Texture2D value)) {
					color = value.GetPixelFromTopLeft(WPI.variationColor, y);
					updateEmissives = true;
				}
			} else if(TOW.GetComponentInParent<WeaponIcon>()) { // Weapons 
				WeaponIcon WPI = TOW.GetComponentInParent<WeaponIcon>();
				int variationColor = WPI.variationColor;
				string key = $"{ComponentToKey(WPI, ref variationColor)}_Palette";
				if(Plugin.ColorOverrides.TryGetValue(key, out Texture2D value)) {
					color = value.GetPixelFromTopLeft(variationColor, Plugin.EmissivesRow);
					updateEmissives = true;
				}
			} else if(TOW.GetComponentInParent<Punch>()) { // Punching arms 
				Punch P = TOW.GetComponentInParent<Punch>();
				switch(P.type) {
					case FistType.Standard:
						if(Plugin.ColorOverrides.TryGetValue("T_Feedbacker_Palette", out Texture2D value)) {
							color = value.GetPixelFromTopLeft((int)WeaponVariant.BlueVariant, Plugin.EmissivesRow);
						}
						break;
					case FistType.Heavy:
						if(Plugin.ColorOverrides.TryGetValue("v2_armtex_Palette", out Texture2D value2)) {
							color = value2.GetPixelFromTopLeft((int)WeaponVariant.RedVariant, Plugin.EmissivesRow);
						}
						break;
				}
				updateFistIcon = true;
			} else if(TOW.GetComponentInParent<HookArm>()) { // Whiplash 
				HookArm H = TOW.GetComponentInParent<HookArm>();
				LineRenderer lineRenderer = H.GetComponentInParent<LineRenderer>();
				if(Plugin.ColorOverrides.TryGetValue("T_GreenArm_Palette", out Texture2D value)) {
					color = value.GetPixelFromTopLeft((int)WeaponVariant.GreenVariant, Plugin.EmissivesRow);
					Color temp = Color.black;
					SetIfOpaque(ref temp, value.GetPixelFromTopLeft((int)WeaponVariant.GreenVariant, Plugin.ExtrasRow), 1.0f, lineRendererColor);
					lineRenderer.startColor = temp;
					SetIfOpaque(ref temp, value.GetPixelFromTopLeft((int)WeaponVariant.GreenVariant, Plugin.ExtrasRow2), 1.0f, lineRendererColor);
					lineRenderer.endColor = temp;
				} else {
					lineRenderer.startColor = lineRendererColor.Clone();
					lineRenderer.endColor = lineRendererColor.Clone();
				}
			}
			SetIfOpaque(ref __result, color, 1.0f);
			if(updateEmissives) {
				if(TOW.TryGetComponentInParent(out WeaponIcon WPI)) {
					WPI.UpdateIcon();
				}
			} else if(updateFistIcon) {
				MonoSingleton<FistControl>.Instance.UpdateFistIcon();
			}
		}

		// Simpler ComponentToKey where variationColor is not needed 
		public static string ComponentToKey(WeaponIcon WPI) {
			int useless = 0;
			return ComponentToKey(WPI, ref useless);
		}

		// Get the texture key name from weapon components 
		public static string ComponentToKey(MonoBehaviour WPI, ref int variationColor) {
			if(WPI.TryGetComponentInParent(out Revolver rev)) {
				if(variationColor == 0) {
					variationColor = 1;
				} else if(variationColor == 1) {
					variationColor = 0;
				}
				return rev.altVersion ? "T_MinosRevolver" : "T_PistolNew";
			} else if(WPI.TryGetComponentInParent(out Shotgun _)) {
				return "T_ShotgunNew";
			} else if(WPI.TryGetComponentInParent(out ShotgunHammer _)) {
				return "T_ImpactHammer";
			} else if(WPI.TryGetComponentInParent(out Nailgun nail)) {
				return nail.altVersion ? "T_SawbladeLauncher" : "T_NailgunNew";
			} else if(WPI.TryGetComponentInParent(out Railcannon _)) {
				return "T_Railcannon";
			} else if(WPI.TryGetComponentInParent(out RocketLauncher _)) {
				return "T_RocketLauncher";
			} else if(WPI.TryGetComponentInParent(out Washer _) || (WPI.TryGetComponentInParent(out WeaponIdentifier wid) && wid.name.Contains("Washer"))) {
				return "T_Washer";
			} else if(WPI.TryGetComponentInParent(out Vacuum _) || (WPI.TryGetComponentInParent(out WeaponIdentifier wid2) && wid2.name.Contains("Vacuum"))) {
				return "T_Vacuum";
			} else if(WPI.TryGetComponentInParent(out SandboxArm _)) {
				return "T_MainArm_White";
			}
			return string.Empty;
		}

		public static void SetIfOpaque(ref Color self, Color test, float newOpacity = -1.0f, Color? other = null, float newOpacity2 = -1.0f) {
			if(test.a > 0) {
				Color temp = test.Clone();
				if(newOpacity != -1.0f) {
					temp.a = newOpacity;
				}
				self = temp;
			} else if(other != null) {
				Color temp = ((Color)other).Clone();
				if(newOpacity2 != -1.0f) {
					temp.a = newOpacity2;
				}
				self = temp;
			}
		}
	}
}
