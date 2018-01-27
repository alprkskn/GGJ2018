using System;
using System.Collections;
using System.Collections.Generic;
using Complete;
using UnityEngine;

namespace GGJ2018
{
    public class GameController : MonoBehaviour
    {
		[SerializeField] private static GameController _instance;

		public static GameController Instance
		{
			get
			{
				if(_instance == null)
				{
					_instance = GameObject.FindObjectOfType<GameController>();
				}
				return _instance;
			}
		}

        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private CameraControl _cameraController;

        private TimeSpan _duration;
        private List<PlayerController> _players;
        private int[] _ampCounts;
		private Camera _mainCamera;

        public Camera GameCamera
        {
            get
            {
                if (_mainCamera == null)
                {
                    _mainCamera = _cameraController.GetComponentInChildren<Camera>();
                }

                return _mainCamera;
            }
        }

        public void StartGame(int playerCount, float duration)
        {
            _cameraController.m_Targets = new Transform[playerCount];
            _players = new List<PlayerController>(playerCount);
            for (int i = 0; i < _spawnPoints.Length && i < playerCount; i++)
            {
                var go = Instantiate(_playerPrefab);
                go.transform.position = _spawnPoints[i].position;
                go.name = "Player_" + (i + 1);
                var controller = go.GetComponent<PlayerController>();
                _players.Add(controller);
                controller.Initialize(i);
                _cameraController.m_Targets[i] = go.transform;

                RegisterToPlayerEvents(controller);
            }

            _ampCounts = new int[playerCount];
        }

        public void ClearGame()
        {
            for (var i = 0; i < _players.Count; i++)
            {
                var player = _players[i];
                Destroy(player.gameObject);
                _cameraController.m_Targets[i] = null;

                UnregisterFromPlayerEvents(player);
            }

            _players.Clear();
        }

        void Start()
        {
            StartGame(2, 100);
        }

        void Update()
        {

        }

        private void RegisterToPlayerEvents(PlayerController playerController)
        {
            playerController.AmpPlaced += OnAmpPlaced;
        }

        private void UnregisterFromPlayerEvents(PlayerController playerController)
        {
            playerController.AmpPlaced -= OnAmpPlaced;
        }

        private void OnAmpPlaced(AmpController amp)
        {
            _ampCounts[amp.OwnerId]++;
        }

        private void OnAmpDestroyed(AmpController amp)
        {
            _ampCounts[amp.OwnerId] = Mathf.Max(0, _ampCounts[amp.OwnerId]);

        }
    }
}