using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

public class BrainFuckCompiler {
  public string[] _args { get; }
  public BrainFuckCompiler(string[] args) {
    this._args = args;
  }
  public void compile() {
    if (this._args.Length < 1) throw new ArgumentNullException("no bf file");
#if BENCHMARK
    Console.WriteLine("BENCHMARK");
#endif
    new CodeGenerator(ParseFile(this._args[0])).gen();
  }

  internal static ASTNode ParseFile(string path) {
    var tokens = new Lexer(File.ReadAllText(path)).lex();
    return new Parser(tokens).ast();
  }
}

public enum Token_Enum {
  None,  /*  default must be added*/
  BeginLoop,/* [ */
  EndLoop, /* ]*/
  Input, /* , */
  Output,  /* .*/
  Increment, /* + */
  Decrement, /* - */
  MoveLeft,/*<*/
  MoveRight, /*>*/
}
public class Token {
  public Token_Enum TokenType { get; }
  public Token(Token_Enum token_Enum) {
    TokenType = token_Enum;
  }
}
public class Lexer {
  private List<Token>tokens = new List<Token>();
  private string _str;
  public Lexer(string str) {
    this._str = str;
  }
  public List<Token> lex () {
    int len = this._str.Length;
    int i = 0;
    int j = 0;
    while(i < len) {
      switch(this._str[i]) {
        case '[': this.tokens.Add(new Token(Token_Enum.BeginLoop)); j++; break;
        case ']': this.tokens.Add(new Token(Token_Enum.EndLoop)); j++; break;
        case '+': this.tokens.Add(new Token(Token_Enum.Increment)); j++; break;
        case '-': this.tokens.Add(new Token(Token_Enum.Decrement)); j++; break;
        case ',': this.tokens.Add(new Token(Token_Enum.Input)); j++; break;
        case '.': this.tokens.Add(new Token(Token_Enum.Output)); j++; break;
        case '<': this.tokens.Add(new Token(Token_Enum.MoveLeft)); j++; break;
        case '>': this.tokens.Add(new Token(Token_Enum.MoveRight)); j++; break;
        default: break; // ignore Error token
      }
      i++;
    }
    return tokens;  
  }
}
public class ASTNode {
  public ASTNode LeftNode { get; set; }
  public ASTNode MiddleNode { get; set; }
  public ASTNode RightNode { get; set; }
  public Token_Enum TokenType { get; }
  public int RepeatCount { get; } = 1;
  public ASTNode() {
  }
  public ASTNode(Token token) : this(token, 1) {
  }
  internal ASTNode(Token token, int repeatCount) {
    TokenType = token.TokenType;
    RepeatCount = repeatCount;
  }
}
public class Parser {
  private List<Token>_tokens = new List<Token>();
  private int _currentIndex;
  private int _endIndex;
  public Parser(List<Token> tokens) {
    _tokens = tokens;
    _currentIndex = 0;
    _endIndex = tokens.Count == 0 ? -1 : tokens.Count-1;
  }
  public ASTNode ast() {
    _checkBracket();
    ASTNode ast = _program();
    return ast;
  }
  private void _checkBracket() {
    Stack<int> st = new Stack<int>();
    for(int index = 0; index < _tokens.Count; index++) {
      Token token = _tokens[index];
      if(token.TokenType == Token_Enum.BeginLoop) {
        st.Push(index);
      }
      else if(st.Count == 0 && token.TokenType == Token_Enum.EndLoop) {
        throw new Exception("no matched [");
      }
      else if (token.TokenType == Token_Enum.EndLoop) {
        st.Pop();
      }
    }
    if(st.Count > 0) {
      throw new Exception("no matched ]");
    }
    
  }

/*
Program → Instr Program | ε
Instr → '+' | '-' | '>' | '<' | ',' | '.' | '[' Program ']'
*/ 
  private ASTNode _program() {
    if (_endIndex == -1)  return null;
    if (_isNextTokenEOF()) return null;
    // A closing bracket ends the current loop body. Leave it for the loop
    // instruction to consume so it cannot be mistaken for another command.
    if (_tokens[_currentIndex].TokenType == Token_Enum.EndLoop) return null;

    ASTNode leftNode = _instr();
    if (leftNode == null) return null;
    ASTNode rightNode = _program();
    if (rightNode == null) return leftNode;
    
    ASTNode root = new ASTNode();
    root.LeftNode = leftNode;
    root.RightNode = rightNode;
    
    return root;
  }
  private ASTNode _instr() {
    Token token = _getCurrentToken();
    if (token == null)
      return null;

    if (token.TokenType == Token_Enum.Increment || token.TokenType == Token_Enum.Decrement ||
        token.TokenType == Token_Enum.MoveLeft || token.TokenType == Token_Enum.MoveRight) {
      int count = 1;
      while (!_isNextTokenEOF() && _tokens[_currentIndex].TokenType == token.TokenType) {
        _currentIndex++;
        count++;
      }
      return new ASTNode(token, count);
    }
    if (token.TokenType == Token_Enum.Input || token.TokenType == Token_Enum.Output) {
      return new ASTNode(token);
    }
    else if (token.TokenType == Token_Enum.BeginLoop) {
      ASTNode parent = new ASTNode();
      parent.LeftNode = new ASTNode(new Token(Token_Enum.BeginLoop));
      parent.RightNode = new ASTNode(new Token(Token_Enum.EndLoop));
      ASTNode middleNode = _program();
      if (middleNode != null)  parent.MiddleNode = middleNode;
      _getCurrentToken(); // consume the matching closing bracket
      return parent;
    }
    return null;
    
  }
  private Token _getCurrentToken() {
    if (_isNextTokenEOF()) return null;
    return _tokens[_currentIndex++];
  }
  private bool _isNextTokenEOF () {
    return _endIndex < _currentIndex;
  }
}
public class CodeGenerator {
  public ASTNode _ast { get; }
  public CodeGenerator(ASTNode ast) {
    _ast = ast;
  }
  public void gen() {
    AssemblyName am = new AssemblyName("bfc");
    AssemblyBuilder ab = AssemblyBuilder.DefineDynamicAssembly(am, AssemblyBuilderAccess.Run);
    ModuleBuilder mb = ab.DefineDynamicModule(am.Name);
    TypeBuilder tb = mb.DefineType("bfcType", TypeAttributes.Public);
    MethodBuilder metb = tb.DefineMethod("bfcCodegen", MethodAttributes.Public |
    MethodAttributes.Static, null, null);

    ILGenerator il = metb.GetILGenerator();
    // setup
    il.DeclareLocal(typeof(byte[]));
    il.DeclareLocal(typeof(int));
    il.Emit(OpCodes.Ldc_I4, 3000);
    il.Emit(OpCodes.Newarr, typeof(byte));
    il.Emit(OpCodes.Stloc_0);
    // int ptr = 0
    il.Emit(OpCodes.Ldc_I4_0);
    il.Emit(OpCodes.Stloc_1);
    
    walk(il, _ast, 0);

    il.Emit(OpCodes.Ret);
    Type bfcType = tb.CreateType();

    //codegen start
    MethodInfo mi = bfcType.GetMethod("bfcCodegen");
    mi.Invoke(null, null);

    //execute without saving into IL file in .net core

  }
  private static MethodInfo writeMethod;
  private static Type[] writeMethodParameters;
  private static MethodInfo readMethod;
  private static readonly Type[] readMethodParameters = null;

