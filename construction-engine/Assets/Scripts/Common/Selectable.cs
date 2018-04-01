using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common
{
    /// <summary>
    /// The Unity EventSystem is great, but the behavior did not fit into my menu model perfectly.
    /// Specifically: 
    ///    UNITY: The Unselect event is fired before MouseDown, or Select
    ///    UNITY: MouseDown unselects the current selection
    /// 
    /// This Selectable base class implements my own behavior for selecting elements.
    ///    CUSTOM: Unselect checks a Parent/Child relationship before unselecting an element
    ///    CUSTOM: Selection is not updated until a new object is selected (including root Canvas)
    /// </summary>
    public class Selectable : MonoBehaviour
    {
        /// <summary>
        /// Whether the object can be selected.
        /// </summary>
        public bool IsEnabled = true;

        /// <summary>
        /// Whether the object remains selected after a click.
        /// </summary>
        public bool Toggleable = true;

        /// <summary>
        /// Tooltip message to display over this button when hovered over.
        /// </summary>
        public string Tooltip = string.Empty;

        /// <summary>
        /// Delay in seconds before the tooltip will pop up.
        /// </summary>
        public float TooltipDelay = 1.0f;

        /// <summary>
        /// Indicates that this object is the child of an object. 
        /// If a child is selected after a parent, the parent stays selected. 
        /// If a child is unselected, all parents are unselected.
        /// </summary>
        public Selectable SelectionParent;

        /// <summary>Action to invoke when selected (one per click).</summary>
        public Action OnSelect { get; set; }

        /// <summary>Action to invoke when deselected.</summary>
        public Action OnDeselect { get; set; }

        /// <summary>Action to invoke when enabled.</summary>
        public Action OnEnabled { get; set; }

        /// <summary>Action to invoke when disabled.</summary>
        public Action OnDisabled { get; set; }

        /// <summary>Action to invoke each step the mouse is down on it.</summary>
        public Action OnMouseOver { get; set; }

        /// <summary>Action to invoke each step the mouse is down on it.</summary>
        public Action OnMouseOut { get; set; }

        /// <summary>Action to invoke each step the mouse is down on it.</summary>
        public Action WhileMouseDown { get; set; }

        /// <summary>
        /// Whether the object is selected.
        /// </summary>
        public bool IsSelected { get; private set; }

        /// <summary>
        /// Whether the mouse is over the object.
        /// </summary>
        public bool IsMouseOver { get; private set; }

        /// <summary>
        /// Whether the mouse is down on the object.
        /// </summary>
        public bool IsMouseDown { get; private set; }

        private EventTrigger _eventTrigger;
        private float _tooltipCount;

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
        /// Unity Update method.
        /// </summary>
        protected void Update()
        {
            // Tooltip Popup
            if (IsMouseOver && !string.IsNullOrEmpty(Tooltip))
            {
                _tooltipCount += Time.deltaTime;

                if (_tooltipCount > TooltipDelay)
                {
                    TooltipManager.PopUp(Tooltip);
                }
            }

            // Call the MouseDown event each step the mouse is down
            if (IsMouseDown && WhileMouseDown != null)
            {
                WhileMouseDown();
            }
        }

        /// <summary>
        /// Unity OnDisable method
        /// </summary>
        protected virtual void OnDisable()
        {
            IsSelected = false;
            IsMouseOver = false;
            IsMouseDown = false;

            AfterEvent();
        }

        /// <summary>
        /// Select this GameObject
        /// </summary>
        public void Select()
        {
            IsSelected = true;

            if (OnSelect != null)
            {
                OnSelect();
            }

            AfterEvent();
        }

        /// <summary>
        /// Deselect this GameObject
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

            if (OnDeselect != null)
            {
                OnDeselect();
            }

            AfterEvent();
        }

        /// <summary>
        /// Enable this selectable instance.
        /// </summary>
        public void Enable()
        {
            IsEnabled = true;
            _eventTrigger.enabled = true;

            if (OnEnabled != null)
            {
                OnEnabled();
            }

            AfterEvent();
        }

        /// <summary>
        /// Disable this selectable instance.
        /// </summary>
        public void Disable()
        {
            IsEnabled = false;
            _eventTrigger.enabled = false;

            if (SelectionManager.Selected == this)
            {
                SelectionManager.UpdateSelection(null);
            }

            if (OnDisabled != null)
            {
                OnDisabled();
            }

            AfterEvent();
        }

        /// <summary>
        /// Click event that is wired into the event system. 
        /// </summary>
        /// <param name="eventData">Event system data</param>
        public void Click(BaseEventData eventData)
        {
            var pointerEventData = eventData as PointerEventData;
            if (pointerEventData == null || pointerEventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (!Toggleable)
            {
                // Only select if toggleable (non-toggleable objects don't affect the global selection).
                return;
            }

            SelectionManager.UpdateSelection(this);

            AfterEvent();
        }

        /// <summary>
        /// MouseOver event that is wired into the event system.
        /// </summary>
        /// <param name="eventData">Event system data</param>
        public void MouseOver(BaseEventData eventData)
        {
            IsMouseOver = true;
            _tooltipCount = 0.0f;

            if (OnMouseOver != null)
            {
                OnMouseOver();
            }

            AfterEvent();
        }

        /// <summary>
        /// MouseOut event that is wired into the event system.
        /// </summary>
        /// <param name="eventData">Event system data</param>
        public void MouseOut(BaseEventData eventData)
        {
            IsMouseOver = false;
            IsMouseDown = false;

            TooltipManager.PopDown();

            if (OnMouseOut != null)
            {
                OnMouseOut();
            }

            AfterEvent();
        }

        /// <summary>
        /// MouseDown event that is wired into the event system.
        /// </summary>
        /// <param name="eventData">Event system data</param>
        public virtual void MouseDown(BaseEventData eventData)
        {
            var pointerEventData = eventData as PointerEventData;
            if (pointerEventData == null || pointerEventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            IsMouseDown = true;

            AfterEvent();
        }

        /// <summary>
        /// MouseUp event that is wired into the event system.
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
        /// Inhereted objects can override this method to update after any event.
        /// </summary>
        public virtual void AfterEvent() { }
    }
}