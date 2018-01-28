using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ2018
{
    public class AmpController : MonoBehaviour
    {
		[SerializeField] GameObject _powerLinePrefab;
		[SerializeField] Material[] _materials;
		// TODO: switch colors.
		[SerializeField] MeshRenderer[] _coloredRenderers;

		private PlayerActions _owner;

		public PlayerActions Owner
		{
			get
			{
				return _owner;
			}
		}

		private PowerLine _powerLine;
		
		public PowerLine PowLine
		{
			get { return _powerLine; }
		}

		public int OwnerId
		{
			get
			{
				return _owner.m_PlayerNumber;
			}
		}

		public void SetOwner(PlayerActions player)
		{
			_owner = player;
			_powerLine = CreatePowerLine();
		}

		private PowerLine CreatePowerLine()
		{
			var go = Instantiate(_powerLinePrefab);
			PowerLine pl = go.GetComponent<PowerLine>();

			pl.Initialize(_owner.transform, this.transform);

			return pl;
		}

		void OnDestroy()
		{
			Destroy(_powerLine.gameObject);
		}
    }
}
