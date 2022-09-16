using UnityEngine;
using System.Collections;

[System.Serializable]
public class ArrayLayout  {

	[System.Serializable]
	public struct rowData{
		public ObstacleType[] row;
	}

    public Grid grid;
    public rowData[] rows = new rowData[14]; //Grid of 7x7
}

public enum ObstacleType
{
	Clear,
	Hole
}
