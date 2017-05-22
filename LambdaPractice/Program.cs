using JQ.LambdaResolve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LambdaPractice
{
    class Program
    {
        static void Main(string[] args)
        {
            //Expression firstArg = Expression.Constant(2);
            //Expression secondArg = Expression.Constant(3);
            //Expression add = Expression.Add(firstArg, secondArg);
            //Console.WriteLine(add);

            //Expression firstArg = Expression.Constant(2);
            //Expression secondArg = Expression.Constant(3);
            //Expression add = Expression.Add(firstArg, secondArg);

            //Func<int> compiled = Expression.Lambda<Func<int>>(add).Compile();
            //Console.WriteLine(compiled());

            //Expression<Func<int>> return5 = () => 5;//Lambda表达式
            //Func<int> compiled1 = return5.Compile();
            //Console.WriteLine(compiled1());

            //Expression<Func<string, string, bool>> expression = (x, y) => x.StartsWith(y);
            //var compiled = expression.Compile();

            //Console.WriteLine(compiled("First", "Second"));
            //Console.WriteLine(compiled("First", "Fir"));

            //MethodInfo method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });//获取方法名为StartsWith，参数为string的公共方法
            //var target = Expression.Parameter(typeof(string), "x");
            //var methodArg = Expression.Parameter(typeof(string), "y");
            //Expression[] methodArgs = new[] { methodArg };

            ////Call(Expression instance, MethodInfo method, params Expression[] arguments)
            //Expression call = Expression.Call(target, method, methodArgs);//x.StartsWith(y),以上部件创建CallExpression

            //var lambdaParameters = new[] { target, methodArg };//这里使用的参数顺序就是调用委托所使用的参数顺序
            //var lambda = Expression.Lambda<Func<string, string, bool>>(call, lambdaParameters);//(x,y)=>x.StartsWith(y),lambdaParameters填充call集合
            //var compiled = lambda.Compile();//生成lambda表达式的委托

            //Console.WriteLine(compiled("First", "csend"));
            //Console.WriteLine(compiled("First", "Fir"));

            //ParameterExpression parameterExp1 = Expression.Parameter(typeof(int), "x");
            //ParameterExpression parameterExp2 = Expression.Parameter(typeof(int), "y");
            //BinaryExpression binaryExpression = Expression.Multiply(parameterExp1, parameterExp2);
            //var func = Expression.Lambda<Func<int, int, int>>(binaryExpression, parameterExp1, parameterExp2).Compile();
            //var result = func(5, 2);
            //Console.WriteLine(result.ToString());

            //ParameterExpression parameterExp3 = Expression.Parameter(typeof(int), "x");
            //ParameterExpression parameterExp4 = Expression.Parameter(typeof(int), "y");
            //BinaryExpression binaryExpression2 = Expression.Divide(parameterExp3, parameterExp4);
            //var func2 = Expression.Lambda<Func<int, int, int>>(binaryExpression2, parameterExp3, parameterExp4).Compile();
            //var result2 = func2(5, 2);
            //Console.WriteLine(result2.ToString());

            //BinaryExpression last = Expression.Add(binaryExpression, binaryExpression2);

            //var lastFunc = Expression.Lambda<Func<int, int, int, int, int>>(last, parameterExp1, parameterExp2, parameterExp3, parameterExp4).Compile();
            //var lastResult = lastFunc(5, 2, 5, 2);
            //Console.WriteLine(lastResult.ToString());

            //ParameterExpression parameterExp5 = Expression.Parameter(typeof(int));
            //var blockExp = Expression.Block( Expression.AddAssign(parameterExp5, Expression.Constant(10)), parameterExp5);
            //Console.WriteLine(Expression.Lambda<Func<int,int>>(blockExp, parameterExp5).Compile().Invoke(10).ToString());

            //LabelTarget returnTarget = Expression.Label(typeof(Int32));
            //LabelExpression returnLabel = Expression.Label(returnTarget, Expression.Constant(10, typeof(Int32)));

            //// 为输入参加+10之后返回
            //ParameterExpression inParam3 = Expression.Parameter(typeof(int));
            //BlockExpression block3 = Expression.Block(
            //    Expression.AddAssign(inParam3, Expression.Constant(10)),
            //    Expression.Return(returnTarget, inParam3),
            //    returnLabel);

            //Expression<Func<int, int>> expr3 = Expression.Lambda<Func<int, int>>(block3, inParam3);
            //Console.WriteLine(expr3.Compile().Invoke(20));

            //Expression<Func<User, bool>> func = m => !(m.Age == 1&&m.Sex<11)&&m.Address.Contains("浙江");
            //var array = new int[] { 1,2,3};
            //Expression<Func<User, bool>> func = m => array.Contains(m.Age);
            //LambdaResolve resolve = new LambdaResolve();
            //var result = resolve.Resolve(func.Body);
            //Console.WriteLine(result);
            //var str=ExpressionHelper.GetSqlByExpression(func.Body);
            //Console.WriteLine(str);



            //Expression<Func<User, bool>> func = m => m.Age == 1;
            //var exp = Evaluator.PartialEval(func);
            //Console.WriteLine(exp.ToString());

            var expression1 = ToExpression3();
            ResolveExpression(expression1.Body);

            Console.Read();
        }

        static void ResolveExpression(Expression expression)
        {
            var nodeType = expression.NodeType;
            Console.WriteLine("类型" + nodeType.ToString());
            if (expression is BinaryExpression)
            {
                ResolveBinaryExpression(expression as BinaryExpression);
            }
        }

        static void ResolveBinaryExpression(BinaryExpression expression)
        {
            var expressionLeft = expression.Left;
            ResolveExpression(expressionLeft);
            Console.WriteLine(expression.NodeType);
            var expressionRight = expression.Right;
            ResolveExpression(expressionRight);
        }

        public static Expression<Func<int, int, int, int, int>> ToExpression3()
        {
            ParameterExpression parameter1 = Expression.Parameter(typeof(int), "a");
            ParameterExpression parameter2 = Expression.Parameter(typeof(int), "b");
            BinaryExpression binaryExpression1 = Expression.Multiply(parameter1, parameter2);
            ParameterExpression parameter3 = Expression.Parameter(typeof(int), "c");
            ParameterExpression parameter4 = Expression.Parameter(typeof(int), "d");
            BinaryExpression binaryExpression2 = Expression.Add(parameter3, parameter4);
            return Expression.Lambda<Func<int, int, int, int, int>>(Expression.Multiply(binaryExpression1, binaryExpression2), parameter1, parameter2, parameter3, parameter4);
        }

        public static Expression<Func<User, bool>> ToExpression4(int age)
        {
            ParameterExpression parameter1 = Expression.Parameter(typeof(User), "m");
            MemberExpression member1 = Expression.Property(parameter1, "Age");
            BinaryExpression binary = Expression.LessThanOrEqual(member1, Expression.Constant(age, typeof(int)));
            return Expression.Lambda<Func<User, bool>>(binary, parameter1);
        }
    }
    class User
    {
        public string Name { get; set; }

        public string Address { get; set; }


        public int Age { get; set; }

        public int Sex { get; set; }
    }
}
