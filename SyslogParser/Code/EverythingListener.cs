
using SyslogServer.grammars;


namespace SyslogParser
{

    public class EverythingListener
    : Rfc5424BaseListener
    {

        public override void EnterEveryRule([Antlr4.Runtime.Misc.NotNull] Antlr4.Runtime.ParserRuleContext context)
        {
            string s = context.GetText();
            System.Console.WriteLine(s);

        }
    }


}
