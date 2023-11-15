using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public static class PrimerColor
    {
        /*
        These are the main colors I use in the vids. I don't own the colors,
        so you're free to use them without constraint.

        That said, if your goal is to publish, you're probably better off creating your own
        color palette to give your content a distinct feel. If you decide to do that,
        coolors.co is a super useful tool for that.

        Main colors: https://coolors.co/2f3336-ffffff-3e7ea0-ff9500-e7e247-d63b50-698f3f-db5abb-5e666c
        Complementary colors: https://coolors.co/ffa06d-073b8f-6d9d37-9fe3da-d0aaff-092d13
        */

        // Defining colors as Vector4 because UnityEngine.Color stores values on a 0-1 scale
        // rather than a 0-255 scale
        
        // Colors with names from Coolors, for interest
        // blue => celadonBlue
        // orange => yellowOrangeColorWheel
        // yellow => corn
        // red => amaranth
        // green => maximumGreen
        // purple => superPink
        // gray => onyx
        // lightGray => blackCoral
        // white => whiteSmoke

        // Main colors
        public static Color blue = ToColor(new Vector3(62, 126, 160));
        public static Color orange = ToColor(new Vector3(255, 148, 0));
        public static Color yellow = ToColor(new Vector3(231, 226, 71));
        public static Color red = ToColor(new Vector3(214, 59, 80));
        public static Color green = ToColor(new Vector3(105, 143, 63));
        public static Color purple = ToColor(new Vector3(219, 90, 186));
        public static Color gray = ToColor(new Vector3(47, 51, 54));
        public static Color lightGray = ToColor(new Vector3(94, 102, 108));
        public static Color white = ToColor(new Vector3(243, 242, 240));
        public static Color black = ToColor(new Vector3(0, 0, 0));

        public static List<Color> all = new() {
            blue,
            red,
            green,
            orange,
            purple,
            yellow,
            gray,
            lightGray,
            white,
            black,
        };

        // Texture base colors
        public static Color woodTextureBase = ToColor(new Vector3 (209, 142, 49));
        public static Color woodTextureBaseDark = ToColor(new Vector3 (87, 52, 9));

        // Complementary colors
        // Perhaps not the most useful names, but it does make it inconvenient to use
        // them as main colors
        public static Color blueComplement = ToColor(new Vector3(255, 160, 109));
        public static Color orangeComplement = ToColor(new Vector3(7, 59, 143));
        public static Color yellowComplement = ToColor(new Vector3(109, 157, 55));
        public static Color redComplement = ToColor(new Vector3(159, 227, 218));
        public static Color greenComplement = ToColor(new Vector3(208, 170, 255));
        public static Color purpleComplement = ToColor(new Vector3(9, 45, 19));

        public static Dictionary<Color, Color> complementaryColorMap = new() {
            {blue, blueComplement},
            {orange, orangeComplement},
            {yellow, yellowComplement},
            {red, redComplement},
            {green, greenComplement},
            {purple, purpleComplement},
            {gray, white},
            {white, gray},
        };

        public static List<Color> blobColors = new() {
            blue,
            green,
            yellow,
            orange,
            red,
            purple,
            gray,
            white,
        };

        public static Color ToColor(Vector4 vector)
        {
            return vector / 255;
        }

        public static Color ToColor(Vector3 vector)
        {
            return new Vector4(vector.x, vector.y, vector.z, 255) / 255;
        }

        // Do we really need this?
        // Not really, but it's currently used in the PrimerBlob class for a color picker that doesn't even work.
        public enum PrimerColors
        {
            Blue,
            Orange,
            Yellow,
            Red,
            Green,
            Purple,
            Gray,
            LightGray,
            White,
            Black,
        }

        public static Color ToColor(this PrimerColors self)
        {
            return self switch {
                PrimerColors.Blue => blue,
                PrimerColors.Orange => orange,
                PrimerColors.Yellow => yellow,
                PrimerColors.Red => red,
                PrimerColors.Green => green,
                PrimerColors.Purple => purple,
                PrimerColors.Gray => gray,
                PrimerColors.LightGray => lightGray,
                PrimerColors.White => white,
                PrimerColors.Black => black,
            };
        }
        
        public static Color JuicyInterpolate(Color a, Color b, float t, float juiciness = 0.5f)
        {
            float Ha, Sa, Va;
            Color.RGBToHSV(a, out Ha, out Sa, out Va);
            float Hb, Sb, Vb;
            Color.RGBToHSV(b, out Hb, out Sb, out Vb);
            
            var lerp = Vector3.Lerp(new Vector4(Ha, Sa, Va), new Vector4(Hb, Sb, Vb), t);
            var hsvLerped = Color.HSVToRGB(lerp.x, lerp.y, lerp.z);
            hsvLerped.a = Mathf.Lerp(a.a, b.a, t);

            var rgbLerped = Color.Lerp(a, b, t);

            return Color.Lerp(rgbLerped, hsvLerped, juiciness);
        }
        public static Color ModeratelyJuicyInterpolate(Color a, Color b, float t)
        {
            return JuicyInterpolate(a, b, t, 0.5f);
        }
        
        public static Dictionary<Color, string> colorToMaterialName = new() {
            {blue, "PrimerDiffuseBlue"},
            {orange, "PrimerDiffuseOrange"},
            {yellow, "PrimerDiffuseYellow"},
            {red, "PrimerDiffuseRed"},
            {green, "PrimerDiffuseGreen"},
            {purple, "PrimerDiffusePurple"},
            {gray, "PrimerDiffuseGray"},
            {lightGray, "PrimerDiffuseLightGray"},
            {white, "PrimerDiffuseWhite"},
            {black, "PrimerDiffuseBlack"}
        };
    }
}
