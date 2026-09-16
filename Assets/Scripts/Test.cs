using UnityEngine;
using UnityEngine.UIElements;

public class Test : MonoBehaviour
{
    public Mesh mesh;
    public Vector3[] vector3s;
    public Transform test;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Matrix4x4 matrix = Matrix4x4.TRS(
            new Vector3(5,0,0),
            Quaternion.Euler(90,0,0),
            new Vector3(1,5,1)
        );

        foreach (Vector3 v in vector3s)
        {
            Vector3 result = matrix.MultiplyPoint(v);
            test.position = result;
            Debug.Log(result +" | Old: " + v);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
