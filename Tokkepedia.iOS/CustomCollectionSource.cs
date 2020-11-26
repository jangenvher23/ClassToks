using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using Tokkepedia.Shared.Models;
using UIKit;
using Tokkepedia.Shared.Extensions;
using System.Diagnostics;
using Tokkepedia.iOS.Views;
using CoreAnimation;

namespace Tokkepedia.iOS
{
    public class CustomCollectionSource : UICollectionViewSource
    {
        List<string> Colors = new List<string>() {
               "#d32f2f", "#C2185B", "#7B1FA2", "#512DA8",
               "#303F9F", "#1976D2", "#0288D1", "#0097A7",
               "#00796B", "#388E3C", "#689F38", "#AFB42B",
               "#FBC02D", "#FFA000", "#F57C00", "#E64A19"
               };
        List<string> randomcolors = new List<string>();

        public List<TokModel> rows { get; set; }
        public CustomCollectionSource(List<TokModel> _rows)
        {
            rows = _rows;
            randomcolors = Colors.Shuffle().ToList();
        }

        public override nint NumberOfSections(UICollectionView collectionView)
        {
            return 1;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return rows.Count;
        }

        public override bool ShouldHighlightItem(UICollectionView collectionView, NSIndexPath indexPath)
        {
            return true;
        }

        //public override void ItemHighlighted(UICollectionView collectionView, NSIndexPath indexPath)
        //{
        //    var cell = (TokTileViewCell)collectionView.CellForItem(indexPath);
        //    cell.mainLabel.Alpha = 0.5f;
        //}

        //public override void ItemUnhighlighted(UICollectionView collectionView, NSIndexPath indexPath)
        //{
        //    var cell = (TokTileViewCell)collectionView.CellForItem(indexPath);
        //    cell.mainLabel.Alpha = 1f;
        //}

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = (TokTileViewCell)collectionView.DequeueReusableCell(TokTileViewCell.CellID, indexPath);

            int ndx = indexPath.Row % Colors.Count;
            cell.UpdateCell(rows[indexPath.Row], randomcolors[ndx]);
            return cell;
        }

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            Debug.WriteLine("Row {0} selected", indexPath.Row);
            var item = (TokTileViewCell)collectionView.CellForItem(indexPath);
            Debug.WriteLine($"Tok {item.Tok.Id} selected");

            //UIStoryboard storyboard = UIStoryboard.FromName("TokInfoViewController", null), tabStoryboard = UIStoryboard.FromName("Main", null);

            //var tokController = storyboard.InstantiateViewController("tokInfo") as TokInfoController;
            //var homeNavController = tabStoryboard.InstantiateViewController("HomeNavigationController") as UINavigationController;

            //homeNavController

            ////TODO: Push TokInfo
            //UIApplication.SharedApplication.Windows[0].RootViewController = tokController;
            ////homeNavController.PresentViewController(tokController, true, null);


            var tok = UIStoryboard.FromName("TokInfoViewController", null).InstantiateViewController("tokInfo") as TokInfoController;
            tok.Tok = item.Tok;
            tok.PreviousTitle = "Home";

            UIApplication.SharedApplication.KeyWindow.MakeKeyAndVisible();
            UIApplication.SharedApplication.KeyWindow.RootViewController.HidesBottomBarWhenPushed = true; //= _navigation;

            var tab = UIApplication.SharedApplication.KeyWindow.RootViewController as UITabBarController;
            var nav = tab.SelectedViewController as UINavigationController;

            nav.Title = item.Tok.PrimaryFieldText;
            nav.NavigationBar.TintColor = UIColor.White;
            nav?.PushViewController(tok, true);
        }


        nfloat previousContentOffset = 0;
        bool scrollStarted = false;

        int maxFloatButtonPaddingDiff = 50;
        UIViewPropertyAnimator anim = new UIViewPropertyAnimator(0.05, UIViewAnimationCurve.Linear, () => { WindowManager.FloatButton.PaddingY -= 1; });

        public override void Scrolled(UIScrollView scrollView)
        {
            if (scrollStarted)
            {
                Debug.WriteLine(scrollView.ContentOffset);
                var currentContentOffset = scrollView.ContentOffset.Y;
                if (currentContentOffset > previousContentOffset)
                {
                    // scrolling towards the bottom
                    if (maxFloatButtonPaddingDiff > 0)
                    {
                        anim = new UIViewPropertyAnimator(0.05,UIViewAnimationCurve.Linear, () => { WindowManager.FloatButton.PaddingY -= 1; });
                        anim.StartAnimation();

                        --maxFloatButtonPaddingDiff;
                    }
                }
                else if (currentContentOffset < previousContentOffset)
                {
                    // scrolling towards the top
                    if (maxFloatButtonPaddingDiff < 100)
                    {
                        anim = new UIViewPropertyAnimator(0.05, UIViewAnimationCurve.Linear, () => { WindowManager.FloatButton.PaddingY += 1; });
                        anim.StartAnimation();

                        ++maxFloatButtonPaddingDiff;
                    }
                }
                previousContentOffset = currentContentOffset;
            }
            else
                scrollStarted = true;
        }
    }
}