using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用來中斷Promisee的類別
/// </summary>
public class PromiseAborter : CustomYieldInstruction
{
    protected Promise target;
    /// <summary>
    /// 建構式，請傳入需要被監控的Promisee，不得為NULL
    /// </summary>
    /// <param name="promisee"></param>
    public PromiseAborter(Promise promisee)
    {
        Debug.Assert(promisee != null, "Promisee不得為null");
        this.target = promisee;
    }

    /// <summary>
    /// 用來支會系統是否等待(true)？
    /// </summary>
    public override bool keepWaiting
    {
        get
        {
            return true;
        }
    }

    /// <summary>
    /// 用來管控Promisee是否還在有效期限內？(true)
    /// </summary>
    bool _keep = true;
    public bool keep
    {
        get { return _keep; }
        set
        {
            _keep = value;
            if (value == false)
            {
                target.Abort("Aborted by " + ToString());
            }
        }
    }
}
