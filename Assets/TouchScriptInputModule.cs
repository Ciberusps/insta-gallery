/**
* Input Module to use the new unity ui with multitouch from https://github.com/TouchScript
* Install TouchScript in your unity project, then add an EventSystem and replace the InputModules by this one.
*
*
* Basically modified TouchInputModule from
* https://bitbucket.org/Unity-Technologies/ui/src/5fc21bb4ecf4b40ff6630057edaa070252909b2e/UnityEngine.UI/EventSystem/InputModules/TouchInputModule.cs?at=4.6
* and changing ProcessTouchEvent to take events from TouchScript
*
* Got the TouchScript stuff from
* https://github.com/TouchScript/TouchScript/blob/master/TouchScript/Debugging/TouchDebugger.cs
*
*/

using System.Text;
using TouchScript;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchScriptInputModule : PointerInputModule
{
    protected TouchScriptInputModule()
    { }

    //adding TouchManager handlers here (hope this works...)
    protected override void OnEnable()
    {
        base.OnEnable();

        if (TouchManager.Instance != null)
        {
            TouchManager.Instance.TouchesBegan += touchesBeganHandler;
            TouchManager.Instance.TouchesEnded += touchesEndedHandler;
            TouchManager.Instance.TouchesMoved += touchesMovedHandler;
            TouchManager.Instance.TouchesCancelled += touchesCancelledHandler;
        }
    }
    protected override void OnDisable()
    {
        if (TouchManager.Instance != null)
        {
            TouchManager.Instance.TouchesBegan -= touchesBeganHandler;
            TouchManager.Instance.TouchesEnded -= touchesEndedHandler;
            TouchManager.Instance.TouchesMoved -= touchesMovedHandler;
            TouchManager.Instance.TouchesCancelled -= touchesCancelledHandler;
        }
        base.OnDisable();
    }


    private bool UseFakeInput()
    {
        //TouchScript will probably fake input already...
        return false;
        //return !Input.touchSupported;
    }
    /// <summary>
    /// Process all touch events. Changing this to process the dummies dictionary containing pairs of int and ITouch
    /// </summary>
    public override void Process()
    {

        foreach (KeyValuePair<int, ITouch> dummy in dummies)
        {
            bool released;
            bool pressed;
            //need to create a PointerEventData
            var pointer = GetTouchScriptPointerEventData(dummy.Value, out pressed, out released);

            ProcessTouchPress(pointer, pressed, released);
            if (!released)
            {
                ProcessMove(pointer);
                ProcessDrag(pointer);
            }
            else
                RemovePointerData(pointer);
        }

        //cleanup: remove ended dummies and clear the list of ended dummie ids and began dummie ids
        foreach (int id in endedDummieIDs)
        {
            dummies.Remove(id);
        }
        endedDummieIDs.Clear();

        beganDummieIDs.Clear();
    }

    private PointerEventData GetTouchScriptPointerEventData(ITouch input, out bool pressed, out bool released)
    {
        PointerEventData pointerData;
        bool created = GetPointerData(input.Id, out pointerData, true);
        pointerData.Reset();

        // are tags the way to go here?
        pressed = created || beganDummieIDs.Contains(input.Id);
        released = endedDummieIDs.Contains(input.Id);

        if (created)
            pointerData.position = input.Position;
        if (pressed)
            pointerData.delta = Vector2.zero;
        else
            pointerData.delta = input.Position - pointerData.position; //use input.PreviousPosition instead of pointerData.position?
        pointerData.position = input.Position;
        pointerData.button = PointerEventData.InputButton.Left;
        eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
        var raycast = FindFirstRaycast(m_RaycastResultCache);
        pointerData.pointerCurrentRaycast = raycast;
        m_RaycastResultCache.Clear();
        return pointerData;
    }
    private void ProcessTouchPress(PointerEventData pointerEvent, bool pressed, bool released)
    {
        var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;
        // PointerDown notification
        if (pressed)
        {
            pointerEvent.eligibleForClick = true;
            pointerEvent.delta = Vector2.zero;
            pointerEvent.dragging = false;
            pointerEvent.useDragThreshold = true;
            pointerEvent.pressPosition = pointerEvent.position;
            pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;
            DeselectIfSelectionChanged(currentOverGo, pointerEvent);
            if (pointerEvent.pointerEnter != currentOverGo)
            {
                // send a pointer enter to the touched element if it isn't the one to select...
                HandlePointerExitAndEnter(pointerEvent, currentOverGo);
                pointerEvent.pointerEnter = currentOverGo;
            }
            // search for the control that will receive the press
            // if we can't find a press handler set the press
            // handler to be what would receive a click.
            var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);
            // didnt find a press handler... search for a click handler
            if (newPressed == null)
                newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
            // Debug.Log("Pressed: " + newPressed);
            float time = Time.unscaledTime;
            if (newPressed == pointerEvent.lastPress)
            {
                var diffTime = time - pointerEvent.clickTime;
                if (diffTime < 0.3f)
                    ++pointerEvent.clickCount;
                else
                    pointerEvent.clickCount = 1;
                pointerEvent.clickTime = time;
            }
            else
            {
                pointerEvent.clickCount = 1;
            }
            pointerEvent.pointerPress = newPressed;
            pointerEvent.rawPointerPress = currentOverGo;
            pointerEvent.clickTime = time;
            // Save the drag handler as well
            pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);
            if (pointerEvent.pointerDrag != null)
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
        }
        // PointerUp notification
        if (released)
        {
            // Debug.Log("Executing pressup on: " + pointer.pointerPress);
            ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
            // Debug.Log("KeyCode: " + pointer.eventData.keyCode);
            // see if we mouse up on the same element that we clicked on...
            var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
            // PointerClick and Drop events
            if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
            {
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
            }
            else if (pointerEvent.pointerDrag != null)
            {
                ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
            }
            pointerEvent.eligibleForClick = false;
            pointerEvent.pointerPress = null;
            pointerEvent.rawPointerPress = null;
            if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
            pointerEvent.dragging = false;
            pointerEvent.pointerDrag = null;
            if (pointerEvent.pointerDrag != null)
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
            pointerEvent.pointerDrag = null;
            // send exit events as we need to simulate this on touch up on touch device
            ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
            pointerEvent.pointerEnter = null;
        }
    }
    public override void DeactivateModule()
    {
        base.DeactivateModule();
        ClearSelection();
    }
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(UseFakeInput() ? "Input: Faked" : "Input: Touch");
        if (UseFakeInput())
        {
            var pointerData = GetLastPointerEventData(kMouseLeftId);
            if (pointerData != null)
                sb.AppendLine(pointerData.ToString());
        }
        else
        {
            foreach (var pointerEventData in m_PointerData)
                sb.AppendLine(pointerEventData.ToString());
        }
        return sb.ToString();
    }


    #region TouchScript stuff

    private Dictionary<int, ITouch> dummies = new Dictionary<int, ITouch>(10);
    private List<int> beganDummieIDs = new List<int>(10);
    private List<int> endedDummieIDs = new List<int>(10);

    private void updateDummy(ITouch dummy)
    {
        dummies[dummy.Id] = dummy;
    }

    private void touchesBeganHandler(object sender, TouchEventArgs e)
    {
        foreach (var touch in e.Touches)
        {
            dummies.Add(touch.Id, touch);
            beganDummieIDs.Add(touch.Id);
        }
    }
    private void touchesMovedHandler(object sender, TouchEventArgs e)
    {
        foreach (var touch in e.Touches)
        {
            ITouch dummy;
            if (!dummies.TryGetValue(touch.Id, out dummy)) return;
            updateDummy(touch);
        }
    }
    private void touchesEndedHandler(object sender, TouchEventArgs e)
    {
        foreach (var touch in e.Touches)
        {
            ITouch dummy;
            if (!dummies.TryGetValue(touch.Id, out dummy)) return;
            endedDummieIDs.Add(touch.Id);
            //dummies.Remove(touch.Id); //remove after process handles the touch.
        }
    }
    private void touchesCancelledHandler(object sender, TouchEventArgs e)
    {
        touchesEndedHandler(sender, e);
    }
    #endregion

}