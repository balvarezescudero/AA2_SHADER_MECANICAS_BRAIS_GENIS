using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boids_prueba1 : MonoBehaviour
{
    public GameObject objeto;

    public GameObject followTarget;

    public ComputeShader shader;
    // Start is called before the first frame update

    public int boidNumber = 5 ;

    List <Vector3> raysBoid;

    List<GameObject> objects;
    ComputeBuffer dataBuffer;
    boidObjInfo[] data;

    Vector3[] velos;

    void Start()
    {
        raysBoid = new List<Vector3>();

        objects = new List<GameObject>(boidNumber * boidNumber);

        for (int i =0; i < boidNumber; i++)
        {
            for (int j = 0; j < boidNumber; j++)
            {

                GameObject inst = Instantiate(objeto, this.transform);
                
                if(i%2 == 0)
                {
                    inst.transform.position = this.transform.position + Vector3.up * i * 0.5f + Vector3.left * j * 0.7f;
                }
                else
                {
                    inst.transform.position = this.transform.position + Vector3.forward * i * 0.5f + Vector3.right * j * 0.7f;
                }
               
                objects.Add(inst);

            }
        }

        int numObjs = objects.Count;

        data = new boidObjInfo[numObjs];
        velos = new Vector3[numObjs];

        for (int i =0; i < numObjs; i++)
        {
            data[i].position = objects[i].transform.position;
            data[i].velocity =Vector3.up;
            data[i].speed = Random.Range(0.3f, 1f);

        }

        dataBuffer = new ComputeBuffer(numObjs, boidObjInfo.Size);

        dataBuffer.SetData(data);


        generateRays();
    }

    // Update is called once per frame
    void Update()
    {
        int numObjs = objects.Count;
        int kernelHandle = shader.FindKernel("CSMain");

        shader.SetBuffer(kernelHandle, "ResultBoid", dataBuffer);

        shader.SetFloat("deltaTime", Time.deltaTime);
        shader.SetFloat("numObjs", numObjs);
        shader.SetVector("target", followTarget.transform.position);
        

        int groups = Mathf.CeilToInt(numObjs / 128.0f);
        shader.Dispatch(kernelHandle, groups, 1, 1);
        dataBuffer.GetData(data);


        //for each thread
        for(int i = 0; i < numObjs; i++)
        {
            objects[i].transform.position = data[i].position;

            objects[i].transform.rotation = RotateToSphere(objects[i].transform);

          
        }
       
      
            
       
        //dataBuffer.Release();
    }

    public struct boidObjInfo
    {
        public Vector3 position;
        public Vector3 velocity;
        public float speed;

        public static int Size
        {
            get
            {
                return sizeof(float) * 7;
            }
        }
    }

    private Quaternion RotateToSphere(Transform t)
    {
        Vector3 targetDirection = followTarget.transform.position - t.position;

        // The step size is equal to speed times frame time.
        float singleStep = 5 * Time.deltaTime;

        // Rotate the forward vector towards the target direction by one step
        Vector3 newDirection = Vector3.RotateTowards(t.forward, targetDirection, singleStep, 0.0f);

        // Calculate a rotation a step closer to the target and applies rotation to this object
        newDirection = new Vector3(newDirection.x, 0,newDirection.z);

        t.rotation = Quaternion.LookRotation(newDirection);

        return t.rotation;
    }

    private Vector3 Avoid_Obstacles (GameObject g)
    {
        
        Vector3 dirFinal =g.transform.forward;

        float distance = 0;

        RaycastHit hit;

        for (int i = 0; i < raysBoid.Count; i++)
        {
            Vector3 dir = g.transform.TransformDirection(raysBoid[i]);

            if (Physics.SphereCast (g.transform.position,2,dir,out hit,2))
            {
                
                if (hit.distance >distance)
                {
                    dirFinal = dir;
                    distance = hit.distance;
                }
            }
            else
            {
                return dir;
            }

        }
        
        return dirFinal;
    }


    private void generateRays()
    {
        
      
        for (int j = 0; j < 4; j++)
        {
            this.transform.Rotate(0, j * 2, 0, Space.Self);

            raysBoid.Add(this.transform.forward);

        }


      

        print(raysBoid.Count);

       

    }
}
