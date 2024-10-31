﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript.Globals
{
    public interface IGlobal
    {
        string TypeAllowance { get; }
        string Call(List<object> arguments);
    }
}