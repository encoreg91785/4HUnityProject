using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 一個逾時中斷Promisee的Yieldable物件
/// </summary>
class TimeAborter: PromiseAborter
{
    float time;
    float waitSec;

    public TimeAborter(float sec, Promise target):base(target)
    {
        time = Time.time + sec;
        waitSec = sec;
    }

    public override bool keepWaiting
    {
        get
        {
            keep = Time.time < time ? true: false;
            return keep;
        }
    }

    public override string ToString()
    {
        return "Time out("+waitSec+")";
    }
}

/// <summary>
/// 一個owner中斷自己即中斷的Yieldable物件
/// </summary>
class ParentAborter : PromiseAborter
{
    Promise owner;
    public ParentAborter(Promise target, Promise owner):base(target)
    {
        this.owner = owner;
    }

    public override bool keepWaiting
    {
        get
        {
            keep = owner.isAbort ? false : true;
            return keep;
        }
    }

    public override string ToString()
    {
        return "Owner aborted!";
    }
}

/// <summary>
/// Promise 第2.1版：新增相關中斷功能與Resolve傳參數方法
/// 中斷等同Reject(除系統中斷外，執行Reject())
/// Done 可以多個
/// </summary>
public class Promise
{
    #region Promisee公開資訊
    /// <summary>
    /// 檢查是否完成
    /// </summary>
    public bool isDone { private set; get; }
    /// <summary>
    /// 檢查是否成功
    /// </summary>
    public bool isResolved { private set; get; }
    /// <summary>
    /// 內部使用檢查是否中斷（對應為 isResolved = false )
    /// </summary>
    public bool isAbort { private set; get; }
    /// <summary>
    /// 最後一個Then的產出
    /// </summary>
    public object lastOut { private set; get; }
    #endregion

    Queue<ProblemSolver> problemContainer = new Queue<ProblemSolver>();
    Action<string> rejectAction = null;
    List<Action> doneAction = new List<Action>();
    Action<Exception> exception = (e) => { Debug.Log("Exception:" + e.ToString()); };
    Promise parent = null;
    Promise[] children;
    MonoBehaviour owner;
    public static bool abortAllPromisees { set; private get; }
    
    /// <summary>
    /// 保護Reject，不被二次設定
    /// </summary>
    public bool protectReject { set; private get; }

    /// <summary>
    /// 設定問題解決後下一個待解決問題
    /// </summary>
    /// <param name="promiseFoo"></param>
    /// <returns></returns>
    public Promise Then(ProblemSolver promiseFoo)
    {
        if (promiseFoo == null) promiseFoo = new ProblemSolver(null);
        problemContainer.Enqueue(promiseFoo);
        return this;
    }

    public Promise Then(ProblemSolver.Solver solver)
    {
        solver = solver?? ((inout)=>{ return Answer.Resolve(); });
        var ps = new ProblemSolver(solver);
        problemContainer.Enqueue(ps);
        return this;
    }

    /// <summary>
    /// 問題解決失敗，Reject的處理函式
    /// </summary>
    /// <param name="promiseFoo"></param>
    /// <returns></returns>
    public Promise Reject(Action<string> promiseFoo)
    {
        if( protectReject==true )
        {
            Debug.LogError("Promise: reject 被重複設定");
            return this;
        }
        if (rejectAction != null) Debug.LogWarning("Promise: reject 被重複設定");
        rejectAction = promiseFoo;
        return this;
    }

    /// <summary>
    /// Promisee結束的處理函式
    /// </summary>
    /// <param name="promiseFoo"></param>
    /// <returns></returns>
    public Promise Done(Action promiseFoo)
    {
        doneAction.Remove(promiseFoo);
        doneAction.Add(promiseFoo);
        return this;
    }

    /// <summary>
    /// Promisee Exception 處理函式
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public Promise Exception(Action<Exception> exception)
    {
        this.exception = exception;
        return this;
    }

    /// <summary>
    /// 執行Promisee
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="sec">你可以指定一個秒數期間來結束</param>
    public Promise Invoke(MonoBehaviour owner, float sec=0)
    {
        PromiseAborter abortCondition = (sec == 0) ? null : new TimeAborter(sec, this);
        Invoke(owner, abortCondition);
        return this;
    }

