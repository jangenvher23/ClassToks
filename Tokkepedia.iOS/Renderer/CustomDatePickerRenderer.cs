using Tokkepedia.iOS.Renderer;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(DatePicker), typeof(CustomDatePickerRenderer))]
namespace Tokkepedia.iOS.Renderer
{
    public class CustomDatePickerRenderer : DatePickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
        {
            base.OnElementChanged(e);

            //Control is UITextField
            var someFontWithName = UIFont.FromName("fontName", 14);
            UIFont font = Control.Font.WithSize(14);
            Control.Font = font;
        }
    }
}