using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayManager : MonoBehaviour
{
    private Rigidbody ballRig;
    [SerializeField]private GameObject ballObj;
    
    private float g = Physics.gravity.y;

    private int[] ballTouchNum = new int[2];

    private Player[] prevTouchPlayer = new Player[2];
    private Player[,] players = new Player[2,2];
    private Player[] willSpikePlayer = new Player[2];
    private Player lastTouchPlayer;
    [Header("一番ボールに近いプレイヤー")] private Player[] clothestPlayer = new Player[2];
    [Header("ポジション[味方か,ポジション番号,LorR]")]private Vector3[,,] positions;

    [Header("ボールが落ちるサイド L:True R:false")]private bool[] ballFallSide = new bool[2];

    [Header("ボールがコート")]private bool ballIn;

    [Header("これからサーブを打つ")]private bool willServe;
    [Header("サーブ権")]private bool serveRight;
    [Header("サーブの準備が整ったか")]private bool isServeSet;
    [Header("これからサーブを打つ人")]private Player[] willServePlayer = new Player[2];
    [Header("サーブ後最初のレシーブをしたか")]private bool isFirstReceive;
    [Header("ボールが落ちたか")]private bool isFallBall;
    [Header("リセット中か")]private bool isReset;
    [Header("最初のリセットのフラグ")]private bool isFirstReset;
    [Header("リセットの時間")]private float resetMaxtTime = 1f;
    [Header("リセットしている時間")]private float resetTime;
    [Header("得点")]private int[] points = new int[2];

    [Header("ポイントテキスト")]private Text[] pointTexts = new Text[2]; 
    


    // Start is called before the first frame update
    void Start()
    {
        ballRig = ballObj.GetComponent<Rigidbody>();
        
        players[0,0] = GameObject.Find("Players/Team/Player").GetComponent<Player>();
        players[0,1] = GameObject.Find("Players/Team/Player (1)").GetComponent<Player>();
        players[1,0] = GameObject.Find("Players/Enemy/Player").GetComponent<Player>();
        players[1,1] = GameObject.Find("Players/Enemy/Player (1)").GetComponent<Player>();

        var positionObj = GameObject.Find("Positions");
        var playerObj = positionObj.transform.Find("Player");
        positions = new Vector3[2,playerObj.childCount,2];
        for(int i=0; i<2; i++){
            var rootObj = positionObj.transform.GetChild(i);
            for(int j=0; j<playerObj.childCount; j++){
                var parentObj = rootObj.transform.GetChild(j);
                for(int k=0; k<2; k++){
                    positions[i,j,k] = parentObj.transform.GetChild(k).position;
                }
            }
        }

        willServePlayer[0] = players[0,0];
        willServePlayer[1] = players[1,0];

        pointTexts[0] = GameObject.Find("Canvas/PointTexts/Player").GetComponent<Text>();
        pointTexts[1] = GameObject.Find("Canvas/PointTexts/Enemy").GetComponent<Text>();
        pointTexts[0].text = pointTexts[1].text = "0";

        serveRight = true;
        willServe = true;
    }

    // Update is called once per frame
    void Update()
    {
        SetBallFallSide();

        if(ballObj.transform.position.z > 0){
            if(!ballIn){
                ballTouchNum[1] = 0;
                prevTouchPlayer[1] = null;
            }
            ballIn = true;
        }else{
            if(ballIn){
                ballTouchNum[0] = 0;
                prevTouchPlayer[0] = null;
            }
            ballIn = false;
        }

        

        ServeProcess();
        ResetProcess();
    }
    

    private void ResetProcess(){
        if(isFallBall && !isFirstReset){
            isFirstReset = true;
            isReset = true;
            
            var isBreak = false;
            if(lastTouchPlayer.GetIsPlayer()){
                var ballX = ballObj.transform.position.x;
                var ballZ = ballObj.transform.position.z;
                if(ballX < 4.5f && ballX > -4.5f && ballZ < 0 && ballZ > -9f){
                    if(serveRight){
                        isBreak = true;
                    }
                    serveRight = true;
                    points[0]++;
                }else{
                    if(!serveRight){
                        isBreak = true;
                    }
                    serveRight = false;
                    points[1]++;
                }
            }else{
                var ballX = ballObj.transform.position.x;
                var ballZ = ballObj.transform.position.z;
                if(ballX < 4.5f && ballX > -4.5f && ballZ < 9 && ballZ > 0){
                    if(!serveRight){
                        isBreak = true;
                    }
                    serveRight = false;
                    points[1]++;
                }else{
                    if(serveRight){
                        isBreak = true;
                    }
                    serveRight = true;
                    points[0]++;
                }
            }

            if(!isBreak){
                Debug.Log("ブレイクしていない");
                if(serveRight){
                    Debug.Log(willServePlayer[0].name + "が前のサーバー");
                    if(willServePlayer[0] == players[0,0]){
                        Debug.Log("上の処理");
                        willServePlayer[0] = players[0,1];
                        willServePlayer[0].SetIsFront(false);
                        players[0,0].SetIsFront(true);
                    }else{
                        Debug.Log("下の処理");
                        willServePlayer[0] = players[0,0];
                        willServePlayer[0].SetIsFront(false);
                        players[0,1].SetIsFront(true);
                    }
                    Debug.Log(willServePlayer[0].name + "が次のサーバー");
                    
                }else{
                    if(willServePlayer[1] == players[1,0]){
                        willServePlayer[1] = players[1,1];
                        willServePlayer[1].SetIsFront(false);
                        players[1,0].SetIsFront(true);
                    }else{
                        willServePlayer[1] = players[1,0];
                        willServePlayer[1].SetIsFront(false);
                        players[1,1].SetIsFront(true);
                    }
                }
            }else{
                Debug.Log("ブレイク");
            }
            resetTime = 0f;

            ballTouchNum[0] = ballTouchNum[1] = 0;
            prevTouchPlayer[0] = prevTouchPlayer[1] = null;
            isFirstReceive = false;
        }

        if(isReset){
            for(int i=0; i<2; i++){
                for(int j=0; j<2; j++){
                    players[i,j].ResetPlayer();
                }
            }
            resetTime += Time.deltaTime;
            if(resetTime > resetMaxtTime){
                isReset = false;
                willServe = true;
                pointTexts[0].text = points[0].ToString();
                pointTexts[1].text = points[1].ToString();
                Debug.Log("おわり");
                isFirstReset = false;
                isFallBall = false;
            }
        }
    }

    public Player GetClothestPlayer(bool isPlayer){
        var i=0;
        if(isPlayer){
            i=0;
        }else{
            i=1;
        }

        Player clothPlayer = null;
        var minDist = 100f;
        var ballY = ballObj.transform.localScale.x/2;
        var point = GetBallPoint(ballY);
        for(int j=0; j<2; j++){
            var pos = players[i,j].transform.position;
            pos.y = 0;
            var dist = Vector3.Distance(point,pos);
            if(minDist > dist){
                clothPlayer = players[i,j];
                minDist = dist;
            }
        }
        clothestPlayer[i] = clothPlayer;
            
        
        return clothPlayer;
    }




    /// <summary>
    /// リセット中か取得
    /// </summary>
    /// <returns></returns>
    public bool GetIsReset(){
        return isReset;
    }

    

    /// <summary>
    /// 最期に触ったプレイヤーを設定
    /// </summary>
    /// <param name="player"></param>
    public void SetLastTouchPlayer(Player player){
        this.lastTouchPlayer = player;
    }

    /// <summary>
    /// ボールが落ちたか設定
    /// </summary>
    /// <param name="isFallBall"></param>
    public void SetIsFallBall(bool isFallBall){
        this.isFallBall = isFallBall;
    }

    public bool GetIsFallBall(){
        return isFallBall;
    }

    /// <summary>
    /// 最初のレシーブをしたか設定
    /// </summary>
    /// <param name="isFirstReceive"></param>
    public void SetFirstReceive(bool isFirstReceive){
        this.isFirstReceive = isFirstReceive;
    }

    /// <summary>
    /// 最初のレシーブをしたか取得
    /// </summary>
    /// <returns></returns>
    public bool GetIsFirstReceive(){
        return isFirstReceive;
    }

    /// <summary>
    /// サーブの処理
    /// </summary>
    private void ServeProcess(){
        if(!willServe) return;
        for(int i=0; i<2; i++){
            for(int j=0; j<2; j++){
                if(!players[i,j].GetIsServeReady()){
                    return;
                }
            }
        }
        if(isServeSet) return;

        Player server;
        if(serveRight){
            server = willServePlayer[0];
        }else{
            server = willServePlayer[1];
        }
        
        ballRig.isKinematic = true;
        ballRig.transform.position = server.GetServeBallPos();
        server.SetAnimation_IsServeSet(true);
        isServeSet = true;
    }

    public void ServeToss(float height,Vector3 aimPos){
        ballRig.isKinematic = false;
        ReceiveForce(height,aimPos);
    }


    /// <summary>
    /// サーバーをチェンジする
    /// </summary>
    /// <param name="isPlayer"></param>
    public void ChangeWillServer(bool isPlayer){
        if(isPlayer){
            if(willServePlayer[0] == players[0,0]){
                willServePlayer[0] = players[0,1];
            }else{
                willServePlayer[0] = players[0,0];
            }
        }else{
            if(willServePlayer[1] == players[1,0]){
                willServePlayer[1] = players[1,1];
            }else{
                willServePlayer[1] = players[1,0];
            }
        }
    }

    public Player GetWillServePlayer(bool isPlayer){
        if(isPlayer){
            return willServePlayer[0];
        }else{
            return willServePlayer[1];
        }
    }

    /// <summary>
    /// サーブ権を取得
    /// </summary>
    /// <returns></returns>
    public bool GetServeRight(){
        return serveRight;
    }

    /// <summary>
    /// これからサーブを打つか設定
    /// </summary>
    /// <param name="willServe"></param>
    public void SetWillServe(bool willServe){
        this.willServe = willServe;
        if(!willServe){
            isServeSet = false;
        }
    }

    public bool GetWillServe(){
        return willServe;
    }



    /// <summary>
    /// スパイクを打つ人を設定
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <param name="player"></param>
    public void SetWillSpikePlayer(bool isPlayer, Player player){
        if(isPlayer){
            willSpikePlayer[0] = player;
        }else{
            willSpikePlayer[1] = player;
        }
    }

    /// <summary>
    /// スパイクを打つ人を取得
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <returns></returns>
    public Player GetWillSpikePlayer(bool isPlayer){
        if(isPlayer){
            return willSpikePlayer[0];
        }else{
            return willSpikePlayer[1];
        }
    }

    public bool GetBallIn(){
        return ballIn;
    }

    /// <summary>
    /// ボールが落ちるサイドを設定
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <param name="LorR"></param>
    public void SetBallFallSide(){
        if(ballIn){
            if(GetBallPoint(1f).x > 0){
                ballFallSide[0] = true;
            }else{
                ballFallSide[0] = false;
            }
        }else{
            if(GetBallPoint(1f).x < 0){
                ballFallSide[1] = true;
            }else{
                ballFallSide[1] = false;
            }
        }
    }

    /// <summary>
    /// ぼーうが落ちるサイドを取得
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <returns></returns>
    public bool GetBallFallSide(bool isPlayer){
        if(isPlayer){
            return ballFallSide[0];
        }else{
            return ballFallSide[1];
        }
    }

    /// <summary>
    /// ポジションを取得
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <param name="posNum">0:SpikeReady 1:TossReady 2:SpikePos 3: ReceivePos 4:Reception 5:Serve</param>
    /// <param name="LorR"></param>
    /// <returns></returns>
    public Vector3 GetPositions(bool isPlayer,int posNum, bool LorR){
        if(isPlayer){
            if(LorR){
                return positions[0,posNum,0];
            }else{
                return positions[0,posNum,1];
            }
            
        }else{
            if(LorR){
                return positions[1,posNum,0];
            }else{
                return positions[1,posNum,1];
            }
        }
    }

    public Vector3 GetSpikePositions(bool isPlayer, bool LorR ,Player willSpikePlayer){
        if(isPlayer){
            if(LorR){
                return positions[0,2,0] - willSpikePlayer.GetSpikePos();
            }else{
                return positions[0,2,1] - willSpikePlayer.GetSpikePos();
            }
            
        }else{
            if(LorR){
                return positions[1,2,0] + willSpikePlayer.GetSpikePos();
            }else{
                return positions[1,2,1] + willSpikePlayer.GetSpikePos();
            }
        }
    }

    /// <summary>
    /// 指定されたY座標でのボールの位置を取得する関数
    /// </summary>
    /// <param name="ballY"></param>
    /// <returns></returns>
    public Vector3 GetBallPoint(float ballY)
    {
        // 現在のボールの位置と速度を取得
        Vector3 currentPosition = ballRig.position;
        Vector3 currentVelocity = ballRig.velocity;

        // 現在の位置よりも目標地点が高い場合、ボールが下降中なら到達できない
        if (currentPosition.y < ballY && currentVelocity.y <= 0)
        {
            //Debug.LogWarning("Ball will not reach the specified height.");
            return currentPosition; // 到達できない場合、現在位置を返す
        }

        // 自由落下の運動方程式から判別式を計算
        float discriminant = currentVelocity.y * currentVelocity.y - 2 * g * (currentPosition.y - ballY);

        // 判別式が負の場合、到達できないので現在位置を返す
        if (discriminant < 0)
        {
            //Debug.LogWarning("Ball will not reach the specified Y position.");
            return currentPosition;
        }

        // 時間を計算 (2つの解: 足す解と引く解)
        float time1 = (-currentVelocity.y + Mathf.Sqrt(discriminant)) / g; // 2回目の通過
        float time2 = (-currentVelocity.y - Mathf.Sqrt(discriminant)) / g; // 1回目の通過

        // 時間が正であるかチェック (負の時間は物理的に無効)
        float validTime;
        if (time2 >= 0)
        {
            validTime = time2; // 1回目の通過が有効
        }
        else if (time1 >= 0)
        {
            validTime = time1; // 1回目が無効なら2回目の通過を選択
        }
        else
        {
            Debug.LogWarning("Both times are negative, ball cannot reach the specified position.");
            return currentPosition;
        }

        // その時間後の位置を計算 (空気抵抗や他の力がない場合)
        Vector3 futurePosition = currentPosition + currentVelocity * validTime + 0.5f * new Vector3(0, g, 0) * validTime * validTime;
        return futurePosition;
    }

    /// <summary>
    /// ある高さまでに到達する時間
    /// </summary>
    /// <param name="ballY"></param>
    /// <param name="vy"></param>
    /// <param name="isFirst"></param>
    /// <returns></returns>
    public float GetBallReachTime(float ballY,bool isFirst){
        // 現在のボールの位置と速度を取得
        Vector3 currentPosition = ballRig.transform.position;
        var  vy = ballRig.velocity.y;
        // 自由落下の運動方程式から判別式を計算
        float discriminant = vy * vy - 2f * g * (currentPosition.y - ballY);

        // 判別式が負の場合、到達できないので現在位置を返す
        if (discriminant < 0)
        {
            return 0;
        }

        // 時間を計算 (2つの解: 足す解と引く解)
        float time1 = (-vy + Mathf.Sqrt(discriminant)) / g; // 1回目の通過
        float time2 = (-vy - Mathf.Sqrt(discriminant)) / g; // 2回目の通過
        if(isFirst){
            return time1;
        }else{
            return time2;
        }
    }


    /// <summary>
    /// 上向きの速さを指定したときの高さまでに到達する時間
    /// </summary>
    /// <param name="ballY"></param>
    /// <param name="vy"></param>
    /// <param name="isFirst"></param>
    /// <returns></returns>
    public float GetBallReachTime(float ballY, float vy,bool isFirst){
        // 現在のボールの位置と速度を取得
        Vector3 currentPosition = ballRig.position;

        // 現在の位置よりも目標地点が高い場合、ボールが下降中なら到達できない
        if (vy < ballY && vy <= 0)
        {
            //Debug.LogWarning("Ball will not reach the specified height.");
            return 0; // 到達できない場合、現在位置を返す
        }

        // 自由落下の運動方程式から判別式を計算
        float discriminant = vy * vy - 2 * g * (currentPosition.y - ballY);

        // 判別式が負の場合、到達できないので現在位置を返す
        if (discriminant < 0)
        {
            Debug.LogWarning("Ball will not reach the specified Y position.");
            return 0;
        }

        // 時間を計算 (2つの解: 足す解と引く解)
        float time1 = (-vy + Mathf.Sqrt(discriminant)) / g; // 1回目の通過
        float time2 = (-vy - Mathf.Sqrt(discriminant)) / g; // 2回目の通過
        if(isFirst){
            return time1;
        }else{
            return time2;
        }
    }

    /// <summary>
    /// スパイクの力を加える
    /// </summary>
    /// <param name="spikeVelocity"></param>
    /// <param name="aimPoint"></param>
    public void SpikeForce(float spikePower, Vector3 aimPoint)
    {
        var time = 20f/spikePower;
        // 現在の位置
        Vector3 currentPosition = ballObj.transform.position;

        // 水平方向の距離 (xの差)
        float horizontalDistance = Vector3.Distance(new Vector3(aimPoint.x, 0, aimPoint.z), new Vector3(currentPosition.x, 0, currentPosition.z));

        // 高さの差 (yの差)
        float heightDifference = aimPoint.y - currentPosition.y;

        // 水平方向の単位ベクトル
        Vector3 direction = (new Vector3(aimPoint.x, 0, aimPoint.z) - new Vector3(currentPosition.x, 0, currentPosition.z)).normalized;

        // // 時間tを求める (時間の解を取得するため、運動方程式を使う)
        // float velocitySquared = spikeVelocity * spikeVelocity;
        // float underRoot = velocitySquared * velocitySquared + g * (g * horizontalDistance * horizontalDistance + 2 * heightDifference * velocitySquared);

        // // underRoot が負の場合、計算不能になるため確認する
        // if (underRoot < 0)
        // {
        //     Debug.LogError("Cannot reach the target with the given velocity.");
        //     return;
        // }

        // float time = (velocitySquared + Mathf.Sqrt(underRoot)) / (-g * horizontalDistance);

        // x方向の速度成分
        float vx = horizontalDistance / time;

        // y方向の速度成分
        float vy = (heightDifference - 0.5f * g * time * time) / time;

        // 最終的な速度ベクトル
        Vector3 velocity = direction * vx;  // 水平方向の速度成分
        velocity.y = vy;                    // 鉛直方向の速度成分

        ballRig.AddForce((velocity-ballRig.velocity)*ballRig.mass,ForceMode.Impulse);
    }


    /// <summary>
    /// レシーブの力を与える
    /// </summary>
    /// <param name="receiveHeight"></param>
    /// <param name="aimPoint"></param>
    public void ReceiveForce(float receiveHeight ,Vector3 aimPoint){
        var currentPosition = ballRig.transform.position;
        var dir2D = aimPoint - currentPosition;
        dir2D.y = 0;

        var vy = Mathf.Sqrt(-2*g*(receiveHeight-currentPosition.y));
        var reachTime = GetBallReachTime(aimPoint.y,vy,false);
        var v2D = (aimPoint - currentPosition)/reachTime;
        var velocity = v2D;
        velocity.y = vy;
        var reverseForce = -ballRig.velocity * ballRig.mass;
        var force = velocity * ballRig.mass + reverseForce;
        
        ballRig.AddForce(force,ForceMode.Impulse);
    }

    /// <summary>
    /// ボールタッチ数を取得
    /// </summary>
    /// <param name="isPlayer">true：味方false：敵</param>
    /// <returns></returns>
    public int GetBallTouchNum(bool isPlayer){
        if(isPlayer){
            return ballTouchNum[0];
        }else{
            return ballTouchNum[1];
        }
    }

    /// <summary>
    /// ボールタッチを一回増やす
    /// </summary>
    /// <param name="isPlayer"></param>
    public void AddBallTouchNum(bool isPlayer){
        if(isPlayer){
            ballTouchNum[0]++;
        }else{
            ballTouchNum[1]++;
        }
    }

    /// <summary>
    /// ボールタッチをリセット
    /// </summary>
    /// <param name="isPlayer"></param>
    public void ResetBallTouchNum(bool isPlayer){
        if(isPlayer){
            ballTouchNum[0] = 0;
        }else{
            ballTouchNum[1] = 0;
        }
    }

    /// <summary>
    /// ひとつ前に触ったプレイヤ―を設定(チームごと)
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <param name="player"></param>
    public void SetPrevTouchPlayer(bool isPlayer, Player player){
        if(isPlayer){
            prevTouchPlayer[0] = player;
        }else{
            prevTouchPlayer[1] = player;
        }
    }

    /// <summary>
    /// 直前に触ったプレイヤーを取得
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <returns></returns>
    public Player GetPrevTouchPlayer(bool isPlayer){
        if(isPlayer){
            return prevTouchPlayer[0];
        }else{
            return prevTouchPlayer[1];
        }
    }    

    public void Serve(float serveNetHeight,Vector3 aimPos){
        Vector3 ballPos = ballRig.transform.position;
        Vector3 direction = aimPos - ballPos;
        Vector3 direction2D = direction;
        direction2D.y = 0f;
        float netDist = Mathf.Abs(ballPos.z)/Mathf.Abs(aimPos.z - ballPos.z) * direction2D.magnitude;

        var y1 = ballPos.y;
        var y2 = serveNetHeight;
        var y3 = aimPos.y;

        var netPassHeight = 1f;
        var vy = 1f;
        var velocity = Vector3.zero;
        while(netPassHeight < serveNetHeight){
            vy = vy + 0.1f;
            var t = vy*vy - 2f*g*(y1-y3);
            float time = (-vy - Mathf.Sqrt(t))/g;
            float A =  netDist / direction2D.magnitude;
            var timeNet = time * A;
            netPassHeight = y1 + vy * timeNet + 0.5f*g*timeNet*timeNet; 
            velocity = direction2D/time;  
        }
        
        velocity.y = vy;
        var force = (velocity - ballRig.velocity) * ballRig.mass;
        ballRig.AddForce(force,ForceMode.Impulse);
    }
    

}
