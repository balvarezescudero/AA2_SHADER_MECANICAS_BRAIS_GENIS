using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boids_prueba1 : MonoBehaviour
{
    public GameObject objeto;

    public GameObject followTarget;

    public ComputeShader shader;
    public bool Activate_Avoidance_Collision = false;
    // Start is called before the first frame update

    public int boidNumber = 5 ;

    List <Vector3> raysBoid;

    List<GameObject> objects;
    ComputeBuffer dataBuffer;
    boidObjInfo[] data;

    Vector3[] velocity;

    void Start()
    {
        raysBoid = new List<Vector3>();

        objects = new List<GameObject>(boidNumber * boidNumber);

        for (int i =0; i < boidNumber; i++)
        {
            for (int j = 0; j < boidNumber; j++)
            {

                GameObject inst = Instantiate(objeto, this.transform);

                if (i % 2 ==0)
                {
                    inst.transform.position = this.transform.position + Vector3.forward * i * 0.6f + Vector3.right * j * 0.6f;
                }
                else
                {
                    inst.transform.position = this.transform.position + Vector3.forward * j * 0.6f + Vector3.right * i * 0.6f;
                }
                    

                

                objects.Add(inst);

            }
        }

        int numObjs = objects.Count;

        data = new boidObjInfo[numObjs];
        velocity = new Vector3[numObjs];

        for (int i =0; i < numObjs; i++)
        {
            data[i].position = objects[i].transform.position;
            if (i%3==0)
            {
                data[i].velocity = velocity[i] = objects[i].transform.forward * 5;
            }
            else
            {
                data[i].velocity = velocity[i] = objects[i].transform.up * 5;
            }
           
            
            data[i].speed = Random.Range(0.3f, 1f);

        }

        dataBuffer = new ComputeBuffer(numObjs, boidObjInfo.Size);

        dataBuffer.SetData(data);


        generateRays();

        print(numObjs);
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
            //objects[i].transform.position = data[i].position;

            //Poner parametro

            Vector3 separationForce =clampForce(data[i].Separation, velocity[i]) * 1.5f;
            Vector3 alignmentForce = clampForce(data[i].Alignment, velocity[i]) * 1f;
            Vector3 cohesionForce = clampForce(data[i].Cohesion, velocity[i]) * 1f;
          //  Vector3 direcball = clampForce(followTarget.transform.position - objects[i].transform.position, velocity[i]) *2f;

            Vector3 aceleration = Vector3.zero;
            aceleration += separationForce;
            aceleration += alignmentForce;
            aceleration += cohesionForce;
          // aceleration += direcball;

            if (Activate_Avoidance_Collision)
            {
                if (collisionBoid(objects[i].transform))
                {
                    Vector3 dir = Avoid_Obstacles(objects[i]);
                    Vector3 force = clampForce(dir, velocity[i]) * 40;

                    aceleration += force;
                }
            }

            velocity[i] += aceleration * Time.deltaTime;
            float speed = velocity[i].magnitude;
            Vector3 direction = velocity[i] / speed;
            speed = Mathf.Clamp(speed, 1, 5);

            velocity[i] = direction * speed;

           objects[i].transform.position += velocity[i] * Time.deltaTime;

           objects[i].transform.LookAt(objects[i].transform.position+velocity[i]);

      

          
        }
       
      
            
       
        //dataBuffer.Release();
    }

    public struct boidObjInfo
    {
        public Vector3 position;
        public Vector3 velocity;
        public float speed;
        public Vector3 Separation;
        public Vector3 Alignment;
        public Vector3 Cohesion;

        public static int Size
        {
            get
            {
                return sizeof(float) * 16;
            }
        }
    }

    private Quaternion RotateToSphere(Transform t)
    {
        Vector3 targetDirection = t.forward*2 - t.position;

        // The step size is equal to speed times frame time.
        float singleStep = 5 * Time.deltaTime;

        // Rotate the forward vector towards the target direction by one step
        Vector3 newDirection = Vector3.RotateTowards(t.forward, targetDirection, singleStep, 0.0f);

        // Calculate a rotation a step closer to the target and applies rotation to this object
        newDirection = new Vector3(newDirection.x, newDirection.y, newDirection.z);

        t.rotation = Quaternion.LookRotation(t.position -t.forward *5);
        // t.rotation = t.LookAt(t.forward * 5, Vector3.forward);
        Debug.DrawLine(t.position, newDirection, Color.red, 10);
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

            if (!Physics.SphereCast (g.transform.position,0.7f,dir,out hit, 5, LayerMask.GetMask("BoidWalls")))
            {
                
                return dir;
            }   

        }
        
        return dirFinal;
    }


    private void generateRays()
    {


        //for (int j = 0; j < 15; j++)
        //{
        //    this.transform.Rotate(0, j *3 , 0, Space.Self);

        //    raysBoid.Add(this.transform.forward);
        //    Debug.DrawRay(this.transform.position, this.transform.forward, Color.red, 10f);

        //}
        //for (int i = 0; i < 15; i++)
        //{
        //    this.transform.Rotate(i * 3, 0, 0, Space.Self);

        //    raysBoid.Add(this.transform.forward);
        //    Debug.DrawRay(this.transform.position, this.transform.forward, Color.red, 10f);

        //}

        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float angleIncrement = Mathf.PI * 2 * goldenRatio;

        for(int i=0; i <100; i++){

            float t = (float)i / 100;
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);
            raysBoid.Add(new Vector3(x,y,z));


            Debug.DrawRay(this.transform.position, new Vector3(x, y, z), Color.red, 10f);

        }


    }
    //collision wegth 20
    bool collisionBoid(Transform position)
    {
        RaycastHit hit;
       // Debug.DrawRay(position.position,position.forward, Color.red, 10f);
        if (Physics.SphereCast(position.position,0.7f,position.forward,out hit,5,LayerMask.GetMask("BoidWalls")))
        {
            print("muro");
            return true;
        }
        
        return false;
    }

    private Vector3 clampForce (Vector3 vector, Vector3 vel)
    {
        Vector3 dir = vector.normalized * 5f-vel;
        return Vector3.ClampMagnitude(dir , 25);
    }
}
