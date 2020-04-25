using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
/*

[ ] Read ',' (which converts number to char)
[ ] Allowed new_line, space or tab in *.bf

*/
public class Program
{
	
	public static void Main()
	{
    string raw_commands = @"+++++ +++++             (initialize counter (cell #0) to 10)
[                       (use loop to set the next four cells to 70/100/30/10)
    > +++++ ++          (    add  7 to cell #1)
    > +++++ +++++       (    add 10 to cell #2 )
    > +++               (    add  3 to cell #3)
    > +                 (    add  1 to cell #4)
    <<<< -              (    decrement counter (cell #0))
]                   
> ++ .                  (print 'H')
> + .                   (print 'e')
+++++ ++ .              (print 'l')
.                       (print 'l')
+++ .                   (print 'o')
> ++ .                  (print ' ')
<< +++++ +++++ +++++ .  (print 'W')
> .                     (print 'o')
+++ .                   (print 'r')
----- - .               (print 'l')
----- --- .             (print 'd')
> + .                   (print bang)
> .                     (print '\n')";
		//string commands = @"++++++++++[>+++++++>++++++++++>+++>+<<<<-]>++.>+.+++++++..+++.>++.<<+++++++++++++++.>.+++.------.--------.>+.>.";
		//string commands = @"++++++++[>++++[>++>+++>+++>+<<<<-]>+>+>->>+[<]<-]>>.>---.+++++++..+++.>>.<-.<.+++.------.--------.>>+.>++.";
    string commands=string.Empty;
    // remove all characters except []<>+-.,
    commands = Regex.Replace(raw_commands, @"[^\[\]<>\+-\.,]", "");
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
