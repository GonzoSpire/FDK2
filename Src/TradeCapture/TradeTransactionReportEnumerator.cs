using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.FDK.Common;

namespace TickTrader.FDK.TradeCapture
{
    public class TradeTransactionReportEnumerator : IDisposable
    {
        internal TradeTransactionReportEnumerator(Client client)
        {
            client_ = client;

            mutex_ = new object();
            completed_ = false;
            taskCompletionSource_ = null;
            tradeTransactionReports_ = new TradeTransactionReport[GrowSize];
            tradeTransactionReportCount_ = 0;
            beginIndex_ = 0;
            endIndex_ = 0;
            exception_ = null;
        }

        public TradeTransactionReport Next(int timeout)
        {
            return Client.ConvertToSync(NextAsync(), timeout);
        }

        public Task<TradeTransactionReport> NextAsync()
        {
            lock (mutex_)
            {
                if (taskCompletionSource_ != null)
                    throw new Exception("Invalid enumerator call");

                if (tradeTransactionReportCount_ > 0)
                {
                    TradeTransactionReport tradeTransactionReport = tradeTransactionReports_[beginIndex_];
                    tradeTransactionReports_[beginIndex_] = null;       // !
                    beginIndex_ = (beginIndex_ + 1) % tradeTransactionReports_.Length;
                    -- tradeTransactionReportCount_;

                    TaskCompletionSource<TradeTransactionReport> taskCompletionSource = new TaskCompletionSource<TradeTransactionReport>();
                    taskCompletionSource.SetResult(tradeTransactionReport);

                    return taskCompletionSource.Task;
                }

                if (exception_ != null)
                {
                    TaskCompletionSource<TradeTransactionReport> taskCompletionSource = new TaskCompletionSource<TradeTransactionReport>();
                    taskCompletionSource.SetException(exception_);

                    return taskCompletionSource.Task;
                }

                if (completed_)
                {
                    TaskCompletionSource<TradeTransactionReport> taskCompletionSource = new TaskCompletionSource<TradeTransactionReport>();
                    taskCompletionSource.SetResult(null);

                    return taskCompletionSource.Task;
                }

                taskCompletionSource_ = new TaskCompletionSource<TradeTransactionReport>();

                return taskCompletionSource_.Task;
            }            
        }

        public void Close()
        {
            lock (mutex_)
            {
                if (! completed_)
                {
                    completed_ = true;

                    if (taskCompletionSource_ != null)
                    {
                        taskCompletionSource_.SetResult(null);
                        taskCompletionSource_ = null;
                    }
                }

                for (int index = beginIndex_; index != endIndex_; ++ index)
                    tradeTransactionReports_[index % tradeTransactionReports_.Length] = null;

                tradeTransactionReportCount_ = 0;
                beginIndex_ = 0;
                endIndex_ = 0;
            }
        }

        public void Dispose()
        {
            Close();

            GC.SuppressFinalize(this);
        }

        internal void SetResult(TradeTransactionReport tradeTransactionReport)
        {
            lock (mutex_)
            {
                if (! completed_)
                {
                    if (taskCompletionSource_ != null)
                    {
                        taskCompletionSource_.SetResult(tradeTransactionReport);
                        taskCompletionSource_ = null;
                    }
                    else
                    {
                        if (tradeTransactionReportCount_ == tradeTransactionReports_.Length)
                        {
                            TradeTransactionReport[] tradeTransactionReports = new TradeTransactionReport[tradeTransactionReports_.Length + GrowSize];

                            if (endIndex_ > beginIndex_)
                            {
                                Array.Copy(tradeTransactionReports_, beginIndex_, tradeTransactionReports, 0, tradeTransactionReportCount_);
                            }
                            else
                            {
                                int count = tradeTransactionReports_.Length - beginIndex_;
                                Array.Copy(tradeTransactionReports_, beginIndex_, tradeTransactionReports, 0, count);
                                Array.Copy(tradeTransactionReports_, 0, tradeTransactionReports, count, endIndex_);
                            }

                            tradeTransactionReports_ = tradeTransactionReports;
                            beginIndex_ = 0;
                            endIndex_ = tradeTransactionReportCount_;
                        }

                        tradeTransactionReports_[endIndex_] = tradeTransactionReport;
                        endIndex_ = (endIndex_ + 1) % tradeTransactionReports_.Length;
                        ++ tradeTransactionReportCount_;
                    }
                }
            }
        }

        internal void SetEnd()
        {
            lock (mutex_)
            {
                if (! completed_)
                {
                    completed_ = true;

                    if (taskCompletionSource_ != null)
                    {
                        taskCompletionSource_.SetResult(null);
                        taskCompletionSource_ = null;
                    }
                }
            }
        }

        internal void SetError(Exception exception)
        {
            lock (mutex_)
            {
                if (! completed_)
                {
                    completed_ = true;
                    exception_ = exception;                        

                    if (taskCompletionSource_ != null)
                    {
                        taskCompletionSource_.SetException(exception);
                        taskCompletionSource_ = null;
                    }                        
                }
            }
        }

        const int GrowSize = 1000;

        Client client_;

        internal object mutex_;
        internal bool completed_;
        TaskCompletionSource<TradeTransactionReport> taskCompletionSource_;
        TradeTransactionReport[] tradeTransactionReports_;
        int tradeTransactionReportCount_;
        int beginIndex_;
        int endIndex_;
        Exception exception_;
    }
}