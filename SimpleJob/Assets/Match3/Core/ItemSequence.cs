using Match3.Interfaces;
using System;
using System.Collections.Generic;


namespace Match3.Core
{
    public class ItemSequence<TGridSlot> where TGridSlot : IGridSlot
    {
        public ItemSequence(Type sequenceDetectorType, IReadOnlyList<TGridSlot> solvedGridSlots)
        {
            SequenceDetectorType = sequenceDetectorType;
            SolvedGridSlots = solvedGridSlots;
        }

        public Type SequenceDetectorType { get; }
        public IReadOnlyList<TGridSlot> SolvedGridSlots { get; }
    }
}