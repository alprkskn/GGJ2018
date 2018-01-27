using System;
using System.Collections;
using System.Collections.Generic;
using Complete;
using UnityEngine;

namespace GGJ2018{
public class GameController : MonoBehaviour
    {

		[SerializeField] private Transform[] _spawnPoints;
		[SerializeField] private GameObject _playerPrefab;
		[SerializeField] private CameraControl _cameraController;

		private TimeSpan _duration;
		private List<PlayerController> _players;

		public void StartGame(int playerCount, float duration)
		{
			_cameraController.m_Targets = new Transform[playerCount];
			_players = new List<PlayerController>(playerCount);
			for(int i = 0; i < _spawnPoints.Length && i < playerCount; i++)
			{
				var go = Instantiate(_playerPrefab);
				go.transform.position = _spawnPoints[i].position;
				go.name = "Player_" + (i + 1);
				var controller = go.GetComponent<PlayerController>();
				_players.Add(controller);
				RegisterToPlayerEvents(controller);
				_cameraController.m_Targets[i] = go.transform;
			}
		}

		public void ClearGame()
		{
			for(var i = 0; i < _players.Count; i++)
			{
				var player = _players[i];
				UnregisterFromPlayerEvents(player);
				Destroy(player.gameObject);
				_cameraController.m_Targets[i] = null;
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

		}

		private void UnregisterFromPlayerEvents(PlayerController playerController)
		{

		}
    }
}