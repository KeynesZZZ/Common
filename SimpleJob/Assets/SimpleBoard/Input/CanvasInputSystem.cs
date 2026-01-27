using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SimpleBoard.Input
{
    public class CanvasInputSystem : MonoBehaviour, IInputSystem
    {
        [Header("Input Settings")]
        [SerializeField] private Camera _camera;
        [SerializeField] private EventTrigger _eventTrigger;
        [SerializeField] private bool _useInputThrottling = true;
        [SerializeField] private float _throttleInterval = 0.016f; // ~60fps

        public event EventHandler<PointerEventArgs> PointerDown;
        public event EventHandler<PointerEventArgs> PointerDrag;
        public event EventHandler<PointerEventArgs> PointerUp;

        private float _lastDragTime;
        private bool _isInitialized;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            ValidateReferences();
            SetupEventTriggers();
            _isInitialized = true;
        }

        private void ValidateReferences()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                {
                    Debug.LogError($"[{nameof(CanvasInputSystem)}] No camera assigned and no main camera found!");
                }
            }

            if (_eventTrigger == null)
            {
                _eventTrigger = GetComponent<EventTrigger>();
                if (_eventTrigger == null)
                {
                    _eventTrigger = gameObject.AddComponent<EventTrigger>();
                }
            }
        }

        private void SetupEventTriggers()
        {
            AddEventListener(EventTriggerType.PointerDown, OnPointerDown);
            AddEventListener(EventTriggerType.Drag, OnPointerDrag);
            AddEventListener(EventTriggerType.PointerUp, OnPointerUp);
        }

        private void AddEventListener(EventTriggerType eventType, Action<PointerEventData> action)
        {
            var entry = new EventTrigger.Entry { eventID = eventType };
            entry.callback.AddListener(data => action?.Invoke((PointerEventData)data));
            _eventTrigger.triggers.Add(entry);
        }

        private void OnPointerDown(PointerEventData eventData)
        {
            if (eventData == null)
            {
                return;
            }

            TryRaiseEvent(PointerDown, eventData);
        }

        private void OnPointerDrag(PointerEventData eventData)
        {
            if (eventData == null)
            {
                return;
            }

            if (_useInputThrottling)
            {
                if (Time.time - _lastDragTime < _throttleInterval)
                {
                    return;
                }
                _lastDragTime = Time.time;
            }

            TryRaiseEvent(PointerDrag, eventData);
        }

        private void OnPointerUp(PointerEventData eventData)
        {
            if (eventData == null)
            {
                return;
            }

            TryRaiseEvent(PointerUp, eventData);
        }

        private void TryRaiseEvent(EventHandler<PointerEventArgs> eventHandler, PointerEventData eventData)
        {
            try
            {
                var eventArgs = CreatePointerEventArgs(eventData);
                eventHandler?.Invoke(this, eventArgs);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(CanvasInputSystem)}] Error raising event: {ex.Message}");
            }
        }

        private PointerEventArgs CreatePointerEventArgs(PointerEventData eventData)
        {
            var worldPosition = ConvertToWorldPosition(eventData.position);
            return new PointerEventArgs(eventData.button, worldPosition);
        }

        private Vector2 ConvertToWorldPosition(Vector2 screenPosition)
        {
            if (_camera == null)
            {
                return screenPosition;
            }

            return _camera.ScreenToWorldPoint(screenPosition);
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            PointerDown = null;
            PointerDrag = null;
            PointerUp = null;
            _isInitialized = false;
        }
    }
}