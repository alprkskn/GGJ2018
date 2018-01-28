using System;
using System.Collections;
using System.Collections.Generic;
using Complete;
using UnityEngine;
using UniversalNetworkInput;

namespace GGJ2018
{
    public class PlayerActions : TankMovement
    {
		public event Action<AmpController> AmpPlaced;
		public event Action<AmpController> AmpDestroyed;
		public float MaxCutSqrDistance = 0.5f * 0.5f;

		[SerializeField] private GameObject _ampPrefab;

		private PlayerController _player;

        public PlayerController PlayerController
        {
            get { return _player; }
        }

		private string _placeAmpButton , _cutButton;
		private List<AmpController> _amps;
		private Transform _transform;
		private Vector3 _lastMovement;
		private Vector3 _up;
		private bool _locked;

		private string _networkVerticalAxis, _networkHorizontalAxis;

		private Vector3 _verticalAxis, _horizontalAxis;

        public void Initialize(PlayerController player)
        {
			_transform = transform;
			_player = player;
			_amps = new List<AmpController>();
            // The axes names are based on player number.
            m_MovementAxisName = "Vertical" + m_PlayerNumber;
            m_TurnAxisName = "Horizontal" + m_PlayerNumber;

			_networkHorizontalAxis = "Horizontal";
			_networkVerticalAxis = "Vertical";

			_placeAmpButton = "Action" + m_PlayerNumber;
			_cutButton = "Fire" + m_PlayerNumber;
			_up = Vector3.up;

			_verticalAxis = ProjectVectorOnPlane(Vector3.up, GameController.Instance.GameCamera.transform.forward).normalized;
			_horizontalAxis = ProjectVectorOnPlane(Vector3.up, GameController.Instance.GameCamera.transform.right).normalized;
        }

		protected override void Move()
		{
			if(_locked) return;
            // Create a vector in the direction the tank is facing with a magnitude based on the input, speed and the time between frames.
            Vector3 movement = (_verticalAxis * m_MovementInputValue + _horizontalAxis * m_TurnInputValue) * m_Speed * Time.deltaTime;

			_lastMovement = movement;

            // Apply this movement to the rigidbody's position.
			_transform.position = _transform.position + movement;
            //m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
		}

        protected override void Turn ()
		{
			if(_lastMovement.magnitude > 0)
			{
                var lastMoveRotation = Quaternion.LookRotation(_lastMovement, _up);

                var targetRotation = Quaternion.RotateTowards(_transform.rotation, lastMoveRotation, 10);

				_transform.rotation = targetRotation;
            }
		}

        private Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 v)
        {
            planeNormal.Normalize();
            var distance = -Vector3.Dot(planeNormal.normalized, v);
            return v + planeNormal * distance;
        }

		private void DestroyAmp(AmpController amp)
        {
			if(amp.OwnerId != this.m_PlayerNumber) return;

            if (AmpDestroyed != null) AmpDestroyed(amp);
            amp.PowLine.LineConstrained -= OnPowerLineConstrained;
            amp.PowLine.LineReleased -= OnPowerLineReleased;

			_amps.Remove(amp);
            Destroy(amp.gameObject);
        }

        protected override void Update()
        {
			base.Update();

            m_MovementInputValue = (m_MovementInputValue == 0) ? UNInput.GetAxis(m_PlayerNumber, _networkVerticalAxis) : m_MovementInputValue;
            m_TurnInputValue = (m_TurnInputValue == 0) ? UNInput.GetAxis(m_PlayerNumber, _networkHorizontalAxis) : m_TurnInputValue;
			
			if(Input.GetButtonDown(_placeAmpButton) || UNInput.GetButtonDown(m_PlayerNumber, "Action"))
			{
				Debug.LogFormat("Player {0} placed an Amp!", m_PlayerNumber);
				PlaceAmp();
			}

			if(Input.GetKeyDown(KeyCode.Backspace) || UNInput.GetButtonDown(m_PlayerNumber, "Start"))
			{
				for(int i = _amps.Count - 1; i >= 0; i--)
				{
					var a = _amps[i];
					DestroyAmp(a);
				}

				_amps.Clear();
			}

			var enemyAmps = GameController.Instance.EnemyAmps(_player);

			AmpController target = null;
			var minDist = float.MaxValue;
			foreach(var a in enemyAmps)
			{
				Vector3 point;
				var dist = a.PowLine.SqrDistanceFromPoint(_transform.position, out point);

				if(dist < MaxCutSqrDistance && dist < minDist)
				{
					minDist = dist;
					Debug.DrawLine(point, point + Vector3.up * 100f, Color.yellow);
					target = a;
				}
			}

			if(target != null && Input.GetButton(_cutButton) || UNInput.GetButton(m_PlayerNumber, "Back"))
			{
				target.Owner.DestroyAmp(target);
			}
        }

		void OnDestroy()
		{
			foreach(var amp in _amps)
			{
				if(AmpDestroyed != null) AmpDestroyed(amp);
				amp.PowLine.LineConstrained += OnPowerLineConstrained;
				amp.PowLine.LineReleased += OnPowerLineReleased;
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
			controller.PowLine.LineConstrained += OnPowerLineConstrained;
			controller.PowLine.LineReleased += OnPowerLineReleased;
			//
			
			if(AmpPlaced != null) AmpPlaced(controller);
		}

		private void OnPowerLineConstrained(Vector3 tip)
		{
			if (!_locked)
            {
                _locked = true;
                var delta = tip - _transform.position;
                m_Rigidbody.velocity = Vector3.zero;
                m_Rigidbody.AddForce(delta.normalized * 5f + Vector3.up * 3f, ForceMode.Impulse);
            }
		}

		private Coroutine _delayCoroutine;
		private void OnPowerLineReleased(Vector3 tip)
		{
			if(_locked && _delayCoroutine == null)
			{
				_delayCoroutine = StartCoroutine(DelayLock(1f));
			}
		}

		private IEnumerator DelayLock(float duration)
		{
			yield return new WaitForSeconds(duration);
			_locked = false;
			_delayCoroutine = null;
		}
    }
}