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
    string bf_filepath = this._args[0];
    string str = File.ReadAllText(bf_filepath);
    //string str = @"+[>[<->+[>+++>[+++++++++++>][]-[<]>-]]++++++++++<]>>>>>>----.<<+++.<-..+++.<-.>>>.<<.+++.------.>-.<<+.<.";
    var tokens = new Lexer(str).lex();
    var ast = new Parser(tokens).ast();
    new CodeGenerator(ast).gen();
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
  public ASTNode() {
  }
  public ASTNode(Token token) {
    TokenType = token.TokenType;
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

    if (isTerminals(token.TokenType)){
      return new ASTNode(token);
    }
    else if (token.TokenType == Token_Enum.BeginLoop) {
      
      ASTNode leftNode = new ASTNode(token);
      ASTNode parent = new ASTNode();
      parent.LeftNode = new ASTNode(new Token(Token_Enum.BeginLoop));
      parent.RightNode = new ASTNode(new Token(Token_Enum.EndLoop));
      ASTNode middleNode = _program();
      if (middleNode != null)  parent.MiddleNode = middleNode;
      return parent;
    }
    return null;
    
  }
  private bool isTerminals(Token_Enum token_type) {
    return
      token_type == Token_Enum.Increment ||
      token_type == Token_Enum.Decrement ||
      token_type == Token_Enum.MoveRight ||
      token_type == Token_Enum.MoveLeft ||
      token_type == Token_Enum.Input ||
      token_type == Token_Enum.Output;
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
     // byte[3000]
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
  private void emit_cell_increment(ILGenerator il)
  {
      //++memory[ptr];
      emit_load_memory(il);
      il.Emit(OpCodes.Ldc_I4_1);
      il.Emit(OpCodes.Add);
      il.Emit(OpCodes.Conv_U1);
      emit_save_memory(il);
  }
  private void emit_cell_decrement(ILGenerator il)
  {
      //--memory[ptr];
      emit_load_memory(il);
      il.Emit(OpCodes.Ldc_I4_1);
      il.Emit(OpCodes.Sub);
      il.Emit(OpCodes.Conv_U1);
      emit_save_memory(il);
  }
  private void emit_ptr_increment(ILGenerator il)
  {
      //++ptr;
      il.Emit(OpCodes.Ldloc_1);
      il.Emit(OpCodes.Ldc_I4_1);
      il.Emit(OpCodes.Add);
      il.Emit(OpCodes.Stloc_1);
  }
  private void emit_ptr_decrement(ILGenerator il)
  {
      //--ptr;
      il.Emit(OpCodes.Ldloc_1);
      il.Emit(OpCodes.Ldc_I4_1);
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
        emit_cell_increment(il);
        break;
      case Token_Enum.Decrement:
        emit_cell_decrement(il);
        break;
      case Token_Enum.MoveLeft:
        emit_ptr_decrement(il);
        break;
      case Token_Enum.MoveRight:
        emit_ptr_increment(il);
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
