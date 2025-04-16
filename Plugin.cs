using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace USGCColorOverride;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
	internal static new ManualLogSource Logger;

	// the dictionary that stores the textures supported by this mod, loaded by UltraSkins GC 
	internal static Dictionary<string, Texture2D> ColorOverrides;

	internal const int EmissivesRow = 0;
	internal const int ExtrasRow = 1;
	internal const int ExtrasRow2 = 2;
	internal const int ExtrasRow3 = 3;
	internal const int IconRow = 4;

	// textures for my colored charge orbs, based on vanilla's charge.png and charge2.png
	// these are used to generate the colored orb textures at runtime in
	// IndividualWeaponPatches.CreateColoredChargeOrb() 
	// Vanilla has its own colorable charge orb texture, but this looks more accurate
	// to the original charge.png and charge2.png textures 
	internal static Texture2D t_charge;
	internal static Texture2D t_charge_ID;
	internal static Texture2D t_charge2;
	internal static Texture2D t_charge2_ID;

	private void Awake() {
		Logger = base.Logger;
		ColorOverrides = new Dictionary<string, Texture2D>();
		t_charge = LoadTexture2D("charge.png");
		t_charge_ID = LoadTexture2D("charge_ID.png");
		t_charge2 = LoadTexture2D("charge2.png");
		t_charge2_ID = LoadTexture2D("charge2_ID.png");
		Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
		harmony.PatchAll();
		Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
	}

	// load in textures from Resources, needs ".png" 
	private Texture2D LoadTexture2D(string fn) {
		var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("USGCColorOverride.Resources." + fn);
		byte[] arr = new byte[s.Length];
		s.Read(arr, 0, arr.Length);
		Texture2D t2d = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		ImageConversion.LoadImage(t2d, arr, false); // don't mark non-readable, pixel data is needed later 
		return t2d;
	}
}
