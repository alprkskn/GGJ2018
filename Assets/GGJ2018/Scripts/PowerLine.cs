using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GGJ2018.Utils;
using System;

namespace GGJ2018
{
    public class PowerLine : MonoBehaviour
    {
        public event Action<Vector3> LineConstrained;
        public event Action<Vector3> LineReleased;

        public GameObject RopeSegmentPrefab;
        public GameObject CableSegmentPrefab;

        public float TipRepulsionFactor = 1f;
        public float SegmentLength = 2f;
        public float MinimumIntervalToInstantiate = 0.3f;
		public float RopeHeight = 1f;
        public int MaxRopeSegment = 20;

		private Transform _player, _amp;
		private List<Rigidbody> _segments;
        private List<Vector3> _nodes;

        private Rigidbody _tip;
        private float LastInstantiationTime;
        private bool Calculate;
		private Vector3 _startPosition;
        private List<Transform> _cableSegments;
        private Rigidbody _playerRigidbody;

		public void Initialize(Transform player, Transform amp)
		{
			_player = player;
            _playerRigidbody = _player.GetComponent<Rigidbody>();
			_amp = amp;
			_segments = new List<Rigidbody>();
            _cableSegments = new List<Transform>();
			StartCoroutine(RopeDelay(MinimumIntervalToInstantiate));
		}

		void LateUpdate()
        {
			if(_tip != null)
            {
                var diff = _player.position - _tip.transform.position;
                var distance = diff.magnitude;
                var direction = diff.normalized;
                _tip.AddForce(direction * TipRepulsionFactor, ForceMode.Acceleration);

                if (_segments.Count < MaxRopeSegment && LastInstantiationTime + MinimumIntervalToInstantiate < Time.time && distance > SegmentLength)
                {
                    _tip = InstantiateRopeSegment(_tip);
                    _segments.Add(_tip);
                }
                else if(_segments.Count >= MaxRopeSegment)
                {
                    var len = Vector3.Distance(_startPosition, _player.position);
                    if(len > SegmentLength * _segments.Count)
                    {
                        if(LineConstrained != null) LineConstrained(_tip.position);
                    }
                    else
                    {
                        if(LineConstrained != null) LineReleased(_tip.position);
                    }
                }

                _nodes = new List<Vector3>(_segments.Count);
                foreach(var seg in _segments)
                {
                    _nodes.Add(seg.position);
                }

                var simplified = _nodes;

                var cursor = 0;
                var s = GetOrInstantiateCableSegment(cursor++);
                s.position = (_amp.position + simplified[0]) / 2;
                s.up = (simplified[0] - _amp.position);
                s.localScale = new Vector3(0.3f, (simplified[0] - _amp.position).magnitude / 2f, 0.3f);

                for(int i = 0; i < simplified.Count - 1; i++)
                {
                    s = GetOrInstantiateCableSegment(cursor++);
                    s.position = (simplified[i] + simplified[i+1]) / 2;
                    s.up = (simplified[i+1] - simplified[i]);
                    s.localScale = new Vector3(0.3f, (simplified[i+1] - simplified[i]).magnitude / 2f, 0.3f);
                }

                s = GetOrInstantiateCableSegment(cursor);
                s.position = (_player.position + simplified[simplified.Count - 1]) / 2;
                s.up = (_player.position - simplified[simplified.Count - 1]);
                s.localScale = new Vector3(0.3f, (_player.position - simplified[simplified.Count - 1]).magnitude / 2f, 0.3f);

                for(int i = cursor; i < simplified.Count; i++)
                {
                    _cableSegments[i].gameObject.SetActive(false);
                }
                /*Debug.DrawLine(_amp.position, simplified[0], Color.black);

                for(int i = 0; i < simplified.Count - 1; i++)
                {
                    Debug.DrawLine(simplified[i], simplified[i+1], Color.black);
                }

                Debug.DrawLine(simplified[simplified.Count - 1], _player.position);*/
            }
        }

        private Transform GetOrInstantiateCableSegment(int cursor)
        {
            if(cursor >= _cableSegments.Count)
            {
                var go = Instantiate(CableSegmentPrefab);
                _cableSegments.Add(go.transform);
                return go.transform;
            }
            else
            {
                var t = _cableSegments[cursor];
                t.gameObject.SetActive(true);
                return t;
            }
        }

		private IEnumerator RopeDelay(float duration)
		{
			_startPosition = _player.position;
			yield return new WaitForSeconds(duration);

			_tip = InstantiateRopeSegment(null);
			_segments.Add(_tip);
		}

        private Rigidbody InstantiateRopeSegment(Rigidbody previousRigidbody)
        {
            LastInstantiationTime = Time.time;
            Vector3 position;
            Quaternion rotation;
            if (!previousRigidbody)
            {
                position = _startPosition + Vector3.up * RopeHeight;
                rotation = Quaternion.Euler(0f, _player.eulerAngles.y, 0f);
            }
            else
            {
                position = previousRigidbody.transform.position + previousRigidbody.transform.forward * SegmentLength;
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
			foreach(var node in _segments)
			{
				Destroy(node.gameObject);
			}

            foreach(var seg in _cableSegments)
            {
                Destroy(seg.gameObject);
            }

            _segments.Clear();
		}

        public float SqrDistanceFromPoint(Vector3 pt, out Vector3 closestPoint)
        {
            closestPoint = Vector3.zero;
            if(_nodes == null)
            {
                return float.PositiveInfinity;
            }

            float minDistance = float.PositiveInfinity;

            for(int i = 0; i < _nodes.Count - 1; i++)
            {
                var dist = PathUtils.SquareDistanceToLineSegment2D(pt, _nodes[i], _nodes[i+1]);
                if(dist < minDistance)
                {
                    minDistance = dist;
                    PathUtils.GetProjectPointOnLine(pt, _nodes[i], _nodes[i+1], out closestPoint);
                }
            }

            return minDistance;
        }
    }
}