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
        mesh.name = "Procedural Cube";
        CreateVertices();
        CreateTriangles();
    }

    // 创建顶点
    private void CreateVertices() {
        int cornerVertices = 8;
        int edgeVertices = (xSize + ySize + zSize - 3) * 4;
        int faceVertices = ((xSize - 1) * (ySize - 1) + (xSize - 1) * (zSize - 1) + (ySize - 1) * (zSize - 1)) * 2;
        vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];

        int v = 0;
        for (int y = 0; y <= ySize; y++) {
            for (int x = 0; x <= xSize; x++) {
                vertices[v++] = new Vector3(x, y, 0);
            }

            for (int z = 1; z <= zSize; z++) {
                vertices[v++] = new Vector3(xSize, y, z);
            }
            
            for (int x = xSize - 1; x >= 0; x--) {
                vertices[v++] = new Vector3(x, y, zSize);
            }

            for (int z = zSize - 1; z > 0; z--) {
                vertices[v++] = new Vector3(0, y, z);
            }
        }
        // 顶面
        for (int z = 1; z < zSize; z++) {
            for (int x = 1; x < xSize; x++) {
                vertices[v++] = new Vector3(x, ySize, z);
            }
        }
        // 底面
        for (int z = 1; z < zSize; z++) {
            for (int x = 1; x < xSize; x++) {
                vertices[v++] = new Vector3(x, 0, z);
            }
        }
        mesh.vertices = vertices;
    }

    // 创建三角形
    private void CreateTriangles() {
        int quads = (xSize * ySize + xSize * zSize + ySize * zSize) * 2;
        int pointCount = 6;
        int[] triangles = new int[quads * pointCount];

        // ring 一环的总数
        int ring = (xSize + zSize) * 2;
        // t是顶点数组的下标 v是第几个顶点
        int t = 0, v = 0;
        for (int y = 0; y < ySize; y++, v++) {
            for (int q = 0; q < ring - 1; q++, v++) {
                // q x轴上一面的第几个正方形
                t = SetQuad(triangles, t, v, v + 1, v + ring, v + ring + 1);
            }
            // 每环结束的最后一个三角形会抬高一环链接到高环上去 我们应该让他和本环第一个相连
            t = SetQuad(triangles, t, v, v - ring + 1, v + ring, v + 1);
        }
        t = CreateTopFace(triangles, t, ring);
        mesh.triangles = triangles;
    }
    // 创建最顶上的面
    private int CreateTopFace(int[] triangles, int t, int ring) {
        // 最上面一环的顶点
        int v = ring * ySize;
        // 靠近x的第一行
        for (int x = 0; x < xSize - 1; x++, v++) {
            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
        }
        t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);
        /**
            o------o--<---o------o
            o------o------o------^
            o------o------o------o
          vMin-->vMid-----o----vMax
           (s)-----o-->--(v)-----o
            vMin: 最外环的一个点 第一个vMin是环算法的最后一个 在算面的时候回是倒着的
            vMid: 面上的一个点 第一个vMid是面算法的第一个 在算面的时候 刚刚好适用于面算法
            vMax: 再算第一行时的最后一个+2得到的 这里+2刚好对应着vMin vMin--时 vMax++
         */
        int vMin = ring * (ySize + 1) - 1;
        int vMid = vMin + 1;
        // 这里这个v还是靠近x的第一行的最后一个的首点
        int vMax = v + 2;
        for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++) {
            // 靠近x的第z行的第一个
            t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + xSize - 1);
            // 第z行剩余的 但不含最后一个 x从1开始-1结束 两边都被绘制过了
            for (int x = 1; x < xSize - 1; x++, vMid++) {
                t = SetQuad(triangles, t, vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
            }
            // 第z行最后一个四边形
            t = SetQuad(triangles, t, vMid, vMax, vMid + xSize - 1, vMax + 1);
        }
        return t;
    }

    // 给予四个点创建两个三角形
    private static int SetQuad(int[] triangles, int i, int a, int b, int c, int d) {
        /**
            c------d
            |      |
            |      |
            a------b
            两个顺时针方向的三角形就是 a->c->b b->c->d
         */
        triangles[i] = a;
        triangles[i + 1] = triangles[i + 4] = c;
        triangles[i + 2] = triangles[i + 3] = b;
        triangles[i + 5] = d;
        return i + 6;
    }
}
