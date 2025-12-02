namespace Neuma.Core.Relations
{
    public record class LinkSelectionViewContext(
        SelectionSlotView? FirstSlot,
        SelectionSlotView? SecondSlot,
        PairMatchResult? LastResult
     );
}

