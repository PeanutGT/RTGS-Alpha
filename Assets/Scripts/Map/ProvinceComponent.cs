using UnityEngine;
using ProjectAlpha.Core;

namespace ProjectAlpha.Map
{
    [RequireComponent(typeof(PolygonCollider2D))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class ProvinceComponent : MonoBehaviour
    {
        public Province ProvinceData;

        public void Initialize(Province data)
        {
            ProvinceData = data;
        }
    }
}