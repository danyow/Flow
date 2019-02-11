using UnityEngine;
using System.Collections;

// 必须要一个网格过滤器和渲染器
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Grid : MonoBehaviour
{
    public int xSize, ySize;
    // 顶点集
    private Vector3[] vertices;
    // 用来收集顶点的网格
    private Mesh mesh;

    private void Awake() {
        Generate();
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

    private void Generate() {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Grid";
        // 每行列的顶点数要比格子数多一个
        vertices = new Vector3[(xSize + 1) * (ySize + 1)];
        for (int i = 0, y = 0; y <= ySize; y++) {
            for (int x = 0; x <= xSize; x++, i++) {
                vertices[i] = new Vector3(x, y);
            }
        }
        mesh.vertices = vertices;
        int pointCount = 6;
        // 开始绘制三角形
        int[] triangles = new int[xSize * pointCount * ySize];
        /**
            三角形绘制有一个绘制方向来确定前后是否可见
            1.!-- 如果是顺时针的话 那么就是面向前方可见
            2.!-- 逆时针就是反方向可见
         */
        // ti 理解为 一格的六个顶点 vi 理解为第几个格子 所以没换一行的时候 在y处 需要++
        for (int vi = 0, ti = 0, y = 0; y < ySize; y++, vi++) {
            for (int x = 0; x < xSize; x++, ti+=pointCount, vi++) {
                triangles[ti] = vi;
                triangles[ti + 4] = triangles[ti + 1] = xSize + vi + 1;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 5] = xSize + vi + 2;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
