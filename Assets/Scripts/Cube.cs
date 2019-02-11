using UnityEngine;
using System.Collections;

// 必须要一个网格过滤器和渲染器
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Cube : MonoBehaviour
{
    public int xSize, ySize, zSize;
    // 用来收集顶点的网格
    private Mesh mesh;
    // 顶点集
    private Vector3[] vertices;

    private void Awake() {
        StartCoroutine(Generate());
    }

    private void OnDrawGizmos() {
        if (vertices == null) {
            return;
        }
        Gizmos.color = Color.black;
        for (int i = 0; i < vertices.Length; i++) {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }

    private IEnumerator Generate() {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Cube";
        WaitForSeconds wait = new WaitForSeconds(0.05f);

        int cornerVertices = 8;
        int edgeVertices = (xSize + ySize + zSize - 3) * 4;
        int faceVertices = ((xSize - 1) * (ySize - 1) + (xSize - 1) * (zSize - 1) + (ySize - 1) * (zSize - 1)) * 2;
        vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];

        int v = 0;
        for (int y = 0; y <= ySize; y++) {
            for (int x = 0; x <= xSize; x++) {
                vertices[v++] = new Vector3(x, y, 0);
                yield return wait;
            }

            for (int z = 0; z <= zSize; z++) {
                vertices[v++] = new Vector3(xSize, y, z);
                yield return wait;
            }
            
            for (int x = xSize - 1; x >= 0; x--) {
                vertices[v++] = new Vector3(x, y, zSize);
                yield return wait;
            }

            for (int z = zSize - 1; z > 0; z--) {
                vertices[v++] = new Vector3(0, y, z);
                yield return wait;
            }
        }
        // 顶面
        for (int z = 1; z < zSize; z++) {
            for (int x = 0; x < xSize; x++) {
                vertices[v++] = new Vector3(x, ySize, z);     
            }
        }
        // 底面
        for (int z = 1; z < zSize; z++) {
            for (int x = 0; x < xSize; x++) {
                vertices[v++] = new Vector3(x, 0, z);     
            }
        }
    }
}
