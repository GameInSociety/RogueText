using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RW_SlotButton : RW_Draggable
{
    private Slot _slot;
    private Slot.Data _slotData;
    public List<Socket> _sockets = new List<Socket>();

    [SerializeField]
    private Transform _parent;
    public Image _outlineImage;

    // TEMP : display slot template ( [type](item, prop, value) )
    public void Display(Slot.Data slotData) {

        _slotData = slotData;

        Show();

        _image.color = EngineManager.Instance.GetVisual(GetSlotData()._type).color_Dark;
        _outlineImage.color = Color.white;

        UpdateSockets();
    }

    public void Display(Slot slot, Slot.Type type) {

        Show();

        _image.color = EngineManager.Instance.GetVisual(type).color;
        _outlineImage.color = EngineManager.Instance.GetVisual(type).color_Light;

        _sockets.Clear();
        _sockets.Add(new Socket(type, slot._input));
        UpdateSockets();
    }

    public void UpdateSockets() {
        foreach (var socket in _sockets) {
            var displaySocket = RW_PoolManager.Instance.Pull("DisplaySocket", _parent) as RW_DisplaySocket;
            displaySocket.Display(socket);
        }
    }

    

    public override void Merge(RW_Draggable target) {
        base.Merge(target);
    }

    public override void HandleMerge(RW_Draggable otherDraggable) {
        base.HandleMerge(otherDraggable);

        if (GetSlotData()._type == Slot.Type.Sequence) {
            Debug.Log($"pipi cucul");
            var sequenceButton = otherDraggable as RW_SequenceButton;
            if (sequenceButton != null) {
                // getting parent item of other property
                var di = sequenceButton.GetComponentInParent<RW_DisplayItem>();

                // creating sockets
                var itemSocket = new Socket(Slot.Type.Item, di._itemData.name);
                var propSocket = new Socket(Slot.Type.Sequence, sequenceButton.Sequence.name);

                _sockets.Clear();
                _sockets.Add(itemSocket);
                _sockets.Add(propSocket);
                UpdateSockets();
            }
        }

        if (GetSlotData()._type == Slot.Type.Property) {
            var propertyButton = otherDraggable as RW_PropertyButton;
            if (propertyButton != null ) {
                // getting parent item of other property
                var di = propertyButton.GetComponentInParent<RW_DisplayItem>();

                // creating sockets
                var itemSocket = new Socket(Slot.Type.Item, di._itemData.name);
                var propSocket = new Socket(Slot.Type.Property, propertyButton.GetProperty().name);

                _sockets.Clear();
                _sockets.Add(itemSocket);
                _sockets.Add(propSocket);
                UpdateSockets();
            }
        }

        if (GetSlotData()._type == Slot.Type.Item) {
            var itemDataButton = otherDraggable as RW_ItemDataButton;
            if (itemDataButton != null) {
                // creating sockets
                var itemSocket = new Socket(Slot.Type.Item, itemDataButton.ItemData.name);

                _sockets.Clear();
                _sockets.Add(itemSocket);
                UpdateSockets();
            }
        }

    }



    public bool HasSlot() {
        return _slot != null;
    }
    public Slot GetSlot() {
        return _slot;
    }
    public Slot.Data GetSlotData() {
        return _slotData;
    }

    private void OnDisable() {
        RW_PoolManager.Instance.Push("SlotButton", this);
        Hide();
    }
}
