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
		private Transform _transform;

		private Vector3 _verticalAxis, _horizontalAxis;

        public void Initialize(PlayerController player)
        {
			_transform = transform;
			_player = player;
			_amps = new List<AmpController>();
            // The axes names are based on player number.
            m_MovementAxisName = "Vertical" + m_PlayerNumber;
            m_TurnAxisName = "Horizontal" + m_PlayerNumber;
			_placeAmpButton = "Action" + m_PlayerNumber;

			_verticalAxis = ProjectVectorOnPlane(Vector3.up, GameController.Instance.GameCamera.transform.forward).normalized;
			_horizontalAxis = ProjectVectorOnPlane(Vector3.up, GameController.Instance.GameCamera.transform.right).normalized;
        }

		protected override void Move()
		{
            // Create a vector in the direction the tank is facing with a magnitude based on the input, speed and the time between frames.
            Vector3 movement = (_verticalAxis * m_MovementInputValue + _horizontalAxis * m_TurnInputValue) * m_Speed * Time.deltaTime;

            // Apply this movement to the rigidbody's position.
            m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
		}

        protected override void Turn ()
		{

		}

        private Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 v)
        {
            planeNormal.Normalize();
            var distance = -Vector3.Dot(planeNormal.normalized, v);
            return v + planeNormal * distance;
        }


        protected override void Update()
        {
			base.Update();
			if(Input.GetButtonDown(_placeAmpButton))
			{
				Debug.LogFormat("Player {0} placed an Amp!", m_PlayerNumber);
				PlaceAmp();
			}

			if(Input.GetKeyDown(KeyCode.Space))
			{
				foreach(var a in _amps)
				{
					Destroy(a.gameObject);
				}

				_amps.Clear();
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