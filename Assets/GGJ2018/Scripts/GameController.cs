using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ2018{
public class GameController : MonoBehaviour
    {

		[SerializeField] private Vector3[] _spawnPoints;
		[SerializeField] private GameObject _playerPrefab;

		private TimeSpan _duration;
		private List<PlayerController> _players;
		public void StartGame(int playerCount, float duration)
		{
			for(int i = 0; i < _spawnPoints.Length && i < playerCount; i++)
			{
				var go = Instantiate(_playerPrefab);
				go.name = "Player_" + (playerCount + 1);
				var controller = go.GetComponent<PlayerController>();
				_players.Add(controller);
				RegisterToPlayerEvents(controller);
			}
		}

		public void ClearGame()
		{
			foreach(var player in _players)
			{
				UnregisterFromPlayerEvents(player);
				Destroy(player.gameObject);
			}

			_players.Clear();
		}

        void Start()
        {

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