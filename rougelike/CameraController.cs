using UnityEngine;
using System.Collections;

//unchanged

public class CameraController : MonoBehaviour {

    public GameObject player;
    
    private Camera gameCamera;
    private Vector3 offSet;
    
    // Use this for initialization
	void Start () {
        gameCamera = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void LateUpdate () {
        // Debug.Log("doing shit");
        //Vector3 tempV = new Vector3(player.transform.position.x,player.transform.position.y,-10f);
        //gameCamera.transform.position = tempV;
	}
}
