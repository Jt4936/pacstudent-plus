using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class chuansong : MonoBehaviour
{
    public GameObject GameObject;
    public Vector3 Vector3;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //����д���������Ƿ�ΪPacman����
        if (collision.gameObject.name == "Pacman")
        {
            //collision.transform.position = GameObject.transform.position;//����λ��
            collision.GetComponent<PacmanMove>().SetVector2(Vector3);
            collision.transform.position = Vector3;//����λ��
        }
    }
}
