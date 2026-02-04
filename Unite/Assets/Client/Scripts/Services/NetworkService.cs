using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using BingoShared.Protocol;
using BingoClient.Events;

namespace BingoClient.Services
{
    public class NetworkService : MonoBehaviour
    {
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly Dictionary<MessageType, Action<string>> _messageHandlers = new();
        private readonly Dictionary<MessageType, TaskCompletionSource<string>> _pendingRequests = new();

        private void Awake()
        {
            RegisterMessageHandlers();
        }

        private void RegisterMessageHandlers()
        {
            _messageHandlers[MessageType.JoinRoomResponse] = HandleJoinRoomResponse;
            _messageHandlers[MessageType.ClickSlotResponse] = HandleClickSlotResponse;
            _messageHandlers[MessageType.CallNumberResponse] = HandleCallNumberResponse;
            _messageHandlers[MessageType.GameEnd] = HandleGameEnd;
        }

        public async Task ConnectAsync(string serverUrl)
        {
            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();

            await _webSocket.ConnectAsync(new Uri(serverUrl), _cancellationTokenSource.Token);
            StartCoroutine(ReceiveMessages());
        }

        private IEnumerator ReceiveMessages()
        {
            var buffer = new byte[4096];

            while (_webSocket.State == WebSocketState.Open)
            {
                var result = _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);

                if (result.Result.MessageType == WebSocketMessageType.Close)
                {
                    _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", _cancellationTokenSource.Token);
                    break;
                }

                var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Result.Count);
                var message = JsonUtility.FromJson<NetworkMessage>(messageJson);

                if (_messageHandlers.TryGetValue(message.Type, out var handler))
                {
                    handler(message.Data);
                }

                if (_pendingRequests.TryGetValue(message.Type, out var tcs))
                {
                    tcs.SetResult(message.Data);
                    _pendingRequests.Remove(message.Type);
                }
            }

            yield return null;
        }

        public async Task<JoinRoomResponse> SendJoinRoomAsync(string roomId, string playerId)
        {
            var request = new JoinRoomRequest
            {
                RoomId = roomId,
                PlayerId = playerId,
                PlayerName = $"Player{playerId}"
            };

            var message = new NetworkMessage
            {
                Type = MessageType.JoinRoom,
                Data = JsonUtility.ToJson(request),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            await SendMessageAsync(message);
            var responseJson = await WaitForResponseAsync(MessageType.JoinRoomResponse);
            return JsonUtility.FromJson<JoinRoomResponse>(responseJson);
        }

        public async Task<ClickSlotResponse> SendClickAsync(int boardIndex, int slotIndex)
        {
            var request = new ClickSlotRequest
            {
                RoomId = BingoClient.Models.GameData.Instance.RoomId,
                PlayerId = BingoClient.Models.GameData.Instance.PlayerId,
                BoardIndex = boardIndex,
                SlotIndex = slotIndex
            };

            var message = new NetworkMessage
            {
                Type = MessageType.ClickSlot,
                Data = JsonUtility.ToJson(request),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            await SendMessageAsync(message);
            var responseJson = await WaitForResponseAsync(MessageType.ClickSlotResponse);
            return JsonUtility.FromJson<ClickSlotResponse>(responseJson);
        }

        private async Task SendMessageAsync(NetworkMessage message)
        {
            var json = JsonUtility.ToJson(message);
            var buffer = Encoding.UTF8.GetBytes(json);
            await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
        }

        private async Task<string> WaitForResponseAsync(MessageType expectedType)
        {
            var tcs = new TaskCompletionSource<string>();
            _pendingRequests[expectedType] = tcs;
            return await tcs.Task;
        }

        private void HandleJoinRoomResponse(string data)
        {
            var response = JsonUtility.FromJson<JoinRoomResponse>(data);
            var eventBus = BingoClient.Utilities.ServiceLocator.GetService<ClientEventBus>();
            eventBus.Publish(new ClientEvents.GameInitialized
            {
                RoomData = response.Room
            });
        }

        private void HandleClickSlotResponse(string data)
        {
            var response = JsonUtility.FromJson<ClickSlotResponse>(data);
            var eventBus = BingoClient.Utilities.ServiceLocator.GetService<ClientEventBus>();
            eventBus.Publish(new ClientEvents.SlotClicked
            {
                SlotIndex = response.SlotIndex,
                IsMarked = response.IsMarked,
                HasPowerUp = response.HasPowerUp,
                PowerUpResult = response.PowerUpResult,
                IsBingo = response.IsBingo,
                WinLines = response.WinLines,
                RemainingBingo = response.RemainingBingo
            });
        }

        private void HandleCallNumberResponse(string data)
        {
            var response = JsonUtility.FromJson<CallNumberResponse>(data);
            var eventBus = BingoClient.Utilities.ServiceLocator.GetService<ClientEventBus>();
            eventBus.Publish(new ClientEvents.NumberCalled
            {
                Number = response.Number,
                CalledNumbers = response.CalledNumbers
            });
        }

        private void HandleGameEnd(string data)
        {
            var response = JsonUtility.FromJson<GameEndResponse>(data);
            var eventBus = BingoClient.Utilities.ServiceLocator.GetService<ClientEventBus>();
            eventBus.Publish(new ClientEvents.GameEnded
            {
                Results = response.Results
            });
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _webSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Destroying");
        }
    }
}