using UnityEngine;
using System.Collections;
using Unity.Netcode;

[RequireComponent(typeof(ParticleSystem))]
public class CFX_AutoDestructShuriken : NetworkBehaviour
{
	public bool OnlyDeactivate;
	
	void OnEnable()
	{
		StartCoroutine("CheckIfAlive");
	}
	
	IEnumerator CheckIfAlive ()
	{
		while(true)
		{
			yield return new WaitForSeconds(0.5f);
			if(!GetComponent<ParticleSystem>().IsAlive(true))
			{
				if (OnlyDeactivate)
				{
#if UNITY_3_5
						this.gameObject.SetActiveRecursively(false);
#else
					this.gameObject.SetActive(false);
#endif
				}
				else
					DestroySelfServerRpc();
				break;
			}
		}
	}
	[ServerRpc(RequireOwnership = false)]
	private void DestroySelfServerRpc()
	{
		GetComponent<NetworkObject>().Despawn();
		
	}
}
