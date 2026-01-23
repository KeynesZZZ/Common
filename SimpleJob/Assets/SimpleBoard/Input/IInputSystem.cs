using System;

namespace SimpleBoard.Input
{
    public interface IInputSystem
    {
        event EventHandler<PointerEventArgs> PointerDown;
        event EventHandler<PointerEventArgs> PointerDrag;
        event EventHandler<PointerEventArgs> PointerUp;
    }
}