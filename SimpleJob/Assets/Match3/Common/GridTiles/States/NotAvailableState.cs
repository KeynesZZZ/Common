using Match3Game.Enums;

namespace Match3Game.GridTiles.States
{
    public class NotAvailableState : GridTile
    {
        public override int GroupId => (int) TileGroup.Unavailable;
        public override bool IsLocked => true;
        public override bool CanContainItem => false;
    }
}