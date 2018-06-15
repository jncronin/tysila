using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TableMap
{
    internal abstract class Statement
    {
        public class SyntaxException : Exception
        {
            public SyntaxException(string msg) : base(msg) { }
        }

        public abstract Expression.EvalResult Execute(MakeState s);
        public bool export = false;

        public static bool FileDirExists(string name)
        {
            try
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(name);
                if (fi.Exists)
                    return true;
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(name);
                return di.Exists;
            } 
            catch (Exception)
            {
                return false;
            }
        }

        internal static void ExportDef(string tag, MakeState s)
        {
            Expression.EvalResult e = s.GetDefine(tag);
            MakeState cur_s = s.parent;
            while (cur_s != null)
            {
                cur_s.SetDefine(tag, e);
                cur_s = cur_s.parent;
            }
        }
    }

    internal abstract class DefineStatement : Statement
    {
        public Tokens assignop;
    }

    internal class DefineStringStatement : DefineStatement 
    {
        public string tok_name;
        public string val;

        public override Expression.EvalResult Execute(MakeState s)
        {
            Expression.EvalResult e = new Expression.EvalResult(val);

            s.SetDefine(tok_name, e, assignop);
            if (export)
                ExportDef(tok_name, s);
            return new Expression.EvalResult(0);
        }
    }

    internal class DefineLabelStatement : DefineStatement
    {
        public string tok_name;
        public string val;

        public override Expression.EvalResult Execute(MakeState s)
        {
            LabelExpression le = new LabelExpression { val = val };
            Expression.EvalResult e = le.Evaluate(s);
            s.SetDefine(tok_name, e, assignop);
            if (export)
                ExportDef(tok_name, s);
            return new Expression.EvalResult(0);
        }
    }

    internal class DefineExprStatement : DefineStatement
    {
        public Expression val;
        public Expression tok_name;

        public override Expression.EvalResult Execute(MakeState s)
        {
            Expression.EvalResult e = val.Evaluate(s);

            if (tok_name is LabelExpression)
            {
                var tn = ((LabelExpression)tok_name).val;
                s.SetLocalDefine(tn, e);
                if (export)
                    ExportDef(tn, s);
            }
            else if(tok_name is LabelMemberExpression)
            {
                // left-most member must be a define
                var lme = tok_name as LabelMemberExpression;
                if (!(lme.label is LabelExpression))
                    throw new Exception("unable to assign to " + tok_name.ToString());

                var o = s.GetDefine(((LabelExpression)lme.label).val);

                // Now iterate through the various members, looking for
                //  what to set
                Expression cur_member_lval = lme.member;
                while(true)
                {
                    if (cur_member_lval is LabelExpression)
                    {
                        // we've reached the end of the chain
                        break;
                    }
                    else if (cur_member_lval is LabelMemberExpression)
                    {
                        var new_lme = cur_member_lval as LabelMemberExpression;

                        if (!(new_lme.label is LabelExpression))
                            throw new Exception("unable to assign to " + tok_name.ToString());
                        lme = new_lme;
                        cur_member_lval = lme.member;

                        if (o.Type != Expression.EvalResult.ResultType.Object)
                            throw new Exception();

                        o = o.objval[((LabelExpression)lme.label).val];
                    }
                    else
                        throw new NotImplementedException();
                }

                string member_name = ((LabelExpression)cur_member_lval).val;
                o.objval[member_name] = e;
            }
            else if (tok_name is LabelIndexedExpression)
            {
                var chain = get_chain(tok_name);

                // left-most member must be a define
                var lme = chain[0];
                if (!(lme is LabelExpression))
                    throw new Exception("unable to assign to " + tok_name.ToString());

                var o = s.GetDefine(((LabelExpression)lme).val);

                // Now iterate through the various members, looking for
                //  what to set
                o = follow_chain(o, chain, 1, chain.Count - 1, s);

                if (o.Type != Expression.EvalResult.ResultType.Array)
                    throw new Exception("unable to assign to " + tok_name.ToString());

                var idx = ((LabelIndexedExpression)tok_name).index.Evaluate(s).AsInt;

                // increase the array size as appropriate
                if(idx >= o.arrval.Count)
                {
                    o.arrval.Capacity = (int)idx * 3 / 2;
                    while (idx >= o.arrval.Count)
                        o.arrval.Add(new Expression.EvalResult { Type = Expression.EvalResult.ResultType.Null });
                }
                o.arrval[(int)idx] = e;
            }
            else
                throw new NotImplementedException();
            return new Expression.EvalResult(0);
        }

        private Expression.EvalResult follow_chain(Expression.EvalResult o,
            List<Expression> chain, int cur_idx, int max_idx,
            MakeState s)
        {
            var next_member = chain[cur_idx];
            Expression.EvalResult next_result = null;

            if (next_member is LabelIndexedExpression)
            {
                var lie = next_member as LabelIndexedExpression;
                if (o.Type != Expression.EvalResult.ResultType.Array)
                    throw new Exception();
                next_result = o.arrval[(int)lie.index.Evaluate(s).AsInt];
            }
            else if (next_member is LabelMemberExpression)
            {
                var lme = next_member as LabelMemberExpression;
                if (o.Type != Expression.EvalResult.ResultType.Object)
                    throw new Exception();
                next_result = o.objval[((LabelExpression)lme.member).val];
            }
            else
                throw new NotImplementedException();

            cur_idx++;
            if (cur_idx == max_idx)
                return next_result;

            return follow_chain(next_result, chain, cur_idx, max_idx, s);
        }

        private List<Expression> get_chain(Expression tok_name)
        {
            List<Expression> ret = new List<Expression>();
            Expression cur_expr = tok_name;

            while(true)
            {
                ret.Insert(0, cur_expr);
                if (cur_expr is LabelIndexedExpression)
                {
                    cur_expr = ((LabelIndexedExpression)cur_expr).label;
                }
                else if (cur_expr is LabelMemberExpression)
                {
                    cur_expr = ((LabelMemberExpression)cur_expr).label;
                }
                else
                    break;
            }
            return ret;
        }
    }

    internal class DefineIntStatement : DefineStatement
    { 
        public string tok_name; 
        public int val;

        public override Expression.EvalResult Execute(MakeState s)
        {
            Expression.EvalResult e = new Expression.EvalResult(val);
            s.SetDefine(tok_name, e, assignop);
            if (export)
                ExportDef(tok_name, s);
            return new Expression.EvalResult(0);
        }
    }

    internal abstract class ControlStatement : Statement
    {
        public Statement code;
        public Expression test;
    }

    internal class IfBlockStatement : ControlStatement 
    { 
        public Statement if_block; 
        public Statement else_block;

        public override Expression.EvalResult Execute(MakeState s)
        {
            if (test.Evaluate(s).AsInt == 0)
            {
                if (else_block != null)
                    return else_block.Execute(s);
            }
            else
            {
                if (if_block != null)
                    return if_block.Execute(s);
            }
            return new Expression.EvalResult(0);
        }
    }

    internal class DoBlock : ControlStatement
    {
        public override Expression.EvalResult Execute(MakeState s)
        {
            throw new NotImplementedException();
        }
    }

    internal class ForBlockStatement : ControlStatement
    {
        public Statement incr, init;

        public override Expression.EvalResult Execute(MakeState s)
        {
            // run initializer
            Expression.EvalResult ret = init.Execute(s);
            if (ret.AsInt != 0)
                return ret;

            while(true)
            {
                // check condition
                if (test.Evaluate(s).AsInt == 0)
                    break;

                // exec code
                ret = code.Execute(s);
                if (ret.AsInt != 0)
                    return ret;
                if(s.returns != null)
                {
                    return new Expression.EvalResult(0);
                }

                // run incrementer
                incr.Execute(s);
            }
            return new Expression.EvalResult(0);
        }
    }

    internal class ForEachBlock : ControlStatement
    {
        public Expression enumeration;
        public string val;

        public override Expression.EvalResult Execute(MakeState s)
        {
            Expression.EvalResult e = enumeration.Evaluate(s);
            if (e.Type != Expression.EvalResult.ResultType.Array)
                throw new Exception("does not evaluate to array");

            foreach (Expression.EvalResult i in e.arrval)
            {
                //MakeState cur_s = s.Clone();
                var cur_s = s;
                cur_s.SetLocalDefine(val, i);
                Expression.EvalResult ret = code.Execute(cur_s);
                if (ret.AsInt != 0)
                    return ret;
                if (cur_s.returns != null)
                {
                    s.returns = cur_s.returns;
                    return new Expression.EvalResult(0);
                }
            }
            return new Expression.EvalResult(0);
        }
    }

    internal class WhileBlock : ControlStatement
    {
        public override Expression.EvalResult Execute(MakeState s)
        {
            throw new NotImplementedException();
        }
    }

    internal class FunctionStatement : Statement
    {
        public string name;
        public List<FunctionArg> args;
        public Statement code;

        public override Expression.EvalResult Execute(MakeState s)
        {
            string mangledname = Mangle();
            s.funcs[mangledname] = this;

            if (export)
            {
                MakeState cur_s = s.parent;
                while (cur_s != null)
                {
                    cur_s.funcs[mangledname] = this;
                    cur_s = cur_s.parent;
                }
            }

            return new Expression.EvalResult(0);
        }

        public virtual Expression.EvalResult Run(MakeState s, List<Expression.EvalResult> passed_args)
        {
            MakeState new_s = s.Clone();
            new_s.ClearLocalDefines();

            for (int i = 0; i < args.Count; i++)
            {
                if (args[i].argtype == Expression.EvalResult.ResultType.Function)
                {
                    FunctionStatement fs = new FunctionStatement();
                    var ofs = passed_args[i].funcval;
                    fs.name = args[i].name;
                    fs.args = ofs.args;
                    fs.code = ofs.code;

                    new_s.funcs[fs.Mangle()] = fs;
                }
                else
                    new_s.SetLocalDefine(args[i].name, passed_args[i]);
            }

            code.Execute(new_s);
            if (new_s.returns != null)
                return new_s.returns;

            return new Expression.EvalResult();
        }

        public class FunctionArg
        {
            public string name;
            public Expression.EvalResult.ResultType argtype;
        }

        public string Mangle()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(name.Length.ToString());
            sb.Append(name);
            foreach (FunctionArg arg in args)
            {
                switch (arg.argtype)
                {
                    case Expression.EvalResult.ResultType.Int:
                        sb.Append("i");
                        break;
                    case Expression.EvalResult.ResultType.String:
                        sb.Append("s");
                        break;
                    case Expression.EvalResult.ResultType.Array:
                        sb.Append("a");
                        break;
                    case Expression.EvalResult.ResultType.Object:
                        sb.Append("o");
                        break;
                    case Expression.EvalResult.ResultType.Void:
                        sb.Append("v");
                        break;
                    case Expression.EvalResult.ResultType.Function:
                        sb.Append("f");
                        break;
                    case Expression.EvalResult.ResultType.Any:
                        sb.Append("x");
                        break;
                }
            }
            return sb.ToString();
        }
    }

    internal class ExportStatement : Statement
    {
        public string v;

        public override Expression.EvalResult Execute(MakeState s)
        {
            if (s.IsDefined(v) == false)
            {
                throw new Exception("export: variable " + v + " is not defined in this scope");
            }

            Expression.EvalResult e = s.GetDefine(v);
            s.SetDefine(v, e);
            MakeState cur_s = s.parent;
            while (cur_s != null)
            {
                cur_s.SetDefine(v, e);
                cur_s = cur_s.parent;
            }

            return new Expression.EvalResult(0);
        }
    }

    internal class ReturnStatement : Statement
    {
        public Expression v;

        public override Expression.EvalResult Execute(MakeState s)
        {
            s.returns = v.Evaluate(s);
            return new Expression.EvalResult(0);
        }
    }

    internal class StatementList : Statement 
    { 
        public List<Statement> list;

        public override Expression.EvalResult Execute(MakeState s)
        {
            if (list != null)
            {
                foreach (Statement st in list)
                {
                    Expression.EvalResult er = st.Execute(s);
                    if (!(st is ExpressionStatement) && er.AsInt != 0)
                        return er;
                    if (s.returns != null)
                        return new Expression.EvalResult(0);
                }
            }
            return new Expression.EvalResult(0);
        }
    }

    internal class StringStatement : Statement 
    { 
        public string val;

        public override Expression.EvalResult Execute(MakeState s)
        {
            throw new NotImplementedException();
        }
    }

    internal class ExpressionStatement : Statement
    {
        public Expression expr;

        public override Expression.EvalResult Execute(MakeState s)
        {
            return expr.Evaluate(s);
        }
    }

    internal class LabelStatement : Statement 
    { 
        public string val;

        public override Expression.EvalResult Execute(MakeState s)
        {
            throw new NotImplementedException();
        }
    }

    internal class IncludeStatement : Statement 
    { 
        public Expression include_file;

        public override Expression.EvalResult Execute(MakeState s)
        {
            var f = include_file.Evaluate(s).strval;

            MakeState new_s = s.Clone();

            return Program.ExecuteFile(f, new_s);
        }
    }
}
