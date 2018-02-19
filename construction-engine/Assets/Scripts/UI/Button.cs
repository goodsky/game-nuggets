using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class Button : MonoBehaviour
    {
        public static HashSet<Button> AllButtons;

        /// <summary>The default background image.</summary>
        public Sprite DefaultImage;
        
        /// <summary>The mouse over background image.</summary>
        public Sprite MouseOverImage;

        /// <summary>The selected/mouse down background image.</summary>
        public Sprite SelectedImage;

        /// <summary>(optional) Icon Image.</summary>
        public Image IconImage;

        /// <summary>Whether the button remains selected after a click.</summary>
        public bool Toggleable;

        /// <summary>Action to invoke when the button is selected (one per click).</summary>
        public Action OnSelect;

        /// <summary>Action to invoke each step the mouse is down on it.</summary>
        public Action OnMouseDown;

        private bool _enabled;
        private bool _mouseOver;
        private bool _mouseDown;
        private bool _selected;

        private Image _image;
        private EventTrigger _eventTrigger;

        void Start()
        {
            _enabled = true;
            _image = gameObject.AddComponent<Image>();
            _eventTrigger = gameObject.AddComponent<EventTrigger>();

            {
                var pointerEnter = new EventTrigger.Entry() { eventID = EventTriggerType.PointerEnter };
                pointerEnter.callback.AddListener(_ => { MouseOver(); UpdateImage(); });
                _eventTrigger.triggers.Add(pointerEnter);
            }

            {
                var pointerExit = new EventTrigger.Entry() { eventID = EventTriggerType.PointerExit };
                pointerExit.callback.AddListener(_ => { MouseOut(); UpdateImage(); });
                _eventTrigger.triggers.Add(pointerExit);
            }

            {
                var pointerDown = new EventTrigger.Entry() { eventID = EventTriggerType.PointerDown };
                pointerDown.callback.AddListener(_ => { MouseDown(); UpdateImage(); });
                _eventTrigger.triggers.Add(pointerDown);
            }

            {
                var pointerUp = new EventTrigger.Entry() { eventID = EventTriggerType.PointerUp };
                pointerUp.callback.AddListener(_ => { MouseUp(); UpdateImage(); });
                _eventTrigger.triggers.Add(pointerUp);
            }

            {
                var click = new EventTrigger.Entry() { eventID = EventTriggerType.PointerClick };
                click.callback.AddListener(_ => { Click(); });
                _eventTrigger.triggers.Add(click);
            }

            {
                var select = new EventTrigger.Entry() { eventID = EventTriggerType.Select };
                select.callback.AddListener(_ => { Selected(); UpdateImage(); });
                _eventTrigger.triggers.Add(select);
            }

            {
                var deselect = new EventTrigger.Entry() { eventID = EventTriggerType.Deselect };
                deselect.callback.AddListener(_ => { Deselect(); UpdateImage(); });
                _eventTrigger.triggers.Add(deselect);
            }

            UpdateImage();
        }

        void Update()
        {
            if (_mouseDown && OnMouseDown != null)
            {
                OnMouseDown();
            }
        }

        public void Click()
        {
            if (_enabled)
            {
                if (Toggleable)
                {
                    EventSystem.current.SetSelectedGameObject(gameObject);
                }
                else
                {
                    if (OnSelect != null)
                    {
                        OnSelect();
                    }
                }
            }
        }

        public void Enable()
        {
            _enabled = true;
        }

        public void Disable()
        {
            _enabled = false;
        }

        public void MouseOver()
        {
            _mouseOver = true;
        }

        public void MouseOut()
        {
            _mouseOver = false;
            _mouseDown = false;
        }

        public void MouseDown()
        {
            _mouseDown = true;
        }

        public void MouseUp()
        {
            _mouseDown = false;
        }

        public void Selected()
        {
            _selected = true;

            if (OnSelect != null)
            {
                OnSelect();
            }
        }

        public void Deselect()
        {
            _selected = false;
        }

        public void UpdateImage()
        {
            if (_enabled)
            {
                if (IconImage != null)
                {
                    // Normal tint icon
                    IconImage.color = Color.white;
                }

                if (_selected || _mouseDown)
                {
                    _image.sprite = SelectedImage;
                }
                else if (_mouseOver)
                {
                    _image.sprite = MouseOverImage;
                }
                else
                {
                    _image.sprite = DefaultImage;
                }
            }
            else
            {
                if (IconImage != null)
                {
                    // Grey-out icon when disabled
                    IconImage.color = Color.grey;
                }
            }
        }
    }
}
