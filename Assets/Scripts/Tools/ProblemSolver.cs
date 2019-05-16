using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 一個Problem Solver(解題器), 你可以自行繼承，並且改寫Solver完成一個複雜的處理
/// 並透過 Then(solver) 將之推入序列
/// </summary>
public class ProblemSolver
{
    public delegate Answer Solver(object inout);
    Solver solver;
    public ProblemSolver(Solver solver)
    {
        if (solver == null) solver = (inout) => { return Answer.Resolve(); };
        this.solver = solver;
    }

    virtual public Answer Solve(object inout)
    {
        return solver(inout);
    }
}

/// <summary>
/// 一個Problem Answer(答題器)
/// Resolver的標準回傳物件
/// </summary>
public class Answer
{
    /// <summary>
    /// 知會Promisee接下來該如何處理
    /// </summary>
    enum Result
    {
        /// <summary>
        /// 問題解決，繼續下一個Then
        /// </summary>
        Resolved,
        /// <summary>
        /// 問題無法解決，結束Promisee
        /// </summary>
        Rejected,
        /// <summary>
        /// 問題完全解決，結束整個Promisee
        /// </summary>
        Finished,
    }
    Result result;
    public Promise promisee { set; get; }
    public object dataDeliver;
    public object yieldInstruction = null;
    public bool isResolved { get { return result == Result.Resolved; } }
    public bool isRejected { get { return result == Result.Rejected; } }
    public bool isFinished { get { return result == Result.Finished; } }

    /// <summary>
    /// 成功，並傳回運算結果
    /// </summary>
    /// <param name="dataDeliver">當yieldInstruction結束後，傳給下個Then的參數</param>
    /// <param name="yieldInstruction">執行下個Then之前的等待程序\n可為YieldInstruction或IEnumerator</param>
    /// <returns></returns>
    public static Answer Resolve(object dataDeliver, object yieldInstruction)
    {
        var rt = new Answer();
        rt.result = Result.Resolved;
        rt.dataDeliver = dataDeliver;
        rt.yieldInstruction = yieldInstruction;
        return rt;
    }

    #region 相容２.0版
    public static Answer Resolve()
    {
        return Resolve(null, null);
    }

    public static Answer Resolve(object dataDeliver)
    {
        if (dataDeliver is UnityWebRequest) return PendingUntil((UnityWebRequest)dataDeliver);
        else return Resolve(dataDeliver, null);
    }

    public static Answer PendingUntil(UnityWebRequest dataDeliver)
    {
        var rt = new Answer();
        rt.result = Result.Resolved;
        rt.dataDeliver = dataDeliver;
        rt.yieldInstruction =  dataDeliver.SendWebRequest();
        return rt;
    }

    public static Answer Resolve(YieldInstruction yieldInstruction)
    {
        return Resolve(null, yieldInstruction);
    }

    public static Answer Resolve(IEnumerator yieldInstruction)
    {
        return Resolve(null, yieldInstruction);
    }
    #endregion
    /// <summary>
    /// 失敗，回傳失敗訊息
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    static public Answer Reject(string msg = "")
    {
        var rt = new Answer();
        rt.result = Result.Rejected;
        rt.dataDeliver = msg;
        return rt;
    }

    /// <summary>
    /// 成功，並傳回運算結果
    /// </summary>
    /// <param name="dataDeliver">當yieldInstruction結束後，傳給下個Then的參數</param>
    /// <param name="yieldInstruction">執行下個Then之前的等待程序\n可為YieldInstruction或IEnumerator</param>
    /// <returns></returns>
    public static Answer PendingUntil(object dataDeliver, Func<bool> yieldInstruction)
    {
        var rt = new Answer();
        rt.result = Result.Resolved;
        rt.dataDeliver = dataDeliver;
        rt.yieldInstruction = new WaitUntil(()=> { return yieldInstruction(); });
        return rt;
    }

    /// <summary>
    /// 等Dialog關閉
    /// </summary>
    /// <param name="dlg"></param>
    /// <returns></returns>
    //public static Answer PendingUntil(UIDialog dlg)
    //{
    //    var rt = new Answer();
    //    rt.result = Result.Resolved;
    //    rt.yieldInstruction = new WaitUntil(() => { return dlg==null; });
    //    return rt;
    //}

    ///// <summary>
    ///// 等Dialog關閉
    ///// </summary>
    ///// <param name="dataDelive"></param>
    ///// <param name="dlg"></param>
    ///// <returns></returns>
    //public static Answer PendingUntil(object dataDelive,UIDialog dlg)
    //{
    //    var rt = PendingUntil(dlg);
    //    rt.dataDeliver = dataDelive;
    //    return rt;
    //}

    /// <summary>
    /// 成功，並傳回運算結果
    /// </summary>
    /// <param name="dataDeliver">當yieldInstruction結束後，傳給下個Then的參數</param>
    /// <param name="yieldInstruction">執行下個Then之前的等待程序\n可為YieldInstruction或IEnumerator</param>
    /// <returns></returns>
    public static Answer PendingUntil(Func<bool> yieldInstruction)
    {
        if (yieldInstruction == null) yieldInstruction = () => { return true; };
        var rt = new Answer();
        rt.result = Result.Resolved;
        rt.dataDeliver = null;
        rt.yieldInstruction = new WaitUntil(() => { return yieldInstruction(); });
        return rt;
    }

    public static Answer PendingUntil(YieldInstruction yieldInstruction)
    {
        var rt = new Answer();
        rt.result = Result.Resolved;
        rt.dataDeliver = null;
        rt.yieldInstruction = yieldInstruction;
        return rt;
    }

    public static Answer PendingUntil(IEnumerator yieldInstruction)
    {
        var rt = new Answer();
        rt.result = Result.Resolved;
        rt.dataDeliver = null;
        rt.yieldInstruction = yieldInstruction;
        return rt;
    }

    public static Answer Finish()
    {
        var rt = new Answer();
        rt.result = Result.Finished;
        rt.dataDeliver = null;
        rt.yieldInstruction = null;
        return rt;
    }
}
