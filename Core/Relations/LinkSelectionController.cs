using System;

namespace Neuma.Core.Relations
{
    public sealed class LinkSelectionController
    {
        private readonly IPairMatchService _pairMatchService;
        private readonly IFoundRelationsTracker _foundRelationsTracker;

        private SelectionSlotView? _firstSlot;
        private AnchorId? _firstAnchor;

        public event EventHandler<LinkSelectionStateChangedEventArgs>? OnSelectionStateChanged;

        public LinkSelectionController(IPairMatchService pairMatchService, IFoundRelationsTracker foundRelationsTracker)
        {
            _pairMatchService = pairMatchService ?? throw new ArgumentNullException(nameof(pairMatchService));
            _foundRelationsTracker = foundRelationsTracker ?? throw new ArgumentNullException(nameof(foundRelationsTracker));
        }

        public void HandleAnchorSelected(object? sender, AnchorSelectedEventArgs args)
        {
            if (args == null)
            {
                return;
            }

            if (_firstSlot == null)
            {
                _firstAnchor = args.Anchor;
                _firstSlot = new SelectionSlotView(
                    args.Anchor.ObjectId,
                    args.DisplayName
                 );

                EmitState(lastResult: null);
            }
            else
            {
                if (_firstAnchor.Value == args.Anchor)
                {
                    ClearSelection();
                    return;
                }

                var secondSlot = new SelectionSlotView(
                    args.Anchor.ObjectId,
                    args.DisplayName
                 );

                EmitState(secondSlot, null);

                CheckMatch(_firstAnchor.Value, args.Anchor);
            }
        }

        public void ClearSelection()
        {
            _firstSlot = null;
            _firstAnchor = null;
            EmitState(lastResult: null);
        }

        private void CheckMatch(AnchorId first, AnchorId second)
        {
            var result = _pairMatchService.MatchPair(first, second);

            if (result.HasMatch)
            {
                _foundRelationsTracker.TryRegisterMatch(result);
            }

            _firstSlot = null;
            _firstAnchor = null;

            EmitState(secondSlot: null, lastResult: result);
        }

        private void EmitState(SelectionSlotView? secondSlot = null, PairMatchResult? lastResult = null)
        {
            var context = new LinkSelectionViewContext(_firstSlot, secondSlot, lastResult);
            OnSelectionStateChanged?.Invoke(this, new LinkSelectionStateChangedEventArgs(context));
        }
    }
}

