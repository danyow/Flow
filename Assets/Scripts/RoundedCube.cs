using UnityEngine;
using System.Collections;

// 必须要一个网格过滤器和渲染器
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoundedCube : MonoBehaviour
{
    public int xSize, ySize, zSize;
    // 圆度
    public int roundness;
    // 用来收集顶点的网格
    private Mesh mesh;
    // 顶点集
    private Vector3[] vertices;
    // 法线
    private Vector3[] normals;

    private void Awake() {
        Generate();
    }

    private void OnDrawGizmos() {
        if (vertices == null) {
            return;
        }
        for (int i = 0; i < vertices.Length; i++) {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(vertices[i], 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(vertices[i], normals[i]);
        }
    }

    private void Generate() {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural RoundedCube";
        CreateVertices();
        CreateTriangles();
    }

    // 创建顶点
    private void CreateVertices() {
        int cornerVertices = 8;
        int edgeVertices = (xSize + ySize + zSize - 3) * 4;
        int faceVertices = ((xSize - 1) * (ySize - 1) + (xSize - 1) * (zSize - 1) + (ySize - 1) * (zSize - 1)) * 2;
        vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
        normals  = new Vector3[vertices.Length];

        int v = 0;
        for (int y = 0; y <= ySize; y++) {
            for (int x = 0; x <= xSize; x++) {
                SetVertex(v++, x, y, 0);
            }

            for (int z = 1; z <= zSize; z++) {
                SetVertex(v++, xSize, y, z);
            }
            
            for (int x = xSize - 1; x >= 0; x--) {
                SetVertex(v++, x, y, zSize);
            }

            for (int z = zSize - 1; z > 0; z--) {
                SetVertex(v++, 0, y, z);
            }
        }
        // 顶面
        for (int z = 1; z < zSize; z++) {
            for (int x = 1; x < xSize; x++) {
                SetVertex(v++, x, ySize, z);
            }
        }
        // 底面
        for (int z = 1; z < zSize; z++) {
            for (int x = 1; x < xSize; x++) {
                SetVertex(v++, x, 0, z);
            }
        }
        mesh.vertices = vertices;
        // mesh.normals  = normals;
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
        t = CreateBottomFace(triangles, t, ring);
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
            o----vTop-<---o------o
            v------o------o------^
            o------o------o------o
          vMin-->vMid-----o----vMax
           (s)-----o-->--(v)-----o
            vMin: 最外环的一个点 第一个vMin是环算法的最后一个 在算面的时候回是倒着的
            vMid: 面上的一个点 第一个vMid是面算法的第一个 在算面的时候 刚刚好适用于面算法
            vMax: 再算第一行时的最后一个+2得到的 这里+2刚好对应着vMin vMin--时 vMax++
            vTop: 等算到该面的最后一行的时候 直接vMin-2即可获得
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
        // 该面最后一个四边形
        int vTop = vMin - 2;
        t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
        for (int x = 1; x < xSize - 1; x++, vTop--, vMid++) {
            t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
        }
        t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);
        return t;
    }

    private int CreateBottomFace(int[] triangles, int t, int ring) {
        // 最下面一环的顶点
        int v = 1;
        int vMid = vertices.Length - (xSize - 1) * (zSize - 1);
        t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
        // 靠近x的第一行
        for (int x = 1; x < xSize - 1; x++, v++, vMid++) {
            t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
        }
        t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);
        /**
           (0)----(1)-->-(v)-----o
           (e)-->-vMid----o-----vMax
          vMin-----o------o------o
            ^------o------o------o
            o-----vTop-<--o------o
            vMin: 最外环的一个点 第一个vMin是环算法的倒数第二个 在算面的时候回是倒着的
            vMid: 面上的一个点 第一个vMid是面算法的第一个 在算面的时候 刚刚好适用于面算法
            vMax: 再算v点时的最后一个+2得到的
            vTop: 等算到该面的最后一行的时候 直接vMin-2即可获得
         */
        int vMin = ring - 2;
        vMid -= xSize - 2;
        // 这里这个v还是靠近x的第一行的最后一个的首点
        int vMax = v + 2;
        for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++) {
            // 靠近x的第z行的第一个
            t = SetQuad(triangles, t, vMin, vMid + xSize - 1, vMin + 1, vMid);
            // 第z行剩余的 但不含最后一个 x从1开始-1结束 两边都被绘制过了
            for (int x = 1; x < xSize - 1; x++, vMid++) {
                t = SetQuad(triangles, t, vMid + xSize - 1, vMid + xSize, vMid, vMid + 1);
            }
            // 第z行最后一个四边形
            t = SetQuad(triangles, t, vMid + xSize - 1, vMax + 1, vMid, vMax);
        }
        // 该面最后一个四边形
        int vTop = vMin - 1;
        t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
        for (int x = 1; x < xSize - 1; x++, vTop--, vMid++) {
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
        }
        t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);
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

    private void SetVertex(int i, int x, int y, int z) {
        Vector3 inner = vertices[i] = new Vector3(x, y, z);

        if (x < roundness) {
            inner.x = roundness;
        } else if (x > xSize - roundness) {
            inner.x = xSize - roundness;
        }

        if (y < roundness) {
            inner.y = roundness;
        } else if (y > ySize - roundness) {
            inner.y = ySize - roundness;
        }

        if (z < roundness) {
            inner.z = roundness;
        } else if (z > zSize - roundness) {
            inner.z = zSize - roundness;
        }

        normals[i] = (vertices[i] - inner).normalized;


        vertices[i] = inner + normals[i] * roundness;
    }
}
