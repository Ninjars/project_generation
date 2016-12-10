using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {
    public MeshFilter walls;
    public MeshFilter surface;
    public SquareGrid squareGrid;
    public float wallHeight = 5;

    // if 1 will dig out tunnels, if 0 will generate islands
    private static int fillFlag = 0;

    List<Vector3> vertices;
    List<int> triangles;

    Dictionary<int, List<Triangle>> triangleDictionary;
    HashSet<int> checkedVertices;

    public void generateMesh(int[,] map, float squareSize) {
        squareGrid = new SquareGrid(map, squareSize);

        vertices = new List<Vector3>();
        triangles = new List<int>();
        triangleDictionary = new Dictionary<int, List<Triangle>>();
        checkedVertices = new HashSet<int>();

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++) {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++) {
                triangulateSquare(squareGrid.squares[x, y]);
            }
        }
        Mesh surfaceMesh = new Mesh();
        surfaceMesh.vertices = vertices.ToArray();
        surfaceMesh.triangles = triangles.ToArray();
        surfaceMesh.RecalculateNormals();
        surface.mesh = surfaceMesh;

        MeshCollider surfaceCollider = surface.GetComponent<MeshCollider>();
        if (surfaceCollider == null) {
            surfaceCollider = surface.gameObject.AddComponent<MeshCollider>();
        }
        surfaceCollider.sharedMesh = surfaceMesh;

        createWallMesh();
    }

    void createWallMesh() {
        List<List<int>> outlines = calculateMeshOutlines();

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        foreach (List<int> outline in outlines) {
            for (int i = 0; i < outline.Count - 1; i++) {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]); // left
                wallVertices.Add(vertices[outline[i + 1]]); // right
                wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight); // bottom left
                wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * wallHeight); // bottom right

                // winding triangles anticlockwise to view from within
                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 0);
            }
        }

        Mesh wallMesh = new Mesh();
        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        walls.mesh = wallMesh;
        MeshCollider collider = walls.GetComponent<MeshCollider>();
        if (collider == null) {
            collider = walls.gameObject.AddComponent<MeshCollider>();
        }
        collider.sharedMesh = wallMesh;
    }

    void meshFromPoints(params Node[] points) {
        assignVertices(points);

        if (points.Length >= 3) {
            createTriangle(points[0], points[1], points[2]);
        }
        if (points.Length >= 4) {
            createTriangle(points[0], points[2], points[3]);
        }
        if (points.Length >= 5) {
            createTriangle(points[0], points[3], points[4]);
        }
        if (points.Length >= 6) {
            createTriangle(points[0], points[4], points[5]);
        }
    }

    void assignVertices(Node[] points) {
        for (int i = 0; i < points.Length; i++) {
            if (points[i].vertexIndex == -1) {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void createTriangle(Node a, Node b, Node c) {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        addTriangleToDictionary(triangle.vertexIndexA, triangle);
        addTriangleToDictionary(triangle.vertexIndexB, triangle);
        addTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    void addTriangleToDictionary(int vertexIndexKey, Triangle triangle) {
        if (triangleDictionary.ContainsKey(vertexIndexKey)) {
            triangleDictionary[vertexIndexKey].Add(triangle);
        } else {
            List<Triangle> list = new List<Triangle>();
            list.Add(triangle);
            triangleDictionary.Add(vertexIndexKey, list);
        }
    }

    List<List<int>> calculateMeshOutlines() {
        List<List<int>> outlines = new List<List<int>>();
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++) {
            if (!checkedVertices.Contains(vertexIndex)) {
                int newOutlineVertex = getConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1) {
                    checkedVertices.Add(newOutlineVertex);
                    List<int> newOutline = new List<int>();
                    followOutline(newOutline, newOutlineVertex);
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                }
            }
        }
        return outlines;
    }

    void followOutline(List<int> outline, int vertexIndex) {
        // recursively follow vertexes around the outside of a mesh
        outline.Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = getConnectedOutlineVertex(vertexIndex);
        if (nextVertexIndex != -1) {
            followOutline(outline, nextVertexIndex);
        }
    }

    int getConnectedOutlineVertex(int vertexIndex) {
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];
        for (int i = 0; i < trianglesContainingVertex.Count; i++) {
            Triangle triangle = trianglesContainingVertex[i];

            for (int j = 0; j < 3; j++) {
                int vertexB = triangle[j];
                if (!checkedVertices.Contains(vertexB)) {
                    if (isOutlineEdge(vertexIndex, vertexB)) {
                        return vertexB;
                    }
                }
            }
        }
        return -1;
    }

    bool isOutlineEdge(int vertexA, int vertexB) {
        if (vertexA == vertexB) return false;
        List<Triangle> trianglesA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;
        for (int i = 0; i < trianglesA.Count; i++) {
            if (trianglesA[i].contains(vertexB)) {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1) {
                    break;
                }

            }
        }
        return sharedTriangleCount == 1;
    }

    void triangulateSquare(Square square) {
        switch (square.configuration) {
            // no points selected
            case 0:
                break;

            // 1 point selected
            case 1:
                meshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
                break;
            case 2:
                meshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
                break;
            case 4:
                meshFromPoints(square.topRight, square.centreRight, square.centreTop);
                break;
            case 8:
                meshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                break;

            // 2 points
            case 3:
                meshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 6:
                meshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                break;
            case 9:
                meshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                break;
            case 12:
                meshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                break;
            case 5:
                meshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 10:
                meshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 3 points
            case 7:
                meshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 11:
                meshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                meshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                break;
            case 14:
                meshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 4 points
            case 15:
                meshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                checkedVertices.Add(square.topLeft.vertexIndex);
                checkedVertices.Add(square.topRight.vertexIndex);
                checkedVertices.Add(square.bottomRight.vertexIndex);
                checkedVertices.Add(square.bottomLeft.vertexIndex);
                break;
        }
    }

    /*
    private void OnDrawGizmos() {
        if (squareGrid != null) {
            for (int x = 0; x < squareGrid.squares.GetLength(0); x++) {
                for (int y = 0; y < squareGrid.squares.GetLength(1); y++) {
                    Gizmos.color = squareGrid.squares[x, y].topLeft.active ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].topLeft.position, Vector3.one * 0.4f);

                    Gizmos.color = squareGrid.squares[x, y].topRight.active ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].topRight.position, Vector3.one * 0.4f);

                    Gizmos.color = squareGrid.squares[x, y].bottomLeft.active ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].bottomLeft.position, Vector3.one * 0.4f);

                    Gizmos.color = squareGrid.squares[x, y].bottomRight.active ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].bottomRight.position, Vector3.one * 0.4f);

                    Gizmos.color = Color.grey;
                    Gizmos.DrawCube(squareGrid.squares[x, y].centreTop.position, Vector3.one * 0.15f);
                    Gizmos.DrawCube(squareGrid.squares[x, y].centreLeft.position, Vector3.one * 0.15f);
                    Gizmos.DrawCube(squareGrid.squares[x, y].centreRight.position, Vector3.one * 0.15f);
                    Gizmos.DrawCube(squareGrid.squares[x, y].centreBottom.position, Vector3.one * 0.15f);
                }
            }
        }
    }
    */

    struct Triangle {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        int[] vertices;

        public Triangle(int a, int b, int c) {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;
            vertices = new int[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        public bool contains(int vertexIndex) {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }

        public int this[int i] {
            get {
                return vertices[i];
            }
        }
    }

    public class SquareGrid {
        public Square[,] squares;

        public SquareGrid(int[,] map, float squareSize) {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];
            for (int x = 0; x < nodeCountX; x++) {
                for (int y = 0; y < nodeCountY; y++) {
                    Vector3 position = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
                    controlNodes[x, y] = new ControlNode(position, map[x, y] == fillFlag, squareSize);
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++) {
                for (int y = 0; y < nodeCountY - 1; y++) {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }
    }

    public class Square {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        public int configuration;

        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft) {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            centreTop = topLeft.rightNode;
            centreRight = bottomRight.aboveNode;
            centreBottom = bottomLeft.rightNode;
            centreLeft = bottomLeft.aboveNode;

            if (topLeft.active) {
                configuration += 8;
            }
            if (topRight.active) {
                configuration += 4;
            }
            if (bottomRight.active) {
                configuration += 2;
            }
            if (bottomLeft.active) {
                configuration += 1;
            }
        }
    }

    public class Node {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 pos) {
            position = pos;
        }
    }

    public class ControlNode : Node {
        public bool active; // true if wall;
        public Node aboveNode;
        public Node rightNode;

        public ControlNode(Vector3 pos, bool isActive, float squareSize) : base(pos) {
            active = isActive;
            aboveNode = new Node(position + Vector3.forward * squareSize / 2f);
            rightNode = new Node(position + Vector3.right * squareSize / 2f);
        }
    }
}
