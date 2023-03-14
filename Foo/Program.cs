﻿// See https://aka.ms/new-console-template for more information
using VioletScript.Parser.Parser;
using VioletScript.Parser.Source;

Script script = new Script("foo.violet", "function f f():Number 10;");
Parser parser = new Parser(script);
parser.ParseProgram();

foreach (var d in script.Diagnostics)
{
    Console.WriteLine(d.ToString());
}
