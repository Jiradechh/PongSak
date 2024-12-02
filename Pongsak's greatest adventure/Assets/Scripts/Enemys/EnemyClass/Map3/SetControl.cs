using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class SetControl : MonoBehaviour
{
    public List<PowerOfDoritos> AllRubik;

    public DoritosBoss DoritosBoss;

    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            AllRubik.Add(transform.GetChild(i).gameObject.GetComponent<PowerOfDoritos>());
            AllRubik[i].setControl = this;
        }   
    }

    public void RubikDestory(PowerOfDoritos _rubik)
    {
        AllRubik.Remove(_rubik);

        if (AllRubik.Count > 0 )
        {
            AllRubik[0].CallReadyToAttack(); //Boss â¨ÁµÕµèÍ 
        }
        else
        {
            //<0 ¨ÐÅ§¾×é¹
            DoritosBoss.CallGoRetreat();
            Destroy(this.gameObject);
        }
    }

    public void CallRubikAttack()
    {
        AllRubik[0].CallReadyToAttack();
    }

}
