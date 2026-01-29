using SimpleBoard.Data;

namespace BlockBlast
{
    /// <summary>
    /// Block Blast 方块槽位状态实现
    /// </summary>
    public class BlockSlotState : IGridSlotState
    {
        public int GroupId { get; }
        public bool IsLocked { get; }
        public bool CanContainItem { get; }

        public BlockSlotState(int groupId = 0, bool isLocked = false, bool canContainItem = true)
        {
            GroupId = groupId;
            IsLocked = isLocked;
            CanContainItem = canContainItem;
        }

        /// <summary>
        /// 创建标准可用槽位状态
        /// </summary>
        public static BlockSlotState Available() => new BlockSlotState(0, false, true);

        /// <summary>
        /// 创建锁定槽位状态
        /// </summary>
        public static BlockSlotState Locked() => new BlockSlotState(0, true, false);

        /// <summary>
        /// 创建不可用槽位状态
        /// </summary>
        public static BlockSlotState Unavailable() => new BlockSlotState(0, false, false);
    }
}
