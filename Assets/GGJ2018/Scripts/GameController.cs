using System;
using System.Collections;
using System.Collections.Generic;
using Complete;
using UnityEngine;
using UnityEngine.UI;
using UniversalNetworkInput.Network;
using UniversalNetworkInput.Network.Internal;

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
		[SerializeField] private Text _mainMessage;
		[SerializeField] private Text _scoreP1, _scoreP2, _timeLeft, _serverIP;

        private TimeSpan _duration;
        private List<PlayerController> _players;
		private Dictionary<int, List<AmpController>> _ampDictionary;
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

		public IEnumerator InitiateStartSequence(int countdown, int playerCount, float duration)
		{
			UNServer.Start(4, 25565, UNNetwork.GetLocalIPAddress());
			_mainMessage.gameObject.SetActive(true);
			_serverIP.text = UNServer.ip_address;
			

			_scoreP1.gameObject.SetActive(false);
			_scoreP2.gameObject.SetActive(false);
			_timeLeft.gameObject.SetActive(false);

			for(int i = countdown; i >= 0; i--)
			{
				_mainMessage.text = (i > 0) ? i.ToString() + "!" : "Start!";
				yield return new WaitForSeconds(1f);
			}
			_mainMessage.gameObject.SetActive(false);
			_scoreP1.gameObject.SetActive(true);
			_scoreP2.gameObject.SetActive(true);
			_timeLeft.gameObject.SetActive(true);
			StartGame(playerCount, duration);
		}

        private void StartGame(int playerCount, float duration)
        {
            _cameraController.m_Targets = new Transform[playerCount];
            _players = new List<PlayerController>(playerCount);
			_ampDictionary = new Dictionary<int, List<AmpController>>();

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
			_duration = TimeSpan.FromSeconds(duration);

			_scoreP1.text = "0";
			_scoreP2.text = "0";
			_timeLeft.text = _duration.ToString();

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

		public List<AmpController> EnemyAmps(PlayerController playerController)
		{
			var result = new List<AmpController>();

			foreach(var pair in _ampDictionary)
			{
				if(pair.Key == playerController.ID) continue;

				foreach(var amp in pair.Value)
				{
					result.Add(amp);
				}
			}

			return result;
		}

        void Start()
        {
            StartCoroutine(InitiateStartSequence(3, 2, 120));
        }

        void Update()
        {
			_duration = _duration.Subtract(TimeSpan.FromSeconds(Time.deltaTime));
			var minutes = _duration.Minutes;
			var seconds = _duration.Seconds;

			_timeLeft.text = _duration.Minutes + ":" + ((seconds < 10) ? "0" : "") + _duration.Seconds;
        }

        private void RegisterToPlayerEvents(PlayerController playerController)
        {
            playerController.AmpPlaced += OnAmpPlaced;
            playerController.AmpDestroyed += OnAmpDestroyed;
        }

        private void UnregisterFromPlayerEvents(PlayerController playerController)
        {
            playerController.AmpPlaced -= OnAmpPlaced;
            playerController.AmpDestroyed -= OnAmpDestroyed;
        }

        private void OnAmpPlaced(AmpController amp)
        {
            _ampCounts[amp.OwnerId]++;

			if(!_ampDictionary.ContainsKey(amp.OwnerId))
			{
				_ampDictionary.Add(amp.OwnerId, new List<AmpController>());
			}

			_ampDictionary[amp.OwnerId].Add(amp);

			_scoreP1.text = _ampCounts[0].ToString();
			_scoreP2.text = _ampCounts[1].ToString();
        }

        private void OnAmpDestroyed(AmpController amp)
        {
            _ampCounts[amp.OwnerId]--;
			
			if(_ampDictionary.ContainsKey(amp.OwnerId))
			{
				_ampDictionary[amp.OwnerId].Remove(amp);
			}

            _ampCounts[amp.OwnerId] = (_ampCounts[amp.OwnerId] < 0) ? 0 : _ampCounts[amp.OwnerId];
			_scoreP1.text = _ampCounts[0].ToString();
			_scoreP2.text = _ampCounts[1].ToString();
        }
    }
}