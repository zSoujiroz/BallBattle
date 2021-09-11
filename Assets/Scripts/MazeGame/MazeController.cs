using UnityEngine;
using System.Collections;

public class MazeController : MonoBehaviour {
	[HideInInspector]
	public int mazeRows, mazeColumns;
	public GameObject wall;
	private float wallLength = 1f;

	private MazeCell[,] mazeCells;

	private float[] footballField;

	private Transform mazeHolder;

	void Start()
	{
		footballField = GameManager.instance.GetFootBallField();
		mazeRows = (int)footballField[0];
		mazeColumns = (int)footballField[1];

		mazeHolder = new GameObject ("PenaltyMaps").transform;
	}

	// Use this for initialization
	public void CreateMazeMap () 
	{
		
		InitializeMaze ();

		MazeAlgorithm ma = new HuntAndKillMazeAlgorithm (mazeCells);
		ma.CreateMaze ();
	}

	public void ClearMazeMap()
	{
		foreach (Transform child in mazeHolder.transform) {
			GameObject.Destroy(child.gameObject);
		}
		mazeHolder.position = Vector3.zero;
	}

	private void InitializeMaze() {
		mazeCells = new MazeCell[mazeRows,mazeColumns];		

		for (int r = 0; r < mazeRows; r++) 
		{
			for (int c = 0; c < mazeColumns; c++) 
			{
				mazeCells [r, c] = new MazeCell ();

				if (c == 0) 
				{
					mazeCells[r, c].westWall = Instantiate (wall, new Vector3 (r*wallLength, 0, (c*wallLength) - (wallLength/2f)), Quaternion.identity, mazeHolder) as GameObject;
					mazeCells[r, c].westWall.name = "West Wall " + r + "," + c;
				}

				mazeCells[r, c].eastWall = Instantiate (wall, new Vector3 (r*wallLength, 0, (c*wallLength) + (wallLength/2f)), Quaternion.identity, mazeHolder) as GameObject;
				mazeCells[r, c].eastWall.name = "East Wall " + r + "," + c;

				if (r == 0) 
				{
					mazeCells[r, c].northWall = Instantiate (wall, new Vector3 ((r*wallLength) - (wallLength/2f), 0, c*wallLength), Quaternion.identity, mazeHolder) as GameObject;
					mazeCells[r, c].northWall.name = "North Wall " + r + "," + c;
					mazeCells[r, c].northWall.transform.Rotate (Vector3.up * 90f);
				}

				mazeCells[r, c].southWall = Instantiate (wall, new Vector3 ((r*wallLength) + (wallLength/2f), 0, c*wallLength), Quaternion.identity, mazeHolder) as GameObject;
				mazeCells[r, c].southWall.name = "South Wall " + r + "," + c;
				mazeCells[r, c].southWall.transform.Rotate (Vector3.up * 90f);
			}
		}

		//mazeHolder.position = initPos;
		//mazeHolder.position = new Vector3(initPos.x + size/2f , 0, initPos.z + size/2f);
		mazeHolder.position = new Vector3((wallLength - footballField[0])/2f, 0f, (wallLength - footballField[1])/2f);
	}
}
