using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ2018
{
    public class PowerLine : MonoBehaviour
    {
		private Transform _player, _amp;
		private LineRenderer _lineRenderer;

		public void Initialize(Transform player, Transform amp)
		{
			_player = player;
			_amp = amp;

			_lineRenderer = GetComponent<LineRenderer>();
			_lineRenderer.positionCount = 2;
		}

		void LateUpdate()
        {
            if (_lineRenderer != null)
            {
                _lineRenderer.SetPositions(new Vector3[2]
                {
                	_player.position,
                	_amp.position
                });
            }
        }
    }
}