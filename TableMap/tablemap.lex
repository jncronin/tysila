%namespace TableMap
%visibility internal

%x str
%x comment

%{
	StringBuilder sb;
	int comment_nesting = 0;
%}

%%

[0-9]+			yylval.intval = Int32.Parse(yytext); return (int)Tokens.INT;
"=="			return (int)Tokens.EQUALS;
"!="			return (int)Tokens.NOTEQUAL;
"!"				return (int)Tokens.NOT;
"<="			return (int)Tokens.LEQUAL;
">="			return (int)Tokens.GEQUAL;
"="				return (int)Tokens.ASSIGN;
"+="			return (int)Tokens.APPEND;
"?="			return (int)Tokens.ASSIGNIF;
"("				return (int)Tokens.LPAREN;
")"				return (int)Tokens.RPAREN;
","				return (int)Tokens.COMMA;
"*"				return (int)Tokens.MUL;
"+"				return (int)Tokens.PLUS;
"-"				return (int)Tokens.MINUS;
"$"				return (int)Tokens.DOLLARS;
":"				return (int)Tokens.COLON;
";"				return (int)Tokens.SEMICOLON;
"["				return (int)Tokens.LBRACK;
"]"				return (int)Tokens.RBRACK;
"{"				return (int)Tokens.LBRACE;
"}"				return (int)Tokens.RBRACE;
"."				return (int)Tokens.DOT;
"<"				return (int)Tokens.LT;
">"				return (int)Tokens.GT;
"<<"			return (int)Tokens.LSHIFT;
">>"			return (int)Tokens.RSHIFT;
"||"			return (int)Tokens.LOR;
"&&"			return (int)Tokens.LAND;
"|"				return (int)Tokens.OR;
"&"				return (int)Tokens.AND;

if				return (int)Tokens.IF;
else			return (int)Tokens.ELSE;
for				return (int)Tokens.FOR;
foreach			return (int)Tokens.FOREACH;
in				return (int)Tokens.IN;
while			return (int)Tokens.WHILE;
do				return (int)Tokens.DO;
include			return (int)Tokens.INCLUDE;
rulefor			return (int)Tokens.RULEFOR;
function		return (int)Tokens.FUNCTION;
return			return (int)Tokens.RETURN;
inputs			return (int)Tokens.INPUTS;
depends			return (int)Tokens.DEPENDS;
always			return (int)Tokens.ALWAYS;
export			return (int)Tokens.EXPORT;

isdir			return (int)Tokens.ISDIR;
isfile			return (int)Tokens.ISFILE;
defined			return (int)Tokens.DEFINED;

int				return (int)Tokens.INTEGER;
string			return (int)Tokens.STRING;
array			return (int)Tokens.ARRAY;
object			return (int)Tokens.OBJECT;
void			return (int)Tokens.VOID;
funcref			return (int)Tokens.FUNCREF;
any				return (int)Tokens.ANY;

null			return (int)Tokens.NULL;

"/*"            BEGIN(comment); ++comment_nesting;
"//".*          /* // comments to end of line */

<comment>[^*/]* /* Eat non-comment delimiters */
<comment>"/*"   ++comment_nesting;
<comment>"*/"   if (--comment_nesting == 0) BEGIN(INITIAL);
<comment>[*/]   /* Eat a / or * if it doesn't match comment sequence */

\"      sb = new StringBuilder(); BEGIN(str);
     
<str>\"        { /* saw closing quote - all done */
        BEGIN(INITIAL);
        /* return string constant token type and
        * value to parser
        */
		yylval.strval = sb.ToString();
		return (int)Tokens.STRING;
        }
     
<str>\n        {
        /* error - unterminated string constant */
        /* generate error message */
		throw new Exception("Unterminated string constant: " + sb.ToString());
        }
     
<str>\\[0-7]{1,3} {
        /* octal escape sequence */
        int result;
     
		result = Convert.ToInt32(yytext.Substring(1), 8);
     
        if ( result > 0xff )
                /* error, constant is out-of-bounds */
     
        sb.Append((char)result);
        }
     
<str>\\[0-9]+ {
        /* generate error - bad escape sequence; something
        * like '\48' or '\0777777'
        */
		throw new Exception("Bad escape sequence: " + yytext);
        }
     
<str>\\n  sb.Append('\n');
<str>\\t  sb.Append('\t');
<str>\\r  sb.Append('\r');
<str>\\b  sb.Append('\b');
<str>\\f  sb.Append('\f');
<str>\\\" sb.Append('\"');
     
<str>\\(.|\n)  sb.Append(yytext[1]);
     
<str>[^\\\n\"]+        {
		sb.Append(yytext);
        }


[a-zA-Z_][a-zA-Z0-9_\#`]*		yylval.strval = yytext; return (int)Tokens.LABEL;

