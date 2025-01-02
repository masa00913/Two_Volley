using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Transform _transform;
    private Vector3 pos;
    private float moveSpeed = 5f;
    private float moveNormalSpeed = 5f;
    private float maxJumpHeight = 2f;
    private float maxJumpTime = 2f;
    [Header("ジャンプで頂点に到達するまでの時間")][SerializeField]private float reachTopTime = 0.5f;
    [Header("ジャンプで頂点に滞在する時間")][SerializeField]private float jumpTopTime = 0.5f;
    [Header("スパイクの高さ")]private float spikeHeight;
    [Header("スパイクのパワー")]private float spikePower = 30f;
    [Header("味方")][SerializeField]private bool isPlayer;
    [Header("操作対象")][SerializeField]private bool isMyPlayer;
    private float jumpTime;
    [Header("地面にいるときの高さ")]private float normalHei;
    [Header("身長")]private float tall;
    private bool isSpikeJump;
    private bool isGround;

    [Header("レシーブの高さ")]private float receiveHeight = 7f;
    [Header("レシーブでAカットを狙ういち")]private Vector3 receiveAimPoint;
    [Header("レシーブのタイミングを合わせたか")]private bool isReceiveTap;
    [Header("レシーブのタイミングの良さ0:great,1:good,2:bad")]private int receiveTimingPoint;
    [Header("レシーブのディレイ")]private float receiveDelay = 0.1f;
    [Header("レシーブのコルーチン中か")]private bool isReceiveCorutine;
    [Header("レシーブの後の硬直時間")]private float receiveFreezeTime = 0.8f;
    [Header("レシーブ後の硬直中か")]private bool isReceiveFreeze;
    [Header("ボールとの2次元距離XZ")]private float ballDist2D;


    private PlayManager playManager;

    private CollisionChecker overCollision;
    private CollisionChecker underCollision;
    private CollisionChecker spikeCollision;
    private CollisionChecker serveCollision;

    [Header("サーブのボールを持つ場所")]private GameObject serveBallPos;
    [Header("サーブトスを上げたか")]private bool isServeToss;
    [Header("サーブ打ったか")]private bool isServe;
    [Header("サーブの準備が完了したか")]private bool isServeReady;
    [Header("サーブをのタップをしたか")]private bool isServeTap;
    [Header("サーブのタイミングの良さ")]private int serveTimingPoint;
    [Header("サーブの角度")]private float serveAngle;
    [Header("サーブトスのディレイ")]private float serveTossDelay = 0.5f;
    [Header("サーブのディレイ")]private float serveDelay = 0f;


    [Header("前衛かどうか")][SerializeField]private bool isFront;
    [Header("スパイクを打ったか")]private bool isSpike;
    [Header("スパイクのためにボタンを押したか")]private bool isSpikeTap;
    [Header("スパイクのためのタイミング")]private int spikeTimingPoint;
    [Header("スパイクのアングル")]private float spikeAngle;
    [Header("スパイクのディレイ")]private float spikeDelay = 0.2f;
    [Header("スパイクのステップを始めるタイミング")]private float spikeStepStart = 1.5f;

    [Header("ブロックのジャンプの高さ")]private float blockJumpHeight = 1.5f;
    [Header("ブロックの高さ")]private float blockHeight;
    [Header("ブロックジャンプで頂点に到達するまでの時間")][SerializeField]private float blockReachTopTime = 0.5f;
    [Header("ブロックジャンプで頂点に滞在する時間")][SerializeField]private float blockTopTime = 0.25f;
    [Header("ブロックジャンプするか")]private bool isBlockJump;
    [Header("ブロックのコリジョン")] private CollisionChecker blockCollision;
    [Header("ブロックタッチをしたか")]private bool isBlockTouch;

    [Header("フライングをしたか")]private bool isFlying;
    [Header("フライングにかかる時間")]private float flyingNeedTime = 0.2f;
    [Header("フライングしている時間")]private float flyingTime;
    [Header("フライングでどのくらいスピード上がるか")]private float flyingSpeedRatio = 1.5f;

    [Header("レフトサイドにいるか")]private bool isLeft;

    [Header("アニメーション")]private Animator animator;
    [Header("アニメーションのパラメータ")]private bool animation_isSet;
    [Header("アニメーションのパラメータ")]private bool animation_isSet_Over;
    [Header("アニメーションのパラメータ")]private bool animation_isSetWalk_l;
    [Header("アニメーションのパラメータ")]private bool animation_isSetWalk_r;
    [Header("アニメーションのパラメータ")]private bool animation_isWalk;
    [Header("アニメーションのパラメータ")]private bool animation_isSpike;
    [Header("アニメーションのパラメータ")]private bool animation_isSpikeJump;
    [Header("アニメーションのパラメータ")]private bool animation_isSpikeStep;
    [Header("アニメーションのパラメータ")]private bool animation_isBlock;
    [Header("アニメーションのパラメータ")]private bool animation_isBlockSet;
    [Header("アニメーションのパラメータ")]private bool animation_isBlockSetWalk_l;
    [Header("アニメーションのパラメータ")]private bool animation_isBlockSetWalk_r;
    [Header("アニメーションのパラメータ")]private bool animation_isUnderReceive;
    [Header("アニメーションのパラメータ")]private bool animation_isOverReceive;
    [Header("アニメーションのパラメータ")]private bool animation_isFlying;
    [Header("アニメーションのパラメータ")]private bool animation_isServeSet;
    [Header("アニメーションのパラメータ")]private bool animation_isServeSetUp;
    [Header("アニメーションのパラメータ")]private bool animation_isServe;


    private GameObject ball;
    private Rigidbody ballRig;




    // Start is called before the first frame update
    void Start()
    {
        _transform = this.transform;
        if(isPlayer){
            normalHei = 0;
            tall = transform.localScale.y;
        }else{
            normalHei = transform.localScale.y;
            tall = transform.localScale.y * 2;
        }
        
        
        isGround = true;
        playManager = GameObject.Find("PlayManager").GetComponent<PlayManager>();

        overCollision = transform.Find("Over").GetComponent<CollisionChecker>();
        underCollision = transform.Find("Under").GetComponent<CollisionChecker>();
        spikeCollision = transform.Find("SpikePos").GetComponent<CollisionChecker>();
        serveCollision = transform.Find("ServePos").GetComponent<CollisionChecker>();
        blockCollision = transform.Find("Block").GetComponent<CollisionChecker>();
        serveBallPos = transform.Find("ServeBallPos").gameObject;

        blockHeight = blockCollision.transform.localPosition.y + blockJumpHeight + normalHei;
        spikeHeight = spikeCollision.transform.localPosition.y + maxJumpHeight + normalHei;

        ball = GameObject.Find("Ball");
        ballRig = ball.GetComponent<Rigidbody>();
        if(isPlayer){
            receiveAimPoint = new Vector3(0,2,3);
        }else{
            receiveAimPoint = new Vector3(0,2,-3);
        }

        animator = GetComponent<Animator>();
        Debug.Log(spikeCollision.transform.localPosition);
    }

    // Update is called once per frame
    void Update()
    {
        ReceiveTiming();
        SpikeTiming();
        ServeTiming();

        SetIsLeft();

        Jump();
        Move();
        
        Block();
        Flying();

        ServeToss();

        AnimationSet();

        var ball2D = new Vector2(ball.transform.position.x,ball.transform.position.z);
        var player2D = new Vector2(_transform.position.x,_transform.position.z);
        ballDist2D = Vector2.Distance(ball2D,player2D);
    }

    private void FixedUpdate() {
        if(playManager.GetIsReset()) return;
        Receive();
        Spike();
        Serve();
        BlockTouch();
    }

    public void ResetPlayer(){
        if(!isPlayer) return;
        animation_isSet = false;
        animation_isSet_Over = false;
        animation_isSetWalk_l = false;
        animation_isSetWalk_r = false;
        animation_isWalk = false;
        animation_isSpike = false;
        animation_isSpikeJump = false;
        animation_isSpikeStep = false;
        animation_isBlock = false;
        animation_isBlockSet = false;
        animation_isBlockSetWalk_l = false;
        animation_isBlockSetWalk_r = false;
        animation_isUnderReceive = false;
        animation_isOverReceive = false;
        animation_isFlying = false;
        animation_isServeSet = false;
        animation_isServeSetUp = false;
        animation_isServe = false;
        animator.Play("metarig|Idle");
    }

    /// <summary>
    /// プレイヤーのアニメーションを指定
    /// </summary>
    private void AnimationSet(){
        if(!isPlayer) return;
        animator.SetBool("isSet", animation_isSet);
        animator.SetBool("isSet_Over", animation_isSet_Over);
        animator.SetBool("isSetWalk_l", animation_isSetWalk_l);
        animator.SetBool("isSetWalk_r", animation_isSetWalk_r);
        animator.SetBool("isWalk", animation_isWalk);
        animator.SetBool("isSpike", animation_isSpike);
        animator.SetBool("isSpikeJump", animation_isSpikeJump);
        animator.SetBool("isSpikeStep", animation_isSpikeStep);
        animator.SetBool("isBlock", animation_isBlock);
        animator.SetBool("isBlockSet", animation_isBlockSet);
        animator.SetBool("isBlockSetWalk_l", animation_isBlockSetWalk_l);
        animator.SetBool("isBlockSetWalk_r", animation_isBlockSetWalk_r);
        animator.SetBool("isUnderReceive", animation_isUnderReceive);
        animator.SetBool("isOverReceive", animation_isOverReceive);
        animator.SetBool("isFlying", animation_isFlying);
        animator.SetBool("isServeSet", animation_isServeSet);
        animator.SetBool("isServeSetUp", animation_isServeSetUp);
        animator.SetBool("isServe", animation_isServe);
    }

    /// <summary>
    /// 今自分が左側にいるか設定
    /// </summary>
    private void SetIsLeft(){
        if(playManager.GetBallIn() == isPlayer){
            if(playManager.GetBallTouchNum(isPlayer) == 2){
                if(playManager.GetWillSpikePlayer(isPlayer) == this){
                    isLeft = playManager.GetBallFallSide(isPlayer);
                    Debug.Log(isLeft + "にボールが落ちる : スパイカー");
                }else{
                    isLeft = playManager.GetBallFallSide(isPlayer);
                }
            }
        }else{
            if(playManager.GetBallTouchNum(!isPlayer) == 2){
                if(isFront){
                    isLeft = !playManager.GetWillSpikePlayer(!isPlayer).GetIsLeft();
                }else{
                    isLeft = playManager.GetWillSpikePlayer(!isPlayer).GetIsLeft();
                    Debug.Log("レシーバーの左右" + isLeft );
                }
            }
        }
        
    }

    public bool GetIsLeft(){
        return isLeft;
    }

    // <-----------------------------ブロック--------------------------------->

    /// <summary>
    /// ブロックタッチをしたか
    /// </summary>
    private void BlockTouch(){
        
        if(!isBlockJump) {
            isBlockTouch = false;
            return;
        }
        if(isBlockTouch) return;
        if(blockCollision.GetIsExistBall()){
            var blockPos = blockCollision.transform.position;
            var blockScale = blockCollision.transform.lossyScale.x;
            Debug.Log(blockScale);
            var ballPos = ball.transform.position;
            var dist = Vector3.Distance(ballPos,blockPos);
            var blockRatio = dist / (blockScale/2);
            var velocity = ballRig.velocity;
            if(blockRatio < 0.2){
                Debug.Log("シャっと");
                var reverseForce = -velocity * ballRig.mass;
                var angle = Random.Range(-45,45) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Pow(Mathf.Sin(angle),2), -1, 1 * Mathf.Pow(Mathf.Cos(angle),2));
                if(isPlayer){
                    dir.z *= -1;
                }
                dir = dir.normalized;
                var force = reverseForce + dir * velocity.magnitude * 2f * ballRig.mass;
                ballRig.AddForce(force,ForceMode.Impulse);
            }else if(blockRatio < 0.8){
                Debug.Log("ナイスワンチ");
                var reverseForce = -velocity * ballRig.mass;
                var angle = Random.Range(-45,45) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Pow(Mathf.Sin(angle),2), 1, 1 * Mathf.Pow(Mathf.Cos(angle),2));
                dir = dir.normalized;
                if(!isPlayer){
                    dir.z *= -1;
                }
                var blockVel = velocity*0.3f +  dir * velocity.magnitude * 0.3f;
                var force = reverseForce + blockVel * ballRig.mass;
                ballRig.AddForce(force,ForceMode.Impulse);
            }else{
                Debug.Log("あんま変わらない");
                var reverseForce = -velocity * ballRig.mass;
                var angle = Random.Range(-45,45) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Pow(Mathf.Sin(angle),2), 1, 1 * Mathf.Pow(Mathf.Cos(angle),2));
                dir = dir.normalized;
                if(!isPlayer){
                    dir.z *= -1;
                }
                var blockVel = velocity*0.6f +  dir * velocity.magnitude * 0.3f;
                var force = reverseForce + blockVel * ballRig.mass;
                ballRig.AddForce(force,ForceMode.Impulse);
            }
            isBlockTouch = true;
            playManager.SetLastTouchPlayer(this);
        }
    }

    

    /// <summary>
    /// ブロックのジャンプの処理
    /// </summary>
    private void Block(){
        if(!isFront) return;
        if(playManager.GetBallIn() == isPlayer) return;
        if(!isGround) return;
        if(playManager.GetBallTouchNum(!isPlayer) == 2){
            if(!isMyPlayer){
                if(playManager.GetBallReachTime(blockHeight,false) != 0 && playManager.GetBallReachTime(blockHeight,false) < blockReachTopTime + blockTopTime/2){
                    isBlockJump  = true;
                }
            }else{
                if(Input.GetMouseButtonUp(0)){
                    isBlockJump =true;
                }
            }
            
        }
    }

    private void Jump(){
        pos = _transform.position;
        
        if(isSpikeJump){
            animation_isSpikeJump = true;
            animation_isSpikeStep = false;
            isGround = false;
            jumpTime += Time.deltaTime;
            var jumpHeight = GetJumpHeight(maxJumpHeight,reachTopTime,jumpTopTime,jumpTime);
            pos.y = normalHei + jumpHeight;
            if(jumpHeight <= 0){
                isSpikeJump = false;
                animation_isSpikeJump = false;
                isGround = true;
                jumpTime = 0;
                pos.y = normalHei;
            }
        }

        if(isBlockJump){
            isGround = false;
            animation_isBlock = true;
            jumpTime += Time.deltaTime;
            var jumpHeight = GetJumpHeight(blockJumpHeight,blockReachTopTime,blockTopTime,jumpTime);
            pos.y = normalHei + jumpHeight;
            if(jumpHeight <= 0){
                isBlockJump = false;
                animation_isBlock = false;
                isGround = true;
                jumpTime = 0;
                pos.y = normalHei;
            }
        }
        
        _transform.position = pos;
    }

    

    /// <summary>
    /// Jumpの高さを取得0~maxJUmp
    /// </summary>
    /// <param name="maxJumpHeight"></param>
    /// <param name="maxJumpTime"></param>
    /// <param name="jumpTime"></param>
    /// <returns></returns>
    private float GetJumpHeight(float maxJumpHeight,float reachTopTime,float topTime,float jumpTime){
        float jumpHeight;

        if(jumpTime < reachTopTime){
            var x = jumpTime - reachTopTime;
            jumpHeight = -(maxJumpHeight / Mathf.Pow(reachTopTime,2)) * (x*x) + maxJumpHeight;
        }else if(jumpTime < reachTopTime + topTime){
            jumpHeight = maxJumpHeight;
        }else{
            var x = jumpTime - topTime - reachTopTime;
            jumpHeight = -(maxJumpHeight / Mathf.Pow(reachTopTime,2)) * (x*x) + maxJumpHeight;
            if(jumpHeight <= 0){
                jumpHeight = 0;
            }
        }
        
        return jumpHeight;
    }

    //　<-----------------------------スパイク--------------------------------->
    /// <summary>
    /// スパイクの処理
    /// </summary>
    private void SpikeTiming(){
        if(!isMyPlayer) return;

        if(isSpikeJump){
            if(!isSpikeTap && Input.GetMouseButtonDown(0)){
                var ballPos = ball.transform.position;
                var spikePos = spikeCollision.transform.position;
                var dist = Vector3.Distance(ballPos,spikePos);
                if(dist < 2f){
                    Debug.Log("ナイスタイミング");
                    spikeTimingPoint = 0;
                }else if(dist < 3f){
                    Debug.Log("速すぎ");
                    spikeTimingPoint = 1;
                }else{
                    Debug.Log("速すぎんご");
                    spikeTimingPoint = 2;
                }
                var mx = Input.mousePosition.x-Screen.width/2;
                spikeAngle = mx/(Screen.width/2) * 90f;
                Debug.Log("セット" + spikeAngle);
                isSpikeTap = true;
            }
        }else{
            isSpikeTap = false;
        }

        if(playManager.GetBallTouchNum(isPlayer) == 2 && playManager.GetWillSpikePlayer(isPlayer) == this){
            if(isMyPlayer){
                if(Input.GetMouseButtonDown(0)){
                    isSpikeJump = true;
                }
            }
        }
    }

    /// <summary>
    /// スパイクの処理
    /// </summary>
    private void Spike(){
    if(playManager.GetBallTouchNum(isPlayer) == 2){
        if(isMyPlayer){

        }else{
            var ballReachiTime = playManager.GetBallReachTime(spikeHeight,false);
            if(ballReachiTime > 0){       
                if(ballReachiTime < (reachTopTime + jumpTopTime/2f)){
                    if(playManager.GetWillSpikePlayer(isPlayer) == this && playManager.GetBallIn() == isPlayer){
                        isSpikeJump = true;
                    }
                }
            }
        }
    }

    if(isGround) {
        isSpike = false;
        animation_isSpike = false;
        animation_isSpikeStep = false;
        animation_isSpikeJump = false;
    }
    if(isGround || isSpike) return;
    if(isMyPlayer && !isSpikeTap) return;

    if(playManager.GetWillSpikePlayer(isPlayer) == this && playManager.GetBallTouchNum(isPlayer) == 2){
        if(spikeCollision.GetIsExistBall()){
            var dist = Vector3.Distance(spikeCollision.transform.position,ball.transform.position);
            if(dist <= 1){ 
                StartCoroutine(SpikeAfterDelay());
                isSpike = true;
                
            }
        }

        if(playManager.GetBallReachTime(spikeHeight,false) != 0 && playManager.GetBallReachTime(spikeHeight,false) < 0.3f){
            animation_isSpike = true;
            animation_isSpikeJump = false;
        }
    }
}

