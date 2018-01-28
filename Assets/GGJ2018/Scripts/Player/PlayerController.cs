using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ2018
{
    public class PlayerController : MonoBehaviour
    {
		private PlayerActions _actions;

		public event Action<AmpController> AmpPlaced
		{
			add
			{
				_actions.AmpPlaced += value;
			}
			remove
			{
				_actions.AmpPlaced -= value;
			}
		}

		public event Action<AmpController> AmpDestroyed
		{
			add
			{
				_actions.AmpDestroyed += value;
			}
			remove
			{
				_actions.AmpDestroyed -= value;
			}
		}

        public void Initialize(int id)
        {
			_actions = GetComponent<PlayerActions>();
			_actions.m_PlayerNumber = id;
			_actions.Initialize(this);
        }

        void OnDestroy()
        {

        }
    }
}