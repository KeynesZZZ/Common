using UnityEngine;
using Cysharp.Threading.Tasks;
using BingoGame.Core;
using BingoGame.Core.Models;
using BingoGame.GameModes.ClassicBingo;

namespace BingoGame.GameModes.ClassicBingo
{
    /// <summary>
    /// 经典数字Bingo游戏模式
    /// </summary>
    public class ClassicBingoGame : BaseGameMode
    {
        /// <summary>
        /// Bingo棋盘
        /// </summary>
        private BingoBoard bingoBoard;

        /// <summary>
        /// 玩法类型
        /// </summary>
        public override GameMode ModeType => GameMode.ClassicBingo;

        /// <summary>
        /// 玩法名称
        /// </summary>
        public override string ModeName => "经典数字Bingo";

        /// <summary>
        /// 玩法描述
        /// </summary>
        public override string Description => "标记数字，达成连线获胜";

        /// <summary>
        /// 初始化游戏模式
        /// </summary>
        public override async UniTask InitializeAsync()
        {
            Debug.Log($"初始化 {ModeName}");
            
            bingoBoard = new BingoBoard();
            bingoBoard.GenerateRandomNumbers(1, 75);
            
            board = bingoBoard;
            
            var view = ServiceLocator.GetService<IBingoBoardView>();
            if (view != null)
            {
                boardViewModel = new BingoBoardViewModel(bingoBoard, view);
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
        /// <returns>Bingo棋盘</returns>
        protected override GameBoard CreateBoard()
        {
            return new BingoBoard();
        }

        /// <summary>
        /// 创建棋盘视图模型
        /// </summary>
        /// <param name="board">棋盘</param>
        /// <returns>视图模型</returns>
        protected override GameBoardViewModel CreateBoardViewModel(GameBoard board)
        {
            var bingoBoard = board as BingoBoard;
            var view = ServiceLocator.GetService<IBingoBoardView>();
            return new BingoBoardViewModel(bingoBoard, view);
        }

        /// <summary>
        /// 标记指定数字
        /// </summary>
        /// <param name="number">要标记的数字</param>
        /// <returns>是否成功标记</returns>
        public bool MarkNumber(int number)
        {
            return bingoBoard.MarkNumber(number);
        }

        /// <summary>
        /// 检查是否有指定数字
        /// </summary>
        /// <param name="number">要查找的数字</param>
        /// <returns>是否包含该数字</returns>
        public bool HasNumber(int number)
        {
            return bingoBoard.HasNumber(number);
        }
    }
}
