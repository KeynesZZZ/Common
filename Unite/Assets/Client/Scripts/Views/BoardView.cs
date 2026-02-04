using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BingoClient.Models;
using BingoClient.Controllers;

namespace BingoClient.Views
{
    /// <summary>
    /// 棋盘视图 - 负责显示游戏棋盘和格子
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private GridLayoutGroup _gridLayout;

        private List<SlotView> _slotViews = new();
        private List<BoardData> _boards;
        private ClientEventBus _eventBus;
        private BoardController _boardController;

        private void Awake()
        {
            _eventBus = BingoClient.Utilities.ServiceLocator.GetService<ClientEventBus>();
            _boardController = FindObjectOfType<BoardController>();
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _eventBus.Subscribe<ClientEvents.SlotClicked>(OnSlotClicked);
        }

        public void Initialize(BoardData boardData)
        {
            CreateSlots(boardData);
        }

        private void CreateSlots(BoardData boardData)
        {
            foreach (var slot in boardData.Slots)
            {
                var slotView = Instantiate(_slotPrefab, _slotContainer).GetComponent<SlotView>();
                slotView.Initialize(slot);
                slotView.OnClicked += OnSlotViewClicked;
                _slotViews.Add(slotView);
            }
        }

        private async void OnSlotViewClicked(SlotView slotView)
        {
            if (_boardController != null)
            {
                await _boardController.ClickSlotAsync(0, slotView.SlotIndex);
            }
        }

        private void OnSlotClicked(ClientEvents.SlotClicked eventData)
        {
            var slotView = _slotViews.FirstOrDefault(s => s.SlotIndex == eventData.SlotIndex);
            slotView?.SetMarked(eventData.IsMarked);
        }

        public void UpdateSlot(int slotIndex, bool isMarked)
        {
            if (slotIndex >= 0 && slotIndex < _slotViews.Count)
            {
                _slotViews[slotIndex].SetMarked(isMarked);
            }
        }

        public void HighlightWinLines(List<WinLine> winLines)
        {
            foreach (var winLine in winLines)
            {
                foreach (var slotIndex in winLine.SlotIndices)
                {
                    if (slotIndex >= 0 && slotIndex < _slotViews.Count)
                    {
                        _slotViews[slotIndex].HighlightWin();
                    }
                }
            }
        }
    }
}