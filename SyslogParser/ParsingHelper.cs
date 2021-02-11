
// using Antlr4.Runtime.Misc;
using SyslogServer.grammars;


namespace SyslogParser
{


    internal class ParsingHelper
    {


        public static System.Collections.Generic.List<string> LexWithAntlr(string text)
        {
            System.Collections.Generic.List<string> ls = new System.Collections.Generic.List<string>();

            System.IO.StringReader reader = new System.IO.StringReader(text);

            // Antlr4.Runtime.AntlrInputStream input = new Antlr4.Runtime.AntlrInputStream(reader);

            Antlr4.Runtime.ICharStream input1 = new Antlr4.Runtime.AntlrInputStream(reader);
            Antlr4.Runtime.CaseChangingCharStream input = new Antlr4.Runtime.CaseChangingCharStream(input1, true);



            Rfc5424Lexer lexer = new Rfc5424Lexer(input);

            Antlr4.Runtime.CommonTokenStream tokenStream = new Antlr4.Runtime.CommonTokenStream(lexer);
            tokenStream.Fill();




            Rfc5424Parser parser = new Rfc5424Parser(tokenStream);

            Rfc5424Parser.Syslog_msgContext msgContext = parser.syslog_msg();


            RfcVisitor vis = new RfcVisitor();
            string s = vis.Visit(msgContext);


            Antlr4.Runtime.Tree.ParseTreeWalker walker = new Antlr4.Runtime.Tree.ParseTreeWalker();
            // AntlrTsqListener listener = new AntlrTsqListener();
            EverythingListener listener = new EverythingListener();

            // walker.Walk(listener, msgContext);


            // new EverythingListener().EnterBom(parser.bom());
            // new EverythingListener().EnterTimestamp(parser.timestamp());
            // new EverythingListener().EnterEveryRule(parser.version());
            // new EverythingListener().EnterEveryRule(parser.timestamp());



            // var x = parser.msg();
            var x = parser.timestamp();



            Antlr4.Runtime.Misc.Interval msgInt = x.SourceInterval; // new Antlr4.Runtime.Misc.Interval(lastIndex, token.StopIndex);
            string extractedMsg = tokenStream.GetText(msgInt);
            System.Console.WriteLine(extractedMsg);




            int lastIndex = 0;

            foreach (Antlr4.Runtime.IToken token in tokenStream.GetTokens())
            {
                // System.Console.WriteLine(token.Text);
                string tokenTypeName = lexer.Vocabulary.GetSymbolicName(token.Type);
                Antlr4.Runtime.Misc.Interval ival = new Antlr4.Runtime.Misc.Interval(lastIndex, token.StopIndex);
                string extracted = token.InputStream.GetText(ival);

                // table_name, cte_name: ID, SQUARE_BRACKET_ID
                // Local variables: LOCAL_ID

                lastIndex = token.StopIndex + 1;
            } // Next token 

            return ls;
        }


    }


}
