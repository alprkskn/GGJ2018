using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ2018
{
    public class PowerLine : MonoBehaviour
    {
        public GameObject RopeSegmentPrefab;
        public float TipRepulsionFactor = 1f;
        public float SegmentLength = 2f;

		private Transform _player, _amp;

		private List<Rigidbody> _nodes;
		private const float MinSegmentLen = 3f;

        private Rigidbody _tip;
        private float LastInstantiationTime;
        public float MinimumIntervalToInstantiate = 0.3f;
        private bool Calculate;
		public float RopeHeight = 1f;

		public void Initialize(Transform player, Transform amp)
		{
			_player = player;
			_amp = amp;
			_nodes = new List<Rigidbody>();

			_tip = InstantiateRopeSegment(null);
			_nodes.Add(_tip);
		}

		void LateUpdate()
        {
			if(_tip != null)
            {
                var diff = _player.position - _tip.transform.position;
                var distance = diff.magnitude;
                var direction = diff.normalized;
                _tip.AddForce(direction * TipRepulsionFactor, ForceMode.Acceleration);

                if (LastInstantiationTime + MinimumIntervalToInstantiate < Time.time && distance > SegmentLength)
                {
                    _tip = InstantiateRopeSegment(_tip);
                    _nodes.Add(_tip);
                }
            }
        }

        private Rigidbody InstantiateRopeSegment(Rigidbody previousRigidbody)
        {
            LastInstantiationTime = Time.time;
            Vector3 position;
            Quaternion rotation;
            if (!previousRigidbody)
            {
                position = _player.position + Vector3.up * RopeHeight;
                rotation = Quaternion.Euler(0f, _player.eulerAngles.y, 0f);
            }
            else
            {
                position = previousRigidbody.transform.position + previousRigidbody.transform.forward * 2f;
                rotation = Quaternion.Euler(0f, previousRigidbody.transform.eulerAngles.y, 0f); ;
            }
            var go = Instantiate(RopeSegmentPrefab, position, rotation);
            var joint = go.GetComponent<ConfigurableJoint>();

            if (!previousRigidbody)
            {
                joint.autoConfigureConnectedAnchor = true;
            }
            else
            {
                joint.connectedBody = previousRigidbody;
                go.GetComponent<Rigidbody>().velocity = previousRigidbody.velocity;
            }
            return go.GetComponent<Rigidbody>();
        }

		void OnDestroy()
		{
			foreach(var node in _nodes)
			{
				Destroy(node.gameObject);
			}
		}
    }
}