grammar Cool;
options{
	language = CSharp;
}

program: (classDefine ';')+ EOF
	   ;

classDefine: CLASS TYPE (INHERITS TYPE)? '{' (feature ';')* '}'
       ;

feature: method
       | property
       ;

method: ID '(' (formal (',' formal)*)* ')' ':' TYPE '{' expr '}'
	  ;

property: formal (ASSIGNMENT expr)?
      ;

formal: ID ':' TYPE;  /* method argument */

expr: result_expr = expr ('@' TYPE)? '.' ID '(' (expr (',' expr)*)* ')'     # dispatchExplicit
    | ID '(' (expr (',' expr)*)* ')'                                        # dispatchImplicit
    | 'if' cond = expr 'then' result = expr 'else' else = expr 'fi'         # if
    | 'while' while = expr 'loop' loop = expr 'pool'                        # while
    | '{' (expressions+=expr';')+ '}'                                       # block
    | CASE case_expr = expr OF (formal IMPLY imply_expr = expr ';')+ ESAC	# case
    | NEW TYPE                                                              # new
    | '~' expr                                                              # bitcompl
    | ISVOID expr                                                           # isvoid
	| left = expr op = (MUL | DIV) right = expr   			                # muldiv
    | left = expr op = (ADD | SUB) right = expr   			                # addsub
    | left = expr op = (LOW | LOE | EQU) right = expr                       # comparer
    | 'not' cond = expr                                                     # not
    | '(' midexp = expr ')'                      			                # parens
    | ID										  			                # id
    | INT                                     			                    # int
    | STRING												                # string
    | value = (TRUE|FALSE)									                # boolean
    | var_name = ID ASSIGNMENT var_value = expr                             # assign
    | LET property (',' property)* IN expr 						            # letIn
    ;

// key words
CLASS               : C L A S S ;
CASE                : C A S E ;
ESAC                : E S A C;
OF                  : O F;
TRUE                : 't' R U E ;
FALSE               : 'f' A L S E ;
INHERITS            : I N H E R I T S;
ISVOID              : I S V O I D ;
NEW                 : N E W ;
IN                  : I N ;
LET                 : L E T ;

STRING              :'"' (ESC | ~ ["\\])* '"';
INT                 :[0-9]+;
TYPE                :[A-Z][_0-9A-Za-z]*;
ID                  :[a-z][_0-9A-Za-z]*;
ASSIGNMENT          :'<-';
IMPLY               :'=>';

WHITESPACE: [ \t\r\n\f]+ -> skip;
BLOCK_COMMENT   :   '(*' (BLOCK_COMMENT|.)*? '*)'   -> channel(HIDDEN);
LINE_COMMENT    :   '--' .*? '\n'                   -> channel(HIDDEN);

MUL 	: '*' ;
DIV 	: '/' ;
ADD 	: '+' ;
SUB 	: '-' ;

LOW     : '<'  ;
LOE     : '<=' ;
EQU     : '='  ;

fragment A: [aA];
fragment C: [cC];
fragment D: [dD];
fragment E: [eE];
fragment F: [fF];
fragment H: [hH];
fragment I: [iI];
fragment L: [lL];
fragment N: [nN];
fragment O: [oO];
fragment P: [pP];
fragment R: [rR];
fragment S: [sS];
fragment T: [tT];
fragment U: [uU];
fragment V: [vV];
fragment W: [wW];

fragment ESC: '\\' (["\\/bfnrt] | UNICODE);
fragment UNICODE: 'u' HEX HEX HEX HEX;
fragment HEX: [0-9a-fA-F];