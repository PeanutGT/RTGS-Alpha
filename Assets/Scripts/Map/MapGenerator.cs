using System.Collections.Generic;
using UnityEngine;
using ProjectAlpha.Core;
using DelaunatorSharp;

namespace ProjectAlpha.Map
{
    public class MapGenerator : MonoBehaviour
    {
        [Header("Terrain Settings")]
        public float noiseScale = 0.05f; // 數值越小，地形區塊越大
        public Vector2 noiseOffset;      // 隨機種子偏移
        
        [Header("Map Settings")]
        public Vector2 mapSize = new Vector2(100, 100);
        public float pointRadius = 5f; 

        [Header("Generated Data")]
        public List<Province> Provinces = new List<Province>();
        public List<Border> Borders = new List<Border>();

        public void GenerateMap()
        {
            Provinces.Clear();
            Borders.Clear();

            // 1. 生成均勻的種子點
            List<Vector2> seedPoints = PoissonDiscSampler.GeneratePoints(pointRadius, mapSize);
            
            IPoint[] points = new IPoint[seedPoints.Count];
            for (int i = 0; i < seedPoints.Count; i++)
            {
                points[i] = new Point(seedPoints[i].x, seedPoints[i].y);
                Province newProvince = new Province(i) { Center = seedPoints[i] };
                Provinces.Add(newProvince);
            }

            // 2. 執行 Delaunay 演算法
            Delaunator delaunator = new Delaunator(points);

            // 3. 解析 Delaunay 轉換為 Voronoi
            ExtractVoronoi(delaunator, points);
            // --- 新增：分配地形與資源 ---
            AssignTerrainAndResources();
            // ------------------------------


            // 4. 建立 Unity 實體物件與 Mesh
            BuildProvinceGameObjects();

            Debug.Log($"[MapGenerator] 地圖生成完畢。地塊數量: {Provinces.Count}, 邊界數量: {Borders.Count}");
        }

        private void ExtractVoronoi(Delaunator delaunator, IPoint[] points)
        {
            Vector2[] circumcenters = new Vector2[delaunator.Triangles.Length / 3];
            for (int t = 0; t < circumcenters.Length; t++)
            {
                Vector2 p1 = new Vector2((float)points[delaunator.Triangles[3 * t]].X, (float)points[delaunator.Triangles[3 * t]].Y);
                Vector2 p2 = new Vector2((float)points[delaunator.Triangles[3 * t + 1]].X, (float)points[delaunator.Triangles[3 * t + 1]].Y);
                Vector2 p3 = new Vector2((float)points[delaunator.Triangles[3 * t + 2]].X, (float)points[delaunator.Triangles[3 * t + 2]].Y);
                circumcenters[t] = GetCircumcenter(p1, p2, p3);
            }

            Dictionary<string, Border> borderDict = new Dictionary<string, Border>();

            for (int e = 0; e < delaunator.Halfedges.Length; e++)
            {
                int nextEdge = NextHalfedge(e);
                int startNodeId = delaunator.Triangles[e];
                int endNodeId = delaunator.Triangles[nextEdge];
                int oppositeEdge = delaunator.Halfedges[e];

                if (oppositeEdge != -1)
                {
                    int triangleA = e / 3;
                    int triangleB = oppositeEdge / 3;

                    Vector2 v1 = circumcenters[triangleA];
                    Vector2 v2 = circumcenters[triangleB];

                    Province p = Provinces[startNodeId];
                    if (!p.Vertices.Contains(v1)) p.Vertices.Add(v1);
                    if (!p.Vertices.Contains(v2)) p.Vertices.Add(v2);

                    string edgeKey = startNodeId < endNodeId ? $"{startNodeId}_{endNodeId}" : $"{endNodeId}_{startNodeId}";
                    if (!borderDict.ContainsKey(edgeKey))
                    {
                        Province neighbor = Provinces[endNodeId];
                        Border newBorder = new Border(p, neighbor);
                        borderDict.Add(edgeKey, newBorder);
                        Borders.Add(newBorder);
                        
                        p.Borders.Add(newBorder);
                        neighbor.Borders.Add(newBorder);
                    }
                }
            }
        }

