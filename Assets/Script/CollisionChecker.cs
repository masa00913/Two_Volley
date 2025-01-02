using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionChecker : MonoBehaviour
{
    private bool isEnterBall;
    private bool isExistBall;
    private bool isExitBall;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(isEnterBall){
            isEnterBall = false;
            isExistBall = true;
        }

        if(isExitBall){
            isExitBall = false;
        }
    }

    /// <summary>
    /// ボールがコリジョンに入った時
    /// </summary>
    /// <returns></returns>
    public bool GetIsEnterBall(){
        return isEnterBall;
    }

    /// <summary>
    /// ボールがコリジョン内にある
    /// </summary>
    /// <returns></returns>
    public bool GetIsExistBall(){
        return isExistBall;
    }

    /// <summary>
    /// ボールが外に出た時
    /// </summary>
    /// <returns></returns>
    public bool GetIsExitBall(){
        return isExitBall;
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "Ball"){
            isEnterBall = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject.tag == "Ball"){
            isExitBall = true;
            isExistBall = false;
        }
    }
}
