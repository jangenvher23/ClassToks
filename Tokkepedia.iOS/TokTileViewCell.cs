using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using CoreGraphics;
using Tokkepedia.Shared.Models;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using FFImageLoading;
using FFImageLoading.Cross;

namespace Tokkepedia.iOS
{
    public class TokTileViewCell : UICollectionViewCell
    {
        private UIImageView profileImage, flagImage;
        public UILabel usernameLabel, titleLabel, primaryLabel, secondaryLabel, groupLabel, categoryLabel;
        public static NSString CellID = new NSString("customCollectionCell");
        public MvxCachedImageView _imageControl;

        public TokModel Tok { get; set; }

        [Export("initWithFrame:")]
        public TokTileViewCell(CGRect frame) : base(frame)
        {
            BackgroundView = new UIView { BackgroundColor = UIColor.Orange };
            usernameLabel = new UILabel();
            titleLabel = new UILabel();
            primaryLabel = new UILabel();
            secondaryLabel = new UILabel();
            groupLabel = new UILabel();
            categoryLabel = new UILabel();
            profileImage = new UIImageView();
            flagImage = new UIImageView();

            _imageControl = new MvxCachedImageView();


            ContentView.AddSubviews(new UIView[] { usernameLabel, titleLabel, primaryLabel, groupLabel, categoryLabel, profileImage, flagImage });
            this.ContentView.UserInteractionEnabled = false;
        }



        public void UpdateCell(TokModel tokModel, string hex)
        {
            Tok = tokModel;

            bool hasImage = string.IsNullOrEmpty(tokModel.Image);

            ImageService.Instance.LoadUrl(tokModel.UserPhoto).Into(profileImage);            
            profileImage.Frame = new CGRect(10, 9, 36, 36);
            profileImage.Layer.CornerRadius = 20;
            profileImage.ClipsToBounds = true;

            flagImage.Image = UIImage.FromBundle("Flags/" + tokModel.UserCountry + ".jpg");
            flagImage.Frame = new CGRect(300, 9, 36, 28);

            usernameLabel.Text = tokModel.UserDisplayName;
            usernameLabel.Frame = new CGRect(66, 8, 200, 12);
            usernameLabel.Font = usernameLabel.Font.WithSize(10);
            usernameLabel.TextAlignment = UITextAlignment.Center;
            usernameLabel.TextColor = !hasImage ? UIColor.White : UIColor.Black;

            if (!string.IsNullOrEmpty(tokModel.TitleDisplay))
            {
                titleLabel.Text = tokModel.TitleId;
                titleLabel.Frame = new CGRect(66, 23, 200, 12);
                titleLabel.Font = titleLabel.Font.WithSize(10);
                titleLabel.TextAlignment = UITextAlignment.Center;
                titleLabel.TextColor = !hasImage ? UIColor.White : UIColor.Black;
                ContentView.AddSubviews(titleLabel);
            }

            primaryLabel.Text = tokModel.PrimaryFieldText;
            int primHeight = hasImage ? 32 : 50;

            primaryLabel.Frame = new CGRect(8, primHeight, 341, 50);
            primaryLabel.LineBreakMode = UILineBreakMode.WordWrap;
            primaryLabel.Font = primaryLabel.Font.WithSize(17);
            primaryLabel.Font = UIFont.BoldSystemFontOfSize(20);
            primaryLabel.TextColor = !hasImage ? UIColor.White : UIColor.Black;
            primaryLabel.TextAlignment = UITextAlignment.Center;

            //Rules
            //2 if no image
            //Always 1 row if image
            //If Quote tok group and nonimage+english, change to up to 3 rows for primary (before truncate), 1 row for secondary
            //If Quote tok group and nonimage+nonenglish, change to 1 row each for primary and secondary nonenglish, 1 row each for primary and secondary english
            if (!string.IsNullOrEmpty(tokModel.Image) || (!string.IsNullOrEmpty(tokModel.Image) && tokModel.Language != "english"))
            {
                primaryLabel.Lines = 1;
            }
            else
            {
                primaryLabel.Lines = 1;
            }

            if (tokModel.TokGroup == "Quote")
                primaryLabel.Lines = 1;

            //Only show secondary for the Quote tok group
            if (tokModel.TokGroup == "Quote")
            {
                secondaryLabel.Text = tokModel.SecondaryFieldText;
                secondaryLabel.Frame = new CGRect(8, 70, 341, 50);
                secondaryLabel.LineBreakMode = UILineBreakMode.WordWrap;
                secondaryLabel.Lines = 1;
                secondaryLabel.Font = secondaryLabel.Font.WithSize(12);
                secondaryLabel.Font = UIFont.BoldSystemFontOfSize(10);
                secondaryLabel.TextColor = !hasImage ? UIColor.White : UIColor.Black;
                secondaryLabel.TextAlignment = UITextAlignment.Center;
                ContentView.AddSubviews(secondaryLabel);
            }

            if (!string.IsNullOrEmpty(tokModel.Image))
            {
                _imageControl.Frame = new CGRect(150, 65, 60, 50);
                _imageControl.ImagePath = !string.IsNullOrEmpty(tokModel.Image) ? tokModel.Image : null; //Should never be null though
                ContentView.AddSubviews(_imageControl);
            }


            categoryLabel.Text = tokModel.Category;
            categoryLabel.Frame = new CGRect(8, 125, 163, 12);
            categoryLabel.Font = categoryLabel.Font.WithSize(8);
            categoryLabel.TextAlignment = UITextAlignment.Center;
            categoryLabel.TextColor = !hasImage ? UIColor.White : UIColor.Black;

            groupLabel.Text = String.Format("{0} - {1}", tokModel.TokGroup, tokModel.TokType);
            groupLabel.Frame = new CGRect(186, 125, 163, 12);
            groupLabel.Font = groupLabel.Font.WithSize(8);
            groupLabel.TextAlignment = UITextAlignment.Center;
            groupLabel.TextColor = !hasImage ? UIColor.White : UIColor.Black;

            ContentView.Layer.BorderColor = UIColor.Gray.CGColor;
            ContentView.Layer.BorderWidth = 0.5f;
            ContentView.BackgroundColor = hasImage ? UIColor.White : Color.FromHex(hex).ToUIColor();
        }


        //static UIImage FromUrl(string uri)
        //{
        //    using (var url = new NSUrl(uri))
        //    using (var data = NSData.FromUrl(url))   
        //    return UIImage.LoadFromData(data);
        //}
    }
}