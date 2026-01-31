using System;

namespace Match3.Core
{
    public abstract class LevelGoal<TGridSlot> : Interfaces.ISolvedSequencesConsumer<TGridSlot> where TGridSlot : Interfaces.IGridSlot
    {
        public bool IsAchieved { get; private set; }

        public event EventHandler Achieved;

        public abstract void OnSequencesSolved(SolvedData<TGridSlot> solvedData);

        protected void MarkAchieved()
        {
            IsAchieved = true;
            Achieved?.Invoke(this, EventArgs.Empty);
        }
    }
}