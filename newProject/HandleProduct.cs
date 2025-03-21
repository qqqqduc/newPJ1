using newProject.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newProject
{
    class HandleProduct
    {
        private Form_Main _formMain;

        private CancellationTokenSource _cts;

        private string Result;
        private int StartTime;
        private int RunTime;
        private int FinishTime;
        private int countOK;
        private int countNG;

        public void Start()
        {
            _cts = new CancellationTokenSource();
            Task.Run(() =>
            {
                Run(_cts.Token);
            }, _cts.Token);
        }

        private void Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Chờ tín hiệu Start từ PLC
                    //while (đọc start (true/false))
                    while (true)
                    {
                        if (token.IsCancellationRequested) return;
                        Thread.Sleep(1000);
                    }

                    // AI xử lý
                    // Result(OK/NG) 

                    _formMain.Invoke(new Action(() =>
                    {
                        _formMain.UpdateUI(Result, StartTime, RunTime, FinishTime, countOK, countNG);
                    }));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi trong StartTest: " + ex.Message);
                }
            }
        }
    }
}