        // --- 新增：建立實體與 Mesh 相關邏輯 ---
        private void BuildProvinceGameObjects()
        {
            GameObject mapRoot = new GameObject("MapRoot");

            foreach (var province in Provinces)
            {
                // 若地塊沒有足夠頂點(通常在邊緣)，直接跳過
                if (province.Vertices.Count < 3) continue;

                SortVerticesClockwise(province);

                GameObject go = new GameObject($"Province_{province.ID}");
                go.transform.SetParent(mapRoot.transform);

                ProvinceComponent comp = go.AddComponent<ProvinceComponent>();
                comp.Initialize(province);

                PolygonCollider2D col = go.GetComponent<PolygonCollider2D>();
                col.SetPath(0, province.Vertices.ToArray());

                MeshFilter meshFilter = go.GetComponent<MeshFilter>();
                MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
                
                // 暫時給予隨機顏色以便在畫面上區分區塊
                meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
                meshRenderer.material.color = GetTerrainColor(province.Terrain);

                meshFilter.mesh = CreateMesh(province.Vertices);
            }
        }

        private void SortVerticesClockwise(Province province)
        {
            Vector2 center = province.Center;
            province.Vertices.Sort((a, b) =>
            {
                float angleA = Mathf.Atan2(a.y - center.y, a.x - center.x);
                float angleB = Mathf.Atan2(b.y - center.y, b.x - center.x);
                return angleB.CompareTo(angleA); 
            });
        }

        private Mesh CreateMesh(List<Vector2> vertices)
        {
            Mesh mesh = new Mesh();
            Vector3[] vertices3D = new Vector3[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices3D[i] = new Vector3(vertices[i].x, vertices[i].y, 0);
            }

            // Voronoi Cell 保證是凸多邊形，適用簡單的 Fan Triangulation
            int[] triangles = new int[(vertices.Count - 2) * 3];
            for (int i = 0; i < vertices.Count - 2; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }

            mesh.vertices = vertices3D;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }
        // ----------------------------------------

        private Vector2 GetCircumcenter(Vector2 a, Vector2 b, Vector2 c)
        {
            float d = 2 * (a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y));
            if (Mathf.Abs(d) <= Mathf.Epsilon) return a; 

            float ax2y2 = a.x * a.x + a.y * a.y;
            float bx2y2 = b.x * b.x + b.y * b.y;
            float cx2y2 = c.x * c.x + c.y * c.y;

            float x = (ax2y2 * (b.y - c.y) + bx2y2 * (c.y - a.y) + cx2y2 * (a.y - b.y)) / d;
            float y = (ax2y2 * (c.x - b.x) + bx2y2 * (a.x - c.x) + cx2y2 * (b.x - a.x)) / d;

            return new Vector2(x, y);
        }

        private int NextHalfedge(int e) => (e % 3 == 2) ? e - 2 : e + 1;

        private void AssignTerrainAndResources()
        {
            // 隨機生成地形偏移值，確保每次生成的地圖地形分布不同
            noiseOffset = new Vector2(Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));

            foreach (var province in Provinces)
            {
                // 取得地塊中心點，配合縮放與偏移計算柏林雜訊 (值域約為 0.0 ~ 1.0)
                float sampleX = province.Center.x * noiseScale + noiseOffset.x;
                float sampleY = province.Center.y * noiseScale + noiseOffset.y;
                float noiseValue = Mathf.PerlinNoise(sampleX, sampleY);

                // 依據雜訊值分配地形與資源
                if (noiseValue < 0.35f)
                {
                    province.Terrain = TerrainType.Plain;
                    province.BaseIC = 2; // 平原最適合發展，基礎產值較高
                }
                else if (noiseValue < 0.50f)
                {
                    province.Terrain = TerrainType.Swamp;
                    province.BaseIC = 1;
                }
                else if (noiseValue < 0.70f)
                {
                    province.Terrain = TerrainType.Hill;
                    province.BaseIC = 1;
                }
                else if (noiseValue < 0.85f)
                {
                    province.Terrain = TerrainType.Mountain;
                    province.BaseIC = 0; // 山地不產 IC
                }
                else
                {
                    province.Terrain = TerrainType.Peak;
                    province.BaseIC = 0;
                }
            }
        }

        private Color GetTerrainColor(TerrainType terrain)
        {
            switch (terrain)
            {
                case TerrainType.Plain: return new Color(0.4f, 0.8f, 0.4f, 0.7f);    // 淺綠
                case TerrainType.Swamp: return new Color(0.3f, 0.4f, 0.3f, 0.7f);    // 暗綠灰
                case TerrainType.Hill: return new Color(0.7f, 0.7f, 0.4f, 0.7f);     // 卡其/黃綠
                case TerrainType.Mountain: return new Color(0.6f, 0.5f, 0.4f, 0.7f); // 棕色
                case TerrainType.Peak: return new Color(0.9f, 0.9f, 0.9f, 0.7f);     // 白色
                default: return Color.gray;
            }
        }
    }
}