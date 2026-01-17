using RoR2BepInExPack.GameAssetPaths.Version_1_39_0;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MoreElites
{
    public static class EliteRampGenerator
    {
        private static Material malachiteOverlayMat = new Material(Addressables.LoadAssetAsync<Material>(RoR2_Base_ElitePoison.matElitePoisonOverlay_mat).WaitForCompletion());

        // First 2 colors will be the most prominent, use darker/bolder colors for these 2, lighter/pastel colors will look very bright.
        // The other 3 add depth and texture on some enemies, that"s why 5 are required, so you don"t get flat colors.
        public static Texture2D CreateGradientTexture(Color32[] colors, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            texture.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Calculate the horizontal position as a value between 0 and 1
                    float t = (float)x / (width - 1);

                    // Determine which colors to interpolate between
                    float scaledT = t * (colors.Length - 1);
                    int colorIndex = Mathf.FloorToInt(scaledT);
                    float lerpFactor = scaledT - colorIndex;

                    // Ensure the last color is not out of bounds
                    if (colorIndex >= colors.Length - 1)
                    {
                        colorIndex = colors.Length - 2;
                        lerpFactor = 1.0f;
                    }

                    // Interpolate between the two colors
                    Color32 color = LerpColor32(colors[colorIndex], colors[colorIndex + 1], lerpFactor);

                    // Set the pixel color
                    texture.SetPixel(x, y, color);
                }
            }

            // Apply changes to the texture
            texture.Apply();
            
            string fileName = "SavedTexture.png";
            SaveTextureToFile(texture, fileName);
            
            malachiteOverlayMat.SetTexture("_RemapTex", texture);

            return texture;
        }

        
        internal static void SaveTextureToFile(Texture2D texture, string fileName)
        {
            return;
          byte[] bytes = texture.EncodeToPNG();
          string path = System.IO.Path.Combine(Application.persistentDataPath, fileName);
          System.IO.File.WriteAllBytes(path, bytes);
          Debug.Log("Texture saved to: " + path);
        }
        

        public static Color32 LerpColor32(Color32 colorA, Color32 colorB, float t)
        {
            byte r = (byte)Mathf.Lerp(colorA.r, colorB.r, t);
            byte g = (byte)Mathf.Lerp(colorA.g, colorB.g, t);
            byte b = (byte)Mathf.Lerp(colorA.b, colorB.b, t);
            byte a = (byte)Mathf.Lerp(colorA.a, colorB.a, t);
            return new Color32(r, g, b, a);
        }
    }
}
