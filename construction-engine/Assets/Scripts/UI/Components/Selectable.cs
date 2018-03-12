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

            private static object globalSelectionLock = new object();

            /// <summary>
            /// Updates which object is currently selected in the UI.
            /// </summary>
            /// <param name="selection">The Selectable component of the new selected GameObject.</param>
            public static void UpdateSelection(Selectable selection)
            {
                lock (globalSelectionLock)
                {
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

        /// <summary>
        /// Unity Start method
        /// </summary>
        protected virtual void Start()
        {
            // Register a custom event system for this GameObject.
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

        /// <summary>
        /// Unity OnDisable method
        /// </summary>
        protected void OnDisable()
        {
            IsMouseOver = false;
            IsMouseDown = false;
            IsSelected = false;

            AfterEvent();
        }

        /// <summary>
        /// Select this GameObject
        /// NOTE: this method wraps InternalSelect.
        /// </summary>
        public void Select()
        {
            IsSelected = true;

            InternalSelect();

            AfterEvent();
        }

        /// <summary>
        /// Deselect this GameObject
        /// NOTE: this method wraps InternalDeselect.
        /// </summary>
        public void Deselect()
        {
            if (SelectionManager.Selected == this ||
                SelectionManager.Selected != null &&
                SelectionManager.Selected.SelectionParent == this)
            {
                // Stop deselecting once we hit the selection
                return;
            }

            IsSelected = false;

            if (SelectionParent != null)
            {
                // Entire tree should deselect
                SelectionParent.Deselect();
            }

            InternalDeselect();

            AfterEvent();
        }

        /// <summary>
        /// Enable this selectable instance.
        /// </summary>
        public virtual void Enable()
        {
            IsEnabled = true;

            AfterEvent();
        }

        /// <summary>
        /// Disable this selectable instance.
        /// </summary>
        public virtual void Disable()
        {
            IsEnabled = false;

            if (SelectionManager.Selected == this)
            {
                SelectionManager.UpdateSelection(null);
            }

            AfterEvent();
        }

        /// <summary>
        /// Click event that is wired into the UI event system. 
        /// </summary>
        /// <param name="eventData">Event system data</param>
        public virtual void Click(BaseEventData eventData)
        {
            var pointerEventData = eventData as PointerEventData;
            if (pointerEventData == null || pointerEventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (IsEnabled && Toggleable)
            {
                SelectionManager.UpdateSelection(this);
            }

            // NB: Disabled and non-selectable (not toggleable) elements don't update selection

            AfterEvent();
        }

        /// <summary>
        /// MouseOver event that is wired into the UI event system.
        /// </summary>
        /// <param name="eventData">Event system data</param>
        public virtual void MouseOver(BaseEventData eventData)
        {
            if (IsEnabled)
            {
                IsMouseOver = true;
            }

            AfterEvent();
        }

        /// <summary>
        /// MouseOut event that is wired into the UI event system.
        /// </summary>
        /// <param name="eventData">Event system data</param>
        public virtual void MouseOut(BaseEventData eventData)
        {
            IsMouseOver = false;
            IsMouseDown = false;

            AfterEvent();
        }

        /// <summary>
        /// MouseDown event that is wired into the UI event system.
        /// </summary>
        /// <param name="eventData">Event system data</param>
        public virtual void MouseDown(BaseEventData eventData)
        {
            var pointerEventData = eventData as PointerEventData;
            if (pointerEventData == null || pointerEventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (IsEnabled)
            {
                IsMouseDown = true;
            }

            AfterEvent();
        }

        /// <summary>
        /// MouseUp event that is wired into the UI event system.
        /// </summary>
        /// <param name="eventData">Event system data</param>
        public virtual void MouseUp(BaseEventData eventData)
        {
            var pointerEventData = eventData as PointerEventData;
            if (pointerEventData == null || pointerEventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            IsMouseDown = false;

            AfterEvent();
        }

        /// <summary>
        /// Inhereted objects can override this method to hook the Select event.
        /// </summary>
        protected virtual void InternalSelect() { }

        /// <summary>
        /// Inhereted objects can override this method to hook the Deselect event.
        /// </summary>
        protected virtual void InternalDeselect() { }

        /// <summary>
        /// Inhereted objects can override this method to hook the Deselect event.
        /// </summary>
        public virtual void AfterEvent() { }
    }
}
