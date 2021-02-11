
using Antlr4.Runtime.Misc;
using SyslogServer.grammars;


namespace SyslogParser
{

    public class RfcVisitor
        : Rfc5424BaseVisitor<string>
    {


        public override string VisitHeaderVersion([NotNull] Rfc5424Parser.HeaderVersionContext context)
        {
            string s = base.VisitChildren(context);
            return s;
        }


        public override string VisitHeaderAppName([NotNull] Rfc5424Parser.HeaderAppNameContext context)
        {
            string s = VisitChildren(context);
            return s;
        }


        public override string VisitHeaderNilAppName([NotNull] Rfc5424Parser.HeaderNilAppNameContext context)
        {
            string s = base.VisitChildren(context);
            return s;
        }


        public override string Visit([Antlr4.Runtime.Misc.NotNull]
        Antlr4.Runtime.Tree.IParseTree tree)
        {
            string t = tree.GetText();
            System.Console.WriteLine(t);

            return base.Visit(tree);
        }

    }


}
