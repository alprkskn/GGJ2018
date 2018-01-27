using System;
using System.Collections;
using System.Collections.Generic;
using Complete;
using UnityEngine;

namespace GGJ2018
{
    public class PlayerActions : TankMovement
    {
		public event Action<AmpController> AmpPlaced;

		[SerializeField] private GameObject _ampPrefab;

		private PlayerController _player;
		private string _placeAmpButton;
		private List<AmpController> _amps;

        public void Initialize(PlayerController player)
        {
			_player = player;
			_amps = new List<AmpController>();
            // The axes names are based on player number.
            m_MovementAxisName = "Vertical" + m_PlayerNumber;
            m_TurnAxisName = "Horizontal" + m_PlayerNumber;
			_placeAmpButton = "Action" + m_PlayerNumber;
        }

        protected override void Update()
        {
			base.Update();
			if(Input.GetButtonDown(_placeAmpButton))
			{
				Debug.LogFormat("Player {0} placed an Amp!", m_PlayerNumber);
				PlaceAmp();
			}			
        }

		void OnDestroy()
		{
			foreach(var amp in _amps)
			{
				Destroy(amp);
			}
			_amps.Clear();
		}

		private void PlaceAmp()
		{
			var go = Instantiate(_ampPrefab);
			var controller = go.GetComponent<AmpController>();
			_amps.Add(controller);
			// TODO: Initiate amp and store.
			go.transform.position = transform.position + transform.forward;
			go.transform.rotation = transform.rotation;

			controller.SetOwner(this);
			//
			
			if(AmpPlaced != null) AmpPlaced(controller);
		}
    }
}