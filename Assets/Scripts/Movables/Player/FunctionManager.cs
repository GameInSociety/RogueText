
using JetBrains.Annotations;
using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionManager
{
    public List<FunctionSequence> functionLists;
    public FunctionSequence CurrentFunctionList;

    public List<Function> functions = new List<Function>();
    public Function CurrentFunction
    {
        get { return functions[0]; }
    }

    public bool breakFunctionList;

    public void CallFunctions(string functionList)
    {
       
    }

    public void Call(string line)
    {
        
    }


    #region break
    public void Break(string text)
    {
        TextManager.Write(text);

        Break();
    }

    public void Break()
    {
        breakFunctionList = true;
    }
    #endregion

    

    

   
}