namespace BlockBlast
{
    /// <summary>
    /// 方块形状枚举
    /// </summary>
    public enum BlockShape
    {
        Single,         // 1x1 单格
        Horizontal2,    // 1x2 横向双格
        Vertical2,      // 2x1 纵向双格
        Horizontal3,    // 1x3 横向三格
        Vertical3,      // 3x1 纵向三格
        Square2x2,      // 2x2 正方形
        LShape,         // L形
        LShapeReverse,  // 反L形
        TShape,         // T形
        ZShape,         // Z形
        ZShapeReverse,  // 反Z形
        Cross,          // 十字形
    }
}
