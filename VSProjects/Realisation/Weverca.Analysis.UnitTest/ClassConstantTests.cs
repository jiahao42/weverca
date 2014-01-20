﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.UnitTest
{
    [TestClass]
    public class ClassConstantTests
    {
        string ClassConstantTest = @"
            class a
            { 
                const x=0;
            }
            $result=a::x;
        ";

        [TestMethod]
        public void ClassConstant()
        {
            var result = TestUtils.ResultTest(ClassConstantTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 0);
        }

        string ClassConstantTest2 = @"
            class a
            { 
                const x=0;
            }
            $a='a';
            $result=$a::x;
        ";

        [TestMethod]
        public void ClassConstant2()
        {
            var result = TestUtils.ResultTest(ClassConstantTest2);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 0);
        }

        string ClassConstantTest3 = @"
            class a
            { 
                const x=0;
            }
            $a=new a();
            $result=a::x;
        ";

        [TestMethod]
        public void ClassConstant3()
        {
            var result = TestUtils.ResultTest(ClassConstantTest3);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 0);
        }


        string ClassConstantTest4 = @"
                class a extends DateTime
                {

                }

                $result=a::ATOM;
        ";

        [TestMethod]
        public void ClassConstant4()
        {
            var result = TestUtils.ResultTest(ClassConstantTest4);
            TestUtils.testType(result, typeof(StringValue));

        }

        string ClassConstantTest5 = @"
                $result=dateTime::ATOM;
        ";

        [TestMethod]
        public void ClassConstant5()
        {
            var result = TestUtils.ResultTest(ClassConstantTest5);
            TestUtils.testType(result, typeof(StringValue));

        }


        string InterfaceConstantTest = @"
                interface a
                {
                    const ATOM='a';
                }                

                $result=a::ATOM;
        ";

        [TestMethod]
        public void InterfaceConstant()
        {
            var result = TestUtils.ResultTest(InterfaceConstantTest);
            TestUtils.testType(result, typeof(StringValue));

        }

        string ClassConstantTest6 = @"
                $result=a::ATOM;
        ";

        [TestMethod]
        public void ClassConstant6()
        {
             var outset = TestUtils.Analyze(ClassConstantTest6);
             TestUtils.ContainsWarning(outset, AnalysisWarningCause.CLASS_CONSTANT_DOESNT_EXIST);

        }

        string ClassConstantTest7 = @"
                class a {    }
                $result=a::ATOM;
        ";

        [TestMethod]
        public void ClassConstant7()
        {
            var outset = TestUtils.Analyze(ClassConstantTest7);
            TestUtils.ContainsWarning(outset, AnalysisWarningCause.CLASS_CONSTANT_DOESNT_EXIST);

        }

        string ClassConstantTest8 = @"
                class a {    }
                $a='aaa';
                $result=$a::ATOM;
        ";

        [TestMethod]
        public void ClassConstant8()
        {
            var outset = TestUtils.Analyze(ClassConstantTest8);
            TestUtils.ContainsWarning(outset, AnalysisWarningCause.CLASS_CONSTANT_DOESNT_EXIST);

        }

        string ClassConstantTest9 = @"
                $a=4;
                $result=$a::ATOM;
        ";

        [TestMethod]
        public void ClassConstant9()
        {
            var outset = TestUtils.Analyze(ClassConstantTest9);
            TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_ACCESS_CONSTANT_ON_NON_OBJECT);

        }

    }
}
