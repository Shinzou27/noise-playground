using System;
using System.Collections.Generic;

public static class PlaygroundManager
{
  public static Dictionary<string, int> integerVariables = new();
  public static Dictionary<string, string> stringVariables = new();
  public static Dictionary<string, bool> booleanVariables = new();
  public static EventHandler OnVariableChange;
}