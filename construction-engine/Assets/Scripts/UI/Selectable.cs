using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    /// <summary>
    /// The Unity EventSystem is great, but the behavior did not fit into my menu model perfectly.
    /// Specifically: 
    ///    UNITY: The Unselect event is fired before MouseDown, or Select
    ///    UNITY: MouseDown unselects the current selection
    /// 
    /// This Selectable base class implements my own behavior for selecting UI elements.
    ///    CUSTOM: Unselect checks a Parent/Child relationship before unselecting an element
    ///    CUSTOM: Selection is not updated until a new object is selected (including root Canvas)
    /// </summary>
    public class Selectable : MonoBehaviour
    {
        /// <summary>
        /// Static tracker of the UI element that is selected.
        /// </summary>
        public static class SelectionManager
        {
            public static Selectable Selected { get { return globalSelection; } }

            private static Selectable globalSelection = null;

            public static void UpdateSelection(Selectable selection)
            {
                // NOTE: thread safety?

                if (globalSelection == selection && selection != null)
                {
                    selection = selection.SelectionParent;  // move up the selection if you select it again
                }

                var oldSelection = globalSelection;
                globalSelection = selection;

                if (oldSelection != null)
                {
                    oldSelection.Deselect();
                }

                if (globalSelection != null)
                {
                    globalSelection.Select();
                }
            }
        }

        /// <summary>
        /// Whether the object can be selected.
        /// </summary>
        public bool IsEnabled = true;

        /// <summary>
        /// Whether the object remains selected after a click.
        /// </summary>
        public bool Toggleable = true;

        /// <summary>
        /// Indicates that this object is the child of an object. 
        /// NOTE: If a child is selected after a parent, the parent stays selected. 
        /// If a child is unselected, all parents are unselected.
        /// </summary>
        public Selectable SelectionParent;

        protected bool IsMouseOver;
        protected bool IsMouseDown;
        protected bool IsSelected;

        private EventTrigger _eventTrigger;

        protected virtual void Start()
        {
            _eventTrigger = gameObject.AddComponent<EventTrigger>();

            {
                var click = new EventTrigger.Entry() { eventID = EventTriggerType.PointerClick };
                click.callback.AddListener(Click);
                _eventTrigger.triggers.Add(click);
            }

            {
                var pointerEnter = new EventTrigger.Entry() { eventID = EventTriggerType.PointerEnter };
                pointerEnter.callback.AddListener(MouseOver);
                _eventTrigger.triggers.Add(pointerEnter);
            }

            {
                var pointerExit = new EventTrigger.Entry() { eventID = EventTriggerType.PointerExit };
                pointerExit.callback.AddListener(MouseOut);
                _eventTrigger.triggers.Add(pointerExit);
            }

            {
                var pointerDown = new EventTrigger.Entry() { eventID = EventTriggerType.PointerDown };
                pointerDown.callback.AddListener(MouseDown);
                _eventTrigger.triggers.Add(pointerDown);
            }

            {
                var pointerUp = new EventTrigger.Entry() { eventID = EventTriggerType.PointerUp };
                pointerUp.callback.AddListener(MouseUp);
                _eventTrigger.triggers.Add(pointerUp);
            }

            // NB: I am not using Unity's Select or Unselect triggers.
        }

        protected void OnDisable()
        {
            IsMouseOver = false;
            IsMouseDown = false;
            IsSelected = false;

            PostEvent();
        }

        public void Select()
        {
            InternalSelect();

            PostEvent();
        }

        public void Deselect()
        {
            if (SelectionManager.Selected == this ||
                SelectionManager.Selected != null &&
                SelectionManager.Selected.SelectionParent == this)
            {
                // Don't deselect if we are a part of the selection tree
                return;
            }

            InternalDeselect();

            PostEvent();
        }

        protected virtual void InternalSelect()
        {
            IsSelected = true;
        }

        protected virtual void InternalDeselect()
        {
            IsSelected = false;

            // Entire tree should deselect
            if (SelectionParent != null)
            {
                SelectionParent.Deselect();
            }
        }

        public virtual void Enable()
        {
            IsEnabled = true;

            PostEvent();
        }

        public virtual void Disable()
        {
            IsEnabled = false;

            if (SelectionManager.Selected == this)
            {
                SelectionManager.UpdateSelection(null);
            }

            PostEvent();
        }

        public virtual void Click(BaseEventData eventData)
        {
            if (IsEnabled && Toggleable)
            {
                SelectionManager.UpdateSelection(this);
            }

            // NB: Disabled and non-selectable (not toggleable) elements don't update selection

            PostEvent();
        }

        public virtual void MouseOver(BaseEventData eventData)
        {
            if (IsEnabled)
            {
                IsMouseOver = true;
            }

            PostEvent();
        }

        public virtual void MouseOut(BaseEventData eventData)
        {
            IsMouseOver = false;
            IsMouseDown = false;

            PostEvent();
        }

        public virtual void MouseDown(BaseEventData eventData)
        {
            if (IsEnabled)
            {
                IsMouseDown = true;
            }

            PostEvent();
        }

        public virtual void MouseUp(BaseEventData eventData)
        {
            IsMouseDown = false;

            PostEvent();
        }

        public virtual void PostEvent()
        {
            // Called after each event update.
        }
    }
}
