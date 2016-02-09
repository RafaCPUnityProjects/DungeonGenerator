using UnityEngine;
using System.Collections;
using DungeonGenerator;

[RequireComponent(typeof(Camera))]
public class CameraAdjust : MonoBehaviour
{
	void Start()
	{
		Dungeon dungeon = FindObjectOfType<Dungeon>();
		transform.position = new Vector3(dungeon.bounds.width / 2, dungeon.bounds.height / 2, transform.position.z);
		GetComponent<Camera>().orthographicSize = Mathf.Max(dungeon.bounds.width, dungeon.bounds.height);
	}
}
