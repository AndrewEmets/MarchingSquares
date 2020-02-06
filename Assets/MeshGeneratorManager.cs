using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGeneratorManager : MonoBehaviour
{
    [SerializeField] private MarchingSquares generatorPrefab;

    [SerializeField] private Transform targetTransform;

    private Dictionary<Vector2Int, MarchingSquares> generators = new Dictionary<Vector2Int, MarchingSquares>();
    private Vector2Int currentCell = new Vector2Int(int.MaxValue, int.MaxValue);


    private void Start()
    {

    }

    private void Update()
    {
        var cell = Vector2Int.FloorToInt(targetTransform.position);

        if (cell != currentCell)
        {
            for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
            {
                var lcell = cell + new Vector2Int(i, j);

                if (!generators.ContainsKey(lcell))
                {
                    var gen = CreateNew(lcell);
                    generators.Add(lcell, gen);
                }
            }

            currentCell = cell;
        }
    }

    private MarchingSquares CreateNew(Vector2Int cell)
    {
        var gen = Instantiate(generatorPrefab);
        gen.transform.position = new Vector3(cell.x, cell.y, 0);
        gen.Offset = new Vector2(cell.x, cell.y) * gen.scale;
        gen.GetComponent<MeshFilter>().sharedMesh = null;
        gen.MakeMesh();

        return gen;
    }
}
