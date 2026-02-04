using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BingoClient.Controllers;
using BingoClient.Models;
using BingoClient.Utilities;
using BingoClient.Events;
using BingoClient.Services;
using BingoShared.Models;
using BingoShared.Protocol;

namespace BingoClient.Tests
{
    public class GameFlowTest : MonoBehaviour
    {
        private GameController _gameController;
        private ClientEventBus _eventBus;
        private NetworkService _networkService;
        private bool _testRunning = false;

        private void Start()
        {
            StartCoroutine(RunGameFlowTest());
        }

        private IEnumerator RunGameFlowTest()
        {
            Debug.Log("=== 开始游戏流程测试 ===");
            _testRunning = true;

            yield return new WaitForSeconds(1f);

            var serviceLocator = ServiceLocator.Instance;
            _eventBus = serviceLocator.GetService<ClientEventBus>();
            _networkService = serviceLocator.GetService<NetworkService>();
            _gameController = FindObjectOfType<GameController>();

            if (_gameController == null)
            {
                Debug.LogError("未找到 GameController，请确保场景中有 GameController");
                _testRunning = false;
                yield break;
            }

            Debug.Log("步骤1: 模拟玩家加入房间");
            yield return SimulateJoinRoom();

            yield return new WaitForSeconds(1f);

            Debug.Log("步骤2: 模拟游戏初始化");
            yield return SimulateGameInitialized();

            yield return new WaitForSeconds(1f);

            Debug.Log("步骤3: 模拟游戏开始");
            yield return SimulateGameStarted();

            yield return new WaitForSeconds(1f);

            Debug.Log("步骤4: 模拟玩家点击格子");
            yield return SimulatePlayerClicks();

            yield return new WaitForSeconds(1f);

            Debug.Log("步骤5: 模拟达成Bingo");
            yield return SimulateBingoAchieved();

            yield return new WaitForSeconds(1f);

            Debug.Log("步骤6: 模拟游戏结束");
            yield return SimulateGameEnded();

            yield return new WaitForSeconds(1f);

            Debug.Log("=== 游戏流程测试完成 ===");
            _testRunning = false;
        }

        private IEnumerator SimulateJoinRoom()
        {
            var testRoomId = "test-room-001";
            var testPlayerId = "player-001";

            Debug.Log($"玩家 {testPlayerId} 尝试加入房间 {testRoomId}");

            var roomData = CreateTestRoomData();
            var joinResponse = new JoinRoomResponse
            {
                Success = true,
                Message = "成功加入房间",
                Room = roomData
            };

            GameData.Instance.RoomId = roomData.Id;
            GameData.Instance.PlayerId = testPlayerId;
            GameData.Instance.Room = roomData;
            GameData.Instance.Boards = roomData.Boards;
            GameData.Instance.Players = roomData.Players;

            _eventBus.Publish(new ClientEvents.GameInitialized
            {
                RoomData = roomData
            });

            Debug.Log($"✓ 玩家成功加入房间，房间ID: {roomData.Id}");
            yield return null;
        }

        private IEnumerator SimulateGameInitialized()
        {
            Debug.Log("游戏初始化完成，等待游戏开始...");
            yield return null;
        }

        private IEnumerator SimulateGameStarted()
        {
            _eventBus.Publish(new ClientEvents.GameStarted
            {
                StartTime = System.DateTime.Now
            });

            Debug.Log("✓ 游戏已开始，玩家可以开始点击格子");
            yield return null;
        }

