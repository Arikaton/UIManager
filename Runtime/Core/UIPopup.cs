using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UIManagement.Core
{
    public class UIPopup : UIView
    {
        private List<UIPopup> _children = new List<UIPopup>();
        [SerializeField] private UIPopup parent;
        public override bool IsPopup => true;

        protected override void Awake()
        {
            base.Awake();
            if (parent)
            {
                parent.AddChild(this);
            }
        }

        private void AddChild(UIPopup child)
        {
            _children.Add(child);
        }

        public override UIView GetUIView()
        {
            return parent ? parent : this;
        }

        public override void Show()
        {
            if (!_showAnimation)
            {
                InstantShow();
                return;
            }
            
            base.Show();
            if (parent)
            {
                parent.Show();
                foreach (var child in parent._children.Where(popup => popup != this))
                {
                    child.HideWithoutParent();
                }
            }
        }

        public override void InstantShow()
        {
            base.InstantShow();
            if (parent)
            {
                parent.InstantShow();
                foreach (var child in parent._children.Where(popup => popup != this))
                {
                    child.InstantHideWithoutParent();
                }
            }
        }

        public override void Hide()
        {
            if (!_hideAnimation)
            {
                InstantHide();
                return;
            }
            base.Hide();
            if (parent)
                parent.Hide();
        }

        public override void InstantHide()
        {
            base.InstantHide();
            if (parent)
                parent.InstantHide();
        }

        private void HideWithoutParent()
        {
            if (!_hideAnimation)
            {
                InstantHideWithoutParent();
                return;
            }
            base.Hide();
        }
        
        private void InstantHideWithoutParent()
        {
            base.InstantHide();
        }
    }
}