  private Stack<Label> _endLabelSt = new Stack<Label>();

  private Stack<Label> _headLabelSt = new Stack<Label>();

  // The generated method runs in a separate assembly, so it needs public access.
  public static void GrowTape(ref byte[] tape, int position) {
    long capacity = Math.Max((long)position + 1, (long)tape.Length * 2);
    Array.Resize(ref tape, (int)Math.Min(capacity, int.MaxValue));
  }

  private void push_locals(ILGenerator il)
  {
    il.Emit(OpCodes.Ldloc_0);
    il.Emit(OpCodes.Ldloc_1);
  }

  private void emit_load_memory(ILGenerator il)
  {
      push_locals(il);
      il.Emit(OpCodes.Ldelema, typeof(byte));
      il.Emit(OpCodes.Dup);
      il.Emit(OpCodes.Ldobj, typeof(byte));
  }
  private void emit_save_memory(ILGenerator il)
  {
      il.Emit(OpCodes.Stobj, typeof(byte));
  }
  private static void emit_repeat_count(ILGenerator il, int count) {
    if (count == 1)
      il.Emit(OpCodes.Ldc_I4_1);
    else if (count <= sbyte.MaxValue)
      il.Emit(OpCodes.Ldc_I4_S, (sbyte)count);
    else
      il.Emit(OpCodes.Ldc_I4, count);
  }
  private void emit_cell_change(ILGenerator il, OpCode operation, int count)
  {
      emit_load_memory(il);
      emit_repeat_count(il, count);
      il.Emit(operation);
      il.Emit(OpCodes.Conv_U1);
      emit_save_memory(il);
  }
  private void emit_ptr_increment(ILGenerator il, int count)
  {
      il.Emit(OpCodes.Ldloc_1);
      emit_repeat_count(il, count);
      il.Emit(OpCodes.Add_Ovf);
      il.Emit(OpCodes.Stloc_1);

      Label withinTape = il.DefineLabel();
      il.Emit(OpCodes.Ldloc_1);
      il.Emit(OpCodes.Ldloc_0);
      il.Emit(OpCodes.Ldlen);
      il.Emit(OpCodes.Conv_I4);
      il.Emit(OpCodes.Blt, withinTape);
      il.Emit(OpCodes.Ldloca_S, (byte)0);
      il.Emit(OpCodes.Ldloc_1);
      il.Emit(OpCodes.Call, typeof(CodeGenerator).GetMethod(nameof(GrowTape)));
      il.MarkLabel(withinTape);
  }
  private void emit_ptr_decrement(ILGenerator il, int count)
  {
      il.Emit(OpCodes.Ldloc_1);
      emit_repeat_count(il, count);
      il.Emit(OpCodes.Sub);
      il.Emit(OpCodes.Stloc_1);
  }
  private void emit_console_write(ILGenerator il)
  {
      writeMethod = writeMethod ?? typeof(Console).GetMethod("Write", new[] { typeof(char) });
      writeMethodParameters = writeMethodParameters ?? new[] { typeof(char) };

      //Console.Write((char)memory[ptr]);
      il.Emit(OpCodes.Ldloc_0);
      il.Emit(OpCodes.Ldloc_1);
      il.Emit(OpCodes.Ldelem_U1);
      il.EmitCall(OpCodes.Call, writeMethod, writeMethodParameters);
  }
  private void emit_console_read(ILGenerator il)
  {
      readMethod = readMethod ?? typeof(Console).GetMethod("Read");
      //memory[ptr] = (byte)(Console.Read() & 0xFF);
      il.Emit(OpCodes.Ldloc_0);
      il.Emit(OpCodes.Ldloc_1);
      il.EmitCall(OpCodes.Call, readMethod, readMethodParameters);
      il.Emit(OpCodes.Ldc_I4, 0xFF);
      il.Emit(OpCodes.And);
      il.Emit(OpCodes.Conv_U1);
      il.Emit(OpCodes.Stelem_I1);
  }
  private void emit_begin_loop(ILGenerator il)
  {
      Label headLabel = il.DefineLabel();
      _headLabelSt.Push(headLabel);
      
      il.MarkLabel(headLabel);
      
      push_locals(il);
      il.Emit(OpCodes.Ldelem_U1);
      il.Emit(OpCodes.Ldc_I4_0);
      il.Emit(OpCodes.Ceq);

      Label endLabel = il.DefineLabel();
      _endLabelSt.Push(endLabel);
      il.Emit(OpCodes.Brtrue, endLabel);
  }
  private void emit_end_loop(ILGenerator il)
  {
      Label headLabel = _headLabelSt.Pop();
      il.Emit(OpCodes.Br, headLabel);
      Label endLabel = _endLabelSt.Pop();
      il.MarkLabel(endLabel);
  }
  private void walk(ILGenerator il, ASTNode node, int layer) {
    if (node == null) return;
    switch(node.TokenType)
    {
      case Token_Enum.Output:
        emit_console_write(il);
        break;
      case Token_Enum.Input:
        emit_console_read(il);
        break;
      case Token_Enum.Increment:
        emit_cell_change(il, OpCodes.Add, node.RepeatCount);
        break;
      case Token_Enum.Decrement:
        emit_cell_change(il, OpCodes.Sub, node.RepeatCount);
        break;
      case Token_Enum.MoveLeft:
        emit_ptr_decrement(il, node.RepeatCount);
        break;
      case Token_Enum.MoveRight:
        emit_ptr_increment(il, node.RepeatCount);
        break;
      case Token_Enum.BeginLoop:
        emit_begin_loop(il);
        break;
      case Token_Enum.EndLoop:
        emit_end_loop(il);
        break;
      default: break;
    }
    walk(il, node.LeftNode, layer+1);
    
    walk(il, node.MiddleNode, layer+1);

    walk(il, node.RightNode, layer+1);
  } 
}