        private IEnumerator SimulatePlayerClicks()
        {
            Debug.Log("模拟玩家点击3个格子");

            for (int i = 0; i < 3; i++)
            {
                var slotIndex = i * 3 + i;
                var clickResponse = new ClickSlotResponse
                {
                    Success = true,
                    SlotIndex = slotIndex,
                    IsMarked = true,
                    HasPowerUp = i == 1,
                    PowerUpResult = i == 1 ? new PowerUpResult 
                    { 
                        Type = PowerUpType.DoublePayout, 
                        Description = "双倍收益已激活" 
                    } : null,
                    IsBingo = false,
                    WinLines = new List<WinLine>(),
                    RemainingBingo = 5 - i
                };

                var feedbackData = new FeedbackData
                {
                    Type = i == 0 ? FeedbackType.Perfect : FeedbackType.Great,
                    Position = Vector3.zero,
                    Message = i == 0 ? "Perfect!" : "Great!"
                };

                _eventBus.Publish(new ClientEvents.SlotClicked
                {
                    BoardIndex = 0,
                    SlotIndex = slotIndex,
                    IsMarked = clickResponse.IsMarked,
                    HasPowerUp = clickResponse.HasPowerUp,
                    PowerUpResult = clickResponse.PowerUpResult,
                    IsBingo = clickResponse.IsBingo,
                    WinLines = clickResponse.WinLines,
                    RemainingBingo = clickResponse.RemainingBingo,
                    Feedback = feedbackData
                });

                Debug.Log($"✓ 玩家点击了格子 {slotIndex}，标记状态: {clickResponse.IsMarked}");
                yield return new WaitForSeconds(0.5f);
            }
        }

        private IEnumerator SimulateBingoAchieved()
        {
            var winLines = new List<WinLine>
            {
                new WinLine
                {
                    Type = WinLineType.Horizontal,
                    Row = 2,
                    Column = -1,
                    DiagonalIndex = -1,
                    SlotIndices = new List<int> { 10, 11, 12, 13, 14 }
                }
            };

            _eventBus.Publish(new ClientEvents.BingoAchieved
            {
                WinLines = winLines
            });

            Debug.Log($"✓ 玩家达成Bingo！连线类型: {winLines[0].Type}");
            yield return null;
        }

        private IEnumerator SimulateGameEnded()
        {
            var results = new List<PlayerResult>
            {
                new PlayerResult
                {
                    PlayerId = "player-001",
                    PlayerName = "测试玩家",
                    Score = 150,
                    BingoCount = 1,
                    Rank = 1
                },
                new PlayerResult
                {
                    PlayerId = "player-002",
                    PlayerName = "AI玩家",
                    Score = 100,
                    BingoCount = 0,
                    Rank = 2
                }
            };

            _eventBus.Publish(new ClientEvents.GameEnded
            {
                Results = results
            });

            Debug.Log($"✓ 游戏结束，玩家排名: 第{results[0].Rank}名，分数: {results[0].Score}");
            yield return null;
        }

        private RoomData CreateTestRoomData()
        {
            var roomData = new RoomData
            {
                Id = "test-room-001",
                Name = "测试房间",
                BingoCount = 1,
                Boards = new List<BoardData>(),
                Players = new List<PlayerData>()
            };

            var player = new PlayerData
            {
                Id = "player-001",
                Name = "测试玩家",
                Score = 0,
                Coins = 100,
                BingoCount = 0,
                IsLocalPlayer = true
            };

            var board = new BoardData
            {
                Id = "board-001",
                PlayerId = "player-001",
                BoardIndex = 0,
                Slots = new List<SlotData>()
            };

            for (int i = 0; i < 25; i++)
            {
                var slot = new SlotData
                {
                    Index = i,
                    Number = i + 1,
                    IsMarked = false,
                    HasPowerUp = i == 4,
                    PowerUp = i == 4 ? new PowerUpData 
                    { 
                        Type = PowerUpType.DoublePayout, 
                        IsActive = true 
                    } : null
                };
                board.Slots.Add(slot);
            }

            roomData.Boards.Add(board);
            roomData.Players.Add(player);

            return roomData;
        }

        private void OnGUI()
        {
            if (!_testRunning) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.BeginVertical();
            GUILayout.Label("=== 游戏流程测试 ===");
            GUILayout.Label("正在运行游戏流程测试...");
            GUILayout.Label("请查看控制台输出");
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}