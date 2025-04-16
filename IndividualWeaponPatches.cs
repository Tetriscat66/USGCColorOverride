using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.UI;
using static Mono.Xml.MiniParser;

namespace USGCColorOverride {
	[Harmony]
	internal class IndividualWeaponPatches {
		// all static fields are for passing information between prefix and postfix methods 
		#region Revolver

		private static bool RevPierceReady = true;
		private static float RevPierceCharge = 100f;
		private static int RevPierceColor = 0;

		// Piercer charge orb/Sharpshooter screen changes 
		[HarmonyPatch(typeof(Revolver), nameof(Revolver.OnEnable))]
		[HarmonyPostfix]
		private static void RevPostEnable(Revolver __instance) {
			if(!(__instance.gunVariation == 0 || __instance.gunVariation == 2))
				return;
			string key = __instance.altVersion ? "MinosRevolver" : "PistolNew";
			if(Plugin.ColorOverrides.TryGetValue($"T_{key}_Palette", out Texture2D value)) {
				bool flag1 = true;
				Color color = value.GetPixelFromTopLeft((int)WeaponVariant.GoldVariant, (__instance.gunVariation == 0) ? Plugin.EmissivesRow : Plugin.ExtrasRow);
				if(color.a == 0) {
					flag1 = false;
				} else {
					color.a = 1;
				}
				bool flag2 = true;
				Color color2 = value.GetPixelFromTopLeft((int)WeaponVariant.GoldVariant, Plugin.ExtrasRow2);
				if(color2.a == 0) {
					flag2 = false;
				} else {
					color2.a = 1;
				}
				Transform temp = __instance.transform.Find($"Revolver_Rerigged_{(__instance.altVersion ? "Alternate" : "Standard")}");
				if((bool)temp) {
					temp = temp.Find("Armature");
					if((bool)temp) {
						temp = temp.Find("Upper Arm");
						if((bool)temp) {
							temp = temp.Find("Forearm");
							if((bool)temp) {
								temp = temp.Find("Hand");
								if((bool)temp) {
									temp = temp.Find("Revolver_Bone");
									if((bool)temp) {
										if(__instance.gunVariation == 0 && flag1) { // Piercer 
											temp = temp.Find($"ShootPoint{(__instance.altVersion ? " (1)" : string.Empty)}"); // slab has " (1)" in this name for some reason 
											if((bool)temp) {
												temp = temp.Find("ChargeEffect");
												if((bool)temp) {
													Texture texture = CreateColoredChargeOrb(color, Plugin.t_charge, Plugin.t_charge_ID);
													if(temp.gameObject.TryGetComponent(out MeshRenderer renderer)) {
														renderer.material.mainTexture = texture;
													}
													if(temp.gameObject.TryGetComponent(out ParticleSystemRenderer renderer2)) {
														renderer2.material.mainTexture = texture;
													}
													if(temp.gameObject.TryGetComponent(out Light light)) {
														light.color = color;
													}
												}
											}
										} else { // Sharpshooter 
											temp = temp.Find("Canvas");
											if((bool)temp) {
												temp = temp.Find("Panel");
												if((bool)temp) {
													if(flag1) {
														for(int i = 0; i < temp.childCount; i++) {
															GameObject ch = temp.GetChild(i).gameObject;
															if(ch.name.Contains("Background") && ch.TryGetComponent(out Image img)) {
																img.color = color;
															}
														}
													}
													if(temp.TryGetComponent(out Image img2) && flag2) {
														img2.color = color2;
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// get what the Piercer screen color should be 
		[HarmonyPatch(typeof(Revolver), nameof(Revolver.Update))]
		[HarmonyPrefix]
		private static void RevPreUpdate(Revolver __instance) {
			RevPierceReady = __instance.pierceReady;
			RevPierceCharge = __instance.pierceCharge;
			RevPierceColor = 0;
			if(!__instance.pierceReady) {
				if(__instance.gunVariation == 0) {
					if(NoWeaponCooldown.NoCooldown) {
						RevPierceCharge = 100f;
					}
					float num = 1f;
					if(__instance.altVersion) {
						num = 0.5f;
					}
					if(RevPierceCharge + 40f * Time.deltaTime < 100f) {
						RevPierceCharge += 40f * Time.deltaTime * num;
					} else {
						RevPierceCharge = 100f;
						RevPierceReady = true;
					}
					if(RevPierceCharge < 50f) {
						RevPierceColor = 2;
					} else if(RevPierceCharge < 100f) {
						RevPierceColor = 1;
					}
				} else if(RevPierceCharge + 480f * Time.deltaTime < 100f) {
					RevPierceCharge += 480f * Time.deltaTime;
				} else {
					RevPierceCharge = 100f;
					RevPierceReady = true;
				}
			}
		}

		// set the Piercer screen color 
		[HarmonyPatch(typeof(Revolver), nameof(Revolver.Update))]
		[HarmonyPostfix]
		private static void RevPostUpdate(Revolver __instance) {
			string key = __instance.altVersion ? "MinosRevolver" : "PistolNew";
			if(Plugin.ColorOverrides.TryGetValue($"T_{key}_Palette", out Texture2D value2)) {
				if(__instance.gunVariation == 0) {
					int y = Plugin.ExtrasRow;
					switch(RevPierceColor) {
						case 1:
							y = Plugin.ExtrasRow2;
							break;
						case 2:
							y = Plugin.ExtrasRow3;
							break;
					}
					Color color = value2.GetPixelFromTopLeft(0, y);
					if(color.a != 0) {
						color.a = 1;
						__instance.screenProps.SetColor("_Color", color);
					}
					__instance.screenMR.SetPropertyBlock(__instance.screenProps);
				}
			}
		}

		// Marksman coin colors and Sharpshooter ricoshot charge colors (both are coinCharges) 
		private static int[] RevCoinColors;
		[HarmonyPatch(typeof(Revolver), nameof(Revolver.CheckCoinCharges))]
		[HarmonyPrefix]
		private static void RevPreCheckCoinCharges(Revolver __instance) {
			if(__instance.coinPanelsCharged == null || __instance.coinPanelsCharged.Length == 0) {
				__instance.coinPanelsCharged = new bool[__instance.coinPanels.Length];
			}
			RevCoinColors = new int[__instance.coinPanels.Length];
			float coinCharge = ((__instance.gunVariation == 1) ? __instance.wc.rev1charge : __instance.wc.rev2charge);
			for(int i = 0; i < __instance.coinPanels.Length; i++) {
				float fillAmount = 0;
				if(__instance.altVersion && __instance.gunVariation == 2) {
					fillAmount = coinCharge / 300f;
				} else {
					fillAmount = coinCharge / 100f - i;
				}
				if(fillAmount < 1f) {
					RevCoinColors[i] = Plugin.ExtrasRow2;
					continue;
				}
				RevCoinColors[i] = Plugin.ExtrasRow;
			}
		}

		// set coin panel colors 
		[HarmonyPatch(typeof(Revolver), nameof(Revolver.CheckCoinCharges))]
		[HarmonyPostfix]
		private static void RevPostCheckCoinCharges(Revolver __instance) {
			string key = __instance.altVersion ? "MinosRevolver" : "PistolNew";
			if(Plugin.ColorOverrides.TryGetValue($"T_{key}_Palette", out Texture2D value2)) {
				for(int i = 0; i < __instance.coinPanels.Length; i++) {
					Color color = value2.GetPixelFromTopLeft(__instance.gunVariation, RevCoinColors[i]);
					if(color.a == 0)
						continue;
					color.a = 1;
					__instance.coinPanels[i].color = color;
				}
			}
		}

		#endregion

		#region Shotgun

		private static int ShotColor = 0;
		private static float ShotLerp = 0;

		// get what the meter color should be 
		[HarmonyPatch(typeof(Shotgun), nameof(Shotgun.UpdateMeter))]
		[HarmonyPrefix]
		private static void PreShotUpdateMeter(Shotgun __instance) {
			if(__instance.variation == 1) {
				float timeToBeep = __instance.timeToBeep;
				if(timeToBeep != 0f) {
					timeToBeep = Mathf.MoveTowards(timeToBeep, 0f, Time.deltaTime * 5f);
				}
				if(__instance.primaryCharge == 3) {
					if(timeToBeep == 0f) {
						ShotColor = 0;
					} else if(timeToBeep < 0.5f) {
						ShotColor = 1;
					}
				} else {
					ShotColor = 4 - __instance.primaryCharge;
				}
			} else if(!__instance.meterOverride) {
				if(__instance.grenadeForce > 0f) {
					ShotColor = 5;
					ShotLerp = __instance.grenadeForce / 60f;
				} else if(__instance.variation == 0) {
					ShotColor = 6;
				} else {
					if(MonoSingleton<WeaponCharges>.Instance.shoSawCharge == 1f) {
						ShotColor = 7;
					} else {
						ShotColor = 8;
					}
				}
			}
		}

		// ...and then set it 
		[HarmonyPatch(typeof(Shotgun), nameof(Shotgun.UpdateMeter))]
		[HarmonyPostfix]
		private static void PostShotUpdateMeter(Shotgun __instance) {
			if(Plugin.ColorOverrides.TryGetValue("T_ShotgunNew_Palette", out Texture2D value)) {
				Color color = Color.black;
				switch(ShotColor) {
					case 0:
						color = value.GetPixelFromTopLeft((int)WeaponVariant.GoldVariant, Plugin.ExtrasRow);
						break;
					case 1:
						color = value.GetPixelFromTopLeft((int)WeaponVariant.GoldVariant, Plugin.ExtrasRow2);
						break;
					case 2:
						color = value.GetPixelFromTopLeft((int)WeaponVariant.GreenVariant, Plugin.ExtrasRow3);
						break;
					case 3:
						color = value.GetPixelFromTopLeft((int)WeaponVariant.GreenVariant, Plugin.ExtrasRow2);
						break;
					case 4:
						color = value.GetPixelFromTopLeft((int)WeaponVariant.GreenVariant, Plugin.ExtrasRow);
						break;
					case 5:
						color = value.GetPixelFromTopLeft(__instance.variation, Plugin.ExtrasRow);
						Color color2 = value.GetPixelFromTopLeft(__instance.variation, Plugin.ExtrasRow3);
						if(color.a == 0) {
							color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[__instance.variation];
						}
						color.a = 1;
						if(color2.a == 0) {
							color2 = new Color(1f, 0.25f, 0.25f);
						}
						color2.a = 1;
						__instance.sliderFill.color = Color.Lerp(color, color2, ShotLerp);
						return;
					case 6:
						color = value.GetPixelFromTopLeft((int)WeaponVariant.BlueVariant, Plugin.ExtrasRow);
						break;
					case 7:
						color = value.GetPixelFromTopLeft((int)WeaponVariant.RedVariant, Plugin.ExtrasRow);
						break;
					case 8:
						color = value.GetPixelFromTopLeft((int)WeaponVariant.RedVariant, Plugin.ExtrasRow2);
						break;
				}
				if(color.a == 0) {
					return;
				}
				color.a = 1;
				__instance.sliderFill.color = color;
			}
		}

		// Chainsaw stuff 
		[HarmonyPatch(typeof(ColorBlindGet), nameof(ColorBlindGet.UpdateColor))]
		[HarmonyPostfix]
		private static void PostCBGUpdateColor(ColorBlindGet __instance) {
			if(__instance.name.Contains("Chainsaw") && Plugin.ColorOverrides.TryGetValue($"T_Chainsaw_Palette", out Texture2D value)) {
				int y = Plugin.ExtrasRow; // attached 
				TrailRenderer renderer = null;
				Light light = null;
				if((bool)__instance.transform.parent) {
					if(__instance.transform.parent.TryGetComponent(out Nail nail)) { // detached 
						y = Plugin.ExtrasRow3;
						if(nail.TryGetComponentInParent(out TrailRenderer rend)) {
							renderer = rend;
						}
						Transform temp = nail.transform.Find("Point Light (1)");
						if((bool)temp && temp.gameObject.TryGetComponent(out Light lit)) {
							light = lit;
						}
					} else if(__instance.transform.parent.TryGetComponent(out Chainsaw saw)) { // chained 
						y = Plugin.ExtrasRow2;
						if(saw.TryGetComponentInParent(out TrailRenderer rend)) {
							renderer = rend;
						}
					}
				}
				Color color = value.GetPixelFromTopLeft((int)WeaponVariant.RedVariant, y);
				if(color.a == 0.0f)
					return;
				color.a = 1.0f;
				if(!__instance.gotTarget) {
					__instance.GetTarget();
				}
				if((bool)__instance.rend) { // set the handle color 
					__instance.rend.GetPropertyBlock(__instance.block);
					__instance.block.SetColor("_CustomColor1", color);
					__instance.rend.SetPropertyBlock(__instance.block);
				}
				if((bool)renderer) { // trail color 
					Color c = color.Clone();
					c.a = renderer.startColor.a;
					renderer.startColor = c;
				}
				if((bool)light) { // light color 
					light.color = color;
				}
			}
		}

		#endregion

		#region ImpactHammer

		private static float HammerSecondaryMeterColor = 0;
		private static float HammerLerp = 0;

		// get the meter color state 
		[HarmonyPatch(typeof(ShotgunHammer), nameof(ShotgunHammer.UpdateMeter))]
		[HarmonyPrefix]
		private static void PreHammerUpdateMeter(ShotgunHammer __instance, bool forceUpdateTexture = false) {
			HammerLerp = 0;
			if(__instance.variation == 0) {
				float secondaryMeterFill = MonoSingleton<WeaponCharges>.Instance.shoAltNadeCharge;
				if(secondaryMeterFill >= 1f) {
					HammerSecondaryMeterColor = 0;
				} else {
					HammerSecondaryMeterColor = 1;
				}
			} else if(__instance.variation == 1) {
				float timeToBeep = __instance.timeToBeep;
				if(timeToBeep != 0f) {
					timeToBeep = Mathf.MoveTowards(timeToBeep, 0f, Time.deltaTime * 5f);
				}
				float secondaryMeterFill = __instance.primaryCharge / 3f;
				if(__instance.primaryCharge == 3) {
					secondaryMeterFill = 1f;
					if(timeToBeep == 0f) {
						HammerSecondaryMeterColor = 2;
					} else if(timeToBeep < 0.5f) {
						HammerSecondaryMeterColor = 3;
					}
				} else if(__instance.primaryCharge == 1) {
					HammerSecondaryMeterColor = 4;
				} else if(__instance.primaryCharge == 2) {
					HammerSecondaryMeterColor = 5;
				}
			} else if(__instance.variation == 2) {
				if(__instance.charging) {
					HammerSecondaryMeterColor = 6;
					HammerLerp = __instance.chargeForce / 60f;
				} else {
					if(MonoSingleton<WeaponCharges>.Instance.shoSawCharge == 1f) {
						HammerSecondaryMeterColor = 7;
					} else {
						HammerSecondaryMeterColor = 8;
					}
				}
			}
		}

		// ...and set to what it should instead be 
		[HarmonyPatch(typeof(ShotgunHammer), nameof(ShotgunHammer.UpdateMeter))]
		[HarmonyPostfix]
		private static void PostHammerUpdateMeter(ShotgunHammer __instance, bool forceUpdateTexture = false) {
			if(Plugin.ColorOverrides.TryGetValue("T_ImpactHammer_Palette", out Texture2D value)) {
				Color color = Color.black;
				switch(HammerSecondaryMeterColor) {
					case 0:
						color = value.GetPixelFromTopLeft((int)WeaponVariant.BlueVariant, Plugin.ExtrasRow);
						break;
					case 1:
						color = value.GetPixelFromTopLeft((int)WeaponVariant.BlueVariant, Plugin.ExtrasRow2);
						break;
					case 2:
						color = value.GetPixelFromTopLeft((int)WeaponVariant.GoldVariant, Plugin.ExtrasRow);
						break;
					case 3:
						color = value.GetPixelFromTopLeft((int)WeaponVariant.GoldVariant, Plugin.ExtrasRow2);
						break;
					case 4:
						color = value.GetPixelFromTopLeft((int)WeaponVariant.GreenVariant, Plugin.ExtrasRow2);
						break;
					case 5:
						color = value.GetPixelFromTopLeft((int)WeaponVariant.GreenVariant, Plugin.ExtrasRow3);
						break;
					case 6:
						color = value.GetPixelFromTopLeft((int)WeaponVariant.RedVariant, Plugin.ExtrasRow);
						Color color2 = value.GetPixelFromTopLeft((int)WeaponVariant.RedVariant, Plugin.ExtrasRow3);
						if(color.a == 0)
							color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[__instance.variation];
						color.a = 1;
						if(color2.a == 0)
							color2 = new Color(1f, 0.25f, 0.25f);
						color2.a = 1;
						__instance.secondaryMeter.color = Color.Lerp(color, color2, HammerLerp);
						return;
					case 7:
						color = value.GetPixelFromTopLeft((int)WeaponVariant.RedVariant, Plugin.ExtrasRow);
						break;
					case 8:
						color = value.GetPixelFromTopLeft((int)WeaponVariant.RedVariant, Plugin.ExtrasRow2);
						break;
				}
				if(color.a == 0)
					return;
				color.a = 1;
				__instance.secondaryMeter.color = color;
			}
		}

		#endregion

		#region Nailgun
		private static bool NailZapperWarningXColor = false;
		private static float NailZapperTextLerp = 0;
		private static int NailZapperColor = 0;
		private static bool NailZapperWarnColor = false;

		// set the heat slider outline color 
		[HarmonyPatch(typeof(Nailgun), nameof(Nailgun.Update))]
		[HarmonyPostfix]
		private static void NailPostUpdate(Nailgun __instance) {
			string key = __instance.altVersion ? "SawbladeLauncher" : "NailgunNew";
			if(Plugin.ColorOverrides.TryGetValue($"T_{key}_Palette", out Texture2D value)) {
				if((bool)__instance.heatSlider) {
					Color color = value.GetPixelFromTopLeft(__instance.CorrectVariation(), Plugin.ExtrasRow3);
					if(color.a == 0) {
						color = __instance.emptyColor;
					}
					color.a = 1;
					Color color2 = value.GetPixelFromTopLeft((int)WeaponVariant.GoldVariant, Plugin.EmissivesRow);
					if(color2.a == 0) {
						color2 = __instance.fullColor;
					}
					color2.a = 1;
					__instance.sliderBg.color = Color.Lerp(color, color2, __instance.heatUp);
					if(__instance.heatUp <= 0f && __instance.heatSinks < 1f) {
						__instance.sliderBg.color = new Color(0f, 0f, 0f, 0f);
					}
				}
			}
		}

		// set the heat slider fill color 
		[HarmonyPatch(typeof(Nailgun), nameof(Nailgun.SetHeat))]
		[HarmonyPostfix]
		private static void NailPostSetHeat(Nailgun __instance, float heat) {
			string key = __instance.altVersion ? "SawbladeLauncher" : "NailgunNew";
			if(Plugin.ColorOverrides.TryGetValue($"T_{key}_Palette", out Texture2D value)) {
				Color color = value.GetPixelFromTopLeft((int)WeaponVariant.GoldVariant, Plugin.EmissivesRow);
				if(color.a == 0) {
					color = __instance.fullColor;
				}
				color.a = 1;
				if((bool)__instance.heatSlider && (bool)__instance.heatSlider.transform && (bool)__instance.heatSlider.transform.Find("Fill Area") && __instance.heatSlider.transform.Find("Fill Area").Find("Fill")) {
					Image img = __instance.heatSlider.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
					if((bool)img) {
						img.color = color;
					}
				}
			}
		}

		// Prevents the constant dinging noise. 
		// Apparently the game checks the heatsink color to make the dinging noise,
		// so changing the color makes it happen every frame. This just sets it back to vanilla values
		// before the sound code runs 
		[HarmonyPatch(typeof(Nailgun), nameof(Nailgun.RefreshHeatSinkFill))]
		[HarmonyPrefix]
		private static void NailPreRefreshHeatSinkFill(Nailgun __instance) {
			for(int i = 0; i < __instance.heatSinkImages.Length; i++) {
				__instance.heatSinkImages[i].color = (__instance.heatSinkFill < (i + 1)) ? __instance.emptyColor : MonoSingleton<ColorBlindSettings>.Instance.variationColors[__instance.CorrectVariation()];
			}
		}

		// Sets the heatsink/magnet icon colors (magnet icons are heatsinks too) and ammo number color 
		[HarmonyPatch(typeof(Nailgun), nameof(Nailgun.RefreshHeatSinkFill))]
		[HarmonyPostfix]
		private static void NailPostRefreshHeatSinkFill(Nailgun __instance) {
			string key = __instance.altVersion ? "SawbladeLauncher" : "NailgunNew";
			for(int i = 0; i < __instance.heatSinkImages.Length; i++) {
				if(__instance.heatSinkFill > i) {
					int num = __instance.CorrectVariation();
					if(__instance.heatSinkFill >= (i + 1)) {
						if(Plugin.ColorOverrides.TryGetValue($"T_{key}_Palette", out Texture2D value)) {
							Color color = value.GetPixelFromTopLeft(__instance.CorrectVariation(), Plugin.ExtrasRow);
							if(color.a != 0) {
								color.a = 1;
								__instance.heatSinkImages[i].color = color;
							}
						}
					} else if(__instance.heatSinkFill < (i + 1)) {
						if(Plugin.ColorOverrides.TryGetValue($"T_{key}_Palette", out Texture2D value)) {
							Color color = value.GetPixelFromTopLeft(__instance.CorrectVariation(), Plugin.ExtrasRow2);
							if(color.a != 0) {
								color.a = 1;
								__instance.heatSinkImages[i].color = color;
							}
						}
					}
				}
			}
			if((bool)__instance.ammoText) {
				if(Plugin.ColorOverrides.TryGetValue($"T_{key}_Palette", out Texture2D value)) {
					Color color = value.GetPixelFromTopLeft(__instance.CorrectVariation(), Plugin.ExtrasRow3);
					if(color.a != 0) {
						color.a = 1;
						__instance.ammoText.color = color;
					}
				}
			}
		}

		// Jumpstart screen stuff -- get what the colors should be 
		[HarmonyPatch(typeof(Nailgun), nameof(Nailgun.UpdateZapHud))]
		[HarmonyPrefix]
		private static void NailPreUpdateZapHud(Nailgun __instance) {
			NailZapperWarnColor = false;
			if(__instance.currentZapper == null) {
				if(Physics.Raycast(__instance.cc.GetDefaultPos(), __instance.cc.transform.forward, out var hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment)) && !LayerMaskDefaults.IsMatchingLayer(hitInfo.collider.gameObject.layer, LMD.Environment) && hitInfo.collider.TryGetComponent<EnemyIdentifierIdentifier>(out var component) && (bool)component.eid && !component.eid.dead) {
					if(hitInfo.distance < __instance.zapper.maxDistance - 5f) {
						NailZapperColor = 0;
					} else {
						NailZapperColor = 1;
					}
				} else {
					NailZapperColor = 2;
				}
			} else {
				if(__instance.currentZapper.distance > __instance.currentZapper.maxDistance || __instance.currentZapper.raycastBlocked) {
					NailZapperWarningXColor = __instance.currentZapper.breakTimer % 0.1f > 0.05f;
					NailZapperColor = 3;
					NailZapperWarnColor = true;
				} else {
					NailZapperTextLerp = (__instance.currentZapper.maxDistance - __instance.currentZapper.distance) / __instance.currentZapper.maxDistance;
					NailZapperColor = 4;
				}
			}
		}

		// ...and then set the colors. This does all of the text colors, screen bar colors,
		// little line colors, and the big warning X when you're too far away 
		[HarmonyPatch(typeof(Nailgun), nameof(Nailgun.UpdateZapHud))]
		[HarmonyPostfix]
		private static void NailPostUpdateZapHud(Nailgun __instance) {
			string key = __instance.altVersion ? "SawbladeLauncher" : "NailgunNew";
			Color[] colors = new Color[] { 
				Color.white,
				MonoSingleton<ColorBlindSettings>.Instance.variationColors[2],
				Color.gray,
				Color.red,
				Color.white,
				Color.red
			};
			if(Plugin.ColorOverrides.TryGetValue($"T_{key}_Palette", out Texture2D value)) {
				Color color = value.GetPixelFromTopLeft((int)WeaponVariant.RedVariant, Plugin.ExtrasRow);
				if(color.a != 0) {
					color.a = 1;
					colors[0] = color.Clone();
				}
				color = value.GetPixelFromTopLeft((int)WeaponVariant.RedVariant, Plugin.ExtrasRow2);
				if(color.a != 0) {
					color.a = 1;
					colors[1] = color.Clone();
				}
				color = value.GetPixelFromTopLeft((int)WeaponVariant.RedVariant, Plugin.ExtrasRow3);
				if(color.a != 0) {
					color.a = 1;
					colors[2] = color.Clone();
				}
				color = value.GetPixelFromTopLeft((int)WeaponVariant.GoldVariant, Plugin.ExtrasRow);
				if(color.a != 0) {
					color.a = 1;
					colors[3] = color.Clone();
				}
				color = value.GetPixelFromTopLeft((int)WeaponVariant.GoldVariant, Plugin.ExtrasRow2);
				if(color.a != 0) {
					color.a = 1;
					colors[4] = color.Clone();
				}
				color = value.GetPixelFromTopLeft((int)WeaponVariant.GoldVariant, Plugin.ExtrasRow3);
				if(color.a != 0) {
					color.a = 1;
					colors[5] = color.Clone();
				}

				Color screenColor = new Color(1f, 0.2352f, 0.2353f);

				color = value.GetPixelFromTopLeft((int)WeaponVariant.GoldVariant, Plugin.IconRow);
				if(color.a != 0) {
					color.a = 1;
					screenColor = color;
				}
				Image bg = __instance.zapMeter.transform.Find("Background").gameObject.GetComponent<Image>();
				bg.color = screenColor;

				Image dmf = __instance.distanceMeter.fillRect.gameObject.GetComponent<Image>();
				dmf.color = screenColor.MultiplyHSV(v: 0.5f);

				Image dmh = __instance.distanceMeter.handleRect.gameObject.GetComponent<Image>();
				dmh.color = screenColor;

				Image h2 = __instance.distanceMeter.transform.Find("Handle (2)").gameObject.GetComponent<Image>();
				h2.color = screenColor.MultiplyHSV(s: 0.5f, v: 0.4f);

				Image h3 = __instance.distanceMeter.transform.Find("Handle (3)").gameObject.GetComponent<Image>();
				h3.color = screenColor.MultiplyHSV(v: 0.4f);
			}
			switch(NailZapperColor) {
				case 0:
					__instance.statusText.color = colors[0];
					break;
				case 1:
					__instance.statusText.color = colors[1];
					break;
				case 2:
					__instance.statusText.color = colors[2];
					break;
				case 3:
					__instance.statusText.color = colors[1];
					break;
				case 4:
					__instance.statusText.color = Color.Lerp(colors[3], colors[0], NailZapperTextLerp);
					break;
			}
			if(NailZapperWarnColor) {
				__instance.warningX.color = NailZapperWarningXColor ? colors[5] : colors[4];
			}
		}

		// Set Jumpstart cable color 
		[HarmonyPatch(typeof(Zapper), nameof(Zapper.Update))]
		[HarmonyPostfix]
		private static void NailPostZapperUpdate(Zapper __instance) {
			GunControl gc = MonoSingleton<GunControl>.Instance;
			GameObject nail = gc.slot3.Find(obj => IsZapper(obj));
			string key = "NailgunNew";
			if((bool)nail && nail.GetComponent<Nailgun>().altVersion) {
				key = "SawbladeLauncher";
			}
			if(Plugin.ColorOverrides.TryGetValue($"T_{key}_Palette", out Texture2D value0)) {
				Color color = value0.GetPixelFromTopLeft((int)WeaponVariant.RedVariant, Plugin.EmissivesRow);
				if(color.a == 0) {
					color = Color.red;
				}
				color.a = 1;
				Color color2 = new Color(0.5f, 0.5f, 0.5f);
				if(__instance.breakTimer > 0f) {
					color2 = ((__instance.breakTimer % 0.1f > 0.05f) ? Color.black : Color.white);
				} else if(__instance.distance > __instance.maxDistance - 10f) {
					color2 = Color.Lerp(color, color2, (__instance.maxDistance - __instance.distance) / 10f);
				}
				__instance.lr.startColor = color2;
				__instance.lr.endColor = color2;
			}
		}

		// for filtering slot 3 for Jumpstart nailgun/sawblade launcher 
		private static bool IsZapper(GameObject obj) {
			if(obj.TryGetComponent(out Nailgun nail)) {
				return nail.variation == 2;
			}
			return false;
		}

		#endregion

		#region Railcannon

		// set Malicious railcannon lightning and charge orb colors 
		// also create the charge orb texture with CreateColoredChargeOrb()
		[HarmonyPatch(typeof(Railcannon), nameof(Railcannon.OnEnable))]
		[HarmonyPostfix]
		private static void PostRailcannonEnable(Railcannon __instance) {
			if(__instance.variation != 2)
				return;
			if(Plugin.ColorOverrides.TryGetValue("T_Railcannon_Palette", out Texture2D value)) {
				bool flag1 = true;
				Color color = value.GetPixelFromTopLeft((int)WeaponVariant.GoldVariant, Plugin.ExtrasRow3);
				if(color.a == 0) {
					flag1 = false;
				} else {
					color.a = 1;
				}
				bool flag2 = true;
				Color color2 = value.GetPixelFromTopLeft((int)WeaponVariant.GoldVariant, Plugin.IconRow);
				if(color2.a == 0) {
					flag2 = false;
				} else {
					color2.a = 1;
				}
				Transform temp = __instance.transform.Find("Railcannon");
				if((bool)temp) {
					temp = temp.Find("Armature");
					if((bool)temp) {
						temp = temp.Find("Base");
						if((bool)temp) {
							temp = temp.Find("FullCharge");
							if((bool)temp) {
								Transform charge2 = temp.Find("charge2");
								if((bool)charge2 && flag2) {
									// change charge orb thingy 
									if(charge2.gameObject.TryGetComponent(out SpriteRenderer renderer)) {
										Texture2D t_charge = CreateColoredChargeOrb(color2, Plugin.t_charge2, Plugin.t_charge2_ID);
										renderer.sprite = Sprite.Create(t_charge, new Rect(0, 0, t_charge.width, t_charge.height), new Vector2(0.5f, 0.5f));
									}
								}
								Transform particleSystem1 = temp.Find("Particle System (1)");
								if((bool)particleSystem1 && flag1 && particleSystem1.gameObject.TryGetComponent(out ParticleSystem particleSystem)) {
									ParticleSystem.MainModule main = particleSystem.main;
									main.startColor = color;
								}
							}
						}
					}
				}
			}
		}

		private static bool RailShouldUpdatePostfix = false;
		// Determine if the light and lightning colors should update 
		[HarmonyPatch(typeof(Railcannon), nameof(Railcannon.Update))]
		[HarmonyPostfix]
		private static void PreRailcannonUpdate(Railcannon __instance) {
			float altCharge = __instance.altCharge;
			if(__instance.wid.delay > 0f && altCharge < __instance.wc.raicharge) {
				altCharge = __instance.wc.raicharge;
			}
			float raicharge = __instance.wc.raicharge;
			if(__instance.wid.delay > 0f) {
				raicharge = altCharge;
			}
			RailShouldUpdatePostfix = __instance.wc.raicharge >= 5f || NoWeaponCooldown.NoCooldown;
		}

		// ...if so, set the light and lightning colors 
		[HarmonyPatch(typeof(Railcannon), nameof(Railcannon.Update))]
		[HarmonyPostfix]
		private static void PostRailcannonUpdate(Railcannon __instance) {
			if(RailShouldUpdatePostfix) {
				if(Plugin.ColorOverrides.TryGetValue("T_Railcannon_Palette", out Texture2D value2)) {
					int y = 0;
					switch((WeaponVariant)__instance.variation) {
						case WeaponVariant.BlueVariant:
							y = Plugin.EmissivesRow;
							break;
						case WeaponVariant.GreenVariant:
							y = Plugin.ExtrasRow;
							break;
						case WeaponVariant.RedVariant:
							y = Plugin.ExtrasRow2;
							break;
					}
					Color color = value2.GetPixelFromTopLeft((int)WeaponVariant.GoldVariant, y);
					if(color.a == 0)
						return;
					color.a = 1;
					__instance.fullChargeLight.color = color;
					ParticleSystem.MainModule main = __instance.fullChargeParticles.main;
					main.startColor = color;
				}
			}
		}

		// Handle railcannon emissives, vanilla does them manually too 
		[HarmonyPatch(typeof(Railcannon), nameof(Railcannon.SetMaterialIntensity))]
		[HarmonyPostfix]
		private static void SetMaterialIntensity(Railcannon __instance, float newIntensity, bool isRecharging) {
			if(Plugin.ColorOverrides.TryGetValue("T_Railcannon_Palette", out Texture2D value)) {
				Color color = value.GetPixelFromTopLeft(__instance.variation, Plugin.EmissivesRow);
				if(color.a == 0)
					return;
				color.a = 1;
				__instance.body.GetPropertyBlock(__instance.propBlock);
				__instance.propBlock.SetColor("_EmissiveColor", color);
				__instance.body.SetPropertyBlock(__instance.propBlock);
				for(int i = 0; i < __instance.pips.Length; i++) {
					SkinnedMeshRenderer obj = __instance.pips[i];
					obj.GetPropertyBlock(__instance.propBlock);
					__instance.propBlock.SetColor("_EmissiveColor", color);
					obj.SetPropertyBlock(__instance.propBlock);
				}
			}
		}

		private static float RailFlashAmount = 0;
		// Get flash lerp value for railcannon meter 
		[HarmonyPatch(typeof(RailcannonMeter), nameof(RailcannonMeter.Update))]
		[HarmonyPrefix]
		private static void RailMeterPreUpdate(RailcannonMeter __instance) {
			if(__instance.self.enabled || __instance.miniVersion.activeSelf) {
				RailFlashAmount = __instance.flashAmount;
				if(MonoSingleton<WeaponCharges>.Instance.raicharge > 4f) {
					if(!__instance.hasFlashed && Time.timeScale > 0f) {
						RailFlashAmount = 1f;
					}
				}
			}
		}

		// recolor the railcannon meter and make it flash if needed 
		[HarmonyPatch(typeof(RailcannonMeter), nameof(RailcannonMeter.Update))]
		[HarmonyPostfix]
		private static void RailMeterPostUpdate(RailcannonMeter __instance) {
			if(__instance.self.enabled || __instance.miniVersion.activeSelf) {
				if(Plugin.ColorOverrides.TryGetValue("T_Railcannon_Palette", out Texture2D value)) {
					if(MonoSingleton<WeaponCharges>.Instance.raicharge > 4f) { // charged colors 
						Color color = value.GetPixelFromTopLeft(LastRailVariation(), Plugin.ExtrasRow);
						if(color.a == 0)
							color = MonoSingleton<ColorBlindSettings>.Instance.railcannonFullColor;
						color.a = 1;
						Color color2 = value.GetPixelFromTopLeft(LastRailVariation(), Plugin.ExtrasRow3);
						if(color2.a == 0)
							color2 = Color.white;
						color2.a = 1;
						if(RailFlashAmount > 0f) {
							color = Color.Lerp(color, color2, RailFlashAmount);
						}
						foreach(Image image in __instance.trueMeters) {
							if(image != __instance.colorlessMeter) {
								image.color = color;
							}
						}
					} else { // charging color 
						Color color = value.GetPixelFromTopLeft(LastRailVariation(), Plugin.ExtrasRow2);
						if(color.a == 0)
							return;
						color.a = 1;
						foreach(Image obj in __instance.trueMeters) {
							obj.color = color;
						}
					}
				}
			}
		}

		// What should the railcannon meter color be? This depends on 
		// Remember Last Used Weapon Variation and if you are currently  
		// holding a railcannon 
		private static int LastRailVariation() {
			GunControl gc = MonoSingleton<GunControl>.Instance;
			if(gc.slot4.Count == 0) {
				return 0;
			}
			if(gc.currentWeapon.TryGetComponent(out Railcannon rc)) {
				return rc.variation;
			}
			int idx = 0;
			if(gc.variationMemory & gc.retainedVariations.TryGetValue(3, out int value) && value >= 0 && value < gc.slot4.Count) { // 3 from 4-1 => base 0 
				idx = value;
			}
			if(gc.slot4[idx].TryGetComponent(out Railcannon rc2)) {
				return rc2.variation;
			}
			return 0;
		}

		#endregion

		#region RocketLauncher
		private static float RocketLauncherCBCharge = 0;

		// get SRS Cannon charge fill amount 
		[HarmonyPatch(typeof(RocketLauncher), nameof(RocketLauncher.Update))]
		[HarmonyPrefix]
		private static void PreRocketLauncherUpdate(RocketLauncher __instance) {
			RocketLauncherCBCharge = Mathf.MoveTowards(__instance.cbCharge, 1f, Time.deltaTime);
		}

		// Set dial colors for rocket launcher 
		[HarmonyPatch(typeof(RocketLauncher), nameof(RocketLauncher.Update))]
		[HarmonyPostfix]
		private static void PostRocketLauncherUpdate(RocketLauncher __instance) {
			if(Plugin.ColorOverrides.TryGetValue("T_RocketLauncher_Palette", out Texture2D value2)) {
				Color color = value2.GetPixelFromTopLeft(__instance.variation, Plugin.ExtrasRow);
				if(color.a == 0)
					color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[__instance.variation];
				color.a = 1;
				float num = 1f;
				Color color2 = value2.GetPixelFromTopLeft(__instance.variation, Plugin.ExtrasRow2);
				if(__instance.variation == 1) {
					if(color2.a == 0)
						color2 = Color.red;
					color2.a = 1;
					if(RocketLauncherCBCharge > 0f) {
						color = Color.Lerp(color, color2, RocketLauncherCBCharge);
					} else if(MonoSingleton<WeaponCharges>.Instance.rocketCannonballCharge < 1f) {
						num = 0.5f;
					}
				} else if(__instance.variation == 2) {
					if(color2.a == 0)
						color2 = Color.gray;
					color2.a = 1;
					if(!__instance.firingNapalm && MonoSingleton<WeaponCharges>.Instance.rocketNapalmFuel < 0.25f) {
						color = color2;
					}
				}
				for(int j = 0; j < __instance.variationColorables.Length; j++) {
					__instance.variationColorables[j].color = color.CloneModified(a: __instance.colorablesTransparencies[j] * num);
				}
			}
		}

		#endregion

		// Generate the colored charge orb texture. The value of 
		// t_ID texture's blue channel is used as the opacity for each pixel
		// of a solid texture of the inputted color. This new image is overlayed
		// with the base texture t_ch, and then the opacity of the pixel is set to the 
		// same opacity as the corresponding t_ch pixel. 
		public static Texture2D CreateColoredChargeOrb(Color color, Texture2D t_ch, Texture2D t_ID) {
			Texture2D t_charge = new Texture2D(32, 32);
			for(int x = 0; x < t_charge.width; x++) {
				for(int y = 0; y < t_charge.height; y++) {
					Color c = color.Clone();
					c.a = t_ID.GetPixel(x, y).b;
					Color c2 = t_ch.GetPixel(x, y);
					if(c2.a != 0) {
						c = (c + (c2 * (1 - c.a))) / (c.a + (c2.a * (1 - c.a)));
						c.a = c2.a;
					}
					t_charge.SetPixel(x, y, c);
				}
			}
			t_charge.Apply();
			return t_charge;
		}
	}
}
