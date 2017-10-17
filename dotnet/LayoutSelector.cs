using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidSegmentedControl;
using Orientation = Android.Widget.Orientation;

namespace AndroidSegementedControl
{
    /// <summary>
    /// This class is used to provide the proper layout based on the view.
    /// Also provides the proper radius for corners.
    /// The layout is the same for each selected left/top middle or right/bottom button.
    /// float tables for setting the radius via Gradient.setCornerRadii are used instead
    /// of multiple xml drawables.
    /// </summary>
    internal class LayoutSelector
    {
        public static readonly int SelectedLayout = Resource.Drawable.radio_checked;
        public static readonly int UnselectedLayout = Resource.Drawable.radio_unchecked;
        private readonly float _r;
        private readonly float _r1;
        private readonly float[] _leftRadius;
        private readonly float[] _rightRadius;
        private readonly float[] _middleRadius;
        private readonly float[] _defaultRadius;
        private readonly float[] _topRadius;
        private readonly float[] _bottomRadius;
        private readonly SegmentedGroup _segmentedGroup;

        public LayoutSelector(SegmentedGroup segmentedGroup, float cornerRadius)
        {
            _segmentedGroup = segmentedGroup;
            _r1 = TypedValue.ApplyDimension(ComplexUnitType.Dip, 0.1f, segmentedGroup.Resources.DisplayMetrics); // 0.1 dp to px
            _r = cornerRadius;
            _leftRadius = new float[8] { _r, _r, _r1, _r1, _r1, _r1, _r, _r };
            _rightRadius = new float[8] { _r1, _r1, _r, _r, _r, _r, _r1, _r1 };
            _middleRadius = new float[8] { _r1, _r1, _r1, _r1, _r1, _r1, _r1, _r1 };
            _defaultRadius = new float[8] { _r, _r, _r, _r, _r, _r, _r, _r };
            _topRadius = new float[8] { _r, _r, _r, _r, _r1, _r1, _r1, _r1 };
            _bottomRadius = new float[8] { _r1, _r1, _r1, _r1, _r, _r, _r, _r };
        }

        public float[] GetChildRadius(View view)
        {
            var children = _segmentedGroup.ChildCount;
            var child = _segmentedGroup.IndexOfChild(view);

            // If there is only one child provide the default radio button
            if (children == -1)
            {
                return _defaultRadius;
            }
            if (child == 0) // left or top
            {
                return _segmentedGroup.Orientation == Orientation.Horizontal ? _leftRadius : _topRadius;
            }
            if (child == children - 1) // right or bottom
            {
                return _segmentedGroup.Orientation == Orientation.Horizontal ? _rightRadius : _bottomRadius;
            }

            // middle
            return _middleRadius;
        }
    }
}