    /// <summary>
    /// 執行Promisee
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="sec">你可以指定一個會被中斷的Promisee來結束</param>
    public void Invoke(MonoBehaviour owner, Promise promiseeOwner )
    {
        PromiseAborter abortCondition = (promiseeOwner == null) ? null : new ParentAborter(this, promiseeOwner);
        Invoke(owner, abortCondition);
    }

    /// <summary>
    /// 執行Promisee
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="abortCondition">你可以指定一個CusomYieldInstruction來控制結束條件</param>
    public void Invoke(MonoBehaviour owner, PromiseAborter abortCondition)
    {
        this.owner = owner;
        if (children != null)
        {
            foreach (var p in children)
            {
                p.parent = this;
                p.Invoke(owner, this);
            }
        }
        if ( abortCondition!=null ) owner.StartCoroutine(abortCondition);
        owner.StartCoroutine(RunThrowableIEnumerator( Yield(abortCondition) ));
    }

    /// <summary>
    /// 中斷此Promisee
    /// </summary>
    public void Abort(string reason="")
    {
        isAbort = true;
        isDone = true;
        isResolved = false;
        lastOut = reason;
    }

    /// <summary>
    /// 處理系統中斷，不執行Reject()但會執行Done()
    /// </summary>
    /// <returns></returns>
    bool SystemAbort()
    {
        if (abortAllPromisees == true )
        {
            string msg = "abort by System";
            Debug.Log(msg);
            Abort(msg);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 處理中斷條件
    /// </summary>
    /// <returns></returns>
    string ConditionAbort(PromiseAborter cond)
    {
        if (cond == null) return null;
        bool keep = cond.keepWaiting;
        if (keep == false)
        {
            return "abort:" + cond.ToString();
        }
        return null;
    }

    /// <summary>
    /// 執行一個會產生Exception的coroutine
    /// </summary>
    /// <param name="enumerator"></param>
    /// <returns></returns>
    IEnumerator RunThrowableIEnumerator(IEnumerator enumerator)
    {
        while (true)
        {
            object current;
            try
            {
                if (enumerator.MoveNext() == false)
                {
                    break;
                }
                current = enumerator.Current;
            }
            catch (Exception ex)
            {
                exception(ex);
                yield break;
            }
            yield return current;
        }
        // 不管如何永遠會跑done
        // 我們可以在這個函式做統一的資料清除、loading狀態解除等等工作
        // 但不應該處裡與遊戲流程有關的程序
        foreach (var done in doneAction) done();
    }

    /// <summary>
    /// 實際解決的ＣＯＤＥ
    /// </summary>
    /// <param name="abortCondition"></param>
    /// <returns></returns>
    IEnumerator Yield(PromiseAborter abortCondition)
    {
        isResolved = true;
        isDone = false;
        isAbort = false;
        Answer result = Answer.Resolve();
        // 確保: 未完成 && 系統沒有發生大事(abortAllPromisees) && 指定的abortCondition條件還沒發生
        while (isDone == false)
        {
            // 查驗結束條件
            // 1. 因為中斷條件發生？？
            // 2. 系統結束
            // 3. 父Promisee被中斷
            // 如果沒有任何指令了，完成這Promisee, 且為問題解決狀態
            if (problemContainer.Count == 0)
            {
                isDone = true;
                isResolved = true;
            }
            else
            {
                ProblemSolver problem = null;
                // 把Then拿出來執行，並等到非Abort狀態
                problem = problemContainer.Dequeue();
                result = problem.Solve(result.dataDeliver);
                result.promisee = this;
                lastOut = result.dataDeliver;
                if(result.isFinished)
                {
                    // 問題完全解決，沒有必要繼續再執行了
                    isResolved = false;
                    isDone = true;
                    break;
                }
                else if (result.isResolved)
                {
                    // 解決的狀態，至少會等一個Frame
                    if (result.yieldInstruction is IEnumerator)
                    {
                        yield return owner.StartCoroutine(result.yieldInstruction as IEnumerator);
                    }
                    else yield return (result.yieldInstruction as YieldInstruction);
                    if (result.dataDeliver is UnityWebRequest)
                    {
                        result = UnityHttpParser((UnityWebRequest)result.dataDeliver);
                    }
                    lastOut = result.dataDeliver;
                }
                if ( SystemAbort()==true ) break;
                string abortMsg = ConditionAbort(abortCondition);
                if (string.IsNullOrEmpty(abortMsg)==false || result.isRejected )
                {
                    string errMsg = abortMsg ?? result.dataDeliver.ToString();
                    if( rejectAction!=null ) rejectAction(errMsg);
                    isResolved = false;
                    isDone = true;
                }
            }
        }
    }

    /// <summary>
    /// 執行一個Promisee陣列
    /// 如果都成功：會把每一個Promisee的回傳值依序置於inout參數中（object[] )
    /// 如果失敗：會把第一個失敗的Promisee的回傳值置於inout參數中（object )
    /// 所以失敗有兩種處理方式：一種是在個別的Promisee.Reject裡面處理，一種是在Promisee.All的Reject裡面處理
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="all"></param>
    /// <returns></returns>
    static public Promise All(params Promise[] all)
    {
        for(int i=0; i<all.Length; i++)
        {
            all[i] = all[i] ?? new Promise();
        }
        var promiseAllInstance = new Promise().Then( (inout)=>
        {
            return Answer.PendingUntil( () =>
            {
                foreach (var p in all)
                {
                    if (p.isDone == false) return false;    // 有Promisee 還沒完結
                    else if (p.isResolved == false) return true;    // 完結的Promisee被拒絕，結束
                }
                return true; // 全Promisee 完結
            });
        }).Then(_=> {
            List<object> returns = new List<object>();
            foreach (var p in all)
            {
                if (p.isResolved == false)
                {
                    return Answer.Reject(p.lastOut.ToString());
                }
                returns.Add(p.lastOut);
            }
            var inout = returns.ToArray();
            return Answer.Resolve(inout);
        });

        promiseAllInstance.children = all;
        return promiseAllInstance;
    }

    Answer UnityHttpParser(UnityWebRequest res)
    {
        if (res.isHttpError==false&& res.isNetworkError == false)
        {
            return Answer.Resolve(res.downloadHandler.text);
        }
        else
        {
            return Answer.Reject(res.error);
        }
            
    }
}

#region Promisee使用範例
class ExampleOfPromisee
{
    //void DemoLogin()
    //{
    //    // login 範例
    //    // 指定ip:port 不用每次做
    //    PromiseeHelper.ip = "192.168.1.1";
    //    PromiseeHelper.port = 12345;
    //    // 參數設定
    //    Dictionary<string, string> param = new Dictionary<string, string>();
    //    param["account"] = "andrew";
    //    param["password"] = "andrew";
    //    // 設定回傳值
    //    var www = PromiseeHelper.WWWPost("login", param);
    //    var p = PromiseeHelper.AccessWebApi(www)
    //    .Then(_www=> { return Answer.Resolve(_www); });
    //    p.Invoke(null /*owner*/);
    //}

    // abort 範例: 當系統因故決定中斷執行該 Promisee
    // 設計上是等目前Then結束才abort
    void DemoAbort()
    {
        Promise p = new Promise().Then((inout) =>
        {
            var wait = new WaitForSeconds(1024);
            // 狀態設定為Resolved => 系統會檢查若為 UnityWebRequest 物件，或 YieldInstruction 都會自動執行
            return Answer.Resolve(wait);
        });
        p.Invoke(null /*owner*/);

        // 但注意: YieldInstruction 物件，因為已經在執行中，最快也會等他跑完有回應才能結束
        // 對isDone的Promisee沒影響
        p.Abort();

        // 系統發生大事，中斷所有Promisee
        Promise.abortAllPromisees = true;
    }

    // 等待一段時間中斷
    void DemoTimeOut()
    {
        Promise p = new Promise().Then((inout) =>
        {
            var wait = new WaitForSeconds(1024);
            // 狀態設定為Resolved => 系統會檢查若為 UnityWebRequest 物件，或 YieldInstruction 都會自動執行
            return Answer.Resolve(wait);
        });

        p.Invoke(null /*owner*/, 10f );
    }

    // 自定義等待條件：等待一個條件中斷
    void DemoCondition()
    {
        Promise p = new Promise().Then((inout) =>
        {
            var wait = new WaitForSeconds(1024);
            // 狀態設定為Resolved => 系統會檢查若為 UnityWebRequest 物件，或 YieldInstruction 都會自動執行
            return Answer.Resolve(wait);
        });

        p.Invoke(null /*owner*/);
    }
}

#endregion