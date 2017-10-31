using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace AndroidSegmentedControl
{
    public class SegmentedGroup : RadioGroup, RadioGroup.IOnCheckedChangeListener
    {
        private readonly LayoutSelector _layoutSelector;
        private Color _tintColor;
        private Color _unCheckedTintColor;
        private int _marginDp;
        private float _cornerRadius;
        private Color _checkedTextColor;
        private IDictionary<int, TransitionDrawable> _drawableMap;
        private int _lastCheckId;
        private IOnCheckedChangeListener _checkedChangeListener;

        public SegmentedGroup(Context context)
            : base(context)
        {
            _layoutSelector = new LayoutSelector(this, _cornerRadius);
            SetDefaultValues();
        }

        public SegmentedGroup(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            _layoutSelector = new LayoutSelector(this, _cornerRadius);
            SetDefaultValues();
            InitAttrs(attrs);
        }

        private Resources.Theme Theme => Context.Theme;

        private Color GetDefaultTintColor() => new Color(ContextCompat.GetColor(Context, Resource.Color.radio_button_selected_color));
        private Color GetDefaultUnCheckedTintColor() => new Color(ContextCompat.GetColor(Context, Resource.Color.radio_button_unselected_color));
        private Color GetDefaultCheckedTextColor() => new Color(ContextCompat.GetColor(Context, Android.Resource.Color.White));
        private int GetDefaultMarginDp() => (int) Resources.GetDimension(Resource.Dimension.radio_button_stroke_border);
        private float GetDefaultCornerRadius() => Resources.GetDimension(Resource.Dimension.radio_button_conner_radius);

        private void SetDefaultValues()
        {
            _tintColor = GetDefaultTintColor();
            _unCheckedTintColor = GetDefaultUnCheckedTintColor();
            _marginDp = GetDefaultMarginDp();
            _cornerRadius = GetDefaultCornerRadius();
        }

        /// <summary>
        /// Reads the attributes from the layout
        /// </summary>
        private void InitAttrs(IAttributeSet attrs)
        {
            if (attrs == null)
            {
                return;
            }

            var typedArray = Theme.ObtainStyledAttributes(attrs, Resource.Styleable.SegmentedGroup, 0, 0);

            try
            {
                _marginDp = (int)typedArray.GetDimension(
                    Resource.Styleable.SegmentedGroup_sc_border_width,
                    GetDefaultMarginDp());

                _cornerRadius = typedArray.GetDimension(
                    Resource.Styleable.SegmentedGroup_sc_corner_radius,
                    GetDefaultCornerRadius());

                _tintColor = typedArray.GetColor(Resource.Styleable.SegmentedGroup_sc_tint_color, GetDefaultTintColor());
                _checkedTextColor = typedArray.GetColor(Resource.Styleable.SegmentedGroup_sc_checked_text_color, GetDefaultCheckedTextColor());
                _unCheckedTintColor = typedArray.GetColor(Resource.Styleable.SegmentedGroup_sc_unchecked_tint_color, GetDefaultUnCheckedTintColor());
            }
            finally
            {
                typedArray.Recycle();
            }
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            // Use holo light for default
            UpdateBackground();
        }

        public void SetTintColor(Color tintColor)
        {
            _tintColor = tintColor;
            UpdateBackground();
        }

        public void SetTintColor(Color tintColor, Color checkedTextColor)
        {
            _tintColor = tintColor;
            _checkedTextColor = checkedTextColor;
            UpdateBackground();
        }

        public void SetUnCheckedTintColor(Color unCheckedTintColor, Color unCheckedTextColor)
        {
            _unCheckedTintColor = unCheckedTextColor;
            UpdateBackground();
        }

        private void UpdateBackground()
        {
            _drawableMap = new Dictionary<int, TransitionDrawable>();
            for (var i = 0; i < ChildCount; i++)
            {
                var child = GetChildAt(i);
                UpdateBackground(child);

                // if this is the last view, don't set LayoutParams
                if (i == ChildCount - 1)
                {
                    break;
                }

                var initParams = (LayoutParams)child.LayoutParameters;
                var layoutParams = new LayoutParams(initParams.Width, initParams.Height, initParams.Weight);
                // Check orientation for proper margins
                if (Orientation == Android.Widget.Orientation.Horizontal)
                {
                    layoutParams.SetMargins(0, 0, -_marginDp, 0);
                }
                else
                {
                    layoutParams.SetMargins(0, 0, 0, -_marginDp);
                }
                child.LayoutParameters = layoutParams;
            }
        }

        private void UpdateBackground(View view)
        {
            var checkedLayout = LayoutSelector.SelectedLayout;
            var uncheckedLayout = LayoutSelector.UnselectedLayout;
            // Set text color
            var states = new []
            {
                new [] {-Android.Resource.Attribute.StateChecked},
                new [] {Android.Resource.Attribute.StateChecked}
            };
            var colors = new[] {_tintColor.ToArgb(), _checkedTextColor.ToArgb()};
            var colorStateList = new ColorStateList(states, colors);
            ((Button)view).SetTextColor(colorStateList);

            // Redraw with tint color
            var checkedDrawable = ContextCompat.GetDrawable(Context, checkedLayout).Mutate() as GradientDrawable;
            var uncheckedDrawable = ContextCompat.GetDrawable(Context, uncheckedLayout).Mutate() as GradientDrawable;

            checkedDrawable?.SetColor(_tintColor);
            checkedDrawable?.SetStroke(_marginDp, _tintColor);

            uncheckedDrawable?.SetStroke(_marginDp, _tintColor);
            uncheckedDrawable?.SetColor(_unCheckedTintColor);

            // Set proper radius
            checkedDrawable?.SetCornerRadii(_layoutSelector.GetChildRadius(view));
            uncheckedDrawable?.SetCornerRadii(_layoutSelector.GetChildRadius(view));

            var maskDrawable = ContextCompat.GetDrawable(Context, uncheckedLayout).Mutate() as GradientDrawable;
            maskDrawable?.SetStroke(_marginDp, _tintColor);
            maskDrawable?.SetColor(_unCheckedTintColor);
            maskDrawable?.SetCornerRadii(_layoutSelector.GetChildRadius(view));
            var maskColor = Color.Argb(50, _tintColor.R, _tintColor.G, _tintColor.B);
            maskDrawable?.SetColor(maskColor);
            var pressedDrawable = new LayerDrawable(new Drawable[] { uncheckedDrawable, maskDrawable });

            var drawables = new Drawable[] {uncheckedDrawable, checkedDrawable};
            var transitionDrawable = new TransitionDrawable(drawables);
            if (((RadioButton)view).Checked)
            {
                transitionDrawable.ReverseTransition(0);
            }

            var stateListDrawable = new StateListDrawable();
            stateListDrawable.AddState(new int[] {-Android.Resource.Attribute.StateChecked, Android.Resource.Attribute.StatePressed}, pressedDrawable);
            stateListDrawable.AddState(StateSet.WildCard.ToArray(), transitionDrawable);

            _drawableMap[view.Id] = transitionDrawable;

            // Set button background
            if ((int)Build.VERSION.SdkInt >= 16)
            {
                view.Background = stateListDrawable;
            }
            else
            {
                view.SetBackgroundDrawable(stateListDrawable);
            }

            base.SetOnCheckedChangeListener(this);
        }

        public void OnCheckedChanged(RadioGroup @group, int checkedId)
        {
            var current = _drawableMap[checkedId];
            current.ReverseTransition(200);
            if (_lastCheckId != 0)
            {
                TransitionDrawable last;
                if (_drawableMap.TryGetValue(_lastCheckId, out last))
                {
                    last.ReverseTransition(200);
                }
            }
            _lastCheckId = checkedId;

            _checkedChangeListener?.OnCheckedChanged(group, checkedId);
        }

        public override void SetOnCheckedChangeListener(IOnCheckedChangeListener listener)
        {
            _checkedChangeListener = listener;
        }
    }
}
