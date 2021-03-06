using Fastlights;
using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.Libraries
{
    public static class ColourExtensions
    {
        public static float GetAlpha(this SpriteRenderer spriteRenderer)
        {
            return spriteRenderer.color.a;
        }

        public static void SetAlpha(this SpriteRenderer spriteRenderer, float alpha)
        {
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
        }

        public static void SetAlpha(this Image image, float alpha)
        {
            Color c = image.color;
            c.a = alpha;
            image.color = c;
        }

        public static float GetAlpha(this Image image)
        {
            return image.color.a;
        }

        public static float GetAlpha(this FastLight light)
        {
            return light.Colour.a;
        }

        public static void SetAlpha(this FastLight light, float alpha)
        {
            Color c = light.Colour;
            c.a = alpha;
            light.Colour = c;
        }
    }
}