using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pacdot : MonoBehaviour
{
    //public Sprite Sprite;
    //unity每帧都会检测Pacdot触发器范围内是否有触发
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //如果有触发，检测是否为Pacman触发
        if (collision.gameObject.name == "Pacman")
        {
            //若当前是超级状态
            if (isSupperPacdot)
            {

                GameManager.Instance.OnEatSuperPacdot();    //准备下一个超级豆

            }
            if (IsPausePacdot)
            {
                GameManager.Instance.OnEatPausePacdot();    //准备下一个暂停道具
            }

            //销毁Pacdot
            GameManager.Instance.OnEatPacdot(gameObject);//从表内移除该豆子
            Destroy(gameObject);


        }
    }

    public bool isSupperPacdot = false;
    public bool IsPausePacdot = false;

    private void Update()
    {
        transform.Rotate(Vector3.forward * 30 * Time.deltaTime);
    }
}
