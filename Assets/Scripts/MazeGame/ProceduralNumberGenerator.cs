using UnityEngine;
using System.Collections;

public class ProceduralNumberGenerator {
	public static int currentPosition = 0;
	public const string key = "12213231323143213324132123232132133213232323344";

	public static int GetNextNumber() {
		string currentNum = key.Substring(currentPosition++ % key.Length, 1);
		return int.Parse (currentNum);
	}
}
