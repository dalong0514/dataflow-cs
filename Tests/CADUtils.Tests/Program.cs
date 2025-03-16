using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace CADUtils.Tests
{
    /// <summary>
    /// 测试运行器，用于运行单元测试
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("开始运行 CADUtils 单元测试...");
            
            try
            {
                // 获取当前程序集
                Assembly assembly = Assembly.GetExecutingAssembly();
                
                // 查找所有测试类
                var testClasses = assembly.GetTypes()
                    .Where(t => t.GetCustomAttributes(typeof(TestClassAttribute), true).Length > 0)
                    .ToList();
                
                Console.WriteLine($"找到 {testClasses.Count} 个测试类");
                
                int totalTests = 0;
                int passedTests = 0;
                int failedTests = 0;
                List<string> failureMessages = new List<string>();
                
                // 遍历每个测试类
                foreach (var testClass in testClasses)
                {
                    Console.WriteLine($"运行测试类: {testClass.Name}");
                    
                    // 创建测试类实例
                    object instance = Activator.CreateInstance(testClass);
                    
                    // 查找所有测试初始化方法
                    var initMethods = testClass.GetMethods()
                        .Where(m => m.GetCustomAttributes(typeof(TestInitializeAttribute), true).Length > 0)
                        .ToList();
                    
                    // 查找所有测试方法
                    var testMethods = testClass.GetMethods()
                        .Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute), true).Length > 0)
                        .ToList();
                    
                    totalTests += testMethods.Count;
                    
                    // 遍历每个测试方法
                    foreach (var testMethod in testMethods)
                    {
                        try
                        {
                            // 调用测试初始化方法
                            foreach (var initMethod in initMethods)
                            {
                                initMethod.Invoke(instance, null);
                            }
                            
                            // 调用测试方法
                            Console.WriteLine($"  运行测试: {testMethod.Name}");
                            testMethod.Invoke(instance, null);
                            
                            // 测试通过
                            passedTests++;
                        }
                        catch (Exception ex)
                        {
                            // 测试失败
                            failedTests++;
                            
                            // 获取实际异常
                            Exception actualException = ex;
                            if (ex is TargetInvocationException && ex.InnerException != null)
                            {
                                actualException = ex.InnerException;
                            }
                            
                            // 记录失败信息
                            failureMessages.Add($"- {testClass.Name}.{testMethod.Name}: {actualException.Message}");
                            Console.WriteLine($"  测试失败: {actualException.Message}");
                        }
                    }
                }
                
                // 输出测试结果
                Console.WriteLine("\n测试结果摘要:");
                Console.WriteLine($"测试总数: {totalTests}");
                Console.WriteLine($"通过: {passedTests}");
                Console.WriteLine($"失败: {failedTests}");
                
                // 如果有失败的测试，输出详细信息
                if (failedTests > 0)
                {
                    Console.WriteLine("\n失败的测试:");
                    foreach (var failure in failureMessages)
                    {
                        Console.WriteLine(failure);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"运行测试时发生错误: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            
            Console.WriteLine("\n测试运行完成。按任意键退出...");
            Console.ReadKey();
        }
    }
} 