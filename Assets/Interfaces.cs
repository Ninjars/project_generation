using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interfaces : MonoBehaviour {

	public interface IWallGenerator {
        // an outline is a list of indexes pointing to a Vector3 vertex in the vertices list
        Mesh generate(List<List<int>> outlines, List<Vector3> vertices);
    }
}
