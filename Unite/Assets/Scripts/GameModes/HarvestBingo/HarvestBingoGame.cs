using UnityEngine;
using Cysharp.Threading.Tasks;
using BingoGame.Core;
using BingoGame.Core.Models;
using BingoGame.GameModes.HarvestBingo;

namespace BingoGame.GameModes.HarvestBingo
{
    /// <summary>
    /// 割草Bingo游戏模式
    /// </summary>
    public class HarvestBingoGame : BaseGameMode
    {
        /// <summary>
        /// 割草棋盘
        /// </summary>
        private HarvestBoard harvestBoard;

        /// <summary>
        /// 玩法类型
        /// </summary>
        public override GameMode ModeType => GameMode.HarvestBingo;

        /// <summary>
        /// 玩法名称
        /// </summary>
        public override string ModeName => "割草Bingo";

        /// <summary>
        /// 玩法描述
        /// </summary>
        public override string Description => "收割草丛，收集道具加速";

        /// <summary>
        /// 初始化游戏模式
        /// </summary>
        public override async UniTask InitializeAsync()
        {
            Debug.Log($"初始化 {ModeName}");
            
            harvestBoard = new HarvestBoard();
            board = harvestBoard;
            
            var view = ServiceLocator.GetService<IHarvestBoardView>();
            if (view != null)
            {
                boardViewModel = new HarvestBoardViewModel(harvestBoard, view);
            }
            
            stateMachine = new GameStateMachine();
            
            if (boardViewModel != null)
            {
                await boardViewModel.InitializeAsync();
            }
            
            Debug.Log($"{ModeName} 初始化完成");
        }

        /// <summary>
        /// 创建棋盘
        /// </summary>
        /// <returns>割草棋盘</returns>
        protected override GameBoard CreateBoard()
        {
            return new HarvestBoard();
        }

        /// <summary>
        /// 创建棋盘视图模型
        /// </summary>
        /// <param name="board">棋盘</param>
        /// <returns>视图模型</returns>
        protected override GameBoardViewModel CreateBoardViewModel(GameBoard board)
        {
            var harvestBoard = board as HarvestBoard;
            var view = ServiceLocator.GetService<IHarvestBoardView>();
            return new HarvestBoardViewModel(harvestBoard, view);
        }

        /// <summary>
        /// 添加小车（汽油道具）
        /// </summary>
        /// <param name="color">颜色</param>
        public void AddCar(GrassColor color)
        {
            harvestBoard.AddCar(color);
        }

        /// <summary>
        /// 添加钥匙（钥匙道具）
        /// </summary>
        /// <param name="color">颜色</param>
        public void AddKey(GrassColor color)
        {
            harvestBoard.AddKey(color);
        }

        /// <summary>
        /// 检查是否有指定颜色的小车
        /// </summary>
        /// <param name="color">颜色</param>
        /// <returns>是否有小车</returns>
        public bool HasCar(GrassColor color)
        {
            return harvestBoard.HasCar(color);
        }

        /// <summary>
        /// 检查是否有指定颜色的钥匙
        /// </summary>
        /// <param name="color">颜色</param>
        /// <returns>是否有钥匙</returns>
        public bool HasKey(GrassColor color)
        {
            return harvestBoard.HasKey(color);
        }

        /// <summary>
        /// 获取指定颜色的小车数量
        /// </summary>
        /// <param name="color">颜色</param>
        /// <returns>小车数量</returns>
        public int GetCarCount(GrassColor color)
        {
            return harvestBoard.GetCarCount(color);
        }

        /// <summary>
        /// 获取指定颜色的钥匙数量
        /// </summary>
        /// <param name="color">颜色</param>
        /// <returns>钥匙数量</returns>
        public int GetKeyCount(GrassColor color)
        {
            return harvestBoard.GetKeyCount(color);
        }
    }
}
