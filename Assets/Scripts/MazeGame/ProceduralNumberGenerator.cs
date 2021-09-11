using UnityEngine;
using System.Collections;

public class ProceduralNumberGenerator {
	public static int currentPosition = 0;
	public const string key = "143342322342313424432134413233133432132133213232323344";

	public static int GetNextNumber() {
		string currentNum = key.Substring(currentPosition++ % key.Length, 1);
		return int.Parse (currentNum);
	}
}
