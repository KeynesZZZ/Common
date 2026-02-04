using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BingoClient.Events;

namespace BingoClient.Views
{
    public class CallerView : MonoBehaviour
    {
        [SerializeField] private Transform _numberContainer;
        [SerializeField] private GameObject _numberBallPrefab;
        [SerializeField] private float _spacing = 50f;
        [SerializeField] private int _maxVisibleNumbers = 10;

        private List<GameObject> _numberBalls = new();
        private Queue<int> _calledNumbers = new();

        public void AddNumber(int number)
        {
            _calledNumbers.Enqueue(number);
            CreateNumberBall(number);
            UpdateLayout();
        }

        private void CreateNumberBall(int number)
        {
            var numberBall = Instantiate(_numberBallPrefab, _numberContainer);
            var text = numberBall.GetComponent<TextMeshProUGUI>();
            text.text = number.ToString();
            _numberBalls.Add(numberBall);

            var rectTransform = numberBall.GetComponent<RectTransform>();
            rectTransform.DOAnchorPosX(_numberBalls.Count * _spacing, 0.3f).SetEase(Ease.OutBack);
        }

        private void UpdateLayout()
        {
            while (_numberBalls.Count > _maxVisibleNumbers)
            {
                var oldBall = _numberBalls[0];
                _numberBalls.RemoveAt(0);
                Destroy(oldBall);
            }

            for (int i = 0; i < _numberBalls.Count; i++)
            {
                var rectTransform = _numberBalls[i].GetComponent<RectTransform>();
                rectTransform.DOAnchorPosX(i * _spacing, 0.3f).SetEase(Ease.OutBack);
            }
        }
    }
}