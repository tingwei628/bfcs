using System;
using System.Collections.Generic;
					
public class Program
{
	// ref: brainfuck visualizer
	// http://fatiherikli.github.io/brainfuck-visualizer
	
	
	// test: read from file brainfk.bf
	// test: nested [ ] (V)
	// test: ',' convert number to char;
	// test: if new_line, space or tab , how to remove
	
	
	// test: is it a valid .bf ? write an ast ? (lexer, ...tokens...)
	// test: check if memory error e.g. pos_tape = -1
	
	
	public static void Main()
	{
		string commands = @"++++++++++[>+++++++>++++++++++>+++>+<<<<-]>++.>+.+++++++..+++.>++.<<+++++++++++++++.>.+++.------.--------.>+.>.";
		//string commands = @"++++++++[>++++[>++>+++>+++>+<<<<-]>+>+>->>+[<]<-]>>.>---.+++++++..+++.>>.<-.<.+++.------.--------.>>+.>++.";
		
		// initially all 0 on the tape
		int[] tape = new int[1024 * 1024];
		int commands_size = commands.Length;
		Stack<int> command_pos_bracket = new Stack<int>();
		int pos_command = 0;
		int pos_tape = 0;

		while(pos_command < commands_size) {
			switch(commands[pos_command]) {
				case '[':
					if(tape[pos_tape] != 0){
						pos_command++;
						command_pos_bracket.Push(pos_command);
					}
					else {
						while(commands[pos_command] != ']') {
							pos_command++; 
						}						
					}
					break;
				case ']':					
					if (tape[pos_tape] == 0) {
						pos_command++;
						// jump out of the loop 
						if (command_pos_bracket.Count > 0) command_pos_bracket.Pop();
					}
					else { pos_command = command_pos_bracket.Peek(); }
					break;
				case '+':
					tape[pos_tape]++;
					pos_command++;
					break;
				case '-':
					tape[pos_tape]--;
					pos_command++;
					break;
				case '>':
					pos_tape++;
					pos_command++;
					break;
				case '<':
					pos_tape--;
					pos_command++;
					break;
				case '.':
					Console.Write("{0}", (char)tape[pos_tape]);
					pos_command++;
					break;
			    case ',':
					tape[pos_tape] = (int)Console.Read();
					pos_command++;
					break;
				default:
					break;
			}
		}
	}
}
