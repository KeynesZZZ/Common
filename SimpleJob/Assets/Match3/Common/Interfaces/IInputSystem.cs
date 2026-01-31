using System;
using Match3Game.Models;

namespace Match3Game.Interfaces
{
    public interface IInputSystem
    {
        event EventHandler<PointerEventArgs> PointerDown;
        event EventHandler<PointerEventArgs> PointerDrag;
        event EventHandler<PointerEventArgs> PointerUp;
    }
}