using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxScrolling : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public GameObject layer;         
        public float parallaxEffect;    
        public float speed;              
        public float length;             
        public float startPosX;         
    }

    public ParallaxLayer[] layers;      
  //  public GameObject cam;              

    void Start()
    {
       
        foreach (ParallaxLayer layer in layers)
        {
            layer.startPosX = layer.layer.transform.position.x;
            layer.length = layer.layer.GetComponent<SpriteRenderer>().bounds.size.x;
        }
    }

    void Update()
    {
        foreach (ParallaxLayer layer in layers)
        {
          
            //float temp = (cam.transform.position.x * (1 - layer.parallaxEffect));
           // float dist = (cam.transform.position.x * layer.parallaxEffect) * layer.speed;

           
           // if (layer.parallaxEffect != 0 || layer.speed != 0)
            {
                //layer.layer.transform.position = new Vector3(layer.startPosX + dist, layer.layer.transform.position.y, layer.layer.transform.position.z);
            }

            
           // if (temp > layer.startPosX + layer.length)
            {
                layer.startPosX += layer.length;
            }
           // else if (temp < layer.startPosX - layer.length)
            {
                layer.startPosX -= layer.length;
            }
        }
    }
}