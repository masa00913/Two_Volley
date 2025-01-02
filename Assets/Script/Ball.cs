using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private bool isEnter;
    private PlayManager playManager;
    private Rigidbody _rigidbody;
    private Vector3 prevVelocity;

    // Start is called before the first frame update
    void Start()
    {
        playManager = GameObject.Find("PlayManager").GetComponent<PlayManager>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        prevVelocity = _rigidbody.velocity;
    }

    public bool GetIsEnter(){
        return isEnter;
    }

    public void SetIsEnter(bool isEnter){
        this.isEnter = isEnter;
    }

    private void OnCollisionEnter(Collision other) {
        if(other.gameObject.tag == "Flore"){
            isEnter = true;
            playManager.SetIsFallBall(true);
            Vector3 bounce = new Vector3(0, -prevVelocity.y * 0.6f, 0) * _rigidbody.mass;
            Debug.Log(_rigidbody.velocity);
            Debug.Log(prevVelocity);
            _rigidbody.AddForce(bounce, ForceMode.Impulse);
            Debug.Log("おちた");
        }
    }
}
