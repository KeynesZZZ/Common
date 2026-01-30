using System;
using System.Collections.Generic;
using SimpleBoard.Core;
using SimpleBoard.Data;
using SimpleBoard.Interfaces;

namespace SimpleBoard
{
   public abstract class BaseGame<TGridSlot> : IDisposable where TGridSlot : IGridSlot
    {
        private readonly GameBoard<TGridSlot> _gameBoard;
        private readonly IGameBoardSolver<TGridSlot> _gameBoardSolver;
        private readonly IGameBoardDataProvider<TGridSlot> _gameBoardDataProvider;
        private readonly ISolvedSequencesConsumer<TGridSlot>[] _solvedSequencesConsumers;

        private bool _isStarted;
        private int _achievedGoals;


        protected BaseGame(IGameBoardSolver<TGridSlot> gameBoardSolver,IGameBoardDataProvider<TGridSlot>  gameBoardDataProvider ,ISolvedSequencesConsumer<TGridSlot>[]  solvedSequencesConsumers )
        {
            _gameBoard = new GameBoard<TGridSlot>();
            _gameBoardSolver=  gameBoardSolver;
            _gameBoardDataProvider = gameBoardDataProvider;
            _solvedSequencesConsumers = solvedSequencesConsumers;
        }

        protected IGameBoard<TGridSlot> GameBoard => _gameBoard;

        public event EventHandler Finished;

        public void InitGameLevel(int level)
        {
            if (_isStarted)
            {
                throw new InvalidOperationException("Can not be initialized while the current game is active.");
            }

            _gameBoard.SetGridSlots(_gameBoardDataProvider.GetGameBoardSlots(level));
        }

        protected void StartGame()
        {
            if (_isStarted)
            {
                throw new InvalidOperationException("Game has already been started.");
            }

            _isStarted = true;
            OnGameStarted();
        }

        protected void StopGame()
        {
            if (_isStarted == false)
            {
                throw new InvalidOperationException("Game has not been started.");
            }

            _isStarted = false;
            OnGameStopped();
        }

        public void ResetGameBoard()
        {
            _achievedGoals = 0;
            _gameBoard.ResetState();
        }

        public void Dispose()
        {
            _gameBoard?.Dispose();
        }

        protected abstract void OnGameStarted();
        protected abstract void OnGameStopped();

        protected bool IsSolved(GridPosition position1, GridPosition position2, out SolvedData<TGridSlot> solvedData)
        {
            solvedData = _gameBoardSolver.Solve(GameBoard, position1, position2);
            return solvedData.SolvedSequences.Count > 0;
        }

        protected bool IsSolved(List<GridPosition> gridPositions, out SolvedData<TGridSlot> solvedData)
        {
            solvedData = _gameBoardSolver.Solve(GameBoard, gridPositions.ToArray());
            return solvedData.SolvedSequences.Count > 0;
        }

        protected void NotifySequencesSolved(SolvedData<TGridSlot> solvedData)
        {
            foreach (var sequencesConsumer in _solvedSequencesConsumers)
            {
                sequencesConsumer.OnSequencesSolved(solvedData);
            }
        }
    
    }
}