private IEnumerator SpikeAfterDelay(){
    // ボールを停止させる
    if(!ballRig.isKinematic){
        ballRig.velocity = Vector3.zero;
        ballRig.isKinematic = true;
    }

    // 0.2秒待つ
    yield return new WaitForSeconds(spikeDelay);

    // ボールの停止を解除
    ballRig.isKinematic = false;
    if(isPlayer){
        if(isMyPlayer){
            switch (spikeTimingPoint)
            {
                case 0:
                    spikePower = 50f;
                    break;
                case 1:
                    spikePower = 30f;
                    break;
                case 2:
                    spikePower = 20f;
                    break;
                default:
                    break;
            }
        }else{
            spikeAngle = Random.Range(-90,90);
        }
        playManager.SpikeForce(spikePower, GetAtackAimPoint(spikeAngle));
    }else{
        Debug.Log("spikeAngle" + spikeAngle);
        playManager.SpikeForce(spikePower, GetAtackAimPoint(spikeAngle));
    }

    
}

    /// <summary>
    /// スパイクのねらう場所を取得
    /// </summary>
    /// <param name="spikeAngle"></param>
    /// <returns></returns>
    private Vector3 GetAtackAimPoint(float spikeAngle){
        Vector3 spikeAimPoint = Vector3.zero;
        if(isPlayer){
            spikeAimPoint.z = -8f;
        }else{
            spikeAimPoint.z = 8f;
        }
        var ballPos = ball.transform.position;
        var xPos = Mathf.Sin(Mathf.Deg2Rad * spikeAngle) * 4.5f;
        spikeAimPoint.x = ballPos.x + xPos;
        if(spikeAimPoint.x > 4.5f){
            spikeAimPoint.x = 4.5f;
        }else if(spikeAimPoint.x < -4.5f){
            spikeAimPoint.x = -4.5f;
        }
        return spikeAimPoint;
    }

    public Vector3 GetSpikePos(){
        
        return spikeCollision.transform.localPosition;
    }



    // <-----------------------------サーブ--------------------------------->

    /// <summary>
    /// サーブトスの処理
    /// </summary>
    private void ServeToss(){
        if(!playManager.GetWillServe())return;
        if(playManager.GetWillServePlayer(isPlayer) != this ) return;
        if(playManager.GetServeRight() != isPlayer) return;

        
        if(!isServeToss && Input.GetMouseButtonDown(0)){
            //トスあげる
            StartCoroutine(ServeTossAfterDelay());
            animation_isServeSetUp = true;
            animation_isServeSet = false;
        }
    }
    
    private IEnumerator ServeTossAfterDelay(){
        yield return new WaitForSeconds(serveTossDelay);
        playManager.ServeToss(5f,serveCollision.transform.position);
        isServeToss = true;
    }

    /// <summary>
    /// サーブを打つ時のタイミングの処理
    /// </summary>
    private void ServeTiming(){
        

        if(!isMyPlayer) return;
        if(isServeToss){
            if(!isServeTap){
                serveTimingPoint = 3;
                if(Input.GetMouseButtonDown(0)){
                    var ballPos = ball.transform.position;
                    var servePos = serveCollision.transform.position;
                    var dist = Vector3.Distance(ballPos,servePos);
                    if(dist < 2f){
                        Debug.Log("ナイスタイミング");
                        serveTimingPoint = 0;
                    }else if(dist < 3f){
                        Debug.Log("速すぎ");
                        serveTimingPoint = 1;
                    }else{
                        Debug.Log("速すぎんご");
                        serveTimingPoint = 2;
                    }
                    isServeTap = true;

                    var mx = Input.mousePosition.x-Screen.width/2;
                    serveAngle = mx/(Screen.width/2) * 90f;
                }
            }
            
        }else{
            isServeTap = false;
        }
    }

    /// <summary>
    /// サーブのミート処理
    /// </summary>
    private void Serve(){
        isServe = false;
        animation_isServe = false;
        //サーブ打つ
        if(isServeToss){
            if(ballRig.velocity.y < 0 && serveCollision.GetIsExistBall()){
                var dist = Vector3.Distance(serveCollision.transform.position,ball.transform.position);
                if(dist <= 1){ 
                    isServe = true;
                    StartCoroutine(ServeAfterDelay());
                    isServeToss = false;
                    
                }
            }
            var serveHei = serveCollision.transform.position.y;
            Debug.Log("サーブまでの時間" + playManager.GetBallReachTime(serveHei,false));
            if(playManager.GetBallReachTime(serveHei,false) != 0 && playManager.GetBallReachTime(serveHei,false) < 0.25f){
                animation_isServeSetUp = false;
                animation_isServe = true;
            }
        }
    }

    private IEnumerator ServeAfterDelay(){
        // ボールを停止させる
        if(!ballRig.isKinematic){
            ballRig.velocity = Vector3.zero;
            ballRig.isKinematic = true;
        }

        // 0.2秒待つ
        yield return new WaitForSeconds(serveDelay);

        // ボールの停止を解除
        ballRig.isKinematic = false;
        var netHeight = 3f;
        if(isPlayer){
            if(isMyPlayer){
                switch (serveTimingPoint)
                {
                    case 0:
                        netHeight = 3f;
                        break;
                    case 1:
                        netHeight = 4f;
                        break;
                    case 2:
                        netHeight = 5f;
                        break;
                    case 3:
                        netHeight = 6f;
                        break;
                    default:
                        break;
                }
            }else{
                serveAngle = Random.Range(-90,90);
            }
            playManager.Serve(netHeight,GetAtackAimPoint(serveAngle));
        }else{
            serveAngle = Random.Range(-90,90);
            playManager.Serve(netHeight,GetAtackAimPoint(serveAngle));
        }
        playManager.SetWillServe(false);
        playManager.SetLastTouchPlayer(this);
    }

    public bool GetIsServeReady(){
        return isServeReady;
    }

    public Vector3 GetServeBallPos(){
        return serveBallPos.transform.position;
    }

    public void SetAnimation_IsServeSet(bool animation_isServeSet){
        this.animation_isServeSet = animation_isServeSet;
    }
    

    //<-----------------レシーブ----------------->

    /// <summary>
    /// 
    /// </summary>
    private void ReceiveTiming(){
        if(playManager.GetWillServe()) isReceiveTap = false;
        if(playManager.GetWillServe()||playManager.GetBallIn() != isPlayer || !isGround) return;
        if(playManager.GetServeRight() == isPlayer && !playManager.GetIsFirstReceive()) return;
        if(playManager.GetPrevTouchPlayer(isPlayer) == this) return;
        if(playManager.GetBallTouchNum(isPlayer) == 0 || playManager.GetBallTouchNum(isPlayer) == 1){
            if(isMyPlayer){
                if(!isReceiveTap && Input.GetMouseButtonDown(0)){
                    var ballPos = ball.transform.position;
                    var receivePos = transform.position;
                    var dist = Vector3.Distance(ballPos,receivePos);
                    if(dist < 2f){
                        Debug.Log("ナイスタイミング");
                        receiveTimingPoint = 0;
                    }else if(dist < 3f){
                        Debug.Log("速すぎ");
                        receiveTimingPoint = 1;
                    }else{
                        Debug.Log("速すぎんご");
                        receiveTimingPoint = 2;
                    }
                    isReceiveTap = true;
                }
            }
        }else{
            isReceiveTap = false;
        }
        
    }

    private void Flying(){
        if(playManager.GetBallIn() != isPlayer) return;
        if(playManager.GetWillServe()) return;
        if(!playManager.GetIsFirstReceive()) return;
        if(isReceiveCorutine) return;
        if(ballDist2D < 1f) return;
        if(playManager.GetBallReachTime(0,false) == 0) return;
        if(playManager.GetBallReachTime(0,false) > 0.5f) return;
        if(!playManager.GetBallFallSide(isPlayer)) return;
        
        
        if(playManager.GetBallTouchNum(isPlayer) == 0){
            var fallPoint = playManager.GetBallPoint(ball.transform.localScale.x/2);
            var dir = fallPoint - transform.position;
            dir.y = 0;

            if(dir.magnitude < 1f) return;
            var moveTime = dir.magnitude/moveSpeed;
            var fallTime = playManager.GetBallReachTime(ball.transform.localScale.x/2,false);
            if(fallTime < moveTime && fallTime < flyingNeedTime){
                isFlying = true;
            }


        }else{
            isFlying = false;
            flyingTime = 0f;
        }

        if(isFlying){
            if(flyingTime < flyingNeedTime){
                flyingTime += Time.deltaTime;
                var flyingTimeRatio = 0.5f;
                var flyingFreezeStartRatio = 0.7f;
                flyingSpeedRatio = 1.5f;
                var maxSpeed = moveNormalSpeed*flyingSpeedRatio;
                if(flyingTime < flyingNeedTime*flyingTimeRatio){
                    //フライングで速い移動時間
                    animation_isFlying = true;
                    moveSpeed = maxSpeed;
                }else if(flyingTime < flyingNeedTime *flyingFreezeStartRatio){
                    //だんだん減速
                    animation_isFlying = false;
                    var time = flyingTime - flyingNeedTime*flyingTimeRatio;
                    var maxTime = flyingNeedTime * (flyingFreezeStartRatio-flyingTimeRatio);
                    moveSpeed = maxSpeed - maxSpeed * time/maxTime;
                }else{
                    //停止時間
                    isFlying = false;
                    moveSpeed = 0f;
                }
            }else{
                
                moveSpeed = moveNormalSpeed;
            }
        }else{
            moveSpeed = moveNormalSpeed;
        }
    }


    /// <summary>
    /// レシーブ
    /// </summary>
    private void Receive(){
        if(playManager.GetWillServe()) return;
        if(!isGround) return;
        if(playManager.GetServeRight() == isPlayer && !playManager.GetIsFirstReceive()) return;
        var isReceive = false;
        var isOver = false;
        var isUnder = false;

        if(overCollision.GetIsExistBall()){
            isReceive = true;
            isOver = true;
        }else if(underCollision.GetIsExistBall()){
            isReceive = true;
            isUnder = true;
        }

        if(playManager.GetBallIn() != isPlayer || !isGround){
            isReceive = false;
        }

        if(isReceive && isMyPlayer){
            if(isReceiveTap){
                isReceive = true;
            }else{
                isReceive = false;
            }
        }

        if(isReceive){
            Vector3 aimPos = Vector3.zero;
            if(playManager.GetPrevTouchPlayer(isPlayer) != this){
                switch (playManager.GetBallTouchNum(isPlayer))
                {
                    case 0:
                        if(isPlayer){
                            aimPos = new Vector3(0,2,2);
                        }else{
                            aimPos = new Vector3(0,2,-2);
                        }
                        playManager.SetWillSpikePlayer(isPlayer, this);
                        break;
                    case 1:
                        //トスを上げる
                        aimPos = playManager.GetPositions(isPlayer,2,!playManager.GetBallFallSide(isPlayer));
                        break;
                    case 2:
                    case 3:
                        isReceive = false;
                        break;
                    default:
                        break;
                }
                if(isReceive){
                    StartCoroutine(ReceiveAfterDelay(aimPos));
                    isReceiveFreeze = true;
                    StartCoroutine(ReceiveFreeze());
                    playManager.SetPrevTouchPlayer(isPlayer, this);
                    playManager.SetFirstReceive(true);
                    playManager.SetLastTouchPlayer(this);

                    if(isOver){
                        animation_isOverReceive = true;
                        animation_isUnderReceive = false;
                    }else if(isUnder){
                        animation_isOverReceive = false;
                        animation_isUnderReceive = true;
                    }


                    Debug.Log("レシーブするよ");
                    isReceiveTap = false;
                }
            }
        }
    }

    private IEnumerator ReceiveFreeze(){
        yield return new WaitForSeconds(receiveFreezeTime);
        isReceiveFreeze = false;
    }

    private IEnumerator ReceiveAfterDelay(Vector3 aimPos){
        if(!ballRig.isKinematic){
            ballRig.velocity = Vector3.zero;
            ballRig.isKinematic = true;
        }
        isReceiveCorutine = true;
        yield return new WaitForSeconds(receiveDelay);
        isReceiveCorutine = false;
        ballRig.isKinematic = false;
        playManager.ReceiveForce(receiveHeight, aimPos);
        
        playManager.AddBallTouchNum(isPlayer);

        animation_isUnderReceive = false;
        animation_isOverReceive = false;
    }

    
    //　<-----------------------------------動く------------------------------------>
    /// <summary>
    /// 動きの処理
    /// </summary>
    private void Move(){
        if(isReceiveFreeze){
            animation_isSet = true;
            return;
        }
        if(!isGround) return;
        animation_isBlockSet = false;
        animation_isSet = false;
        animation_isSet_Over = false;

        Vector3[] forwardRot = new Vector3[2];
        forwardRot[0] = new Vector3(0,180,0);
        forwardRot[1] = new Vector3(0,0,0);

        if(playManager.GetWillServe()){
            //サーブ中の処理
            var moveToPos = transform.position;
            moveSpeed = moveNormalSpeed;
            if(playManager.GetServeRight() == isPlayer){
                //サーブ権を持つチーム
                if(isFront){
                    //サーブではない
                    moveToPos = playManager.GetPositions(isPlayer,4,true);
                    animation_isSet = true;
                }else{
                    //サーブ打つ人
                    if(isPlayer){
                        moveToPos = new Vector3(0,0,10);
                    }else{
                        moveToPos = new Vector3(0,0,-10);
                    }
                }

            }else{
                //サーブ権を持たないチーム
                animation_isSet = true;
                if(isFront){
                    moveToPos = playManager.GetPositions(isPlayer,4,true);
                }else{
                    moveToPos = playManager.GetPositions(isPlayer,4,false);
                }
            }

            if(isPlayer){
                //自分のチーム
                transform.rotation = Quaternion.Euler(forwardRot[0]);
            }else{
                //相手のチーム
                transform.rotation = Quaternion.Euler(forwardRot[1]);
            }
            isServeReady = true;
            moveToPos.y = normalHei;
            transform.position = moveToPos;
        }else{
            //サーブ以外の処理
            var pos = transform.position;
            var moveToPos = transform.position; //移動先

            if(!playManager.GetIsFirstReceive()){
                //サーブレシーブ
                if(isPlayer == playManager.GetServeRight()){
                    //サーブ権ある側
                    if(isFront){
                        //前衛
                        animation_isBlockSet = true;
                        if(isPlayer){
                            moveToPos = new Vector3(0,0,1);
                        }else{
                            moveToPos = new Vector3(0,0,-1);
                        }
                        
                    }else{
                        //後衛
                        animation_isSet = false;
                        if(isPlayer){
                            moveToPos = new Vector3(0,0,6);
                        }else{
                            moveToPos = new Vector3(0,0,-6);
                        }
                    }
                }else{
                    //サーブ権がない側
                    var clothestPlayer = playManager.GetClothestPlayer(isPlayer);
                    if(clothestPlayer != null){
                        if(clothestPlayer == this){
                            //レシーブをこれからする人
                            animation_isSet = true;
                            moveToPos = playManager.GetBallPoint(ballRig.transform.localScale.x/2);
                        }else{
                            //レシーブをしない人
                            animation_isSet = false;
                            var pos1 = playManager.GetPositions(isPlayer,1,true);
                            var pos2 = playManager.GetPositions(isPlayer,1,false);
                            if(Vector3.Distance(pos,pos1) < Vector3.Distance(pos,pos2)){
                                moveToPos = pos1;
                            }else{
                                moveToPos = pos2;
                            }
                        }
                        
                    }
                }

                if(isPlayer){
                    //自分のチーム
                    transform.rotation = Quaternion.Euler(forwardRot[0]);
                }else{
                    //相手のチーム
                    transform.rotation = Quaternion.Euler(forwardRot[1]);
                }
            }else{
                //サーブレシーブ後の動き
                if(isPlayer == playManager.GetBallIn()){
                    //ボールが自分のコートにある場合
                    switch (playManager.GetBallTouchNum(isPlayer))
                    {
                        case 0:
                            if(!isFront){
                                //後衛　これからレシーブする
                                animation_isSet = true;
                                moveToPos = playManager.GetBallPoint(0.2f);
                                // Rotation(moveToPos-pos,playManager.GetBallPoint(0));
                            }else{
                                //前衛　この後トスする
                                animation_isSet = false;
                                if(isPlayer){
                                    Rotation(moveToPos - pos, new Vector3(pos.x,pos.y,-100));
                                }else{
                                    Rotation(moveToPos - pos, new Vector3(pos.x,pos.y,100));
                                }
                                
                            }

                            break;
                        case 1:
                            if(playManager.GetWillSpikePlayer(isPlayer) != this){
                                //これからトスする人
                                animation_isSet = true;
                                animation_isSet_Over = true;
                                moveToPos = playManager.GetBallPoint(tall);
                                Rotation(moveToPos-pos,new Vector3(playManager.GetWillSpikePlayer(isPlayer).transform.position.x,pos.y,pos.z));

                                
                                // Rotation(moveToPos-pos,new Vector3(0,pos.y,pos.z));
                            }else{
                                //これからスパイクする人
                                animation_isSet = false;
                                moveToPos = playManager.GetPositions(isPlayer,0,!playManager.GetBallFallSide(isPlayer));
                                if(isPlayer){
                                    Rotation(moveToPos - pos, new Vector3(pos.x,pos.y,-100));
                                }else{
                                    Rotation(moveToPos - pos, new Vector3(pos.x,pos.y,100));
                                }
                            }

                            
                            break;
                        case 2:
                            if(playManager.GetWillSpikePlayer(isPlayer) == this){
                                if(playManager.GetBallReachTime(spikeHeight,false) != 0 && playManager.GetBallReachTime(spikeHeight,false) < spikeStepStart){
                                    //これからスパイクする人
                                    if(!animation_isSpikeStep){
                                        animation_isSpikeStep = true;
                                        if(isPlayer){
                                            animator.Play("metarig|SpikeStep");
                                        }
                                        
                                    }
                                    
                                    
                                    if(isPlayer){
                                        moveToPos = playManager.GetBallPoint(maxJumpHeight + normalHei) + GetSpikePos();
                                        Rotation(moveToPos - pos, new Vector3(pos.x,pos.y,-100));
                                    }else{
                                        moveToPos = playManager.GetBallPoint(maxJumpHeight + normalHei) - GetSpikePos();
                                        Rotation(moveToPos - pos, new Vector3(pos.x,pos.y,100));
                                        
                                    }
                                }
                            }else{
                                //これからボロックフォロー
                                animation_isSet = true;
                                
                                Rotation(moveToPos - pos, new Vector3(0,pos.y,pos.z));
                            }
                            break;
                        case 3:
                            break;
                        default:
                            break;
                    }
                }else{
                    //ボールが相手コートにある場合
                    switch (playManager.GetBallTouchNum(!isPlayer))
                    {
                        case 0:
                            if(isFront){
                                //前衛　ブロック
                                animation_isBlockSet = true;
                                if(isPlayer){
                                    moveToPos = new Vector3(0,0,1);
                                }else{
                                    moveToPos = new Vector3(0,0,-1);
                                }
                            }else{
                                //後衛　レシーブレシーブ
                                animation_isSet = true;
                                if(isPlayer){
                                    moveToPos = new Vector3(0,0,6);
                                }else{
                                    moveToPos = new Vector3(0,0,-6);
                                }
                            }
                            break;
                        case 1:
                            var spikerPosX = playManager.GetWillSpikePlayer(!isPlayer).transform.position.x;

                            if(isFront){
                                //前衛　これからブロック
                                animation_isBlockSet = true;
                                if(isPlayer){
                                    moveToPos = new Vector3(spikerPosX,0,1);
                                    if(isMyPlayer){
                                        //自分が制御するキャラクターの場合
                                        if(Input.GetMouseButton(0)){
                                            if(Input.mousePosition.x > Screen.width/7*5){
                                                //右に移動
                                                animation_isBlockSetWalk_r = true;
                                                moveToPos.x = -10;
                                            }else if(Input.mousePosition.x < Screen.width/7*2){
                                                //左に移動
                                                animation_isBlockSetWalk_l = true;
                                                moveToPos.x = 10;
                                            }else{
                                                animation_isBlockSetWalk_l = false;
                                                animation_isBlockSetWalk_r = false;
                                                moveToPos.x = pos.x;
                                            }
                                        }else{
                                            animation_isBlockSetWalk_l = false;
                                            animation_isBlockSetWalk_r = false;
                                            moveToPos.x = pos.x;
                                        }
                                    }
                                }else{
                                    moveToPos = new Vector3(spikerPosX,0,-1);
                                }
                            }else{
                                //後衛　これからレシーブ
                                animation_isSet = true;
                                moveToPos = playManager.GetPositions(isPlayer,3,isLeft);
                                if(isMyPlayer){
                                    //自分の場合
                                    if(Input.GetMouseButton(0)){
                                        if(Input.mousePosition.x > Screen.width/7*5){
                                            //右に移動
                                            animation_isSetWalk_r = true;
                                            moveToPos.x = -10;
                                        }else if(Input.mousePosition.x < Screen.width/7*2){
                                            animation_isSetWalk_l = true;
                                            moveToPos.x = 10;
                                        }else{
                                            animation_isSetWalk_l = false;
                                            animation_isSetWalk_r = false;
                                            moveToPos.x = pos.x;
                                        }
                                    }else{
                                        animation_isSetWalk_l = false;
                                        animation_isSetWalk_r = false;
                                        moveToPos.x = pos.x;
                                    }
                                }
                            }
                            break;
                        case 2:
                            if(isFront){
                                //前衛　これからブロック
                                spikerPosX = playManager.GetWillSpikePlayer(!isPlayer).transform.position.x;
                                animation_isBlockSet = true;
                                if(isPlayer){
                                    moveToPos = new Vector3(spikerPosX,0,1);
                                    if(isMyPlayer){
                                        if(Input.GetMouseButton(0)){
                                            if(Input.mousePosition.x > Screen.width/7*5){
                                                //右に移動
                                                animation_isBlockSetWalk_r = true;
                                                moveToPos.x = -10;
                                            }else if(Input.mousePosition.x < Screen.width/7*2){
                                                //左に移動
                                                animation_isBlockSetWalk_l = true;
                                                moveToPos.x = 10;
                                            }else{
                                                animation_isBlockSetWalk_l = false;
                                                animation_isBlockSetWalk_r = false;
                                                moveToPos.x = pos.x;
                                            }
                                        }else{
                                            animation_isBlockSetWalk_l = false;
                                            animation_isBlockSetWalk_r = false;
                                            moveToPos.x = pos.x;
                                        }
                                    }
                                }else{
                                    moveToPos = new Vector3(spikerPosX,0,-1);
                                }
                            }else{
                                //後衛　これからレシーブ
                                moveToPos = playManager.GetPositions(isPlayer,3,isLeft);
                                animation_isSet = true;
                                Debug.Log(isPlayer + ":" + name + "が移動するのは" + isLeft +"の" + moveToPos);
                                if(isMyPlayer){
                                    if(Input.GetMouseButton(0)){
                                        if(Input.mousePosition.x > Screen.width/7*5){
                                            //右に移動
                                            animation_isSetWalk_r = true;
                                            moveToPos.x = -10;
                                        }else if(Input.mousePosition.x < Screen.width/7*2){
                                            //左に移動
                                            animation_isSetWalk_l = true;
                                            moveToPos.x = 10;
                                        }else{
                                            animation_isSetWalk_l = false;
                                            animation_isSetWalk_r = false;
                                            moveToPos.x = pos.x;
                                        }
                                    }else{
                                        animation_isSetWalk_l = false;
                                        animation_isSetWalk_r = false;
                                        moveToPos.x = pos.x;
                                    }
                                }
                            }
                            break;
                        case 3:
                            if(isFront){
                                //前衛　基本はこの場所に
                                animation_isBlockSet = true;
                                spikerPosX = playManager.GetWillSpikePlayer(!isPlayer).transform.position.x;
                                if(isPlayer){
                                    moveToPos = new Vector3(spikerPosX,0,1);
                                }else{
                                    moveToPos = new Vector3(spikerPosX,0,-1);
                                }
                            }else{
                                //後衛　基本はこの場所に
                                animation_isSet = true;
                                moveToPos = playManager.GetPositions(isPlayer,3,playManager.GetBallFallSide(!isPlayer));
                            }
                            break;
                        default:
                            break;
                    }
                    if(isPlayer){
                        Rotation(moveToPos - pos, new Vector3(pos.x,pos.y,-100));
                    }else{
                        Rotation(moveToPos - pos, new Vector3(pos.x,pos.y,100));
                    }
                }
            }

            var dir = moveToPos - pos;
            dir.y = 0;
            var dist = dir.magnitude;
            var dir2D = dir.normalized;
            if(dist >= 0.1f){
                //移動アリ
                pos += dir2D * moveSpeed * Time.deltaTime;
                transform.position = pos;
                
                if(animation_isSet){
                    if(isPlayer){
                        if(dir2D.x < 0){
                            animation_isBlockSetWalk_r = true;
                            animation_isSetWalk_r = true;
                            animation_isBlockSetWalk_l = false;
                            animation_isSetWalk_l = false;
                        }else{
                            animation_isBlockSetWalk_l = true;
                            animation_isSetWalk_l = true;
                            animation_isBlockSetWalk_r = false;
                            animation_isSetWalk_r = false;
                        }
                    }else{
                        if(dir2D.x < 0){
                            animation_isBlockSetWalk_l = true;
                            animation_isSetWalk_l = true;
                            animation_isBlockSetWalk_r = false;
                            animation_isSetWalk_r = false;
                        }else{
                            animation_isBlockSetWalk_r = true;
                            animation_isSetWalk_r = true;
                            animation_isBlockSetWalk_l = false;
                            animation_isSetWalk_l = false;
                        }
                        
                    }
                }else{
                    animation_isWalk = true;
                }
                
            }else{
                animation_isWalk = false;
                animation_isBlockSetWalk_r = false;
                animation_isSetWalk_r = false;
                animation_isBlockSetWalk_l = false;
                animation_isSetWalk_l = false;
            }
        }

    }

    private void Rotation(Vector3 moveDir, Vector3 lookTo){
        var rotationSpeed = 5f;
        var lookRot = lookTo -transform.position;
        lookRot.y = 0;

        if (moveDir.magnitude > 0.01f)
        {
            Quaternion toRotation = Quaternion.LookRotation(lookRot, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
    }


    
    

    public bool GetIsPlayer(){
        return isPlayer;
    }

    public void SetIsFront(bool isFront){
        this.isFront = isFront;
    }

}
