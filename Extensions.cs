using System;
using System.Collections.Generic;
using UnityEngine;

namespace USGCColorOverride {
	// Helpful extension methods that either are or were once used 
	internal static class Extensions {
		public static void TryCopyValue<K, V>(this Dictionary<K, V> self, Dictionary<K, V> from, K key) {
			if(from.TryGetValue(key, out V value)) {
				self.TryAdd(key, value);
			}
		}

		public static bool TryGetComponentInParent<T>(this MonoBehaviour self, out T _comp) {
			self.TryGetComponent(out T comp);
			_comp = comp;
			return _comp != null;
		}

		public static Color Clone(this Color self) {
			return new Color(self.r, self.g, self.b, self.a);
		}

		// Why does unity start from the bottom left with GetPixel() anyways??? 
		public static Color GetPixelFromTopLeft(this Texture2D self, int x, int y) {
			return self.GetPixel(x, self.height - y - 1);
		}

		public static Color CloneModified(this Color self, float r = -1, float g = -1, float b = -1, float a = -1) {
			Color temp = self.Clone();
			if(r != -1)
				temp.r = r;
			if(g != -1)
				temp.g = g;
			if(b != -1)
				temp.b = b;
			if(a != -1)
				temp.a = a;
			return temp;
		}

		internal static T[] Fill<T>(this T[] arr, T value) {
			Array.Fill(arr, value);
			return arr;
		}

		internal static Color MultiplyHSV(this Color c, float h = 1, float s = 1, float v = 1) {
			Color.RGBToHSV(c, out float h2, out float s2, out float v2);
			return Color.HSVToRGB(h * h2, s * s2, v * v2);
		}
	}
}
