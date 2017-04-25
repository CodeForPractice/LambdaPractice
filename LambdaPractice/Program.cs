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

            MethodInfo method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });//获取方法名为StartsWith，参数为string的公共方法
            var target = Expression.Parameter(typeof(string), "x");
            var methodArg = Expression.Parameter(typeof(string), "y");
            Expression[] methodArgs = new[] { methodArg };

            //Call(Expression instance, MethodInfo method, params Expression[] arguments)
            Expression call = Expression.Call(target, method, methodArgs);//x.StartsWith(y),以上部件创建CallExpression

            var lambdaParameters = new[] { target, methodArg };//这里使用的参数顺序就是调用委托所使用的参数顺序
            var lambda = Expression.Lambda<Func<string, string, bool>>(call, lambdaParameters);//(x,y)=>x.StartsWith(y),lambdaParameters填充call集合
            var compiled = lambda.Compile();//生成lambda表达式的委托

            Console.WriteLine(compiled("First", "csend"));
            Console.WriteLine(compiled("First", "Fir"));

            Console.Read();
        }
    }
}
