using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Graphics;
using Android.Views;
using Android.Widget;

namespace Tokkepedia.Helpers
{
    public class ManipulateColor
    {
        public static Android.Graphics.Color manipulateColor(int color, float factor)
        {
            int a = Color.GetAlphaComponent(color);
            int r = Convert.ToInt32(Math.Round(Color.GetRedComponent(color) * factor));
            int g = Convert.ToInt32(Math.Round(Color.GetGreenComponent(color) * factor));
            int b = Convert.ToInt32(Math.Round(Color.GetBlueComponent(color) * factor));

            //int r = Math.Round(Color.Red(color) * factor);
            //int g = Math.Round(Color.Green(color) * factor);
            //int b = Math.Round(Color.Blue(color) * factor);
            return Color.Argb(a,
                    Math.Min(r, 255),
                    Math.Min(g, 255),
                    Math.Min(b, 255));
        }
    }
}