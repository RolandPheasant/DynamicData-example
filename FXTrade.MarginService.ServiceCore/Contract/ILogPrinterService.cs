﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;

namespace FXTrade.MarginService.ServiceCore.Contract
{
   public interface ILogPrinterService
    {
        void PrintClientBalances();
        void PrintcurPairPositionPerClient();
        void PrintCurPositionPerClient();
        void PrintmyTrades();
    }
}
