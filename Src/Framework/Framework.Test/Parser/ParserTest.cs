﻿////////////////////////////////////////////////////////
/*
  This file is part of CNCLib - A library for stepper motors.

  Copyright (c) 2013-2018 Herbert Aitenbichler

  CNCLib is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  CNCLib is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.
  http://www.gnu.org/licenses/
*/

namespace Framework.Test.Parser
{
    using System;

    using FluentAssertions;

    using Framework.Parser;

    using Xunit;

    public class ParserTest
    {
        #region Constant

        [Fact]
        public void ParserConstantValueInt()
        {
            var stream = new CommandStream() { Line = "1" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(1.0);
        }

        [Fact]
        public void ParserConstantValueIntMinus()
        {
            var stream = new CommandStream() { Line = "-1" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(-1.0);
        }

        [Fact]
        public void ParserConstantValueFloat()
        {
            var stream = new CommandStream() { Line = "1.5" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(1.5);
        }

        [Fact]
        public void ParserConstantValueFloatMinus()
        {
            var stream = new CommandStream() { Line = "-1.5" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(-1.5);
        }

        [Fact]
        public void ParserConstantValueFloatDot()
        {
            var stream = new CommandStream() { Line = ".5" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(0.5);
        }

        [Fact]
        public void ParserConstantValueFloatDotMinus()
        {
            var stream = new CommandStream() { Line = "-.5" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(-0.5);
        }

        #endregion

        #region AddEntity/Sub/Mul/Div

        [Fact]
        public void ParserAdd()
        {
            var stream = new CommandStream() { Line = "1+2" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(3.0);
        }

        [Fact]
        public void ParserSub()
        {
            var stream = new CommandStream() { Line = "9-2" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(7.0);
        }

        [Fact]
        public void ParserMul()
        {
            var stream = new CommandStream() { Line = "2*3" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(6.0);
        }

        [Fact]
        public void ParserDiv()
        {
            var stream = new CommandStream() { Line = "10/2" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(5.0);
        }

        [Fact]
        public void ParserMod()
        {
            var stream = new CommandStream() { Line = "14%10" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(4.0);
        }

        [Fact]
        public void ParserPow()
        {
            var stream = new CommandStream() { Line = "2^8" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(256);
        }

        #endregion

        #region BitOpAndShift

        [Fact]
        public void ParserBitLeftRight()
        {
            var stream = new CommandStream() { Line = "(1<<1)+(16>>1)" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(8.0 + 2.0);
        }

        [Fact]
        public void ParserBitAndOr()
        {
            var stream = new CommandStream() { Line = "(255&8)+(1|2)" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(11.0);
        }

        [Fact]
        public void ParserXOr()
        {
            var stream = new CommandStream() { Line = "(3||1)" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(2.0);
        }

        #endregion

        #region Compare

        [Fact]
        public void ParserCompareEqual()
        {
            var stream = new CommandStream() { Line = "(1==2)+(1==1)" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(1.0);
        }

        [Fact]
        public void ParserCompareNotEqual()
        {
            var stream = new CommandStream() { Line = "(1!=2)+(1!=1)" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(1.0);
        }

        [Fact]
        public void ParserCompareGt()
        {
            var stream = new CommandStream() { Line = "(1>2)+(1>=1)+(4>1)" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(2.0);
        }

        [Fact]
        public void ParserCompareLt()
        {
            var stream = new CommandStream() { Line = "(1<2)+(1<=1)+(4<1)" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(2.0);
        }

        #endregion

        #region Functions

        [Fact]
        public void ParserAbsCeilFloor()
        {
            var stream = new CommandStream() { Line = "ABS(-1)+FIX(1.8)+FUP(0.5)+ROUND(1.7)" }; // FIX=>floor, FUP=>Ceil
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(5.0);
        }

        [Fact]
        public void ParserSin()
        {
            var stream = new CommandStream() { Line = "SIN(1.5707963267948966192313216916398)" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(1.0);
        }

        [Fact]
        public void ParserSinCosTan()
        {
            var stream = new CommandStream()
            {
                Line = "SIN(1.5707963267948966192313216916398)+COS(0)+TAN(0.78539816339744830961566084581988)"
            };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(3.0);
        }

        [Fact]
        public void ParserASinACosATan()
        {
            var stream = new CommandStream() { Line = "ASIN(1.0)+ACOS(1.0)+ATAN(1.0)" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(2.3561944901923448);
        }

        [Fact]
        public void ParserSqrtFractional()
        {
            var stream = new CommandStream() { Line = "SQRT(9)+FACTORIAL(2)" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(5);
        }

        [Fact]
        public void ParserSign()
        {
            var stream = new CommandStream() { Line = "SIGN(-100)+SIGN(30)+SIGN(0)+SIGN(234.32)" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(1.0);
        }

        [Fact]
        public void ParserLog()
        {
            var stream = new CommandStream() { Line = "LOG(2)+LOG10(10)" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(2.0);
        }

        #endregion

        #region Variable

        [Fact]
        public void ParserConstantValuePi()
        {
            var stream = new CommandStream() { Line = "PI" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(Math.PI);
        }

        [Fact]
        public void ParserConstantValueE()
        {
            var stream = new CommandStream() { Line = "E" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(Math.E);
        }

        [Fact]
        public void ParserAssignVariable()
        {
            var stream = new CommandStream() { Line = "VAR=1" };
            var parser = new ExpressionParser(stream);

            parser.Parse();

            parser.IsError().Should().BeFalse();
            parser.Answer.Should().Be(1);
        }

        #endregion
    }
}