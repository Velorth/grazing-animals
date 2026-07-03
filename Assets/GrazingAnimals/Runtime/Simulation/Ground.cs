using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;

namespace GrazingAnimals
{
    public class Ground : MonoBehaviour
    {
        [SerializeField] private ProBuilderMesh _proBuilderMesh;
        
        public void Generate(int size)
        {
            var shape = new Cube();
            shape.RebuildMesh(_proBuilderMesh, new Vector3(size, 1, size), Quaternion.identity);
            transform.localPosition = new Vector3(.5f * size - .5f, -0.5f, .5f * size - .5f);
        }
    }
}