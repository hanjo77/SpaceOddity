using UnityEngine;
using System.Collections;

public class LaserController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	void OnTriggerEnter(Collider other) {
		// MeshExploder meshExploder = other.transform.GetComponent<MeshExploder> ();
//		if (other.name != "laser") {
//			ArwingController arwing = other.GetComponentInParent<ArwingController> ();
//			if (meshExploder != null && !this.transform.IsChildOf(other.transform)) {
//				meshExploder.Explode ();
//				if (arwing) {
//					if (arwing.isLocalPlayer) {
//						Transform camera = Instantiate (arwing.cam) as Transform;
//						camera.position = arwing.transform.position;
//						camera.rotation = arwing.transform.rotation;
//						arwing.Destroy ();
//					}
//					GameObject.Destroy(arwing.transform.gameObject);
//				}
//			}
//		}
	}
		
	// Update is called once per frame
	void Update () {
	
	}
}
