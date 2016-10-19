/*

::::::::::::::::::::::::Examlpe::::::::::::::::::::::::

public class Worker
{
    //Threaded method 
    public void DoWork(object sender, DoWorkEventArgs e)
    {
        BackgroundWorker worker = sender as BackgroundWorker;               
        for (int i = 0; i < int.MaxValue; i++)
        {
            if (worker.CancellationPending)
            {
                e.Cancel = true;
                break;
            }
            
            //-------------------DO A LOT OF WORK-------------------

        }
    }

    //Callback method
    public void Callback(object sender, RunWorkerCompletedEventArgs e)
    {
        BackgroundWorker worker = sender as BackgroundWorker;
        if (e.Cancelled)
        {
            //-------------------Handle cancellation-------------------
        }
        else if (e.Error != null)
        {
            //-------------------Handle error-------------------
        }
        else
        {
            //-------------------Handle correct completion-------------------
        }

    }

    //Use ThreadUtil
    static void Main(string[] args)
    {
        ThreadUtil t = new ThreadUtil(this.DoWork, this.Callback);
        t.Run();

        if(t.IsBusy())
        {
            Console.WriteLine("Working");
        }


    }
}

:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

*/



using UnityEngine;
using System.ComponentModel;
using System;

public class ThreadUtil {

    BackgroundWorker backgroundWorker;

//    private DoWorkEventHandler threadedMethod;
    private RunWorkerCompletedEventHandler callbackMethod;

    private DateTime start;

    public ThreadUtil(DoWorkEventHandler threadedMethod, RunWorkerCompletedEventHandler callbackMethod)
    {
        //this.threadedMethod = threadedMethod;
        this.callbackMethod = callbackMethod;

        backgroundWorker = new BackgroundWorker();
        //backgroundWorker.WorkerReportsProgress = true;
        backgroundWorker.WorkerSupportsCancellation = true;
        backgroundWorker.DoWork += new DoWorkEventHandler(threadedMethod);
        backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Callback);
    }

    private void Callback(object sender, RunWorkerCompletedEventArgs e)
    {
        callbackMethod(sender, e);
        //Debug.Log("[ThreadUtil] Thread finished - duration: " + (DateTime.Now - start));
    }

    public void Run()
    {
        start = DateTime.Now;
        backgroundWorker.RunWorkerAsync();
        Debug.Log("[ThreadUtil] Thread started");
    }

    /*
    You have to implement the canclellation in your DoWork-method. If DoWorkEventArgs.Cancel = true, abort the method;
    */
    public void Abort()
    {
        backgroundWorker.CancelAsync();
        Debug.Log("[ThreadUtil] Thread aborted");
    }

    public bool IsBusy()
    {
        return backgroundWorker.IsBusy;
    }
}
